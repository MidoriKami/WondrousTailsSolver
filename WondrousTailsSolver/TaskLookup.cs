﻿using System.Collections.Generic;
using System.Linq;

using Dalamud;
using Dalamud.Logging;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace WondrousTailsSolver;

/// <summary>
/// Helper class for converting Task ID's into Zone Lists
/// </summary>
public static class TaskLookup
{
    /// <summary>
    /// Processes Wondrous Tails TaskID into a list of zones
    /// </summary>
    /// <param name="id">Wondrous Tails TaskID.</param>
    /// <returns>List of TerritoryType row Id's.</returns>
    public static List<uint> GetInstanceListFromID(uint id)
    {
        var bingoOrderData = Service.DataManager.GetExcelSheet<WeeklyBingoOrderData>()!.GetRow(id);
        if (bingoOrderData is null) return new List<uint>();

        switch (bingoOrderData.Type)
        {
            // Specific Duty
            case 0:
                return Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
                    .Where(c => c.Content == bingoOrderData.Data)
                    .Select(c => c.TerritoryType.Row)
                    .ToList();

            // Specific Level Dungeon
            case 1:
                return Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
                    .Where(m => m.ContentType.Row is 2)
                    .Where(m => m.ClassJobLevelRequired == bingoOrderData.Data)
                    .Select(m => m.TerritoryType.Row)
                    .ToList();

            // Level Range Dungeon
            case 2:
                return bingoOrderData.Data switch
                {
                    // Level 1 - 50
                    50 => Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
                        .Where(m => m.ContentType.Row is 2)
                        .Where(m => m.ClassJobLevelRequired is >= 1 and <= 49)
                        .Select(m => m.TerritoryType.Row)
                        .ToList(),

                    // Level (x - 9) - (x - 1) :: Example => 60 becomes 51 - 59
                    _ => Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
                        .Where(m => m.ContentType.Row is 2)
                        .Where(m => m.ClassJobLevelRequired >= bingoOrderData.Data - 9 && m.ClassJobLevelRequired <= bingoOrderData.Data - 1)
                        .Select(m => m.TerritoryType.Row)
                        .ToList(),
                };

            // Special categories
            case 3:
                return bingoOrderData.Unknown5 switch
                {
                    // Treasure Map Instances are Not Supported
                    1 => new List<uint>(),

                    // PvP Categories are Not Supported
                    2 => new List<uint>(),

                    // Deep Dungeons
                    3 => Service.DataManager.GetExcelSheet<ContentFinderCondition>()!
                        .Where(m => m.ContentType.Row is 21)
                        .Select(m => m.TerritoryType.Row)
                        .ToList(),

                    _ => new List<uint>(),
                };

            // Multi-instance raids
            case 4:
                return bingoOrderData.Data switch
                {
                    // Binding Coil, Second Coil, Final Coil
                    2 => new List<uint> { 241, 242, 243, 244, 245 },
                    3 => new List<uint> { 355, 356, 357, 358 },
                    4 => new List<uint> { 193, 194, 195, 196 },

                    // Gordias, Midas, The Creator
                    5 => new List<uint> { 442, 443, 444, 445 },
                    6 => new List<uint> {520, 521, 522, 523},
                    7 => new List<uint> { 580, 581, 582, 583 },

                    // Deltascape, Sigmascape, Alphascape
                    8 => new List<uint> { 691, 692, 693, 694 },
                    9 => new List<uint> { 748, 749, 750, 751 },
                    10 => new List<uint> { 798, 799, 800, 801 },

                    > 10 => Service.DataManager.GetExcelSheet<ContentFinderCondition>(ClientLanguage.English)!
                        .Where(row => row.ContentType.Row is 5)
                        .Where(row => row.ContentMemberType.Row is 3)
                        .Where(row => !row.Name.ToDalamudString().TextValue.Contains("Savage"))
                        .Where(row => row.ItemLevelRequired >= 425)
                        .OrderBy(row => row.SortKey)
                        .Select(row => row.TerritoryType.Row)
                        .ToArray()[(int)(-11 + bingoOrderData.Data)..(int)(-10 + bingoOrderData.Data)]
                        .ToList(),

                    _ => new List<uint>(),
                };
        }

        PluginLog.Information($"[WondrousTails] Unrecognized ID: {id}");
        return new List<uint>();
    }
}