// TODO: Clean this up, the thread stuff is basically just copied 1 to 1 from Freedom Planet 2.
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using NNT_Archipealgo.CustomData;
using NNT_Archipealgo.Patchers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace NNT_Archipealgo
{
    [BepInPlugin("K24_NNT_Archipelago", "Archipelago", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        // Logger.
        public static ManualLogSource consoleLog;

        public static ConfigEntry<string> configServerAddress;
        public static ConfigEntry<string> configSlotName;
        public static ConfigEntry<string> configPassword;

        public static ArchipelagoSession session;
        public static Dictionary<string, object> slotData;
        public static DeathLinkService DeathLink;

        public static ArchipelagoSave save;

        public static Dictionary<ArchipelagoItem, int> itemQueue = [];
        public static float itemQueueTimer = -1;

        public static string infoString = string.Empty;
        public static List<string> infoStringQueue = [];
        public static float infoTimer = 0f;

        public static System.Random rng = new();

        private static readonly Queue<LocationData> LocationQueue = new();
        private static readonly AutoResetEvent LocationSignal = new(false);
        private static Thread locationThread;

        // Background bounce-packet sender to keep SendPacket off the main thread.
        private static readonly Queue<BouncePacket> BounceQueue = new();
        private static readonly AutoResetEvent BounceSignal = new(false);
        private static Thread bounceThread;

        // RingLink based values.
        public static int RingLinkMudokonCount = 0;

        private void Awake()
        {
            // Set up the logger.
            consoleLog = Logger;

            // Get the config options.
            configServerAddress = Config.Bind("Connection",
                                              "Server Address",
                                              "archipelago.gg:",
                                              "The server address that was last connected to.");

            configSlotName = Config.Bind("Connection",
                                         "Slot Name",
                                         "New 'n' Tasty",
                                         "The name of the last slot that was connected to.");

            configPassword = Config.Bind("Connection",
                                         "Password",
                                         "",
                                         "The password that was used for the last session connected to.");

            // Patch all the functions that need patching.
            Harmony.CreateAndPatchAll(typeof(AbePatcher));
            Harmony.CreateAndPatchAll(typeof(AppPatcher));
            Harmony.CreateAndPatchAll(typeof(DisableAbilities));
            Harmony.CreateAndPatchAll(typeof(MainMenuPatcher));
            Harmony.CreateAndPatchAll(typeof(MudokonSlavePatcher));
            Harmony.CreateAndPatchAll(typeof(PortalPatcher));
            Harmony.CreateAndPatchAll(typeof(StatusBoardPatcher));
            Harmony.CreateAndPatchAll(typeof(TrialZulagLocationSender));
        }

        private void Start()
        {
            StartCoroutine(ItemQueueLoop());
            StartCoroutine(RingLinkLoop());

            if (locationThread == null)
            {
                locationThread = new Thread(LocationSenderLoop) { IsBackground = true, Name = "AP Location Sender" };
                locationThread.Start();
            }
            // Start background sender for bounce packets to avoid blocking the main thread.
            if (bounceThread == null)
            {
                bounceThread = new Thread(BounceSenderLoop) { IsBackground = true, Name = "AP Bounce Sender" };
                bounceThread.Start();
            }
        }

        private void OnGUI()
        {
            GUIStyle textStyle = new()
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerCenter
            };

            textStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(2, 2, Screen.width, Screen.height), infoString, textStyle);
            textStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), infoString, textStyle);
        }

        private void Update()
        {
            infoTimer -= Time.deltaTime;

            if (infoTimer <= 0)
            {
                if (infoStringQueue.Count > 0)
                {
                    infoString = infoStringQueue[0];
                    infoStringQueue.RemoveAt(0);
                    infoTimer = 3f;
                }
                else
                {
                    infoString = string.Empty;
                    infoTimer = 0;
                }
            }
        }

        public static void EnqueueLocation(long locationIndex)
        {
            lock (LocationQueue)
            {
                LocationQueue.Enqueue(new LocationData
                {
                    LocationIndex = locationIndex
                });
                LocationSignal.Set();
            }
        }

        private static void LocationSenderLoop()
        {
            while (Application.isPlaying)
            {
                LocationSignal.WaitOne();
                while (Application.isPlaying)
                {
                    LocationData location = null;
                    lock (LocationQueue)
                    {
                        if (LocationQueue.Count > 0)
                            location = LocationQueue.Dequeue();
                        else
                            break;
                    }

                    try
                    {
                        if (session != null && session.Socket != null && location != null)
                        {
                            session.Locations.CompleteLocationChecks(location.LocationIndex);
                            session.Locations.ScoutLocationsAsync(_ => { }, location.LocationIndex);
                        }

                    }
                    catch (Exception ex)
                    {
                        consoleLog?.LogWarning($"Location send failed: {ex.Message}");
                    }
                }
            }
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
                        Plugin.infoStringQueue.Add(message);
                    }

                    // Maintain similar semantics to original timer.
                    itemQueueTimer = Math.Max(0f, itemQueueTimer - 0.25f);
                }

                yield return tick;
            }
        }
        private IEnumerator RingLinkLoop()
        {
            var tick = new WaitForSeconds(0.25f);
            while (Application.isPlaying)
            {
                if (RingLinkMudokonCount != 0 && session != null)
                {
                    BouncePacket packet = new()
                    {
                        Tags = ["RingLink"],
                        Data = new()
                        {
                            { "time", (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds },
                            { "source", session.ConnectionInfo.Slot },
                            { "amount", RingLinkMudokonCount }
                        }
                    };

                    // Enqueue to background sender to avoid main-thread blocking.
                    EnqueueBounce(packet);
                    RingLinkMudokonCount = 0;
                }
                yield return tick;
            }
        }

        public static void EnqueueBounce(BouncePacket packet)
        {
            lock (BounceQueue)
            {
                BounceQueue.Enqueue(packet);
                BounceSignal.Set();
            }
        }

        private static void BounceSenderLoop()
        {
            while (Application.isPlaying)
            {
                BounceSignal.WaitOne();
                while (Application.isPlaying)
                {
                    BouncePacket packet = null;
                    lock (BounceQueue)
                    {
                        if (BounceQueue.Count > 0)
                            packet = BounceQueue.Dequeue();
                        else
                            break;
                    }

                    try
                    {
                        if (session != null && session.Socket != null && packet != null)
                            session.Socket.SendPacket(packet);
                    }
                    catch (Exception ex)
                    {
                        consoleLog?.LogWarning($"Bounce send failed: {ex.Message}");
                    }
                }
            }
        }
    }
}
