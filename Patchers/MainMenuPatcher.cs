using static MainMenuController;

namespace NNT_Archipealgo.Patchers
{
    internal class MainMenuPatcher
    {
        /// <summary>
        /// Disables the back buttons on the main menu and chapter select.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "Start")]
        static void DisableBackButtons(MainMenuController __instance)
        {
            // Deactivate the main menu's back button (also gets rid of the social media buttons that are there on the Steam version).
            __instance.m_frontEnd.transform.GetChild(1).transform.GetChild(6).gameObject.SetActive(false);

            // Deactivate the chapter select's back button.
            __instance.m_levelSelect.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.SetActive(false);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "InitSaveSlotUI")]
        static bool DisableSaveUI() => false;

        /// <summary>
        /// Forces the game to go to the chapter select rather than starting a new game.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "StartFollowMe")]
        static void RedirectToChapterSelect(ref FinishedFollowMeAction eNextAction, ref FinishedFollowMeAction ___m_eFinishedFollowMeAction, ref bool ___m_selectingPlayers)
        {
            // Check if we're trying to start a new game.
            // If so, redirect it to the chapter select and remove the flag telling the menu that we're on the player count select menu.
            if (eNextAction == FinishedFollowMeAction.NewGame)
            {
                ___m_eFinishedFollowMeAction = FinishedFollowMeAction.ToChapterSelect;
                ___m_selectingPlayers = false;
            }
        }

        /// <summary>
        /// Kills the function that would normally remove a locked button from a menu list, preventing scrolling through the chapter select.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuElement), "RemoveFromButtonKeyChain")]
        static bool DisableButtonRemovalFromList() => false;

        /// <summary>
        /// Lock/Unlock Chapters on the menu and change unused ones to the NOT IN SEED string.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChapterSelectPanel), "SetChapterInfo")]
        [HarmonyPatch(typeof(ChapterSelectPanel), "UpdateButtons")]
        static void HandleChapterLocks(ref ScrollViewButton[] ___m_acScrollViewButtons)
        {
            // Loop through each button, specifically as a ChapterSelectButton rather than a generic ScrollViewButton.
            foreach (ChapterSelectButton button in ___m_acScrollViewButtons)
            {
                // Lock/Unlock this button.
                button.DoToggleLocked();

                // If we're not using Extra Area Clears, then set the NOT IN SEED value on the unused areas.
                if ((button.m_eChapter is LevelList.Chapters.MonsaicLines or
                                         LevelList.Chapters.Paramonia or
                                         LevelList.Chapters.ParamonianNests or
                                         LevelList.Chapters.Scrabania or
                                         LevelList.Chapters.ScrabanianNests or
                                         LevelList.Chapters.FreeFireZone) &&
                                         ((long)Plugin.slotData["area_clears"] == 0 || (long)Plugin.slotData["extra_area_clears"] == 0))
                    button.m_cJAWMenuLocalisation.SetKey("AP_NotInSeed");

                // Set the NOT IN SEED value on the unused goal area.
                if (button.m_eChapter == LevelList.Chapters.TheBoardroom && (long)Plugin.slotData["goal"] == 1)
                    button.m_cJAWMenuLocalisation.SetKey("AP_NotInSeed");
                if (button.m_eChapter == LevelList.Chapters.Alf && (long)Plugin.slotData["goal"] == 0)
                    button.m_cJAWMenuLocalisation.SetKey("AP_NotInSeed");
            }
        }

        /// <summary>
        /// Replaces the BAD KEY output from a missing string call with NOT IN SEED if the passed in key was "AP_NotInSeed".
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LanguagePack), "GetString")]
        static void CustomLockString(ref string key, ref string __result)
        {
            if (key == "AP_NotInSeed")
                __result = "NOT IN SEED";
        }

        /// <summary>
        /// Stops the game from even attempting to update the leaderboard data.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LeaderBoardDataHandler), "UpdateLeaderBoardHandler")]
        [HarmonyPatch(typeof(LeaderBoardDataHandler), "UploadToLeaderBoards")]
        static bool DisableLeaderBoardData() => false;

        /// <summary>
        /// Disables the back button on the main menu and chapter select.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "BackToSaveSlotSelect")]
        [HarmonyPatch(typeof(MainMenuController), "BackToBegin")]
        static bool DisableReturnToSaveSelect(ref bool ___m_bDoBackOutcome)
        {
            ___m_bDoBackOutcome = false;
            return false;
        }
    }
}
