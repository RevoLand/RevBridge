using RevBridge.Functions.Security;
using System;
using System.Windows;

namespace RevBridge.Forms
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings
    {
        public Settings()
        {
            InitializeComponent();
        }

        private async void Security_ProxyList_Update_ClickAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                await Proxy.UpdateProxyListAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
            }
        }

        public void UpdateGmList()
        {
            try
            {
                SecurityGmListView.ItemsSource = Properties.Settings.Default.Security_GMList;
                SecurityGmListView.Items.Refresh();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
            }
        }

        private void Security_GMList_AddButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(Security_GMList_AccountName.Text))
                    return;

                if (Properties.Settings.Default.Security_GMList.Contains(Security_GMList_AccountName.Text
                    .ToLowerInvariant()))
                    return;

                Properties.Settings.Default.Security_GMList.Add(Security_GMList_AccountName.Text.ToLowerInvariant());

                Security_GMList_AccountName.Text = "";
                UpdateGmList();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
            }
        }

        private void Security_GMList_RemoveButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SecurityGmListView.SelectedValue == null)
                    return;

                if (!Properties.Settings.Default.Security_GMList.Contains(SecurityGmListView.SelectedValue.ToString()))
                    return;

                Properties.Settings.Default.Security_GMList.Remove(SecurityGmListView.SelectedValue.ToString());

                UpdateGmList();
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
            }
        }
    }
}