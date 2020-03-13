using System;

namespace RevBridge
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public readonly string CommandHelpText;
        public readonly string CommandString;

        public bool GreedyArg
        {
            get;
            set;
        }

        public bool SensitiveInfo
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public CommandAttribute()
        {
            CommandString = null;
            CommandHelpText = null;
        }

        public CommandAttribute(string command)
        {
            CommandString = command;
            CommandHelpText = null;
        }

        public CommandAttribute(string command, string helpText)
        {
            CommandString = command;
            CommandHelpText = helpText;
        }
    }
}