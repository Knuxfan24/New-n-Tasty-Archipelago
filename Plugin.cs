// TODO: Clean this up, the thread stuff is basically just copied 1 to 1 from Freedom Planet 2.
// Padlock Sprite sourced from: https://karsiori.itch.io/pixel-art-padlock-pack-animated
// DVD Screensaver Logo sourced from: https://commons.wikimedia.org/wiki/File:DVD_logo.svg
global using HarmonyLib;
global using System.Collections.Generic;
global using UnityEngine;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using NNT_Archipealgo.CustomData;
using NNT_Archipealgo.Patchers;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using Color = UnityEngine.Color;

namespace NNT_Archipealgo
{
    [BepInPlugin("K24_NNT_Archipelago", "Archipelago", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        // Logger.
        public static ManualLogSource consoleLog;

        // The config options.
        public static ConfigEntry<string> configServerAddress;
        public static ConfigEntry<string> configSlotName;
        public static ConfigEntry<string> configPassword;
        public static ConfigEntry<bool> configJokeSigns;
        public static ConfigEntry<bool> configJokeSignsTutorials;

        // The AP session's data.
        public static ArchipelagoSession session;
        public static Dictionary<string, object> slotData;
        public static DeathLinkService DeathLink;

        // The AP's save.
        public static ArchipelagoSave save;

        // The item queue and its timer.
        public static Dictionary<ArchipelagoItem, int> itemQueue = [];
        public static float itemQueueTimer = -1;

        // The message queue's values.
        public static string infoString = string.Empty;
        public static List<string> infoStringQueue = [];
        public static float infoTimer = 0f;

        // Random Number Generator.
        public static System.Random rng = new();

        // Background bounce-packet sender to keep SendPacket off the main thread.
        private static readonly Queue<LocationData> LocationQueue = new();
        private static readonly AutoResetEvent LocationSignal = new(false);
        private static Thread locationThread;

        private static readonly Queue<BouncePacket> BounceQueue = new();
        private static readonly AutoResetEvent BounceSignal = new(false);
        private static Thread bounceThread;

        // RingLink based values.
        public static int RingLinkMudokonCount = 0;

        // DVD Screensaver Trap values.
        public static float DVDTimer = 0;
        public static float DVDXPos = (Screen.width / 2) - 128;
        public static float DVDYPos = (Screen.height / 2) - 128;
        public static bool DVDRight = true;
        public static bool DVDUp = true;
        public static Color DVDColour = Color.white;

        private void Awake()
        {
            // Set up the logger.
            consoleLog = Logger;

            // Get the config options.
            configServerAddress = Config.Bind("Connection",
                                              "Server Address",
                                              "archipelago.gg:",
                                              "The server address to connect to.");

            configSlotName = Config.Bind("Connection",
                                         "Slot Name",
                                         "New 'n' Tasty",
                                         "The slot to connect to.");

            configPassword = Config.Bind("Connection",
                                         "Password",
                                         "",
                                         "The password for the server we're connecting to (leave blank if no password is required).");

            configJokeSigns = Config.Bind("Joke Signs",
                                          "Enabled",
                                          true,
                                          "Replaces the scrolling signs in Rupture Farms and the fireflies with jokes referencing memes and other games.");

            configJokeSignsTutorials = Config.Bind("Joke Signs",
                                                   "Include Tutorial Signs",
                                                   false,
                                                   "Overwrites the tutorial signs with jokes as well.");

            // Patch all the functions that need patching.
            Harmony.CreateAndPatchAll(typeof(AbePatcher));
            Harmony.CreateAndPatchAll(typeof(AppPatcher));
            Harmony.CreateAndPatchAll(typeof(MainMenuPatcher));
            Harmony.CreateAndPatchAll(typeof(MudokonListPatcher));
            Harmony.CreateAndPatchAll(typeof(MudokonSlavePatcher));
            Harmony.CreateAndPatchAll(typeof(PortalPatcher));
            Harmony.CreateAndPatchAll(typeof(StatusBoardPatcher));
            Harmony.CreateAndPatchAll(typeof(TrialZulagLocationSender));

            Harmony.CreateAndPatchAll(typeof(DisableAbilities));
            Harmony.CreateAndPatchAll(typeof(JokeMessages));
            Harmony.CreateAndPatchAll(typeof(LockedSprites));
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
            // If the DVD Screensaver Trap timer is above 0, then set the GUI colour to the random logo colour and draw the icon.
            if (DVDTimer > 0)
            {
                GUI.color = DVDColour;
                GUI.DrawTexture(new Rect(DVDXPos, DVDYPos, 256f * ((float)Screen.width / (float)1280), 256f * ((float)Screen.height / (float)720)), Helpers.GetTexture($@"{Paths.GameRootPath}\mod_overrides\Archipelago\dvd_screensaver.png"), ScaleMode.ScaleToFit, true, 0f);
            }

            // Reset the GUI colour to white to not interfere with the other UI elements.
            GUI.color = Color.white;

            // Check if we're not connected to an AP session.
            if (session == null)
            {
                // Find the main menu, aborting if we haven't found it (can't use the SceneManager, so can't just check for the Front_End scene...)
                MainMenuController menu = UnityEngine.GameObject.FindObjectOfType<MainMenuController>();
                if (menu == null)
                    return;

                // Create the GUIStyle for the labels.
                GUIStyle labelStyle = new(GUI.skin.label)
                {
                    fontSize = (int)(32.0f * ((float)Screen.width / (float)1280)),
                    fontStyle = FontStyle.Bold
                };

                // Draw the labels, including their shadows which are offset by 2 pixels.
                Vector2 textPosition = new(Screen.width / 160, Screen.height / 90);
                labelStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(textPosition.x + (2 * Screen.width / 1280), textPosition.y + (2 * Screen.height / 720), Screen.width - ((Screen.width / 40) * 2), Screen.height / 18), "Server:", labelStyle);
                labelStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(textPosition.x, textPosition.y, Screen.width - ((Screen.width / 40) * 2), Screen.height / 18), "Server:", labelStyle);

                textPosition = new(Screen.width / 160, Screen.height / 15);
                labelStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(textPosition.x + (2 * Screen.width / 1280), textPosition.y + (2 * Screen.height / 720), Screen.width - ((Screen.width / 40) * 2), Screen.height / 18), "Slot:", labelStyle);
                labelStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(textPosition.x, textPosition.y, Screen.width - ((Screen.width / 40) * 2), Screen.height / 18), "Slot:", labelStyle);

