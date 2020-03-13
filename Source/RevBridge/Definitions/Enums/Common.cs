namespace RevBridge.Definitions.Enums.Common
{
    public enum Bridge
    {
        GatewayServer,
        AgentServer
    }

    public enum PacketType
    {
        IncomingClientPacket,
        IncomingServerPacket,
        OutgoingClientPacket,
        OutgoingServerPacket
    }

    public enum Service
    {
        Passive = 0,
        Active = 1
    }

    public enum Country
    {
        Chinese = 0,
        Europe = 1,
        Islam = 2,
        Notset = 3
    }

    public enum ReqGender
    {
        Female = 0,
        Male = 1,
        DoesntMatter = 2,
        Pet2_3 = 3,
        Pet2_4 = 4,
        Pet2_5 = 5
    }

    public enum ItemClass
    {
        Degree1_A = 1,
        Degree1_B = 2,
        Degree1_C = 3,

        Degree2_A = 4,
        Degree2_B = 5,
        Degree2_C = 6,

        Degree3_A = 7,
        Degree3_B = 8,
        Degree3_C = 9,

        Degree4_A = 10,
        Degree4_B = 11,
        Degree4_C = 12,

        Degree5_A = 13,
        Degree5_B = 14,
        Degree5_C = 15,

        Degree6_A = 16,
        Degree6_B = 17,
        Degree6_C = 18,

        Degree7_A = 19,
        Degree7_B = 20,
        Degree7_C = 21,

        Degree8_A = 22,
        Degree8_B = 23,
        Degree8_C = 24,

        Degree9_A = 25,
        Degree9_B = 26,
        Degree9_C = 27,

        Degree10_A = 28,
        Degree10_B = 29,
        Degree10_C = 30,

        Degree11_A = 31,
        Degree11_B = 32,
        Degree11_C = 33,

        Degree12_A = 34,
        Degree12_B = 35,
        Degree12_C = 36,

        Degree13_A = 37,
        Degree13_B = 38,
        Degree13_C = 39,

        Degree14_A = 40,
        Degree14_B = 41,
        Degree14_C = 42,

        Degree15_A = 43,
        Degree15_B = 44,
        Degree15_C = 45,

        Degree16_A = 46,
        Degree16_B = 47,
        Degree16_C = 48,

        Degree17_A = 49,
        Degree17_B = 50,
        Degree17_C = 51,

        Degree18_A = 52,
        Degree18_B = 53,
        Degree18_C = 54,

        Degree19_A = 55,
        Degree19_B = 56,
        Degree19_C = 57,

        Degree20_A = 58,
        Degree20_B = 59,
        Degree20_C = 60,
    }

    public enum PaymentDevice
    {
        Gold = 1,
        Silk = 2,
        Gift_Silk = 4,
        Guild_Point = 8,
        Point_Silk = 16,
        Honor_Point = 32,
        Copper_Coin = 64,
        Iron_Coin = 128,
        Silver_Coin = 256,
        Gold_Coin = 512,
        Arena_Coin = 1024
    }
}