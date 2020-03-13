using Quartz;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace RevBridge.Functions.Security
{
    internal sealed class Proxy : IJob
    {
        private const string Rulename = "RevBridge_ProxyIps";
        private static readonly List<IPAddress> ProxyIps = new List<IPAddress>();
        private static bool UpdateStatus;

        public async Task Execute(IJobExecutionContext context)
        {
            await UpdateProxyListAsync().ConfigureAwait(false);
        }

        [MethodTimer.Time]
        public static async Task UpdateProxyListAsync()
        {
            try
            {
                if (Properties.Settings.Default.Security_ProxyList_Active)
                {
                    if (!UpdateStatus)
                    {
                        UpdateStatus = true;

                        ProxyIps.Clear();

                        await ParseProxyListAsync("https://raw.githubusercontent.com/a2u/free-proxy-list/master/free-proxy-list.txt").ConfigureAwait(false);
                        await ParseProxyListAsync("https://raw.githubusercontent.com/clarketm/proxy-list/master/proxy-list.txt").ConfigureAwait(false);
                        await ParseProxyListAsync("https://raw.githubusercontent.com/fate0/proxylist/master/proxy.list", true).ConfigureAwait(false);

                        if (Properties.Settings.Default.Security_ProxyList_FirewallBlock)
                        {
                            Firewall.BlockIPRange(ProxyIps, Rulename);

                            Debug.WriteLine($"[Security] Proxy List: Blocked {ProxyIps.Count} IP.");
                        }

                        UpdateStatus = false;
                        Properties.Settings.Default.Security_ProxyList_LastUpdate = DateTime.UtcNow;
                    }
                    else
                    {
                        Definitions.List.ProgramLogger.Error("Security: Proxy List is already checking for updates.");
                    }
                }
                else
                {
                    Definitions.List.ProgramLogger.Information("Security: Proxy List is not active. Skipping update control.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static async Task ParseProxyListAsync(string ProxyUrl, bool ParseAsJson = false)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    var listDownloadResult = await webClient.DownloadStringTaskAsync(ProxyUrl).ConfigureAwait(false);
                    foreach (string line in listDownloadResult.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string ipAddress = "";
                        if (ParseAsJson)
                        {
                            //JObject o = JObject.Parse(line);

                            //if (o["host"] != null)
                            //{
                            //    ipAddress = o["host"].ToString();
                            //}
                        }
                        else
                        {
                            var ip = line.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                            ipAddress = ip[0];
                        }

                        if (IPAddress.TryParse(ipAddress, out IPAddress address))
                        {
                            if (Properties.Settings.Default.Security_ProxyList_FirewallBlock)
                            {
                                if (!ProxyIps.Contains(address))
                                {
                                    ProxyIps.Add(address);
                                }
                            }
                            else
                            {
                                Definitions.List.Security.ProxyIps.Add(address.ToString(), address);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}