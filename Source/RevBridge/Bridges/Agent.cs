using RevBridge.Framework.SilkroadSecurityApi;
using Serilog;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows;

namespace RevBridge.Bridges
{
    internal sealed class Agent : IDisposable
    {
        public Definitions.Character Character { get; } = new Definitions.Character();
        public IPAddress ClientIp { get; }

        public readonly Security SecurityProxyToClient;
        public readonly Security SecurityProxyToServer = new Security();

        private Serilog.Core.Logger _logger;

        private readonly Socket _clientSocket;
        private readonly Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private readonly byte[] _clientLocalBuffers = new byte[4096];
        private readonly byte[] _clientRemoteBuffers = new byte[4096];

        private Functions.PacketHandlers.GroupSpawn _groupSpawn;

        private bool _disconnectCalled;

        public Agent(ref Socket clientSocket)
        {
            try
            {
                Character.Agent = this;
                _clientSocket = clientSocket;

                ClientIp = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;

                if (!Properties.Settings.Default.Security_ProxyList_FirewallBlock && Definitions.List.Security.ProxyIps.ContainsKey(ClientIp.ToString()))
                {
                    Disconnect();
                    return;
                }

                SecurityProxyToClient = new Security();
                SecurityProxyToClient.ChangeIdentity("SR_Client", 0);
                SecurityProxyToClient.GenerateSecurity(true, true, true);

                DoRecvFromClient();
                Task.Run(HandleOutgoingClientPackets_SendPacketsToServerAsync);
                Task.Run(HandleOutgoingServerPacketsAsync);
                ConnectToServer();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void ConnectToServer()
        {
            try
            {
                _serverSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.RevBridge_Agent_IP), Properties.Settings.Default.RevBridge_Agent_RealPort), ConnectToServerCallback, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void ConnectToServerCallback(IAsyncResult ar)
        {
            try
            {
                _serverSocket.EndConnect(ar);

                DoRecvFromServer();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Disconnect();
            }
        }

        private void DoRecvFromClient()
        {
            try
            {
                _clientSocket.BeginReceive(_clientLocalBuffers, 0, _clientLocalBuffers.Length, SocketFlags.None, OnReceiveFromClient, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);
                Disconnect();
            }
        }

        private void OnReceiveFromClient(IAsyncResult ar)
        {
            try
            {
                var bytesRead = _clientSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    SecurityProxyToClient.Recv(_clientLocalBuffers, 0, bytesRead);

                    HandleIncomingClientPackets();
                }
                else
                {
                    Debug.WriteLine("Bitti? Agent?? - " + Character.AccountName);
                    Disconnect();
                    return;
                }

                DoRecvFromClient();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
                Disconnect();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Disconnect();
            }
        }

        private void DoRecvFromServer()
        {
            try
            {
                _serverSocket.BeginReceive(_clientRemoteBuffers, 0, _clientRemoteBuffers.Length, SocketFlags.None, OnReceiveFromServer, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);

                Disconnect();
            }
        }

        private void OnReceiveFromServer(IAsyncResult ar)
        {
            try
            {
                var bytesRead = _serverSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    SecurityProxyToServer.Recv(_clientRemoteBuffers, 0, bytesRead);

                    HandleIncomingServerPackets();
                }
                else
                {
                    return;
                }

                DoRecvFromServer();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);

                Disconnect();
            }
        }

