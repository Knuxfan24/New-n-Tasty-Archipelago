using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using System.Collections.ObjectModel;
using System.IO;
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
            // Check that we're not already connected to the server.
            if (Plugin.session == null)
            {
                // Create our session and attempt to connect with our config settings.
                Plugin.session = ArchipelagoSessionFactory.CreateSession(Plugin.configServerAddress.Value);
                LoginResult connectionResult = Plugin.session.TryConnectAndLogin("New 'n' Tasty", Plugin.configSlotName.Value, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, null, null, null, Plugin.configPassword.Value, true);

                // Check if the connection failed.
                if (!connectionResult.Successful)
                {
                    // Get the failure data.
                    LoginFailure connectionFailure = (LoginFailure)connectionResult;

                    // Create our error message and push it to the string queue.
                    string errorMessage = $"Failed to Connect to {Plugin.configServerAddress.Value} as {Plugin.configSlotName.Value} with password {Plugin.configPassword.Value}:";
                    foreach (string error in connectionFailure.Errors)
                        errorMessage += $"\n{error}";
                    foreach (ConnectionRefusedError error in connectionFailure.ErrorCodes)
                        errorMessage += $"\n{error}";
                    errorMessage += $"\n\nCheck your config settings and restart the game.";
                    Plugin.infoStringQueue.Add(errorMessage);

                    // Stop running the rest of the function, leaving us on a blank menu screen.
                    return false;
                }

                // Get the success data.
                LoginSuccessful connectionSuccess = (LoginSuccessful)connectionResult;

                // Push our connected message to the string queue.
                Plugin.infoStringQueue.Add($"Connected to {Plugin.configServerAddress.Value} as {Plugin.configSlotName.Value}");

                // Get the slot data and debug print it.
                Plugin.slotData = connectionSuccess.SlotData;
                foreach (var key in Plugin.slotData)
                    Plugin.consoleLog.LogDebug($"{key.Key}: {key.Value} (Type: {key.Value.GetType()})");

                // Create and setup DeathLink stuff, enabling it if needed.
                Plugin.DeathLink = Plugin.session.CreateDeathLinkService();
                Plugin.DeathLink.OnDeathLinkReceived += SocketEvents.Socket_ReceiveDeathLink;
                if ((long)Plugin.slotData["death_link"] != 0)
                    Plugin.DeathLink.EnableDeathLink();
                AbePatcher.deathLinkAmnesty = (int)(long)Plugin.slotData["death_link_amnesty"];

                // Add the RingLink tag if its enabled in our slot data.
                if ((long)Plugin.slotData["ring_link"] != 0)
                    Plugin.session.ConnectionInfo.UpdateConnectionOptions([.. Plugin.session.ConnectionInfo.Tags, .. new string[1] { "RingLink" }]);

                // Create the handler for item receives.
                Plugin.session.Items.ItemReceived += SocketEvents.Socket_ReceiveItem;

                // Start the item queue timer.
                Plugin.itemQueueTimer = 1f;

                // Loop through and handle each item that has previously been received.
                foreach (ItemInfo item in Plugin.session.Items.AllItemsReceived)
                {
                    SocketEvents.SetUpQueue(item);
                    Plugin.session.Items.DequeueItem();
                }

                // Set up the handler to update the remaining locations count on the Status Boards.
                Plugin.session.Locations.CheckedLocationsUpdated += SocketEvents.Socket_UpdateRemainingLocationsCount;

                // Create our internal Archipelago save.
                Plugin.save = new()
                {
                    RemainingLocations = Plugin.session.Locations.AllLocations.Count - Plugin.session.Locations.AllLocationsChecked.Count
                };

                // Fetch all the locations.
                ReadOnlyCollection<long> locations = Plugin.session.Locations.AllLocations;
                Plugin.session.Locations.ScoutLocationsAsync(items =>
                {
                    Plugin.save.items = items;
                },
                false, [.. locations]);
            }

            // Delete save slot 24 if it exists.
            // The GOG version doesn't even have the Steam Manager, so we specifically need to compile it out.
            #if !GOG
            if (File.Exists($"SaveGame/{SteamManager.GetInstance.GetAccountID()}/SaveSlot24.NnT"))
                    File.Delete($"SaveGame/{SteamManager.GetInstance.GetAccountID()}/SaveSlot24.NnT");
            #else
            if (File.Exists($"SaveGame/SaveSlot24.NnT"))
                File.Delete($"SaveGame/SaveSlot24.NnT");
            #endif

            // Load the now nonexistant save slot 24.
            App.getInstance().LoadSaveFile(24);

            // Force the menu controller to move to the main menu.
            __instance.SaveSlotToFrontEnd();

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
        /// Removes the Back button from the Chapter Select.
        /// TODO: The button still reacts to controller input.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChapterSelectPanel), "Start")]
        static void RemoveChapterSelectBackButton(ChapterSelectPanel __instance)
        {
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
