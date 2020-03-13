using RevBridge.Framework.SilkroadSecurityApi;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace RevBridge.Definitions
{
    class TestServer
    {
        Connection currentCon;

        Socket clientSocket, serverSocket;
        List.Bridge currentBridge;
        object m_Lock = new object();

        // New buffers
        byte[] clientLocalBuffers = new byte[8192];
        byte[] clientRemoteBuffers = new byte[8192];

        Security clientLocalSecurity = new Security();
        Security clientRemoteSecurity = new Security();

        Thread m_TransferPoolThread = null;

        // User IP
        string ip = string.Empty;


        public TestServer(Connection clientCon, Socket ClientSocket)
        {
            currentCon = clientCon;
            clientSocket = ClientSocket;
            currentBridge = List.Bridge.GatewayServer;

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Register ip
            ip = ((IPEndPoint)(clientSocket.RemoteEndPoint)).Address.ToString();

            try
            {
                /*
                if (FilterMain.PIONEER)
                {
                    if (this.connect)
                    {
                        this.m_ModuleSocket.Connect(new IPEndPoint(IPAddress.Parse(FilterMain.REMOTE_GATEWAY), FilterMain.GATEWAY_LISTEN_PORT));
                    }
                }
                else
                {
                    if (this.connect)
                    {
                        this.m_ModuleSocket.Connect(new IPEndPoint(IPAddress.Parse(FilterMain.GATEWAY_IP), FilterMain.GATEWAY_LISTEN_PORT));
                    }
                }
                */

                clientLocalSecurity.GenerateSecurity(true, true, true);
                DoRecvFromClient();
                Send(false);

            }
            catch { }
        }

        void DisconnectModuleSocket()
        {
            try
            {
                if (serverSocket != null)
                {
                    // DISCONNECT
                    serverSocket.Close();
                }

                // NULL
                serverSocket = null;

            }
            catch { }
        }

        void OnReceive_FromServer(IAsyncResult iar)
        {
            lock (m_Lock)
            {
                try
                {
                    int nReceived = serverSocket.EndReceive(iar);

                    if (nReceived != 0)
                    {
                        clientRemoteSecurity.Recv(clientRemoteBuffers, 0, nReceived);

                        List<Packet> RemotePackets = clientRemoteSecurity.TransferIncoming();

                        if (RemotePackets != null)
                        {
                            foreach (Packet _pck in RemotePackets)
                            {
                                #region Handshake
                                // Handshake
                                if (_pck.Opcode == 0x5000 || _pck.Opcode == 0x9000)
                                {
                                    Send(true);
                                    continue;
                                }
                                #endregion

                                // Send packet
                                clientLocalSecurity.Send(_pck);
                                Send(false);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            m_TransferPoolThread.Abort();
                        }
                        catch { }
                        DisconnectModuleSocket();
                        return;
                    }
                    DoRecvFromServer();
                }
                catch (Exception AnyEx)
                {
                    try
                    {
                        m_TransferPoolThread.Abort();
                    }
                    catch { }
                    // Disconnect
                    DisconnectModuleSocket();
                }
            }
        }


        public void Send(bool ToHost)//that codes done by Excellency he fixed mbot for me
        {
            
                foreach (var p in (ToHost ? clientRemoteSecurity : clientLocalSecurity).TransferOutgoing())
                {

                    Socket ss = (ToHost ? serverSocket : clientSocket);

                    ss.Send(p.Key.Buffer);
                }
            
        }

        void OnReceive_FromClient(IAsyncResult iar)
        {
            lock (m_Lock)
            {
                try
                {
                    int nReceived = clientSocket.EndReceive(iar);

                    if (nReceived != 0)
                    {

                        clientLocalSecurity.Recv(clientLocalBuffers, 0, nReceived);

                        List<Packet> receivedPackets = clientLocalSecurity.TransferIncoming();

                        if (receivedPackets != null)
                        {
                            foreach (Packet currentPacket in receivedPackets)
                            {
                                logPackets(currentPacket, List.PacketType.IncomingClientPackets);

                                switch (currentBridge)
                                {
                                    case List.Bridge.GatewayServer:
                                        switch (currentPacket.Opcode)
                                        {
                                            case 0x5000:
                                            case 0x9000:
                                            case 0x2001:
                                                if (!serverSocket.Connected)
                                                {
                                                    serverSocket.Connect(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.RevBridge_Gateway_IP), Properties.Settings.Default.RevBridge_Gateway_RealPort));

                                                    DoRecvFromServer();
                                                }
                                                continue;

                                            case 0x9001: // Hardware ID
                                                currentCon.Character.HardwareID = currentPacket.ReadAscii();
                                                break;
                                        }
                                        break; // GatewayServer
                                    case List.Bridge.AgentServer:
                                        switch (currentPacket.Opcode)
                                        {
                                            case 0x5000:
                                            case 0x9000:
                                            case 0x2001:
                                                if (!serverSocket.Connected)
                                                {
                                                    serverSocket.Connect(new IPEndPoint(IPAddress.Parse(Properties.Settings.Default.RevBridge_Agent_IP), Properties.Settings.Default.RevBridge_Agent_RealPort));

                                                    DoRecvFromServer();
                                                }
                                                continue;
                                        }
                                        break;
                                } // switch ends here

                                clientRemoteSecurity.Send(currentPacket);
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            // Abort connection
                            m_TransferPoolThread.Abort();
                        }
                        catch { }

                        // Disconnect
                        DisconnectModuleSocket();
                        return;
                    }


                    DoRecvFromClient();
                }
                catch
                {
                    try
                    {
                        // Abort connection
                        m_TransferPoolThread.Abort();
                    }
                    catch { }

                    // Disconnect
                    DisconnectModuleSocket();
                }
            }
        }

        void DoRecvFromServer()
        {
            try
            {
                serverSocket.BeginReceive(clientRemoteBuffers, 0, clientRemoteBuffers.Length,
                SocketFlags.None,
                new AsyncCallback(OnReceive_FromServer), null);
            }
            catch
            {
                try
                {
                    m_TransferPoolThread.Abort();
                }
                catch { }

                // Disconnect
                DisconnectModuleSocket();
            }
        }

        void DoRecvFromClient()
        {
            try
            {
                clientSocket.BeginReceive(clientLocalBuffers, 0, clientLocalBuffers.Length,
                SocketFlags.None,
                new AsyncCallback(OnReceive_FromClient), null);

            }
            catch
            {
                try
                {
                    m_TransferPoolThread.Abort();
                }
                catch { }
            }
        }

        public void logPackets(Packet packet, List.PacketType packetType)
        {
            try
            {
                if (!Properties.Settings.Default.RevBridge_Debugging) return;

                /*
                Logs newLog = new Logs();

                newLog.clientSocket = clientSocket;
                newLog.HardwareID = currentCharacter.HardwareID;
                newLog.Packet = packet;
                newLog.Date = DateTime.Now;
                */

                switch (currentBridge)
                {
                    case List.Bridge.GatewayServer:
                        switch (packetType)
                        {
                            case List.PacketType.IncomingClientPackets:
                                Debug.WriteLine(string.Format("[C->P][GS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                            case List.PacketType.IncomingServerPackets:
                                Debug.WriteLine(string.Format("[S->P][GS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                            case List.PacketType.OutgoingClientPackets:
                                Debug.WriteLine(string.Format("[P->C][GS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                            case List.PacketType.OutgoingServerPackets:
                                Debug.WriteLine(string.Format("[P->S][GS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                        }
                        break;
                    case List.Bridge.AgentServer:
                        switch (packetType)
                        {
                            case List.PacketType.IncomingClientPackets:
                                Debug.WriteLine(string.Format("[C->P][AS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                            case List.PacketType.IncomingServerPackets:
                                Debug.WriteLine(string.Format("[S->P][AS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                            case List.PacketType.OutgoingClientPackets:
                                Debug.WriteLine(string.Format("[P->C][AS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                            case List.PacketType.OutgoingServerPackets:
                                Debug.WriteLine(string.Format("[P->S][AS][{0:X4}][{1} bytes]{2}{3}{4}{5}{6}", packet.Opcode, packet.GetBytes().Length, packet.Encrypted ? "[Encrypted]" : "", packet.Massive ? "[Massive]" : "", Environment.NewLine, Utility.HexDump(packet.GetBytes()), Environment.NewLine));

                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

    }
}
