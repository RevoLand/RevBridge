using System.Collections.Generic;

namespace RevBridge.Definitions
{
    internal sealed class Character
    {
        public Bridges.Agent Agent;
        public bool IsFirstSpawn = true, IsGm;

        public string AccountName, TempAccountName, Name;

        public int JID, UniqueId;
        public byte InCombat;

        public Structs.Character.Stats Stats;
        public Structs.Character.Position Position;
        public Structs.Character.Job Job;
        public List<Structs.Character.InventoryItem> Inventory = new List<Structs.Character.InventoryItem>();
        public List<Structs.Character.InventoryItem> AvatarInventory = new List<Structs.Character.InventoryItem>();
        public List<Structs.Character.Mastery> Masteries = new List<Structs.Character.Mastery>();
        public List<Structs.Character.Skill> Skills = new List<Structs.Character.Skill>();
        public List<Structs.Character.ActiveBuff> ActiveBuffs = new List<Structs.Character.ActiveBuff>();

        public void SendNotice(string message)
        {
            SendPacketToClient(Functions.PacketCreators.Chat.Notice(message));
        }

        public void SendPacketToClient(Framework.SilkroadSecurityApi.Packet packet)
        {
            Agent.SecurityProxyToClient.Send(packet);
        }
    }
}