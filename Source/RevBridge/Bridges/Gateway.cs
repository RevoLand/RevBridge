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
    internal sealed class Gateway : IDisposable
    {
        public readonly Serilog.Core.Logger Logger;

        #region Network Stuff

        private readonly Socket ClientSocket;
        private readonly Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private readonly IPAddress ClientIP;

        public Security Security_ProxyToClient;
        public Security Security_ProxyToServer = new Security();
        private readonly byte[] ClientLocalBuffers = new byte[4096];
        private readonly byte[] ClientRemoteBuffers = new byte[4096];

        #endregion Network Stuff

        private bool _disconnectCalled;

        public Gateway(ref Socket clientSocket)
        {
            try
            {
                ClientSocket = clientSocket;

                ClientIP = ((IPEndPoint)clientSocket.RemoteEndPoint).Address;

                if (!Properties.Settings.Default.Security_ProxyList_FirewallBlock && Definitions.List.Security.ProxyIps.ContainsKey(ClientIP.ToString()))
                {
                    Disconnect();
                    return;
                }

                Security_ProxyToClient = new Security();
                Security_ProxyToClient.ChangeIdentity("SR_Client", 0);
                Security_ProxyToClient.GenerateSecurity(true, true, true);

                ServerSocket.ReceiveTimeout = 10;
                ServerSocket.SendTimeout = 10;

                Logger = new LoggerConfiguration()
                    .WriteTo.Async(a =>
                    {
                        a.File(string.Format(Definitions.RevBridge.Gateway.Logger.ConnectionFileFormat,
                            ClientIP,
                            DateTime.Now.ToShortDateString(),
                            DateTime.Now.ToString("HH-mm-ss"))
                            , rollOnFileSizeLimit: true, fileSizeLimitBytes: Definitions.RevBridge.Gateway.Logger.FileSizeLimit);
                    }, bufferSize: Definitions.RevBridge.Gateway.Logger.ConnectionBufferSize)
                    .CreateLogger();

                DoRecvFromClient();
                Task.Run(HandleOutgoingClientPackets_SendPacketsToServerAsync);
                Task.Run(HandleOutgoingServerPacketsAsync);
                ConnectToServer();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void ConnectToServer()
        {
            try
            {
                ServerSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.RevBridge_Gateway_IP), Properties.Settings.Default.RevBridge_Gateway_RealPort), ConnectToServerCallback, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void ConnectToServerCallback(IAsyncResult ar)
        {
            try
            {
                ServerSocket.EndConnect(ar);

                DoRecvFromServer();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void DoRecvFromClient()
        {
            try
            {
                ClientSocket.BeginReceive(ClientLocalBuffers, 0, ClientLocalBuffers.Length, SocketFlags.None, OnReceiveFromClient, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Disconnect();
            }
        }

        private void OnReceiveFromClient(IAsyncResult ar)
        {
            try
            {
                var bytesRead = ClientSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    Security_ProxyToClient.Recv(ClientLocalBuffers, 0, bytesRead);

                    HandleIncomingClientPackets();
                }
                else
                {
                    Disconnect();
                    return;
                }

                DoRecvFromClient();
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        private void DoRecvFromServer()
        {
            try
            {
                ServerSocket.BeginReceive(ClientRemoteBuffers, 0, ClientRemoteBuffers.Length, SocketFlags.None, OnReceiveFromServer, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                Disconnect();
            }
        }

        private void OnReceiveFromServer(IAsyncResult ar)
        {
            try
            {
                var bytesRead = ServerSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    Security_ProxyToServer.Recv(ClientRemoteBuffers, 0, bytesRead);
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
                Debug.WriteLine(ex);

                Disconnect();
            }
        }

        private void HandleIncomingClientPackets()
        {
            try
            {
                var receivedPackets = Security_ProxyToClient.TransferIncoming();
                if (receivedPackets != null)
                {
                    for (var i = 0; i < receivedPackets.Count; i++)
                    {
                        var currentPacket = receivedPackets[i];
                        LogPackets(currentPacket, Definitions.Enums.Common.PacketType.IncomingClientPacket);

                        switch (currentPacket.Opcode)
                        {
                            case Opcodes.Global.Client.MODULE_IDENTIFICATION:
                            case Opcodes.Gateway.Client.LOGIN_IBUV:
                                continue;
                            case Opcodes.Gateway.Client.LOGIN:
                                var locale = currentPacket.ReadByte();
                                var username = currentPacket.ReadAscii();
                                var password = currentPacket.ReadAscii();
                                var shardId = currentPacket.ReadShort();

                                var newPacket = new Packet(0x6102, true);
                                newPacket.WriteByte(locale);
                                newPacket.WriteAscii(username);
                                newPacket.WriteAscii(password);
                                newPacket.WriteShort(3);
                                Security_ProxyToServer.Send(newPacket);
                                continue;
                        }

                        Security_ProxyToServer.Send(currentPacket);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void HandleIncomingServerPackets()
        {
            try
            {
                var receivedPackets = Security_ProxyToServer.TransferIncoming();
                if (receivedPackets != null)
                {
                    for (var i = 0; i < receivedPackets.Count; i++)
                    {
                        var currentPacket = receivedPackets[i];
                        LogPackets(currentPacket, Definitions.Enums.Common.PacketType.IncomingServerPacket);
                        switch (currentPacket.Opcode)
                        {
                            case Opcodes.Gateway.Client.LOGIN_IBUV_CHALLENGE:
                                if (!string.IsNullOrEmpty(Properties.Settings.Default.RevBridge_Captcha_AutoCaptcha))
                                {
                                    Security_ProxyToServer.Send(Functions.PacketCreators.Captcha.CreateResponse(Properties.Settings.Default.RevBridge_Captcha_AutoCaptcha));
                                    continue;
                                }
                                break;

                            case Opcodes.Gateway.Server.LOGIN:
                                var res = currentPacket.ReadByte();
                                if (res == 1)
                                {
                                    var id = currentPacket.ReadInt();

                                    var newPacket = new Packet(Opcodes.Gateway.Server.LOGIN, true);
                                    newPacket.WriteByte(1);
                                    newPacket.WriteUInt32(id);

                                    newPacket.WriteAscii(Properties.Settings.Default.RevBridge_ListenIP);
                                    newPacket.WriteUInt16(Properties.Settings.Default.RevBridge_Agent_ListenPort);
                                    newPacket.WriteInt(0);

                                    Security_ProxyToClient.Send(newPacket);
                                    continue;
                                }
                                break;
                        }

                        Security_ProxyToClient.Send(currentPacket);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private async Task HandleOutgoingClientPackets_SendPacketsToServerAsync()
        {
            try
            {
                while (!Security_ProxyToClient.HasPacketToSend())
                {
                    await Task.Delay(250).ConfigureAwait(false);
                }

                var outgoingPackets = Security_ProxyToClient.TransferOutgoing();

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
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private async Task HandleOutgoingServerPacketsAsync()
        {
            try
            {
                while (!Security_ProxyToServer.HasPacketToSend())
                {
                    await Task.Delay(250).ConfigureAwait(false);
                }

                var outgoingPackets = Security_ProxyToServer.TransferOutgoing();

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
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void SendToServerAsClient(byte[] buffer)
        {
            try
            {
                ClientSocket.BeginSend(buffer, 0, buffer.Length, 0, SendTClient, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                Disconnect();
            }
        }

        private void SendTClient(IAsyncResult ar)
        {
            try
            {
                ClientSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());

                Disconnect();
            }
        }

        private void SendToClientAsServer(byte[] buffer)
        {
            try
            {
                ServerSocket.BeginSend(buffer, 0, buffer.Length, 0, SendTServer, null);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                Disconnect();
            }
        }

        private void SendTServer(IAsyncResult ar)
        {
            try
            {
                ServerSocket.EndSend(ar);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception)
            {
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

                Debug.WriteLine($"[Disconnect] {ClientIP}");

                if (Definitions.List.GatewayConnections.Contains(this))
                    Definitions.List.GatewayConnections.Remove(this);

                Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show(ex.ToString());
            }
        }

        public void LogPackets(Packet packet, Definitions.Enums.Common.PacketType packetType)
        {
            try
            {
                if (Logger == null || !Properties.Settings.Default.RevBridge_Debugging)
                    return;

                var logMessage = $"[{((packetType == (byte)Definitions.Enums.Common.PacketType.IncomingClientPacket) ? "[C->P]" : "[S->P]")}][{packet.Opcode:X4}]{Environment.NewLine}{Utility.HexDump(packet.GetBytes())}{Environment.NewLine}";

                Logger.Information(logMessage);

                //logMessage = $"[{ClientIP}] " + logMessage;
                //Definitions.List.GatewayLogger.Information(logMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void Dispose()
        {
            try
            {
                ClientSocket.Disconnect(false);
                ServerSocket.Disconnect(false);

                ClientSocket.Dispose();
                ServerSocket.Dispose();
                Logger.Dispose();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
            }
        }
    }
}