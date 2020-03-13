using System.Linq;
using System.Reflection;

namespace RevBridge.Framework.Commands
{
    public struct Info
    {
        public string Command;

        public string CustomUsage;

        public string Usage;

        public bool Greedy;

        public ParameterInfo[] Parameters;

        public bool Sensitive;

        public string Description;

        public Info(string Cmd, ParameterInfo[] param, CommandAttribute from)
        {
            Command = Cmd;
            CustomUsage = from.CommandHelpText;
            Parameters = param;
            Greedy = from.GreedyArg;
            Sensitive = from.SensitiveInfo;
            Description = from.Description;
            if (Parameters.Length > 1)
            {
                int paramCounter = 0;
                Usage = " [" + Parameters.Skip(1).Select((ParameterInfo par) =>
                {
                    if (!par.IsOptional)
                    {
                        return par.Name;
                    }
                    return par.Name + "?";
                }).Aggregate((string prev, string next) => prev + ((++paramCounter == 0) ? "]" : "") + " [" + next + "]") + ((Parameters.Length == 2) ? "]" : "");
            }
            else
            {
                Usage = "";
            }
        }
    }
}