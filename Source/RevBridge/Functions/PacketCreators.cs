using RevBridge.Framework.SilkroadSecurityApi;

namespace RevBridge.Functions
{
    internal static class PacketCreators
    {
        public static class Captcha
        {
            public static Packet CreateResponse(string captchaText)
            {
                var newPacket = new Packet(0x6323);
                newPacket.WriteAscii(captchaText);

                return newPacket;
            }
        }

        public static class Chat
        {
            public static Packet Notice(string noticeText)
            {
                var newPacket = new Packet(0x3026);
                newPacket.WriteByte(7); // Notice
                newPacket.WriteAscii(noticeText);
                return newPacket;
            }
        }
    }
}