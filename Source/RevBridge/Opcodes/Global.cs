namespace RevBridge.Opcodes
{
    public class Global
    {
        public sealed class Client
        {
            public const ushort
            HANDSHAKE_RESPONSE = 0x5000,
            HANDSHAKE_ACCEPT = 0x9000,
            MODULE_IDENTIFICATION = 0x2001,
            MODULE_KEEP_ALIVE = 0x2002,
            MODULE_CERTIFICATION_REQUEST = 0x6003,
            MODULE_CERTIFICATION_RESPONSE = 0xA003,
            MODULE_RELAY_REQUEST = 0x6008,
            MODULE_RELAY_RESPONSE = 0xA008,
            MASSIVE_MESSAGE = 0x600D;
        }

        public sealed class Server
        {
            public const ushort
            HANDSHAKE_SETUP_CHALLENGE = 0x5000,
            MODULE_IDENTIFICATION = 0x2001,
            NODE_STATUS1 = 0x2005,
            NODE_STATUS2 = 0x6005,
            MASSIVE_MESSAGE = 0x600D;
        }
    }
}