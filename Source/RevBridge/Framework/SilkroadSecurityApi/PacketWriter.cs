using System.IO;

namespace RevBridge.Framework.SilkroadSecurityApi
{
    internal class PacketWriter : BinaryWriter
    {
        readonly MemoryStream m_ms;

        public PacketWriter()
        {
            m_ms = new MemoryStream();
            OutStream = m_ms;
        }

        public byte[] GetBytes()
        {
            return m_ms.ToArray();
        }
    }
}
