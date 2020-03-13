using DataModel;
using LinqToDB;
using RevBridge.Definitions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RevBridge.Functions
{
    internal static class TableData
    {
        [MethodTimer.Time]
        public static async Task LoadAsync()
        {
            try
            {
                await using (var db = new ShardDb())
                {
                    // _RefObjCommon table
                    var objCommonQuery = from objCommon in db.RefObjCommons
                                         where objCommon.ID > 0
                                         orderby objCommon.ID
                                         select objCommon;

                    List.RefObjCommon = await objCommonQuery.ToListAsync();
                    List.RefObjCommonDict = List.RefObjCommon.ToDictionary(x => x.ID, item => item);

                    // _RefObjChar table
                    var objCharQuery = from objChar in db.RefObjChars
                                       orderby objChar.ID
                                       select objChar;
                    List.RefObjChar = await objCharQuery.ToListAsync();
                    List.RefObjCharDict = List.RefObjChar.ToDictionary(x => x.ID, item => item);

                    // _RefObjItem table
                    var objItemQuery = from objItem in db.RefObjItems
                                       orderby objItem.ID
                                       select objItem;
                    List.RefObjItem = await objItemQuery.ToListAsync();
                    List.RefObjItemDict = List.RefObjItem.ToDictionary(x => x.ID, item => item);

                    // _RefObjStruct table
                    var objStructQuery = from objStruct in db.RefObjStructs
                                         orderby objStruct.ID
                                         select objStruct;
                    List.RefObjStruct = await objStructQuery.ToListAsync();
                    List.RefObjStructDict = List.RefObjStruct.ToDictionary(x => x.ID, item => item);

                    // _RefSkill table
                    var refSkillQuery = from refSkill in db.RefSkills
                                        orderby refSkill.ID
                                        select refSkill;
                    List.RefSkill = await refSkillQuery.ToListAsync();
                    List.RefSkillDict = List.RefSkill.ToDictionary(x => x.ID, item => item);
                }

                Debug.WriteLine($"Veritabanı tanımlamaları yüklendi. RefObjCommon: {List.RefObjCommon.Count} - RefObjChar: {List.RefObjChar.Count} - RefObjItem: {List.RefObjItem.Count} - RefObjStruct: {List.RefObjStruct.Count} - RefSkill: {List.RefSkill.Count}");

                Debug.WriteLine($"Veritabanı sözlüğü oluşturuldu. RefObjCommon: {List.RefObjCommonDict.Count} - RefObjChar: {List.RefObjCharDict.Count} - RefObjItem: {List.RefObjItemDict.Count} - RefSkill: {List.RefSkillDict.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}