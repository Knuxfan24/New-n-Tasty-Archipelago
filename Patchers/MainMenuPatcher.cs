using static MainMenuController;

namespace NNT_Archipealgo.Patchers
{
    internal class MainMenuPatcher
    {
        /// <summary>
        /// Connects to the AP server while also stopping the save select screen from showing.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "InitSaveSlotUI")]
        static bool KillMenuPlusConnect(MainMenuController __instance)
        {
            // Deactivate the main menu's back button (also gets rid of the social media buttons that are there on the Steam version).
            __instance.m_frontEnd.transform.GetChild(1).transform.GetChild(6).gameObject.SetActive(false);

            // Deactivate the chapter select's back button.
            __instance.m_levelSelect.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.SetActive(false);

            // Stop the original function from running so we don't end up with a left over save select menu.
            return false;
        }

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
        /// Forces each chapter select button to run its DoToggleLocked function when the chapter select calls for chapter info.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChapterSelectPanel), "SetChapterInfo")]
        static void HandleChapterLocks(ref ScrollViewButton[] ___m_acScrollViewButtons)
        {
            foreach (ScrollViewButton button in ___m_acScrollViewButtons)
                button.DoToggleLocked();
        }

        /// <summary>
        /// Stops the game from even attempting to update the leaderboard data.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LeaderBoardDataHandler), "UpdateLeaderBoardHandler")]
        static bool DisableLeaderBoardData() => false;

        /// <summary>
        /// Disables the back button on the main menu.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "BackToSaveSlotSelect")]
        static bool DisableReturnToSaveSelect(ref bool ___m_bDoBackOutcome)
        {
            ___m_bDoBackOutcome = false;
            return false;
        }

        /// <summary>
        /// Disables the back button on the chapter select.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "BackToBegin")]
        static bool DisableReturnToMenu(ref bool ___m_bDoBackOutcome)
        {
            ___m_bDoBackOutcome = false;
            return false;
        }
    }
}
