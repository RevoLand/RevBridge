using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RevBridge.Framework.Commands
{
    internal class Collection
    {
        public Dictionary<string, Parser> ChatCommandsDic = new Dictionary<string, Parser>();

        public Collection()
        {
            try
            {
                List<MethodInfo> list = (from ifo in typeof(Definitions.Commands).GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                         where ifo.CustomAttributes.Any((CustomAttributeData att) => att.AttributeType == typeof(CommandAttribute))
                                         select ifo).ToList();
                if (list.Count > 0)
                {
                    Debug.WriteLine($"Loading commands.. found {list.Count} command(s)");
                }
                foreach (MethodInfo item in list)
                {
                    CommandAttribute customAttribute = ((MemberInfo)item).GetCustomAttribute<CommandAttribute>();
                    ParameterInfo[] parameters = item.GetParameters();
                    Parser commandParser = new Parser
                    {
                        Command = (string.IsNullOrWhiteSpace(customAttribute.CommandString) ? item.Name.ToLower() : customAttribute.CommandString),
                        Greedy = customAttribute.GreedyArg,
                        Parameters = parameters,
                        Method = item,
                        Sensitive = customAttribute.SensitiveInfo,
                        Usage = customAttribute.CommandHelpText,
                        Description = customAttribute.Description
                    };
                    commandParser.PublicInfo = new Info(commandParser.Command, parameters, customAttribute);
                    if (ChatCommandsDic.ContainsKey(commandParser.Command))
                    {
                        Debug.WriteLine($"Command '{commandParser.Command}' already exists!");
                    }
                    else
                    {
                        ChatCommandsDic.Add(commandParser.Command, commandParser);
                    }
                }
                if (ChatCommandsDic.Count > 0)
                {
                    Debug.WriteLine($"Loaded {ChatCommandsDic.Count} commands(s)", System.Drawing.Color.Gray);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}