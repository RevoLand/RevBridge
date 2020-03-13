using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace RevBridge.Bridges
{
    public sealed class RevBridge : IDisposable
    {
        private Socket _listenerSocket;
        private Definitions.Enums.Common.Bridge _type;

        private readonly ManualResetEvent _bridgeMre = new ManualResetEvent(false);

        public void Open(string bridgeIp, int bridgePort, Definitions.Enums.Common.Bridge BridgeType)
        {
            try
            {
                _type = BridgeType;

                if (Functions.RevBridge.GetPortStatus(bridgePort))
                {
                    throw new Exception($"Port is in use! Port: {bridgePort} - Available Port: {Functions.RevBridge.GetFreeport()}");
                }

                if (string.IsNullOrEmpty(bridgeIp))
                    throw new Exception($"Invalid IP set for RevBridge. IP Set: {bridgeIp}");

                var localEndPoint = new IPEndPoint(IPAddress.Parse(bridgeIp), bridgePort);

                _listenerSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                _listenerSocket.Bind(localEndPoint);
                _listenerSocket.Listen(5000);

                var bridgeHandler = new Thread(HandleBridge);
                bridgeHandler.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void HandleBridge()
        {
            try
            {
                while (true)
                {
                    _bridgeMre.Reset();

                    _listenerSocket?.BeginAccept(BridgeAcceptCallback, _listenerSocket);

                    _bridgeMre.WaitOne();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void BridgeAcceptCallback(IAsyncResult ar)
        {
            try
            {
                _bridgeMre.Set();

                var clientSocket = _listenerSocket?.EndAccept(ar);

                if (_type == (byte)Definitions.Enums.Common.Bridge.GatewayServer)
                {
                    Definitions.List.GatewayConnections.Add(new Gateway(ref clientSocket));
                }
                else
                {
                    Definitions.List.AgentConnections.Add(new Agent(ref clientSocket));
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Close()
        {
            try
            {
                if (_type == (byte)Definitions.Enums.Common.Bridge.GatewayServer)
                {
                    Parallel.ForEach(Definitions.List.GatewayConnections, x => x.Disconnect());
                }
                else
                {
                    Parallel.ForEach(Definitions.List.AgentConnections, x =>
                    {
                        x.Character.SendPacketToClient(Functions.PacketCreators.Chat.Notice("Filter kapatılıyor..."));
                        x.Disconnect();
                    });
                }

                _listenerSocket?.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            _listenerSocket?.Dispose();
            _bridgeMre?.Dispose();
        }
    }
}