using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;

namespace NNT_Archipealgo.Patchers
{
    internal class AppPatcher
    {
        /// <summary>
        /// Determines whether or not a chapter should be unlocked or not.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(App), "GetChapterStarted")]
        static bool HijackChapterUnlocks(ref LevelList.Chapters eChapter, ref bool __result)
        {
            // Check if the Paramonian Trials have been done.
            bool paramoniaTrialsDone = false;
            if (Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Paramonian Trial 1")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Paramonian Trial 2")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Paramonian Trial 3")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Paramonian Trial 4")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Paramonian Trial 5")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Paramonian Trial 6")) &&
                ((long)Plugin.slotData["area_clears"] == 1))
                paramoniaTrialsDone = true;

            // Check if the Scrabanian Trials have been done.
            bool scrabaniaTrialsDone = false;
            if (Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 1")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 2")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 3")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 4")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 5")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 6")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 7")) &&
                Plugin.session.Locations.AllLocationsChecked.Contains(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Trial 8")) &&
                ((long)Plugin.slotData["area_clears"] == 1))
                scrabaniaTrialsDone = true;

            // Handle replacing the result depending on our chapter.
            switch (eChapter)
            {
                case LevelList.Chapters.RuptureFarms: __result = Plugin.save.UnlockedLocations[0]; return false;
                case LevelList.Chapters.StockyardEscape: __result = Plugin.save.UnlockedLocations[1]; return false;
                case LevelList.Chapters.MonsaicLines: if ((long)Plugin.slotData["area_clears"] == 1 && (long)Plugin.slotData["extra_area_clears"] == 1) __result = Plugin.save.UnlockedLocations[8]; return false;
                case LevelList.Chapters.Paramonia: if ((long)Plugin.slotData["area_clears"] == 1 && (long)Plugin.slotData["extra_area_clears"] == 1) __result = Plugin.save.UnlockedLocations[2]; return false;
                case LevelList.Chapters.ParamonianTemple: __result = Plugin.save.UnlockedLocations[2]; return false;
                case LevelList.Chapters.ParamonianNests: __result = paramoniaTrialsDone; return false;
                case LevelList.Chapters.Scrabania: if ((long)Plugin.slotData["area_clears"] == 1 && (long)Plugin.slotData["extra_area_clears"] == 1) __result = Plugin.save.UnlockedLocations[3]; return false;
                case LevelList.Chapters.ScrabanianTemple: __result = Plugin.save.UnlockedLocations[3]; return false;
                case LevelList.Chapters.ScrabanianNests: __result = scrabaniaTrialsDone; return false;
                case LevelList.Chapters.FreeFireZone: if ((long)Plugin.slotData["area_clears"] == 1 && (long)Plugin.slotData["extra_area_clears"] == 1) __result = Plugin.save.UnlockedLocations[1]; return false;
                case LevelList.Chapters.RescueZulag1: __result = Plugin.save.UnlockedLocations[4]; return false;
                case LevelList.Chapters.RescueZulag2: __result = Plugin.save.UnlockedLocations[5]; return false;
                case LevelList.Chapters.RescueZulag3: __result = Plugin.save.UnlockedLocations[6]; return false;
                case LevelList.Chapters.RescueZulag4: __result = Plugin.save.UnlockedLocations[7]; return false;
                case LevelList.Chapters.TheBoardroom: __result = Plugin.save.MudokonCount >= (long)Plugin.slotData["required_muds"] && (long)Plugin.slotData["goal"] == 0; return false;
                case LevelList.Chapters.Alf: __result = Plugin.save.MudokonCount >= (long)Plugin.slotData["required_muds"] && (long)Plugin.slotData["goal"] == 1; return false;

                default:
                    Plugin.consoleLog.LogWarning($"Handling for {eChapter} not implemented!"); __result = false; return false;
            }
        }

        /// <summary>
        /// Sends out a location after clearing a chapter.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(App), "CompletedChapter")]
        static void HandleChapterClear(ref LevelList.Chapters ___m_eCurrentChapter)
        {
            // Send the clear location for the current chapter (or the goal position for The Boardroom or Alf's Escape).
            switch (___m_eCurrentChapter)
            {
                case LevelList.Chapters.RuptureFarms: Helpers.CompleteLocationCheck("Rupture Farms - Clear"); break;
                case LevelList.Chapters.StockyardEscape: Helpers.CompleteLocationCheck("Stockyard Escape - Clear"); break;
                case LevelList.Chapters.MonsaicLines: Helpers.CompleteLocationCheck("Monsaic Lines - Clear"); break;
                case LevelList.Chapters.Paramonia: Helpers.CompleteLocationCheck("Paramonia - Clear"); break;
                case LevelList.Chapters.ParamonianNests: Helpers.CompleteLocationCheck("Paramonian Nests - Clear"); break;
                case LevelList.Chapters.Scrabania: Helpers.CompleteLocationCheck("Scrabania - Clear"); break;
                case LevelList.Chapters.ScrabanianNests: Helpers.CompleteLocationCheck("Scrabanian Nests - Clear"); break;
                case LevelList.Chapters.FreeFireZone: Helpers.CompleteLocationCheck("Stockyard Return - Clear"); break;
                case LevelList.Chapters.RescueZulag1: Helpers.CompleteLocationCheck("Zulag 1 - Clear"); break;
                case LevelList.Chapters.RescueZulag2: Helpers.CompleteLocationCheck("Zulag 2 - Clear"); break;
                case LevelList.Chapters.RescueZulag3: Helpers.CompleteLocationCheck("Zulag 3 - Clear"); break;
                case LevelList.Chapters.RescueZulag4: Helpers.CompleteLocationCheck("Zulag 4 - Clear"); break;

                case LevelList.Chapters.TheBoardroom:
                case LevelList.Chapters.Alf:
                    StatusUpdatePacket goalPacket = new() { Status = ArchipelagoClientState.ClientGoal };
                    Plugin.session.Socket.SendPacketAsync(goalPacket);
                    break;
            }

            // Return to the menu if we're not in either of the Temples or the goal areas.
            if (___m_eCurrentChapter is not LevelList.Chapters.TheBoardroom and not
                                            LevelList.Chapters.Alf)
                App.getInstance().QuitGameToFrontEnd(false);
        }
    }
}