                textPosition = new(Screen.width / 160, Screen.height / 8.2f);
                labelStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(textPosition.x + (2 * Screen.width / 1280), textPosition.y + (2 * Screen.height / 720), Screen.width - ((Screen.width / 40) * 2), Screen.height / 18), "Password:", labelStyle);
                labelStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(textPosition.x, textPosition.y, Screen.width - ((Screen.width / 40) * 2), Screen.height / 18), "Password:", labelStyle);

                // Create the GUIStyle for the text boxes.
                GUIStyle textBoxStyle = new(GUI.skin.textField)
                {
                    fontSize = (int)(32.0f * ((float)Screen.width / (float)1280))
                };

                // Draw the text boxes and link them to the config values.
                configServerAddress.Value = GUI.TextField(new Rect(Screen.width / 7, Screen.height / 90, (Screen.width / 2.35f) * 2, Screen.height / 18), configServerAddress.Value, textBoxStyle);
                configSlotName.Value = GUI.TextField(new Rect(Screen.width / 7, Screen.height / 15, (Screen.width / 2.35f) * 2, Screen.height / 18), configSlotName.Value, textBoxStyle);
                configPassword.Value = GUI.TextField(new Rect(Screen.width / 7, Screen.height / 8.2f, (Screen.width / 2.35f) * 2, Screen.height / 18), configPassword.Value, textBoxStyle);

                // Create the GUIStyle for the connect button..
                GUIStyle buttonStyle = new(GUI.skin.button)
                {
                    fontSize = (int)(32.0 * (Screen.width / 1280)),
                    fontStyle = FontStyle.Bold,
                };

                // Draw the connect button and check for its click.
                if (GUI.Button(new(Screen.width / 160, Screen.height - (Screen.height / 12.86f), Screen.width - ((Screen.width / 160) * 2), (Screen.height / 15)), "Connect", buttonStyle))
                {
                    // Create our session and attempt to connect with our config settings.
                    session = ArchipelagoSessionFactory.CreateSession(Plugin.configServerAddress.Value);
                    LoginResult connectionResult = session.TryConnectAndLogin("New 'n' Tasty", Plugin.configSlotName.Value, ItemsHandlingFlags.AllItems, null, null, null, Plugin.configPassword.Value, true);

                    // Check if the connection failed.
                    if (!connectionResult.Successful)
                    {
                        // Get the failure data.
                        LoginFailure connectionFailure = (LoginFailure)connectionResult;

                        // Create our error message and push it to the string queue.
                        string errorMessage = $"Failed to connect to {configServerAddress.Value} as {configSlotName.Value} with password {configPassword.Value}:";
                        foreach (string error in connectionFailure.Errors)
                            errorMessage += $"\n{error}";
                        foreach (ConnectionRefusedError error in connectionFailure.ErrorCodes)
                            errorMessage += $"\n{error}";
                        errorMessage += $"\n\nCheck your connection settings.";
                        infoStringQueue.Add(errorMessage);

                        // Null out the session and stop here.
                        session = null;
                        return;
                    }

                    // Get the success data.
                    LoginSuccessful connectionSuccess = (LoginSuccessful)connectionResult;

                    // Push our connected message to the string queue.
                    infoStringQueue.Add($"Connected to {configServerAddress.Value} as {configSlotName.Value}");

                    // Get the slot data (add the genocide entry if its missing for some semblence of backwards compatibility) and debug print it.
                    slotData = connectionSuccess.SlotData;
                    if (!slotData.ContainsKey("genocide")) slotData.Add("genocide", 0L);
                    foreach (var key in slotData)
                        consoleLog.LogDebug($"{key.Key}: {key.Value} (Type: {key.Value.GetType()})");

                    // Create and setup DeathLink stuff, enabling it if needed.
                    DeathLink = session.CreateDeathLinkService();
                    DeathLink.OnDeathLinkReceived += SocketEvents.Socket_ReceiveDeathLink;
                    if ((long)slotData["death_link"] != 0)
                        DeathLink.EnableDeathLink();
                    AbePatcher.deathLinkAmnesty = (int)(long)slotData["death_link_amnesty"];

                    // Add the RingLink tag if its enabled in our slot data.
                    if ((long)slotData["ring_link"] != 0)
                        session.ConnectionInfo.UpdateConnectionOptions([.. session.ConnectionInfo.Tags, .. new string[1] { "RingLink" }]);

                    // Create the handler for item receives.
                    session.Items.ItemReceived += SocketEvents.Socket_ReceiveItem;

                    // Start the item queue timer.
                    itemQueueTimer = 1f;

                    // Loop through and handle each item that has previously been received.
                    foreach (ItemInfo item in session.Items.AllItemsReceived)
                    {
                        SocketEvents.SetUpQueue(item);
                        session.Items.DequeueItem();
                    }

                    // Reset the DVD Screensaver Trap timer so we don't have a long lingering one from previously received copies of the trap.
                    DVDTimer = 0;

                    // Set up the handler to update the remaining locations count on the Status Boards.
                    session.Locations.CheckedLocationsUpdated += SocketEvents.Socket_UpdateRemainingLocationsCount;

                    // Create our internal Archipelago save.
                    save = new()
                    {
                        RemainingLocations = session.Locations.AllLocations.Count - session.Locations.AllLocationsChecked.Count
                    };

                    // Fetch all the locations.
                    ReadOnlyCollection<long> locations = session.Locations.AllLocations;
                    session.Locations.ScoutLocationsAsync(items =>
                    {
                        save.items = items;
                    },
                    false, [.. locations]);

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

                    // Force the menu to advance to the main menu.
                    menu.SaveSlotToFrontEnd();
                }
            }

            // Create the text style.
            GUIStyle notificationLogStyle = new()
            {
                fontSize = (int)(32.0 * (Screen.width / 1280)),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.LowerCenter
            };

            // Set the style's colour to black and write the info string with a 2 pixel offset to create a drop shadow.
            notificationLogStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(2, 2, Screen.width, Screen.height), infoString, notificationLogStyle);

            // Set the style's colour back to white and write the info string without an offset.
            notificationLogStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), infoString, notificationLogStyle);
        }

        private void Update()
        {
            static void GenerateColour() => DVDColour = new((float)rng.NextDouble(), (float)rng.NextDouble(), (float)rng.NextDouble());

            // Check if the DVD Screensaver is active and decrement its timer if so.
            if (DVDTimer > 0)
                DVDTimer -= Time.deltaTime;

            // Bounce the DVD logo off edges and picking a new colour with each bounce.
            if (DVDXPos >= Screen.width - (256 * ((float)Screen.width / (float)1280))) { DVDRight = false; GenerateColour(); }
            if (DVDXPos <= 0) { DVDRight = true; GenerateColour(); }
            if (DVDYPos >= Screen.height - (184 * ((float)Screen.height / (float)720))) { DVDUp = true; GenerateColour(); }
            if (DVDYPos <= (-72 * ((float)Screen.height / (float)720))) { DVDUp = false; GenerateColour(); }

            // Move the DVD logo based on its Right and Up values.
            if (DVDRight) DVDXPos += 8 * (Screen.width / 1280);
            else DVDXPos -= 8 * (Screen.width / 1280);
            if (DVDUp) DVDYPos -= 8 * (Screen.height / 720);
            else DVDYPos += 8 * (Screen.height / 720);

            // Decrement the timer by the game's delta time.
            infoTimer -= Time.deltaTime;

            // Check if the timer has reached or gone below 0.
            if (infoTimer <= 0)
            {
                // Check that we have any messages queued.
                if (infoStringQueue.Count > 0)
                {
                    // Get the first queued message and set info string to it.
                    infoString = infoStringQueue[0];

                    // Remove the first message from the queue.
                    infoStringQueue.RemoveAt(0);

                    // Set the timer back to 3 seconds.
                    infoTimer = 3f;
                }

                // If we don't have any messages queued then blank out the string and keep the timer at 0.
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
                        consoleLog?.LogError($"Location send failed: {ex.Message}");
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
                        consoleLog?.LogError($"Bounce send failed: {ex.Message}");
                    }
                }
            }
        }
    }
}
