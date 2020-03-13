using RevBridge.Framework.SilkroadSecurityApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RevBridge.Functions.PacketHandlers
{
    internal sealed class Entity
    {
        public static bool SingleSpawn(Packet packet, Definitions.Character Character)
        {
            try
            {
                //spawn
                List<Definitions.Structs.Character.InventoryItem> Inventory = new List<Definitions.Structs.Character.InventoryItem>();

                if (packet.RemainingRead() == 0)
                {
                    Debug.WriteLine("0 geldi ?!");
                }

                var RefObjID = packet.ReadInt();

                if (Definitions.List.RefObjCommonDict.ContainsKey(RefObjID))
                {
                    var RefObjCommon = Definitions.List.RefObjCommonDict[RefObjID];

                    if (RefObjCommon.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Mob)
                    {
                        if (RefObjCommon.TypeID2 == 1)
                        {
                            var scale = packet.ReadByte();
                            var hwanlevel = packet.ReadByte();
                            var pvpcape = packet.ReadByte();
                            var autoinverstexp = packet.ReadByte();

                            var inventorySize = packet.ReadByte();
                            var inventoryItemCount = packet.ReadByte();

                            for (int i = 0; i < inventoryItemCount; i++)
                            {
                                var refitemid = packet.ReadInt();

                                var refiteminfo = Definitions.List.RefObjCommonDict[refitemid];

                                Inventory.Add(new Definitions.Structs.Character.InventoryItem
                                {
                                    RefObjCommon = refiteminfo
                                });

                                if (refiteminfo.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Item && refiteminfo.TypeID2 == 1)
                                {
                                    packet.ReadByte();
                                }
                            } //inventory for

                            packet.ReadByte();
                            var avatarInventoryItemCount = packet.ReadByte();

                            for (int i = 0; i < avatarInventoryItemCount; i++)
                            {
                                var refitemid = packet.ReadInt();

                                var refiteminfo = Definitions.List.RefObjCommonDict[refitemid];

                                if (refiteminfo.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Item && refiteminfo.TypeID2 == 1)
                                {
                                    packet.ReadByte();
                                }
                            } // avatar inventory for

                            var hasMask = packet.ReadByte();

                            if (hasMask == 1)
                            {
                                var refitemid = packet.ReadInt();

                                var refiteminfo = Definitions.List.RefObjCommonDict[refitemid];

                                if (refiteminfo.TypeID1 == RefObjCommon.TypeID1 && refiteminfo.TypeID2 == RefObjCommon.TypeID2)
                                {
                                    packet.ReadByte();
                                    var maskItemCount = packet.ReadByte();

                                    for (int i = 0; i < maskItemCount; i++)
                                    {
                                        packet.ReadInt();
                                    } // mask for
                                }
                            } // if has mask
                        } // if (RefObjItem.TypeID2 == 1)
                        else if (RefObjCommon.TypeID2 == 2 && RefObjCommon.TypeID3 == 5)
                        {
                            packet.ReadInt(); // HP
                            packet.ReadInt(); // RefEventStructID
                            packet.ReadShort(); // State
                        }

                        var uniqueID = packet.ReadInt(); // UniqueID

                        if (RefObjCommon.TypeID2 == 2 && RefObjCommon.TypeID3 == 2 && !Definitions.List.UniqueIds.ContainsKey(uniqueID))
                        {
                            Definitions.List.UniqueIds.Add(uniqueID, RefObjCommon);
                        }
                        /*
                        else
                        {
                            Definitions.List.UniqueIds[uniqueID].Count++;
                        }*/

                        var regionID = packet.ReadShort();
                        var positionX = packet.ReadFloat();
                        var positionY = packet.ReadFloat();
                        var positionZ = packet.ReadFloat();
                        var positionAngle = packet.ReadShort();

                        var hasDestination = packet.ReadByte();
                        packet.ReadByte();

                        if (hasDestination == 1)
                        {
                            packet.ReadShort();

                            if (regionID < short.MaxValue)
                            {
                                packet.ReadShort();
                                packet.ReadShort();
                                packet.ReadShort();
                            }
                            else
                            {
                                packet.ReadInt();
                                packet.ReadInt();
                                packet.ReadInt();
                            }
                        } // if (hasDestination == 1)
                        else
                        {
                            packet.ReadByte();
                            packet.ReadShort();
                        } // has destination

                        packet.ReadByte(); // lifestate
                        packet.ReadByte(); // unk
                        packet.ReadByte(); // motionstate
                        packet.ReadByte(); // status
                        packet.ReadFloat(); // walk speed
                        packet.ReadFloat(); // run speed
                        packet.ReadFloat(); // hwan speed

                        var buffCount = packet.ReadByte();

                        for (int i = 0; i < buffCount; i++)
                        {
                            var refskillid = packet.ReadInt();
                            packet.ReadInt();

                            if (Definitions.List.RefSkill.Any(x => x.ID == refskillid && x.Param2 == 1701213281))
                            {
                                packet.ReadByte();
                            }
                        }

                        if (RefObjCommon.TypeID2 == 1)
                        {
                            var characterName = packet.ReadAscii();

                            packet.ReadByte(); // job type
                            packet.ReadByte(); // job level
                            packet.ReadByte(); // pvp state
                            var transportFlag = packet.ReadByte(); // transport flag
                            packet.ReadByte(); // in combat

                            if (transportFlag == 1)
                            {
                                packet.ReadInt(); // transport uniq id
                            }

                            packet.ReadByte(); // scroll mode
                            byte interactMode = packet.ReadByte();
                            packet.ReadByte(); // unkbyte4

                            var guildName = packet.ReadAscii();

                            if (Inventory.Any(x => x.RefObjCommon.TypeID2 == 1 && x.RefObjCommon.TypeID3 == 7))
                            {
                                Debug.WriteLine("Envanterde job item var! - " + RefObjID);
                                packet.ReadInt();
                                packet.ReadAscii();
                                packet.ReadInt();
                                packet.ReadInt(); // union id
                                packet.ReadInt();
                                packet.ReadByte(); // is friendly
                                packet.ReadByte(); // siege authority
                            }

                            if (interactMode == 4) // stall
                            {
                                packet.ReadAscii();
                                packet.ReadInt();
                            }

                            packet.ReadByte(); // EquipmentCooldown
                            packet.ReadByte(); // PKFlag
                        }
                        else if (RefObjCommon.TypeID2 == 2)
                        {
                            var talkFlag = packet.ReadByte();

                            if (talkFlag == 2)
                            {
                                var talkOptionCount = packet.ReadByte();
                                var talkOptions = packet.ReadUInt8Array(talkOptionCount);
                            }

                            if (RefObjCommon.TypeID3 == 2)
                            {
                                if (packet.RemainingRead() != 0)
                                {
                                    packet.ReadByte(); // Rarity
                                    if (RefObjCommon.TypeID4 == 2 || RefObjCommon.TypeID4 == 3)
                                    {
                                        packet.ReadByte(); // Randomized by server?
                                    }
                                }
                            }
                            else if (RefObjCommon.TypeID3 == 3)
                            {
                                if (RefObjCommon.TypeID4 == 3 || RefObjCommon.TypeID4 == 4)
                                {
                                    packet.ReadAscii();
                                }

                                //packet.ReadAscii(); // If TypeID4 == 5 then it is Guildname, else it is owner name

                                if (RefObjCommon.TypeID4 == 2 || RefObjCommon.TypeID4 == 3 || RefObjCommon.TypeID4 == 4 || RefObjCommon.TypeID4 == 5)
                                {
                                    packet.ReadByte();

                                    if (RefObjCommon.TypeID4 != 4)
                                    {
                                        packet.ReadByte();
                                    }

                                    if (RefObjCommon.TypeID4 == 5)
                                    {
                                        packet.ReadInt(); // owner refobjid
                                    }
                                }

                                //packet.ReadInt(); // owner unique id
                            }
                            else if (RefObjCommon.TypeID3 == 4)
                            {
                                packet.ReadInt(); // guild id
                                packet.ReadAscii(); // guild name
                            }
                        }
                    } // if (RefObjItem.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Mob)
                    else if (RefObjCommon.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Item)
                    {
                        if (RefObjCommon.TypeID2 == 1)
                        {
                            packet.ReadByte(); // OptLevel
                        }
                        else if (RefObjCommon.TypeID2 == 3)
                        {
                            if (RefObjCommon.TypeID3 == 5 && RefObjCommon.TypeID4 == 0)
                            {
                                packet.ReadInt();
                            }
                            else if (RefObjCommon.TypeID3 == 8 || RefObjCommon.TypeID3 == 9)
                            {
                                packet.ReadAscii();
                            }
                        }
                        var uniqueID = packet.ReadInt();
                        packet.ReadShort(); // RegionID
                        packet.ReadFloat(); // X
                        packet.ReadFloat(); // Y
                        packet.ReadFloat(); // Z
                        packet.ReadShort(); // Angle
                        byte hasOwner = packet.ReadByte();

                        int ownerJid = 0;
                        if (hasOwner == 1)
                        {
                            ownerJid = packet.ReadInt();
                        }

                        var rarity = packet.ReadByte(); // Rarity
                    } //  else if (RefObjItem.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Item)
                    else if (RefObjCommon.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Object)
                    {
                        var uniqueID = packet.ReadInt();
                        packet.ReadShort(); // RegionID
                        packet.ReadFloat(); // X
                        packet.ReadFloat(); // Y
                        packet.ReadFloat(); // Z
                        packet.ReadShort(); // Angle

                        var unkByte1 = packet.ReadByte();
                        packet.ReadByte();
                        var unkByte3 = packet.ReadByte();

                        if (unkByte3 == 1)
                        {
                            packet.ReadInt();
                            packet.ReadInt();
                        }
                        else if (unkByte3 == 6)
                        {
                            packet.ReadAscii(); // owner name
                            packet.ReadInt(); // unique id
                        }

                        if (unkByte1 == 1)
                        {
                            packet.ReadInt();
                            packet.ReadByte();
                        }
                    } // else if(RefObjItem.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Object)

                    if (RefObjCommon.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Mob || RefObjCommon.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Object)
                    {
                        packet.ReadByte(); // unkbyte6
                    }
                    else if (RefObjCommon.TypeID1 == (byte)Definitions.Enums.Tables.RefObjCommon.TypeID1.Item)
                    {
                        packet.ReadByte(); // drop source
                        packet.ReadInt(); // drop source uniqueid
                    }
                }

                if ((uint)RefObjID == uint.MaxValue)
                {
                    packet.ReadShort();
                    packet.ReadInt(); // refskillid
                    packet.ReadInt(); // unique id
                    packet.ReadShort(); // RegionID
                    packet.ReadFloat(); // X
                    packet.ReadFloat(); // Y
                    packet.ReadFloat(); // Z
                    packet.ReadShort(); // Angle
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return true;
            }
        }

        public static void SingleDespawn(Packet packet, Definitions.Character Character)
        {
            try
            {
                var uniqueID = packet.ReadInt();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}