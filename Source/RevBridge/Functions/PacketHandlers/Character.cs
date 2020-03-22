using RevBridge.Framework.SilkroadSecurityApi;
using System;
using System.Diagnostics;
using System.Windows;

namespace RevBridge.Functions.PacketHandlers
{
    internal sealed class Character
    {
        //[MethodTimer.Time]
        public static void CastingAndBasicHits(Packet packet, Definitions.Character Character)
        {
            try
            {
                var Type1 = packet.ReadByte();
                if (Type1 != 1)
                {
                    Debug.WriteLine($"Bilinmeyen Type1: {Type1}");
                    return;
                }
                var Type2 = packet.ReadByte();
                if (Type2 != 2)
                {
                    Debug.WriteLine($"Bilinmeyen Type2: {Type2}");
                    return;
                }
                var Unk1 = packet.ReadByte();

                if (Unk1 != 0x30)
                {
                    Debug.WriteLine($"Bilinmeyen Unk1: {Unk1}");
                    return;
                }

                var SkillID = packet.ReadInt();
                var OwnerUniqueID = packet.ReadInt();

                if (OwnerUniqueID != Character.UniqueId)
                {
                    // Biz vurmamışız, geçiniz.
                    return;
                }

                var AttackingIDUnk = packet.ReadInt();
                var TargetUniqueID = packet.ReadInt();
                var continueType = packet.ReadByte();

                if (continueType == 0x01)
                {
                    var hitCount = packet.ReadByte();
                    var targetCount = packet.ReadByte();

                    Debug.WriteLine($"[Character UniqueID: {Character.UniqueId}] SkillID: {SkillID} - OwnerUniqueID: {OwnerUniqueID} - AttackingIDUnk: {AttackingIDUnk} - TargetUniqueID: {TargetUniqueID} - continueType: {continueType}");

                    for (int i2 = 1; i2 <= targetCount; i2++)
                    {
                        var targetUniqueID = packet.ReadInt();

                        for (int i3 = 1; i3 <= hitCount; i3++)
                        {
                            var hitResult = packet.ReadByte(); // 0 : Alive - 128: Dead
                            var Critic = packet.ReadByte();
                            var Damage = packet.ReadInt();
                            var unk1 = packet.ReadByte();
                            var unk2 = packet.ReadByte();
                            var unk3 = packet.ReadByte();

                            Debug.WriteLine($"hitResult: {hitResult} - Crit: {Critic} - Damage: {Damage} - Unk1: {unk1} - Unk2: {unk2} - Unk3: {unk3} - RemainingRead: {packet.RemainingRead()}");
                            // bitmask --> ATTACK_STATE_KNOCKBACK = 0x01,    ATTACK_STATE_BLOCK = 0x02,    ATTACK_STATE_POSITION = 0x04,   abort ? --> 0x08   ATTACK_STATE_DEAD = 0x80

                            if (hitResult == 0x80) // 0x80 = 128 -- dead
                            {
                                //new Thread(new ThreadStart(() => RemoveFromDictionary(targetUniqueID, 5000))).Start();
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        [MethodTimer.Time]
        public static void SkillHits(Packet packet, Definitions.Character Character)
        {
            try
            {
                var Unk1 = packet.ReadByte();
                if (Unk1 != 1)
                {
                    Debug.WriteLine($"Bilinmeyen Unk1: {Unk1}");
                    return;
                }

                var SkillID = packet.ReadInt();
                var MainTargetUniqueID = packet.ReadInt();

                var Unk2 = packet.ReadByte();
                if (Unk2 != 1)
                {
                    Debug.WriteLine($"Bilinmeyen Unk2: {Unk2}");
                    return;
                }

                var NumberOfAttacks = packet.ReadByte();
                var FoundTargets = packet.ReadByte();

                //Debug.WriteLine($"[{Definitions.List.RefSkillDict[SkillID].Basic_Code}] SkillID: {SkillID} - MainTargetUniqueID: {MainTargetUniqueID} - NumberOfAttacks: {NumberOfAttacks} - FoundTargets: {FoundTargets}");
                for (int f = 0; f < FoundTargets; f++)
                {
                    var TargetID = packet.ReadInt();

                    for (int n = 0; n < NumberOfAttacks; n++)
                    {
                        var Status = packet.ReadByte();

                        if (Status != 2) // 2 = Block
                        {
                            var Critic = packet.ReadByte();
                            var Damage = packet.ReadInt();
                            var UnkN1 = packet.ReadByte();
                            var UnkN2 = packet.ReadShort();

                            if (Status == 4) // Knockdown
                            {
                                packet.ReadByte();
                                packet.ReadByte();

                                packet.ReadShort();
                                packet.ReadShort();
                                packet.ReadShort();
                                packet.ReadShort();
                                packet.ReadShort();
                                packet.ReadShort();
                            }
                            else
                            {
                                // TO-DO: Knockdown vurduk, istatistiklere ekle...
                            }

                            Debug.WriteLine($"TargetID: {TargetID} - Status: {Status} - Critic: {Critic} - Damage: {Damage} - UnkN1: {UnkN1} - UnkN2: {UnkN2}");
                        }
                        else
                        {
                            // TO-DO: Blok vurduk, istatistiklere ekle...
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine(ex);
            }
        }

        [MethodTimer.Time]
        public static void SpawnPacket(Packet packet, Definitions.Character Character)
        {
            try
            {
                packet.ReadInt(); // SROTimeStamp

                Character.Stats.Model = packet.ReadInt();
                Character.Stats.Height = packet.ReadByte();

                Character.Stats.CurrentLevel = packet.ReadByte();
                Character.Stats.MaxLevel = packet.ReadByte();

                Character.Stats.XP = packet.ReadLong();
                packet.ReadInt(); //sp bar
                Character.Stats.Gold = packet.ReadLong();
                Character.Stats.SkillPoints = packet.ReadInt();

                Character.Stats.AttrPoints = (short)packet.ReadShort();
                packet.ReadByte(); // berserk bar
                packet.ReadInt(); // expGathered?

                Character.Stats.HP = packet.ReadInt();
                Character.Stats.MP = packet.ReadInt();

                packet.ReadByte(); // 20 level < icon

                Character.Stats.PKInfo.DailyPK = packet.ReadByte();
                Character.Stats.PKInfo.PKLevel = (short)packet.ReadShort();
                Character.Stats.PKInfo.MurderLevel = packet.ReadInt();

                Character.Stats.HwanLevel = packet.ReadByte();
                Character.Stats.PVPInfo.FreePvp = packet.ReadByte();

                Character.Stats.InvSlots = packet.ReadByte();
                // Inventory
                Character.Inventory.Clear();
                var inventoryItemCount = packet.ReadByte();
                for (int i = 0; i < inventoryItemCount; i++)
                {
                    var itemSlot = packet.ReadByte();

                    var rentType = packet.ReadInt();
                    if (rentType == 1)
                    {
                        packet.ReadShort(); // CanDelete
                        packet.ReadInt(); // PeriodBeginTime
                        packet.ReadInt(); // PeriodEndTime
                    }
                    else if (rentType == 2)
                    {
                        packet.ReadShort(); // CanDelete
                        packet.ReadShort(); // CanRecharge
                        packet.ReadInt(); // MeterRateTime
                    }
                    else if (rentType == 3)
                    {
                        packet.ReadShort(); // CanDelete
                        packet.ReadShort(); // CanRecharge
                        packet.ReadInt(); // PeriodBeginTime
                        packet.ReadInt(); // PeriodEndTime
                        packet.ReadInt(); // PackingTime
                    }

                    var refItemID = packet.ReadInt();

                    if (!Definitions.List.RefObjCommonDict.ContainsKey(refItemID))
                    {
                        Debug.WriteLine("ITEM BULUNAMADI !??! - " + refItemID);
                    }

                    var RefItem = Definitions.List.RefObjCommonDict[refItemID];

                    Definitions.List.ProgramLogger.Information($"Item: [{RefItem.CodeName128}] TypeID2: {RefItem.TypeID2} - TypeID3: {RefItem.TypeID3} - TypeID4: {RefItem.TypeID4}");

                    Character.Inventory.Add(new Definitions.Structs.Character.InventoryItem
                    {
                        RefObjCommon = RefItem,
                        Slot = itemSlot
                    });
                    switch (RefItem.TypeID2)
                    {
                        case 1:
                            //ITEM_CH
                            //ITEM_EU
                            //AVATAR_
                            packet.ReadByte(); // OptLevel
                            packet.ReadLong(); // Variance
                            packet.ReadInt(); // Durability

                            var magParamNum = packet.ReadByte();
                            for (int paramIndex = 0; paramIndex < magParamNum; paramIndex++)
                            {
                                packet.ReadInt(); // magparam.Type
                                packet.ReadInt(); // magparam.Value
                            }

                            packet.ReadByte(); // 1 = can add Socket
                            var bindingOptionCount = packet.ReadByte();
                            for (int bindingOptionIndex = 0; bindingOptionIndex < bindingOptionCount; bindingOptionIndex++)
                            {
                                packet.ReadByte(); // Slot
                                packet.ReadInt(); // ID
                                packet.ReadInt(); // nParam1
                            }

                            packet.ReadByte(); // 2 = can add Adv elixir
                            bindingOptionCount = packet.ReadByte();
                            for (int bindingOptionIndex = 0; bindingOptionIndex < bindingOptionCount; bindingOptionIndex++)
                            {
                                packet.ReadByte(); // Slot
                                packet.ReadInt(); // ID
                                packet.ReadInt(); // nParam1
                            }
                            break;

                        case 2:
                            switch (RefItem.TypeID3)
                            {
                                case 1:
                                    int petState = packet.ReadByte(); // State

                                    if (petState != 1)
                                    {
                                        // refObjID
                                        packet.ReadInt();
                                        packet.ReadAscii(); // Name

                                        if (RefItem.TypeID4 == 2)
                                        {
                                            packet.ReadInt(); // SecondsToRentEndTime
                                        }

                                        packet.ReadByte(); // UnkByte0 - TODO? LEVEL?
                                    }
                                    break;

                                case 2:
                                // ITEM_ETC_TRANS_MONSTER
                                case 3:
                                    // MAGIC_CUBE
                                    packet.ReadInt(); // 2 = RefObjID | 3 = Quantity
                                    break;
                            }
                            break;

                        case 3: // ITEM_ETC
                            packet.ReadShort(); // quantity

                            switch (RefItem.TypeID3)
                            {
                                default:
                                    //Debug.WriteLine($"Unknown item TypeID3: {RefItem.TypeID3} from item: [ {RefItem.ID} ] {RefItem.CodeName128}");
                                    Definitions.List.ProgramLogger.Information($"Unknown item TypeID3: {RefItem.TypeID3} from item: [ {RefItem.ID} ] {RefItem.CodeName128}");
                                    break;

                                case 11:
                                    switch (RefItem.TypeID4)
                                    {
                                        case 1:
                                        case 2:
                                            packet.ReadByte(); // AttributeAssimilationProbability
                                            break;
                                    }
                                    break;

                                case 14:
                                    switch (RefItem.TypeID4)
                                    {
                                        default:
                                            //Debug.WriteLine($"Unknown item TypeID4: {RefItem.TypeID4} from item: [ {RefItem.ID} ] {RefItem.CodeName128}");
                                            Definitions.List.ProgramLogger.Information($"Unknown item TypeID4: {RefItem.TypeID4} from item: [ {RefItem.ID} ] {RefItem.CodeName128}");
                                            break;

                                        case 2:
                                            var magParamCount = packet.ReadByte();
                                            for (int paramIndex = 0; paramIndex < magParamCount; paramIndex++)
                                            {
                                                packet.ReadInt(); // Param Type
                                                packet.ReadInt(); // Param Value
                                            }
                                            break;
                                    }
                                    break;
                            }

                            break;

                        default:
                            //Debug.WriteLine($"Unknown item TypeID2: {RefItem.TypeID2} from item: [ {RefItem.ID} ] {RefItem.CodeName128}");
                            Definitions.List.ProgramLogger.Information($"Unknown item TypeID2: {RefItem.TypeID2} from item: [ {RefItem.ID} ] {RefItem.CodeName128}");
                            break;
                    } // switch (RefItem.TypeID2)
                } // For

                // Avatar Inventory
                packet.ReadByte(); // Avatar Inventory Size
                var avatarInventoryItemCount = packet.ReadByte();
                Character.AvatarInventory.Clear();
                for (int i = 0; i < avatarInventoryItemCount; i++)
                {
                    var itemSlot = packet.ReadByte(); // Slot

                    var rentType = packet.ReadInt();

                    switch (rentType)
                    {
                        default:
                            Debug.WriteLine($"Unknown avatar rent type: {rentType} character: {Character.Name} - item slot: {itemSlot}");
                            break;

                        case 1:
                            packet.ReadShort(); // CanDelete
                            packet.ReadInt(); // PeriodBeginTime
                            packet.ReadInt(); // PeriodEndTime
                            break;

                        case 2:
                            packet.ReadShort(); // CanDelete
                            packet.ReadShort(); // CanRecharge
                            packet.ReadInt(); // MeterRateTime
                            break;

                        case 3:
                            packet.ReadShort(); // CanDelete
                            packet.ReadShort(); // CanRecharge
                            packet.ReadInt(); // PeriodBeginTime
                            packet.ReadInt(); // PeriodEndTime
                            packet.ReadInt(); // PackingTime
                            break;
                    }

                    var refItemID = packet.ReadInt();
                    var RefItem = Definitions.List.RefObjCommonDict[refItemID];
                    Character.AvatarInventory.Add(new Definitions.Structs.Character.InventoryItem
                    {
                        RefObjCommon = RefItem,
                        Slot = itemSlot
                    });
                    if (RefItem.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Item)
                    {
                        if (RefItem.TypeID2 == 1)
                        {
                            packet.ReadByte(); // OptLevel
                            packet.ReadLong(); // Variance
                            packet.ReadInt(); // Durability

                            var magParamNum = packet.ReadByte();
                            for (int paramIndex = 0; paramIndex < magParamNum; paramIndex++)
                            {
                                packet.ReadInt(); // Type
                                packet.ReadInt(); // Value
                            }

                            packet.ReadByte(); // 1 = Socket
                            var bindingOptionCount = packet.ReadByte();
                            for (int bindingOptionIndex = 0; bindingOptionIndex < bindingOptionCount; bindingOptionIndex++)
                            {
                                packet.ReadByte(); // Slot
                                packet.ReadInt(); // ID
                                packet.ReadInt(); // nParam1
                            }

                            packet.ReadByte(); // 2 = Adv elixir
                            bindingOptionCount = packet.ReadByte();
                            for (int bindingOptionIndex = 0; bindingOptionIndex < bindingOptionCount; bindingOptionIndex++)
                            {
                                packet.ReadByte(); // Slot
                                packet.ReadInt(); // ID
                                packet.ReadInt(); // nParam1
                            }
                        }
                        else
                        {
                            Debug.WriteLine($"Item TypeID2 is not correct - WTF ?! - Item: {RefItem.CodeName128}");
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Item TypeID1 is not ITEM WTF ?! - Item: {RefItem.CodeName128}");
                    }
                } // avatar items - for

                packet.ReadByte(); // unkByte

                // Masteries
                Character.Masteries.Clear();
                var nextMastery = packet.ReadByte();
                while (nextMastery == 1)
                {
                    var masteryId = packet.ReadInt();
                    var masteryLevel = packet.ReadByte();
                    Character.Masteries.Add(new Definitions.Structs.Character.Mastery
                    {
                        MasteryID = masteryId,
                        MasteryLevel = masteryLevel
                    });

                    nextMastery = packet.ReadByte(); // nextMastery? --- bittiğinde 02?
                }
                packet.ReadByte(); // unkByte 00

                Character.Skills.Clear();
                var nextSkill = packet.ReadByte();
                while (nextSkill == 1)
                {
                    Character.Skills.Add(new Definitions.Structs.Character.Skill
                    {
                        SkillID = packet.ReadInt(),
                        Enabled = packet.ReadByte()
                    });

                    nextSkill = packet.ReadByte(); // bittiğinde 02?
                }

                var completedQuestCount = packet.ReadShort();
                uint[] completedQuests = packet.ReadUInt32Array(completedQuestCount);

                var activeQuestCount = packet.ReadByte();
                for (int activeQuestIndex = 0; activeQuestIndex < activeQuestCount; activeQuestIndex++)
                {
                    packet.ReadInt(); // RefQuestID
                    packet.ReadByte(); // AchievementCount
                    packet.ReadByte(); // Requires AutoShareParty
                    var questType = packet.ReadByte(); // Type

                    if (questType == 28)
                    {
                        packet.ReadInt(); // Remaining Time
                    }
                    packet.ReadByte(); // Quest status

                    if (questType != 8)
                    {
                        var objectiveCount = packet.ReadByte();
                        for (int objectiveIndex = 0; objectiveIndex < objectiveCount; objectiveIndex++)
                        {
                            packet.ReadByte(); // objective ID
                            packet.ReadByte(); // objective status
                            packet.ReadAscii(); // objective name

                            var taskCount = packet.ReadByte(); // TaskConut
                            for (int taskIndex = 0; taskIndex < taskCount; taskIndex++)
                            {
                                packet.ReadInt(); // task value
                            }
                        }
                    }

                    if (questType == 88)
                    {
                        var refObjCount = packet.ReadByte();
                        for (int refObjIndex = 0; refObjIndex < refObjCount; refObjIndex++)
                        {
                            packet.ReadInt(); // NPCs
                        }
                    }
                } // active quest - for

                packet.ReadByte(); // unknown byte

                // Collection Book
                var collectionBookCount = packet.ReadInt();
                for (int i = 0; i < collectionBookCount; i++)
                {
                    packet.ReadInt(); // Index
                    packet.ReadInt(); // StartedDateTime
                    packet.ReadInt(); // Pages
                }

                Character.UniqueId = packet.ReadInt();

                Character.Position.RegionID = (short)packet.ReadShort();
                Character.Position.X = packet.ReadFloat();
                Character.Position.Y = packet.ReadFloat();
                Character.Position.Z = packet.ReadFloat();
                Character.Position.Angle = (short)packet.ReadShort();

                Debug.WriteLine($"Character unique id: {Character.UniqueId} - Position: X: {Character.Position.X} - Y: {Character.Position.Y} - Z: {Character.Position.Z} - RegionID: {Character.Position.RegionID}");

                var movementHasDestination = packet.ReadByte();
                packet.ReadByte(); // movement type
                if (movementHasDestination == 1)
                {
                    packet.ReadShort(); // Destination region
                    if (Character.Position.RegionID < short.MaxValue)
                    {
                        packet.ReadShort(); // DestinationOffsetX
                        packet.ReadShort(); // DestinationOffsetY
                        packet.ReadShort(); // DestinationOffsetZ
                    }
                    else
                    {
                        // Dungeon
                        packet.ReadInt(); // DestinationOffsetX
                        packet.ReadInt(); // DestinationOffsetY
                        packet.ReadInt(); // DestinationOffsetZ
                    }
                }
                else
                {
                    packet.ReadByte(); // movement source      //0 = Spinning, 1 = Sky-/Key-walking
                    packet.ReadShort(); // angle //Represents the new angle, character is looking at
                }

                var CharacterLifeState = packet.ReadByte(); // ÇALIŞMIYOR
                var unkbyte0 = packet.ReadByte(); // unkbyte0
                packet.ReadByte(); // MotionState =        //0 = None, 2 = Walking, 3 = Running, 4 = Sitting
                var Status = packet.ReadByte();  // ÇALIŞMIYOR

                Character.Stats.WalkSpeed = packet.ReadFloat();
                Character.Stats.RunSpeed = packet.ReadFloat();
                Character.Stats.HwanSpeed = packet.ReadFloat();

                var activeBuffCount = packet.ReadByte();
                Character.ActiveBuffs.Clear();
                for (int i = 0; i < activeBuffCount; i++)
                {
                    var skillID = packet.ReadInt();
                    var skillData = Definitions.List.RefSkillDict[skillID];

                    Character.ActiveBuffs.Add(new Definitions.Structs.Character.ActiveBuff
                    {
                        Skill = skillData,
                        Duration = packet.ReadInt() // Duration
                    });

                    if (skillData.Param2 == 1701213281)
                    {
                        packet.ReadByte();
                    }
                }

                Character.Name = packet.ReadAscii();
                Character.Job.Name = packet.ReadAscii();
                Character.Job.Type = packet.ReadByte();
                Character.Job.Level = packet.ReadByte();
                Character.Job.Exp = packet.ReadInt();
                Character.Job.Contribution = packet.ReadInt();
                Character.Job.Reward = packet.ReadInt();

                Character.Stats.PVPInfo.PvpState = packet.ReadByte(); //0 = White, 1 = Purple, 2 = Red
                Character.Job.TransportFlag = packet.ReadByte();
                Character.InCombat = packet.ReadByte();
                if (Character.Job.TransportFlag == 1)
                {
                    Character.Job.TransportID = packet.ReadInt();
                }

                Character.Stats.PVPInfo.PvpFlag = packet.ReadByte(); //0 = Red Side, 1 = Blue Side, 0xFF = None
                packet.ReadLong(); // GuideFlag
                Character.JID = packet.ReadInt();
                Character.IsGm = packet.ReadByte() == 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Definitions.List.ProgramLogger.Error(ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }

        [MethodTimer.Time]
        public static bool ChatPacket(Packet currentPacket, Definitions.Character Character)
        {
            try
            {
                byte chatType = currentPacket.ReadByte();
                byte chatIndex = currentPacket.ReadByte();

                if (chatType == (byte)Definitions.Enums.Character.ChatType.PM)
                {
                    string receiver = currentPacket.ReadAscii();
                }

                string message = currentPacket.ReadAscii();

                string[] commandMessage = message.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                Definitions.List.ChatCommands.ChatCommandsDic.TryGetValue(commandMessage[0], out Framework.Commands.Parser chatCommand);
                if (chatCommand != null)
                {
                    chatCommand.Parse(Character, chatCommand.Command, commandMessage);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Definitions.List.ProgramLogger.Error(ex.ToString());
                Debug.WriteLine("PacketHandlers.Character.ChatPacket Exception: " + ex);
                return true;
            }
        }
    }
}