        private void HandleIncomingClientPackets()
        {
            try
            {
                var receivedPackets = SecurityProxyToClient.TransferIncoming();
                if (receivedPackets == null) return;

                for (var i = 0; i < receivedPackets.Count; i++)
                {
                    var currentPacket = receivedPackets[i];

                    if (!Properties.Settings.Default.RevBridge_Debugging)
                    {
                        LogPackets(currentPacket, Definitions.Enums.Common.PacketType.IncomingClientPacket);
                    }

                    switch (currentPacket.Opcode)
                    {
                        case Opcodes.Global.Client.MODULE_IDENTIFICATION:
                            continue;
                        case Opcodes.Agent.Client.AUTH:
                            currentPacket.ReadInt(); // Token
                            Character.TempAccountName = currentPacket.ReadAscii();
                            break;

                        case Opcodes.Agent.Client.GAME_READY:

                            if (!string.IsNullOrEmpty(Properties.Settings.Default.Server_WelcomeMessage) && Character.IsFirstSpawn)
                                SecurityProxyToClient.Send(Functions.PacketCreators.Chat.Notice(string.Format(Properties.Settings.Default.Server_WelcomeMessage, Character.Name)));

                            Character.IsFirstSpawn = false;

                            foreach (var packetToSend in Definitions.List.RegisteredPackets) { SecurityProxyToClient.Send(packetToSend); }
                            break;

                        case Opcodes.Agent.Client.CHARACTER_SELECTION_JOIN:
                            Character.Name = currentPacket.ReadAscii();
                            break;

                        case Opcodes.Agent.Client.CHAT:
                            {
                                if (!Functions.PacketHandlers.Character.ChatPacket(currentPacket, Character))
                                {
                                    continue;
                                }
                            }
                            break;
                    }
                    SecurityProxyToServer.Send(currentPacket);
                }
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void HandleIncomingServerPackets()
        {
            try
            {
                var receivedPackets = SecurityProxyToServer.TransferIncoming();
                if (receivedPackets == null) return;

                for (var i = 0; i < receivedPackets.Count; i++)
                {
                    var currentPacket = receivedPackets[i];

                    if (!Properties.Settings.Default.RevBridge_Debugging)
                    {
                        LogPackets(currentPacket, Definitions.Enums.Common.PacketType.IncomingServerPacket);
                    }

                    switch (currentPacket.Opcode)
                    {
                        case Opcodes.Agent.Server.AUTH:
                            if (currentPacket.ReadByte() == 0x01)
                            {
                                Character.AccountName = Character.TempAccountName;

                                Character.TempAccountName = null;

                                _logger = new LoggerConfiguration()
                                    .WriteTo.Async(a =>
                                    {
                                        a.File(string.Format(Definitions.RevBridge.Agent.Logger.ConnectionFileFormat,
                                                Character.AccountName,
                                                DateTime.Now.ToShortDateString(),
                                                DateTime.Now.ToString("HH-mm-ss"))
                                            , rollOnFileSizeLimit: true, fileSizeLimitBytes: Definitions.RevBridge.Agent.Logger.FileSizeLimit);
                                    }, bufferSize: Definitions.RevBridge.Agent.Logger.ConnectionBufferSize)
                                    .CreateLogger();
                            }
                            break;

                        case Opcodes.Agent.Server.CHAR_DATA:
                            Functions.PacketHandlers.Character.SpawnPacket(currentPacket, Character);

                            if (Character.IsGm && Properties.Settings.Default.Security_GMList_Active && !Properties.Settings.Default.Security_GMList.Contains(Character.AccountName.ToLowerInvariant()))
                            {
                                Definitions.List.ProgramLogger.Fatal($"GM listesinde bulunmayan bir hesap GM girişi yaptı! Kullanıcı adı: {Character.AccountName} - IP: {Character.Agent.ClientIp}");
                                Character.Agent.Disconnect();
                            }

                            break;

                            /*
                            case Opcodes.Agent.Server.CHAR_ACTION_DATA:
                                Functions.PacketHandlers.Character.CastingAndBasicHits(currentPacket, Character);
                                break;

                            case Opcodes.Agent.Server.SKILL_DATA:
                                Functions.PacketHandlers.Character.SkillHits(currentPacket, Character);
                                break;
                                */

                            //case Opcodes.Agent.Server.ENTITY_GROUPSPAWN_START:
                            //    _groupSpawn = new Functions.PacketHandlers.GroupSpawn();

                            //    _groupSpawn.Begin(currentPacket);
                            //    break;

                            //case Opcodes.Agent.Server.ENTITY_GROUPSPAWN_DATA:
                            //    _groupSpawn.Data(currentPacket);
                            //    break;

                            //case Opcodes.Agent.Server.ENTITY_GROUPSPAWN_END:
                            //    _groupSpawn.End(Character);
                            //    break;

                            //case Opcodes.Agent.Server.ENTITY_SOLO_SPAWN:
                            //    Functions.PacketHandlers.Entity.SingleSpawn(currentPacket, Character);
                            //    break;

                            //case Opcodes.Agent.Server.ENTITY_SOLO_DESPAWN:
                            //    Functions.PacketHandlers.Entity.SingleDespawn(currentPacket, Character);
                            //    break;
                    }

                    SecurityProxyToClient.Send(currentPacket);
                }
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private async Task HandleOutgoingClientPackets_SendPacketsToServerAsync()
        {
            try
            {
                while (!SecurityProxyToClient.HasPacketToSend())
                {
                    await Task.Delay(50).ConfigureAwait(false);
                }

                var outgoingPackets = SecurityProxyToClient.TransferOutgoing();

                if (outgoingPackets == null)
                    return;

                for (var i = 0; i < outgoingPackets.Count; i++)
                {
                    var outgoingPacket = outgoingPackets[i];
                    var Packet = outgoingPacket.Value;

                    LogPackets(Packet, Definitions.Enums.Common.PacketType.OutgoingClientPacket);

                    /*
                    switch (Packet.Opcode)
                    {
                        default:
                            break;
                    }
                    */

                    SendToServerAsClient(outgoingPacket.Key.Buffer);
                }

                await HandleOutgoingClientPackets_SendPacketsToServerAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private async Task HandleOutgoingServerPacketsAsync()
        {
            try
            {
                while (!SecurityProxyToServer.HasPacketToSend())
                {
                    await Task.Delay(50).ConfigureAwait(false);
                }

                var outgoingPackets = SecurityProxyToServer.TransferOutgoing();

                for (var i = 0; i < outgoingPackets.Count; i++)
                {
                    var outgoingPacket = outgoingPackets[i];
                    var Packet = outgoingPacket.Value;

                    LogPackets(Packet, Definitions.Enums.Common.PacketType.OutgoingServerPacket);

                    /*
                    switch (Packet.Opcode)
                    {
                        default:
                            break;
                    }
                    */

                    SendToClientAsServer(outgoingPacket.Key.Buffer);
                }

                await HandleOutgoingServerPacketsAsync().ConfigureAwait(false);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);

                Disconnect();
            }
        }

        private void SendToServerAsClient(byte[] buffer)
        {
            try
            {
                _clientSocket.BeginSend(buffer, 0, buffer.Length, 0, SendTClient, null);
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Disconnect();
            }
        }

        private void SendTClient(IAsyncResult ar)
        {
            try
            {
                _clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void SendToClientAsServer(byte[] buffer)
        {
            try
            {
                _serverSocket.BeginSend(buffer, 0, buffer.Length, 0, SendTServer, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);

                Disconnect();
            }
        }

        private void SendTServer(IAsyncResult ar)
        {
            try
            {
                _serverSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                Debug.WriteLine(Environment.StackTrace);

                Disconnect();
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_disconnectCalled)
                    return;

                _disconnectCalled = true;

                if (Definitions.List.AgentConnections.Contains(this))
                {
                    Definitions.List.AgentConnections.Remove(this);
                }

                Dispose();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        private void LogPackets(Packet packet, Definitions.Enums.Common.PacketType packetType)
        {
            try
            {
                if (_logger == null || !Properties.Settings.Default.RevBridge_Debugging)
                    return;

                var logMessage = $"[{((packetType == (byte)Definitions.Enums.Common.PacketType.IncomingClientPacket) ? "[C->P]" : "[S->P]")}][{packet.Opcode:X4}]{Environment.NewLine}{Utility.HexDump(packet.GetBytes())}{Environment.NewLine}";

                _logger.Information(logMessage);

                logMessage = $"[{ClientIp}] {Character.AccountName ?? ""} " + logMessage;
                Definitions.List.AgentLogger.Information(logMessage);
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        public void Dispose()
        {
            try
            {
                _clientSocket.Disconnect(false);
                _serverSocket.Disconnect(false);

                _clientSocket.Dispose();
                _serverSocket.Dispose();
                _logger.Dispose();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                //Debug.WriteLine(ex);
            }
        }
    }
}