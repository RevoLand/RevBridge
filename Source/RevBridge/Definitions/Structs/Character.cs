namespace RevBridge.Definitions.Structs
{
    internal sealed class Character
    {
        internal struct Stats
        {
            public int Model, SkillPoints, HP, MP;
            public byte InvSlots, Height, CurrentLevel, MaxLevel, HwanLevel;
            public ulong XP, Gold;
            public short AttrPoints;
            public float WalkSpeed, RunSpeed, HwanSpeed;

            public PKInfo PKInfo;
            public PVP PVPInfo;
        }

        internal struct PKInfo
        {
            public int MurderLevel;
            public short PKLevel;
            public byte DailyPK;
        }

        internal struct PVP
        {
            /*
                PvpState:   0 = White,      1 = Purple,     2 = Red
                PvpFlag:    0 = Red Side,   1 = Blue Side,  0xFF = None
             */
            public byte FreePvp, PvpState, PvpFlag;
        }

        internal struct Position
        {
            public short RegionID, Angle;
            public float X, Y, Z;
        }

        internal struct Job
        {
            public string Name;
            public byte Type, Level, TransportFlag;
            public int Exp, Contribution, Reward, TransportID;
        }

        internal struct ActiveBuff
        {
            public DataModel.RefSkill Skill;
            public int Duration;
        }

        internal struct InventoryItem
        {
            public DataModel.RefObjCommon RefObjCommon;
            public byte Slot;
        }

        internal struct Mastery
        {
            public int MasteryID;
            public byte MasteryLevel;
        }

        internal struct Skill
        {
            public int SkillID;
            public byte Enabled;
        }
    }
}