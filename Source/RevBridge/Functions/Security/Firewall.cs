using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using WindowsFirewallHelper;

namespace RevBridge.Functions.Security
{
    internal sealed class Firewall
    {
        private const string RuleName = "RevBridge_Block_";

        public static void BlockIP(IPAddress IP)
        {
            try
            {
                IRule currentRule = GetRule(IP);

                if (currentRule == null)
                {
                    var blockRule = new WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRuleWin8(RuleName + IP.ToString(), Process.GetCurrentProcess().MainModule.FileName, FirewallAction.Block, FirewallDirection.Inbound, FirewallProfiles.Public)
                    {
                        Protocol = FirewallProtocol.TCP
                    };

                    blockRule.UnderlyingObject.RemoteAddresses = IP.ToString();

                    WindowsFirewallHelper.FirewallAPIv2.Firewall.Instance.Rules.Add(blockRule);

                    Debug.WriteLine("Firewall: IP engellendi. IP: " + IP);
                }
                else
                {
                    Debug.WriteLine("Firewall: IP zaten engellenmiş! IP: " + IP);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static void BlockIPRange(List<IPAddress> addresses, string Rulename)
        {
            try
            {
                List<IAddress> IpList = new List<IAddress>();
                IRule currentRule = GetRule(Rulename);

                addresses.ForEach(x => IpList.Add(WindowsFirewallHelper.Addresses.SingleIP.FromIPAddress(x)));

                if (currentRule == null)
                {
                    var blockRule = new WindowsFirewallHelper.FirewallAPIv2.Rules.StandardRuleWin8(Rulename, Process.GetCurrentProcess().MainModule.FileName, FirewallAction.Block, FirewallDirection.Inbound, FirewallProfiles.Public)
                    {
                        Protocol = FirewallProtocol.TCP
                    };

                    blockRule.RemoteAddresses = IpList.ToArray();

                    WindowsFirewallHelper.FirewallAPIv2.Firewall.Instance.Rules.Add(blockRule);

                    Debug.WriteLine("Firewall: Kural oluşturuldu. Kural ismi: " + Rulename);
                }
                else
                {
                    currentRule.RemoteAddresses = IpList.ToArray();

                    Debug.WriteLine("Firewall: Varolan kural düzenlendi ve IP Listesi güncellendi. Kural ismi: " + Rulename);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        public static void UnblockIP(IPAddress IP)
        {
            IRule rule = GetRule(IP);

            if (rule != null)
            {
                WindowsFirewallHelper.FirewallAPIv2.Firewall.Instance.Rules.Remove(rule);
                Debug.WriteLine("Firewall: IP engeli kaldırıldı. IP: " + IP);
            }
        }

        private static IRule GetRule(IPAddress IP)
        {
            return WindowsFirewallHelper.FirewallAPIv2.Firewall.Instance.Rules.SingleOrDefault(r => r.Name == RuleName + IP.ToString());
        }

        private static IRule GetRule(IPAddress IP, string Rulename)
        {
            return WindowsFirewallHelper.FirewallAPIv2.Firewall.Instance.Rules.SingleOrDefault(r => r.Name == Rulename + IP.ToString());
        }

        private static IRule GetRule(string Rulename)
        {
            return WindowsFirewallHelper.FirewallAPIv2.Firewall.Instance.Rules.SingleOrDefault(r => r.Name == Rulename);
        }
    }
}