namespace RevBridge.Definitions
{
    internal sealed class Directories
    {
        public sealed class RevBridge
        {
            public static string Base = System.AppDomain.CurrentDomain.BaseDirectory;
            public static string Logs = System.IO.Path.Combine(Base, "Logs");
        }
    }
}
