using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using HarmonyLib;
using Newtonsoft.Json;
using NNT_Archipealgo.CustomData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static MainMenuController;

namespace NNT_Archipealgo.Patchers
{
    internal class TempBullshit
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "BeginToChapterSelect")]
        static bool Test(MainMenuController __instance)
        {
            return true;

            __instance.ToDebugLevelSelect();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuController), "InitSaveSlotUI")]
        static bool KillMenu(MainMenuController __instance)
        {
            if (Plugin.session == null)
            {
                Plugin.session = ArchipelagoSessionFactory.CreateSession(Plugin.configServerAddress.Value);
                LoginResult connectionResult = Plugin.session.TryConnectAndLogin("New 'n' Tasty", Plugin.configSlotName.Value, Archipelago.MultiClient.Net.Enums.ItemsHandlingFlags.AllItems, null, null, null, Plugin.configPassword.Value, true);

                // Get the success data.
                LoginSuccessful connectionSuccess = (LoginSuccessful)connectionResult;

                // Get the slot data.
                Plugin.slotData = connectionSuccess.SlotData;

                Plugin.DeathLink = Plugin.session.CreateDeathLinkService();
                Plugin.DeathLink.OnDeathLinkReceived += DeathLink_OnDeathLinkReceived;

                if ((long)Plugin.slotData["death_link"] != 0)
                    Plugin.DeathLink.EnableDeathLink();

                AbePatcher.deathLinkAmnesty = (int)(long)Plugin.slotData["death_link_amnesty"];

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
        static void Test2(object[] __args, ref FinishedFollowMeAction ___m_eFinishedFollowMeAction, ref bool ___m_selectingPlayers)
        {

            if ((FinishedFollowMeAction)__args[0] == FinishedFollowMeAction.NewGame)
            {
                ___m_eFinishedFollowMeAction = FinishedFollowMeAction.ToChapterSelect;
                ___m_selectingPlayers = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(App), "GetChapterStarted")]
        static bool HijackLocks(object[] __args, ref bool __result)
        {
            switch ((LevelList.Chapters)__args[0])
            {
                case LevelList.Chapters.RuptureFarms: __result = Plugin.save.UnlockedLocations[0]; return false;
                case LevelList.Chapters.StockyardEscape: __result = Plugin.save.UnlockedLocations[1]; return false;
                case LevelList.Chapters.ParamonianTemple: __result = Plugin.save.UnlockedLocations[2]; return false;
                case LevelList.Chapters.ScrabanianTemple: __result = Plugin.save.UnlockedLocations[3]; return false;
                case LevelList.Chapters.RescueZulag1: __result = Plugin.save.UnlockedLocations[4]; return false;
                case LevelList.Chapters.RescueZulag2: __result = Plugin.save.UnlockedLocations[5]; return false;
                case LevelList.Chapters.RescueZulag3: __result = Plugin.save.UnlockedLocations[6]; return false;
                case LevelList.Chapters.RescueZulag4: __result = Plugin.save.UnlockedLocations[7]; return false;
                case LevelList.Chapters.TheBoardroom: __result = Plugin.save.MudokonCount >= (long)Plugin.slotData["required_muds"]; return false;

                default:
                    Plugin.consoleLog.LogWarning($"Handling for {__args[0]} not implemented!"); break;
            }

            __result = false;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MenuElement), "RemoveFromButtonKeyChain")]
        static bool Test3()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChapterSelectPanel), "SetChapterInfo")]
        static void Test4(ref ScrollViewButton[] ___m_acScrollViewButtons)
        {
            foreach (var button in ___m_acScrollViewButtons)
                button.DoToggleLocked();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChapterSelectPanel), "Start")]
        static void Test5(ChapterSelectPanel __instance)
        {
            // isn't enough to stop the button from being interacted with on controller argh
            __instance.transform.GetChild(1).GetChild(0).GetChild(2).gameObject.SetActive(false);
        }


        private static void DeathLink_OnDeathLinkReceived(DeathLink deathLink)
        {
            // Set up the message showing our DeathLink source.
            string notifyMessage = $"DeathLink received from {deathLink.Source}";

            // Present the cause and source of the DeathLink.
            if (deathLink.Cause != null)
                notifyMessage = $"{deathLink.Cause}";

            Plugin.consoleLog.LogInfo(notifyMessage);
            AbePatcher.hasBufferedDeathLink = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(App), "PlayMovie", new Type[] { typeof(Movie.eMovies), typeof(bool), typeof(bool) })]
        static void SendGoal(object[] __args)
        {
            if ((bool)__args[2]) return;

            if ((Movie.eMovies)__args[0] == Movie.eMovies.BadEnding)
                __args[0] = Movie.eMovies.GoodEnding;

            if ((Movie.eMovies)__args[0] == Movie.eMovies.GoodEnding)
            {
                StatusUpdatePacket goalPacket = new() { Status = ArchipelagoClientState.ClientGoal };
                Plugin.session.Socket.SendPacketAsync(goalPacket);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LeaderBoardDataHandler), "UpdateLeaderBoardHandler")]
        static bool NoLeaderboardPlz()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(App), "CompletedChapter")]
        static void TestIDK()
        {
            Plugin.consoleLog.LogInfo("TODO: Make this return to chapter select plz.");
        }
    }
}
