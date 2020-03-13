using RevBridge.Framework.SilkroadSecurityApi;
using Serilog;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;

namespace RevBridge.Definitions
{
    internal static class List
    {
        public static readonly Serilog.Core.Logger GatewayLogger = new LoggerConfiguration()
                    .WriteTo.Async(a => a.File(RevBridge.Gateway.Logger.FileFormat, rollOnFileSizeLimit: true, fileSizeLimitBytes: RevBridge.Gateway.Logger.FileSizeLimit), bufferSize: RevBridge.Gateway.Logger.BufferSize)
                    .CreateLogger();

        public static readonly Serilog.Core.Logger AgentLogger = new LoggerConfiguration()
                    .WriteTo.Async(a => a.File(RevBridge.Agent.Logger.FileFormat, rollOnFileSizeLimit: true, fileSizeLimitBytes: RevBridge.Agent.Logger.FileSizeLimit), bufferSize: RevBridge.Agent.Logger.BufferSize)
                    .CreateLogger();

        public static readonly Serilog.Core.Logger ProgramLogger = new LoggerConfiguration()
                    .WriteTo.Async(a => a.File(RevBridge.Logger.FileFormat, rollOnFileSizeLimit: true, fileSizeLimitBytes: RevBridge.Logger.FileSizeLimit), bufferSize: RevBridge.Logger.BufferSize)
                    .CreateLogger();

        public static Framework.Commands.Collection ChatCommands;

        public static readonly ObservableCollection<Bridges.Gateway> GatewayConnections = new ObservableCollection<Bridges.Gateway>();
        public static readonly ObservableCollection<Bridges.Agent> AgentConnections = new ObservableCollection<Bridges.Agent>();
        public static readonly List<Packet> RegisteredPackets = new List<Packet>();

        public static readonly Dictionary<int, DataModel.RefObjCommon> UniqueIds = new Dictionary<int, DataModel.RefObjCommon>();

        public static List<DataModel.RefObjCommon> RefObjCommon = new List<DataModel.RefObjCommon>();
        public static List<DataModel.RefObjItem> RefObjItem = new List<DataModel.RefObjItem>();
        public static List<DataModel.RefObjChar> RefObjChar = new List<DataModel.RefObjChar>();
        public static List<DataModel.RefObjStruct> RefObjStruct = new List<DataModel.RefObjStruct>();
        public static List<DataModel.RefSkill> RefSkill = new List<DataModel.RefSkill>();

        public static Dictionary<int, DataModel.RefObjCommon> RefObjCommonDict = new Dictionary<int, DataModel.RefObjCommon>();
        public static Dictionary<int, DataModel.RefObjChar> RefObjCharDict = new Dictionary<int, DataModel.RefObjChar>();
        public static Dictionary<int, DataModel.RefObjItem> RefObjItemDict = new Dictionary<int, DataModel.RefObjItem>();
        public static Dictionary<int, DataModel.RefObjStruct> RefObjStructDict = new Dictionary<int, DataModel.RefObjStruct>();
        public static Dictionary<int, DataModel.RefSkill> RefSkillDict = new Dictionary<int, DataModel.RefSkill>();

        public static class Security
        {
            public static readonly Dictionary<string, IPAddress> ProxyIps = new Dictionary<string, IPAddress>();
        }

        public static class Progresses
        {
            //public static readonly IProgress<string> SecurityGmList = new Progress<string>(username => Properties.Settings.Default.Security_GMList.Add(username));
        }
    }
}