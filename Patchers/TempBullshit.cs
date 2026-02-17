using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Models;
using HarmonyLib;
using System.IO;
using static MainMenuController;

namespace NNT_Archipealgo.Patchers
{
    internal class TempBullshit
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "InitSaveSlotUI")]
        static bool KillMenuPlusConnect(MainMenuController __instance)
        {
            if (Plugin.session == null)
            {
                Plugin.session = ArchipelagoSessionFactory.CreateSession(Plugin.configServerAddress.Value);
                LoginResult connectionResult = Plugin.session.TryConnectAndLogin("New 'n' Tasty", Plugin.configSlotName.Value, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, null, null, null, Plugin.configPassword.Value, true);

                // Get the success data.
                LoginSuccessful connectionSuccess = (LoginSuccessful)connectionResult;

                Plugin.infoStringQueue.Add($"Connected to {Plugin.configServerAddress.Value} as {Plugin.configSlotName.Value}");

                // Get the slot data.
                Plugin.slotData = connectionSuccess.SlotData;

                Plugin.DeathLink = Plugin.session.CreateDeathLinkService();
                Plugin.DeathLink.OnDeathLinkReceived += SocketEvents.Socket_ReceiveDeathLink;

                if ((long)Plugin.slotData["death_link"] != 0)
                    Plugin.DeathLink.EnableDeathLink();

                AbePatcher.deathLinkAmnesty = (int)(long)Plugin.slotData["death_link_amnesty"];

                // Add the RingLink tag if its enabled in our slot data.
                if ((long)Plugin.slotData["ring_link"] != 0)
                    Plugin.session.ConnectionInfo.UpdateConnectionOptions([.. Plugin.session.ConnectionInfo.Tags, .. new string[1] { "RingLink" }]);

                Plugin.session.Items.ItemReceived += SocketEvents.Socket_ReceiveItem;
                Plugin.itemQueueTimer = 1f;
                foreach (ItemInfo item in Plugin.session.Items.AllItemsReceived)
                {
                    SocketEvents.SetUpQueue(item);
                    Plugin.session.Items.DequeueItem();
                }
                Plugin.session.Locations.CheckedLocationsUpdated += SocketEvents.Socket_UpdateRemainingLocationsCount;

                Plugin.save = new();
                Plugin.save.RemainingLocations = Plugin.session.Locations.AllLocations.Count - Plugin.session.Locations.AllLocationsChecked.Count;

                // Fetch all the locations.
                var locations = Plugin.session.Locations.AllLocations;
                Plugin.session.Locations.ScoutLocationsAsync(items =>
                {
                    Plugin.save.items = items;
                },
                false, [.. locations]);

                // Print all the slot data to the log, as a debug log.
                foreach (var key in Plugin.slotData)
                    Plugin.consoleLog.LogDebug($"{key.Key}: {key.Value} (Type: {key.Value.GetType()})");

            }


            if (File.Exists($"SaveGame/{SteamManager.GetInstance.GetAccountID()}/SaveSlot24.NnT"))
                File.Delete($"SaveGame/{SteamManager.GetInstance.GetAccountID()}/SaveSlot24.NnT");

            App.getInstance().LoadSaveFile(24);
            __instance.SaveSlotToFrontEnd();

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "StartFollowMe")]
        static void RedirectToChapterSelect(object[] __args, ref FinishedFollowMeAction ___m_eFinishedFollowMeAction, ref bool ___m_selectingPlayers)
        {

            if ((FinishedFollowMeAction)__args[0] == FinishedFollowMeAction.NewGame)
            {
                ___m_eFinishedFollowMeAction = FinishedFollowMeAction.ToChapterSelect;
                ___m_selectingPlayers = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuElement), "RemoveFromButtonKeyChain")]
        static bool DisableButtonRemovalFromList() => false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChapterSelectPanel), "SetChapterInfo")]
        static void HandleChapterLocks(ref ScrollViewButton[] ___m_acScrollViewButtons)
        {
            foreach (var button in ___m_acScrollViewButtons)
                button.DoToggleLocked();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChapterSelectPanel), "Start")]
        static void RemoveChapterSelectBackButton(ChapterSelectPanel __instance)
        {
            // isn't enough to stop the button from being interacted with on controller argh
            __instance.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.SetActive(false);
        }

        /// <summary>
        /// Stops the game from even attempting to update the leaderboard data.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LeaderBoardDataHandler), "UpdateLeaderBoardHandler")]
        static bool DisableLeaderBoardData() => false;
    }
}
