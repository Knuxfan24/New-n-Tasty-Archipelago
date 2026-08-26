using Archipelago.MultiClient.Net.Models;
using BepInEx;
using NNT_Archipealgo.CustomData;
using NNT_Archipealgo.Patchers;
using System;
using System.IO;
using static JAWStateMachine;

namespace NNT_Archipealgo
{
    internal class Helpers
    {
        public static void HandleItem(KeyValuePair<ArchipelagoItem, int> item)
        {
            switch (item.Key.ItemName)
            {
                case "Levers": 
                    Plugin.save.HasLevers = true;
                    var levers = UnityEngine.GameObject.FindObjectsOfType<Lever>();
                    foreach (var lever in levers)
                        UnlockElement(lever.transform); 
                    break;

                case "Possession": 
                    Plugin.save.CanPosses = true;
                    var sligs = UnityEngine.GameObject.FindObjectsOfType<Slig>();
                    foreach (var slig in sligs)
                        UnlockElement(slig.transform);
                    break;

                case "Grenades": 
                    Plugin.save.HasGrenades = true;
                    var boomMachines = UnityEngine.GameObject.FindObjectsOfType<GrenadeDispenser>();
                    foreach (var boomMachine in boomMachines)
                        UnlockElement(boomMachine.transform);
                    break;

                case "Rocks": Plugin.save.HasRocks = true;
                    var rockSacks = UnityEngine.GameObject.FindObjectsOfType<RockBag>();
                    foreach (var rockSack in rockSacks)
                        if (!rockSack.name.Contains("meat"))
                        UnlockElement(rockSack.transform);
                    break;

                case "UXB Defusion": 
                    Plugin.save.CanDefuseUXBs = true;
                    var uxbs = UnityEngine.GameObject.FindObjectsOfType<ToggleMine>();
                    foreach (var uxb in uxbs)
                        UnlockElement(uxb.transform);
                    break;

                case "Lifts": 
                    Plugin.save.CanUseLifts = true;
                    var lifts = UnityEngine.GameObject.FindObjectsOfType<Elevator>();
                    foreach (var lift in lifts)
                        UnlockElement(lift.transform);
                    var cargoLifts = UnityEngine.GameObject.FindObjectsOfType<CargoElevator>();
                    foreach (var cargoLift in cargoLifts)
                        UnlockElement(cargoLift.transform);
                    break;

                case "Spirit Rings": 
                    Plugin.save.CanUseSpiritRings = true;
                    var natives = UnityEngine.GameObject.FindObjectsOfType<MudokonNative>();
                    foreach (var native in natives)
                        UnlockElement(native.transform);
                    break;

                case "Meat": 
                    Plugin.save.CanUseMeatSacks = true;
                    var meatSacks = UnityEngine.GameObject.FindObjectsOfType<RockBag>();
                    foreach (var meatSack in meatSacks)
                        if (meatSack.name.Contains("meat"))
                            UnlockElement(meatSack.transform); break;

                case "Shrykull": Plugin.save.CanUseShrykull = true;
                    var portals = UnityEngine.GameObject.FindObjectsOfType<Portal>();
                    foreach (var portal in portals)
                        UnlockElement(portal.transform);
                    break;

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

                case "Shock Trap": AbePatcher.SetTrapState(SMStates.AbeZap); break;
                case "Trip Trap": case "lol brawl reference": AbePatcher.SetTrapState(SMStates.AbeLandDamage); break;
                case "QuikSave Trap": App.getInstance().SaveQuickSave(); break;
                case "Drop Trap": AbePatcher.DropTrap(); break;

                // Unhandled items, throw an error into the console.
                default: Plugin.consoleLog.LogError($"Item Type '{item.Key.ItemName}' (sent by '{item.Key.Source}' {item.Value} time(s)) not yet handled!"); return;
            }

            static void UnlockElement(Transform obj)
            {
                // Loop through the provided object's children in search of an AP Indicator to kill.
                for (int childIndex = obj.childCount - 1; childIndex >= 0; childIndex--)
                {
                    if (obj.GetChild(childIndex).name == "AP Indicator")
                    {
                        GameObject.Destroy(obj.GetChild(childIndex).gameObject);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks if a location exists in the multiworld.
        /// </summary>
        public static bool CheckLocationExists(long locationIndex) => locationIndex != -1 && Plugin.session.Locations.AllLocations.Contains(locationIndex);

        /// <summary>
        /// Gets a location index by its name then passes it to the other CompleteLocationCheck function to do the actual check.
        /// </summary>
        public static void CompleteLocationCheck(string locationName) => CompleteLocationCheck(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", locationName));

        /// <summary>
        /// Completes a location check.
        /// </summary>
        public static void CompleteLocationCheck(long locationIndex)
        {
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

        /// <summary>
        /// Loads the specified file as a sprite.
        /// </summary>
        public static Sprite GetCustomSprite(string file, float pixelsPerUnit = 100f)
        {
            Texture2D texture = GetTexture();
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);

            Texture2D GetTexture()
            {
                // Set up a new texture using point filtering.
                Texture2D texture = new(32, 32) { filterMode = FilterMode.Point };

                // Read the sprite for this texture.
                texture.LoadImage(File.ReadAllBytes(file));

                // Return our custom texture.
                return texture;
            }
        }
    }
}
