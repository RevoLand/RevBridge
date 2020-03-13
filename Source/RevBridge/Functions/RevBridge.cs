using Quartz;
using RevBridge.Framework.SilkroadSecurityApi;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RevBridge.Functions
{
    internal sealed class RevBridge
    {
        public sealed class Packets
        {
            // IN GAME KONTROL GEREKLİ
            public static void SendPacketToPlayerAsServer(Packet packetToSend)
            {
                try
                {
                    Parallel.ForEach(Definitions.List.AgentConnections.ToList(), currentConnection => currentConnection.SecurityProxyToClient?.Send(packetToSend));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

            public static void SendPacketToServerAsPlayer(Packet packetToSend)
            {
                try
                {
                    Parallel.ForEach(Definitions.List.AgentConnections.ToList(), currentConnection => currentConnection.SecurityProxyToServer?.Send(packetToSend));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        public static async Task OnBridgeStartupAsync()
        {
            try
            {
                if (Properties.Settings.Default.Security_GMList == null)
                {
                    Properties.Settings.Default.Security_GMList = new StringCollection();
                }

                await TableData.LoadAsync();
                Definitions.List.ChatCommands = new Framework.Commands.Collection();

                await DefineScheduler().ConfigureAwait(false);

                //Timers.RandomWeather._Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static async Task DefineScheduler()
        {
            try
            {
                await Definitions.RevBridge.Scheduler.Start().ConfigureAwait(false);

                var job = JobBuilder.Create<Security.Proxy>()
                    .WithIdentity("ProxyJob", "Security")
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("ProxyTrigger", "Security")
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(6)
                        .RepeatForever())
                    .Build();

                await Definitions.RevBridge.Scheduler.ScheduleJob(job, trigger).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static void OnBridgeClosing()
        {
            try
            {
                if (Forms.MainWindow.GatewayBridge != null)
                {
                    Forms.MainWindow.GatewayBridge.Close();
                    Forms.MainWindow.GatewayBridge = null;
                }

                if (Forms.MainWindow.AgentBridge != null)
                {
                    Forms.MainWindow.AgentBridge.Close();
                    Forms.MainWindow.AgentBridge = null;
                }

                Definitions.RevBridge.Scheduler.Shutdown();
                Application.Current.Shutdown();

                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public static int GetFreeport()
        {
            try
            {
                var usedPorts = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(p => p.Port).ToList();
                var FreePort = 0;

                for (var port = 10000; port < 20000; port++)
                {
                    if (!usedPorts.Contains(port))
                    {
                        FreePort = port;
                        break;
                    }
                }

                return FreePort;
            }
            catch
            {
                return 19000;
            }
        }

        public static bool GetPortStatus(int Port)
        {
            try
            {
                var properties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
                var tcpEndPoints = properties.GetActiveTcpListeners();

                var usedPorts = tcpEndPoints.Select(p => p.Port).ToList();

                return usedPorts.Contains(Port);
            }
            catch
            {
                return true;
            }
        }
    }
}