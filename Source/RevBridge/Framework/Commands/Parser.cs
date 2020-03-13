using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RevBridge.Framework.Commands
{
    internal class Parser
    {
        public string Command;

        public string Description;

        public bool Greedy;

        public MethodInfo Method;

        public ParameterInfo[] Parameters;

        public Info PublicInfo;

        public bool Sensitive;

        public string Usage;

        public bool Parse(Definitions.Character Client, string Cmd, string[] Args)
        {
            string text;
            if (!string.IsNullOrEmpty(Usage))
            {
                text = Usage;
            }
            else if (Parameters.Length > 1)
            {
                int paramCounter = 0;
                text = "USAGE: /" + Cmd + " [" + Parameters.Skip(1).Select((ParameterInfo param) =>
                {
                    if (!param.IsOptional)
                    {
                        return param.Name;
                    }
                    return param.Name + "?";
                }).Aggregate((string prev, string next) => prev + ((++paramCounter == 0) ? "]" : "") + " [" + next + "]") + ((Parameters.Length == 2) ? "]" : "");
            }
            else
            {
                text = "USAGE: /" + Cmd;
            }
            int num = Parameters.Skip(1).Count((ParameterInfo p) => p.IsOptional);

            if (Args.Length < Parameters.Length - num || (Args.Length > Parameters.Length && !Greedy))
            {
                Client.Agent.SecurityProxyToClient.Send(Functions.PacketCreators.Chat.Notice(text));
                return true;
            }

            object[] array = new object[Parameters.Length];
            array[0] = Client;
            for (int i = 1; i < Parameters.Length; i++)
            {
                if (Args.Length <= i)
                {
                    array[i] = Type.Missing;
                }
                else
                {
                    if (Parameters[i].ParameterType.IsEnum)
                    {
                        object obj2;
                        try
                        {
                            obj2 = Enum.Parse(Parameters[i].ParameterType, Args[i], true);
                        }
                        catch (ArgumentException)
                        {
                            Debug.WriteLine("ArgumentException happened");
                            return true;
                        }
                        array[i] = obj2;
                    }
                    else if (i == Parameters.Length - 1 && Greedy)
                    {
                        array[i] = string.Join(" ", Args.Skip(i));
                    }
                    else
                    {
                        try
                        {
                            array[i] = Convert.ChangeType(Args[i], Parameters[i].ParameterType, CultureInfo.InvariantCulture);
                        }
                        catch (Exception)
                        {
                            Debug.WriteLine("Exception2 happened");
                            Client.Agent.SecurityProxyToClient.Send(Functions.PacketCreators.Chat.Notice(text));
                            return true;
                        }
                    }
                }
            }
            try
            {
                Method.Invoke(null, array);
            }
            catch (Exception exception3)
            {
                Debug.WriteLine("Exception3 happened: " + exception3);
            }
            return true;
        }
    }
}