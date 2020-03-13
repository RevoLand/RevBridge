using RevBridge.Framework;
using System.ComponentModel;

namespace RevBridge.Definitions.Enums.Tables.RefObjCommon
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CashItem
    {
        Normal = 0,

        [Description("Item Mall Item")]
        CashItem = 1
    }

    public enum TypeID1
    {
        Mob = 1,
        Item = 3,
        Object = 4
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Rarity
    {
        Normal = 0,
        SoX = 2,

        [Description("Item: Set item | Character: Unique")]
        Egypt = 3,

        Quest = 4,

        [Description("Item: Seal of Nova (Set item) | Character: Elite")]
        SoN_Egypt = 6,

        Elite = 7,
        Unique = 8
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CanTrade
    {
        [Description("Can not trade")]
        TradeEdilemez = 0,

        [Description("Can be trade")]
        TradeEdilebilir = 1
    }

    public enum CanSell
    {
        CanNotBe = 0,
        CanBe = 1
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum CanBuy
    {
        [Description("Can not be")]
        SatınAlınamaz = 0,

        [Description("Can be")]
        SatınAlınabilir = 1
    }

    // Bilinmeyenler?
    public enum CanBorrow
    {
        //Bit 1: EXCHANGE ?
        //Bit 2: STORAGE / GUILD_STORAGE ?
        //Bit 3: PET2 ?
        //Bit 4-8: ???

        Default = 0,      //0 0 0 00000
        Unknown128 = 128, //1 0 0 00000
        Unknown159 = 159, //1 0 0 11111
        Unknown160 = 160, //1 0 1 00000
        Unknown192 = 192, //1 1 0 11111
        Unknown223 = 223, //1 1 0 00000
        Unknown255 = 255  //1 1 1 11111
    }

    // Bilinmeyen?
    public enum CanDrop
    {
        CanNotBe = 0,
        DropWithoutAsking = 1,
        Unknown = 2,
        CanBe = 3
    }

    public enum CanPick
    {
        CanNotBe = 0,
        CanBe = 1
    }

    public enum CanRepair
    {
        CanNotBe = 0,
        CanBe = 1
    }

    public enum CanRevive
    {
        CanNotBe = 0,
        CanBe = 1
    }

    // Bilinmeyen
    public enum CanUse
    {
        CanNotBe = 0,
        CanBe = 1,
        Unknown129 = 129,
        Unknown255 = 255
    }

    public enum CanThrow
    {
        CanNotBe = 0,
        CanBe = 1
    }

    public enum ReqLevelType
    {
        None = -1,
        Character = 1,
        Trader = 2,
        Thief = 3,
        Hunter = 4,
        Pet2 = 5,
        Guild = 10,
        WarriorMastery = 513,
        WizardMastery = 514,
        RogueMastery = 515,
        WarlockMastery = 516,
        BardMastery = 517,
        ClericMastery = 518,
        Unknown1025 = 1025,
        Pet2_2 = 1026
    }
}