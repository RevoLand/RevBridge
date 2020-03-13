namespace RevBridge.Definitions.Enums.Character
{
    public enum LifeState
    {
        Alive = 1,
        Dead = 2
    }

    public enum Status
    {
        None = 0,
        Hwan = 1,
        Untouchable = 2,
        GMInvincible = 3,
        GMInvisible = 4,
        UNK = 5,
        Stealth = 6,
        Invisible = 7
    }

    public enum ChatType : byte
    {
        ///<summary>
        /// General chat visible to nearby entities
        ///</summary>
        All = 1,

        ///<summary>
        ///$
        ///</summary>
        PM = 2,

        ///<summary>
        /// General (GM) chat visible to nearby entities
        ///</summary>
        AllGM = 3,

        ///<summary>
        ///#
        ///</summary>
        Party = 4,

        ///<summary>
        /// @
        ///</summary>
        Guild = 5,

        ///<summary>
        /// Global chatting item
        ///</summary>
        Global = 6,

        ///<summary>
        /// ~
        ///</summary>
        Notice = 7,

        ///<summary>
        /// While in stall window
        ///</summary>
        Stall = 9,

        ///<summary>
        /// %
        ///</summary>
        Union = 11,

        ///<summary>
        /// SN_TALK_QNO_EU_EASTEU_21_14
        ///</summary>
        NPC = 13,

        ///<summary>
        /// &
        ///</summary>
        Accademy = 16,
    }
}