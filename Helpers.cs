using Archipelago.MultiClient.Net.Models;
using NNT_Archipealgo.CustomData;
using NNT_Archipealgo.Patchers;
using System.Collections.Generic;
using static JAWStateMachine;

namespace NNT_Archipealgo
{
    internal class Helpers
    {
        public static void HandleItem(KeyValuePair<ArchipelagoItem, int> item, bool fromStart = false, bool trapLink = false)
        {
            switch (item.Key.ItemName)
            {
                case "Levers": Plugin.save.HasLevers = true; break;
                case "Possession": Plugin.save.CanPosses = true; break;
                case "Grenades": Plugin.save.HasGrenades = true; break;
                case "Rocks": Plugin.save.HasRocks = true; break;
                case "UXB Defusion": Plugin.save.CanDefuseUXBs = true; break;
                case "Lifts": Plugin.save.CanUseLifts = true; break;
                case "Spirit Rings": Plugin.save.CanUseSpiritRings = true; break;
                case "Meat": Plugin.save.CanUseMeatSacks = true; break;
                case "Shrykull": Plugin.save.CanUseShrykull = true; break;
                case "Rescued Mudokon": Plugin.save.MudokonCount += item.Value; break;

                case "Rupture Farms": Plugin.save.UnlockedLocations[0] = true; break;
                case "Stockyards": Plugin.save.UnlockedLocations[1] = true; break;
                case "Paramonia": Plugin.save.UnlockedLocations[2] = true; break;
                case "Scrabania": Plugin.save.UnlockedLocations[3] = true; break;
                case "Zulag 1": Plugin.save.UnlockedLocations[4] = true; break;
                case "Zulag 2": Plugin.save.UnlockedLocations[5] = true; break;
                case "Zulag 3": Plugin.save.UnlockedLocations[6] = true; break;
                case "Zulag 4": Plugin.save.UnlockedLocations[7] = true; break;
                case "Monsaic Lines": Plugin.save.UnlockedLocations[8] = true; break;

                case "Shock Trap": AbePatcher.player?.ReInitStateMachine(SMStates.AbeZap); break;
                case "Trip Trap": AbePatcher.player?.SetState(SMStates.AbeHitWall); break;

                // Unhandled items, throw an error into the console.
                default: Plugin.consoleLog.LogError($"Item Type '{item.Key.ItemName}' (sent by '{item.Key.Source}' {item.Value} time(s)) not yet handled!"); return;
            }
        }

        /// <summary>
        /// Checks if a location exists in the multiworld.
        /// </summary>
        public static bool CheckLocationExists(long locationIndex) => locationIndex != -1 && Plugin.session.Locations.AllLocations.Contains(locationIndex);

        /// <summary>
        /// Completes a location check.
        /// </summary>
        public static void CompleteLocationCheck(string locationName)
        {
            // Get the ID of this location.
            long locationIndex = Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", locationName);

            // Check if this location exists and hasn't already been checked.
            if (CheckLocationExists(locationIndex) && !Plugin.session.Locations.AllLocationsChecked.Contains(locationIndex))
            {
                // Queue up this location.
                Plugin.EnqueueLocation(locationIndex);

                // Get the info from this item.
                ScoutedItemInfo item = Plugin.save.items[locationIndex];

                // If this isn't an item for ourselves, then add a message to our info string queue to be displayed when possible.
                if (item.Player.Name != Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot))
                    Plugin.infoStringQueue.Add($"Found {item.Player.Name}'s {item.ItemName}.");
            }
        }
    }
}
