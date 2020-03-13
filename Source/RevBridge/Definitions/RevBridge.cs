using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;

namespace RevBridge.Definitions
{
    public static class RevBridge
    {
        public static readonly IScheduler Scheduler = new StdSchedulerFactory(new NameValueCollection { { "quartz.serializer.type", "binary" } }).GetScheduler().Result;

        public static class Gateway
        {
            public static class Logger
            {
                public static readonly string FileFormat = $"Logs/Gateway/{DateTime.Now.ToShortDateString()}/{DateTime.Now:HH-mm-ss}.log";
                public static readonly string ConnectionFileFormat = "Logs/Gateway/Connections/{1}/{0}/{2}.log";

                public const int BufferSize = 1024000;
                public const int ConnectionBufferSize = 4096;

                public const long FileSizeLimit = 100000000;
            }
        }

        public static class Agent
        {
            public static class Logger
            {
                public static readonly string FileFormat = $"Logs/Agent/{DateTime.Now.ToShortDateString()}/{DateTime.Now:HH-mm-ss}.log";
                public static readonly string ConnectionFileFormat = "Logs/Agent/Connections/{1}/{0}/{2}.log";

                public const int BufferSize = 1024000;
                public const int ConnectionBufferSize = 4096;

                public const long FileSizeLimit = 100000000;
            }
        }

        public static class Logger
        {
            public static readonly string FileFormat = $"Logs/RevBridge/{DateTime.Now.ToShortDateString()}/{DateTime.Now:HH-mm-ss}.log";

            public const int BufferSize = 1024000;

            public const long FileSizeLimit = 100000000;
        }
    }
}