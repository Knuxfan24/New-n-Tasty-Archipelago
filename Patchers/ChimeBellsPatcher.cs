namespace NNT_Archipealgo.Patchers
{
    internal class ChimeBellsPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ChimeBells), "Possessed")]
        static void BellSongItems(ChimeBells __instance)
        {
            // Only do this if we're using the Bell Song option.
            if ((long)Plugin.slotData["bell_song_items"] == 0)
                return;

            // Find all the Flint Locks in the scene (stupid workaround to not being able to use the SceneManager to get the scene name).
            FlintLock[] flintlockObjects = Resources.FindObjectsOfTypeAll<FlintLock>();
            foreach (FlintLock flintlock in flintlockObjects)
            {
                // Set the unlocked value for this set of bells to the approriate value in the save array.
                switch (flintlock.TrialValue)
                {
                    case FlintLock.Trial.paraTrial1: __instance.unlocked = Plugin.save.ParamoniaBellSongs[0]; return;
                    case FlintLock.Trial.paraTrial2: __instance.unlocked = Plugin.save.ParamoniaBellSongs[1]; return;
                    case FlintLock.Trial.paraTrial3: __instance.unlocked = Plugin.save.ParamoniaBellSongs[2]; return;
                    case FlintLock.Trial.paraTrial4: __instance.unlocked = Plugin.save.ParamoniaBellSongs[3]; return;
                    case FlintLock.Trial.paraTrial5: __instance.unlocked = Plugin.save.ParamoniaBellSongs[4]; return;
                    case FlintLock.Trial.paraTrial6: __instance.unlocked = Plugin.save.ParamoniaBellSongs[5]; return;

                    case FlintLock.Trial.scrabTrial1: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[0]; return;
                    case FlintLock.Trial.scrabTrial2: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[1]; return;
                    case FlintLock.Trial.scrabTrial3: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[2]; return;
                    case FlintLock.Trial.scrabTrial4: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[3]; return;
                    case FlintLock.Trial.scrabTrial5: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[4]; return;
                    case FlintLock.Trial.scrabTrial6: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[5]; return;
                    case FlintLock.Trial.scrabTrial7: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[6]; return;
                    case FlintLock.Trial.scrabTrial8: __instance.unlocked = Plugin.save.ScrabaniaBellSongs[7]; return;

                    default: Plugin.consoleLog.LogError($"Flint Lock type {flintlock.TrialValue} not handled for Bell Song Items! Acting as if the option isn't enabled."); return;
                }
            }

            // Assume this is in the Monsaic Lines and check that value.
            // TODO: Make sure this doesn't blow up in my face horribly.
            __instance.unlocked = Plugin.save.MonsaicLinesBellSong;
        }
    }
}
