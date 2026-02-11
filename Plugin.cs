using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using NNT_Archipealgo.CustomData;
using NNT_Archipealgo.Patchers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NNT_Archipealgo
{
    [BepInPlugin("K24_NNT_Archipelago", "Archipelago", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        // Logger.
        public static ManualLogSource consoleLog;

        public static ArchipelagoSession session;
        public static Dictionary<string, object> slotData;
        public static DeathLinkService DeathLink;

        public static ArchipelagoSave save;

        public static Dictionary<ArchipelagoItem, int> itemQueue = [];
        public static float itemQueueTimer = -1;

        private void Awake()
        {
            // Set up the logger.
            consoleLog = Logger;

            // Patch all the functions that need patching.
            Harmony.CreateAndPatchAll(typeof(AbePatcher));
            Harmony.CreateAndPatchAll(typeof(DisableAbilities));
            Harmony.CreateAndPatchAll(typeof(PortalPatcher));
            Harmony.CreateAndPatchAll(typeof(StatusBoardPatcher));
            Harmony.CreateAndPatchAll(typeof(TrialZulagLocationSender));

            Harmony.CreateAndPatchAll(typeof(TempBullshit));
        }

        private void Start()
        {
            StartCoroutine(ItemQueueLoop());
        }

        private IEnumerator ItemQueueLoop()
        {
            var tick = new WaitForSeconds(0.25f);

            while (Application.isPlaying)
            {
                if (itemQueueTimer != -1)
                {
                    if (itemQueue.Count != 0)
                    {
                        KeyValuePair<ArchipelagoItem, int> item = itemQueue.ElementAt(0);
                        Helpers.HandleItem(item);
                        itemQueue.Remove(item.Key);

                        string selfName = session != null ? session.Players.GetPlayerName(session.ConnectionInfo.Slot) : "";
                        string message = $"Recieved {item.Key.ItemName} from {item.Key.Source}.";
                        if (item.Key.Source == selfName) message = $"Found your {item.Key.ItemName}.";
                        if (item.Value > 1) message = $"Recieved {item.Key.ItemName} ({item.Value}x) from {item.Key.Source}.";
                        if (item.Key.Source == selfName && item.Value > 1) message = $"Found your {item.Key.ItemName} ({item.Value}x).";
                        Plugin.consoleLog.LogDebug(message);
                    }

                    // Maintain similar semantics to original timer.
                    itemQueueTimer = Math.Max(0f, itemQueueTimer - 0.25f);
                }

                yield return tick;
            }
        }
    }
}
