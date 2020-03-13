using LinqToDB.Data;
using System;
using System.Diagnostics;
using System.Windows;

namespace RevBridge.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static MainWindow FormAccessor;
        public static Bridges.RevBridge GatewayBridge;
        public static Bridges.RevBridge AgentBridge;

        public MainWindow()
        {
            InitializeComponent();
            FormAccessor = this;

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            DataConnection.DefaultSettings = new Definitions.SqlSettings();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
        }

        private async void Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                await Functions.RevBridge.OnBridgeStartupAsync().ConfigureAwait(false);

                GatewayConnectionCount.DataContext = Definitions.List.GatewayConnections;
                AgentConnectionCount.DataContext = Definitions.List.AgentConnections;

                FormAccessor.Settings.UpdateGmList();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Functions.RevBridge.OnBridgeClosing();
        }

        private void StartBridge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GatewayBridge = new Bridges.RevBridge();
                AgentBridge = new Bridges.RevBridge();

                GatewayBridge.Open(Properties.Settings.Default.RevBridge_ListenIP, Properties.Settings.Default.RevBridge_Gateway_ListenPort, Definitions.Enums.Common.Bridge.GatewayServer);
                AgentBridge.Open(Properties.Settings.Default.RevBridge_ListenIP, Properties.Settings.Default.RevBridge_Agent_ListenPort, Definitions.Enums.Common.Bridge.AgentServer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                StartBridge.IsEnabled = false;
                StopBridge.IsEnabled = true;
            }
        }

        private void StopBridge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GatewayBridge.Close();
                AgentBridge.Close();

                GatewayBridge.Dispose();
                AgentBridge.Dispose();

                GatewayBridge = null;
                AgentBridge = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                StartBridge.IsEnabled = true;
                StopBridge.IsEnabled = false;
            }
        }

        private void Button_BanIP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Functions.Security.Firewall.BlockIP(System.Net.IPAddress.Parse(Ip.Text));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void Button_UnbanIP_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Functions.Security.Firewall.UnblockIP(System.Net.IPAddress.Parse(Ip.Text));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        [MethodTimer.Time]
        private void Button_Test1_Click(object sender, RoutedEventArgs e)
        {
        }

        [MethodTimer.Time]
        private void Button_Test2_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}