using static DirectoryStoryStone;

namespace NNT_Archipealgo.Patchers
{
    internal class DirectoryStoryStonePatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DirectoryStoryStone), "DirectoryStoryStoneActivate")]
        static void BellSongLocations(ref DirectoryType ___m_iDirectoryType)
        {
            // Only do this if we're using the Bell Song option and this is a Bell Song Stone.
            if ((long)Plugin.slotData["bell_song_items"] == 0 || ___m_iDirectoryType != DirectoryType.Password_ChimeBells)
                return;

            // Find all the Flint Locks in the scene (stupid workaround to not being able to use the SceneManager to get the scene name).
            FlintLock[] flintlockObjects = Resources.FindObjectsOfTypeAll<FlintLock>();
            foreach (FlintLock flintlock in flintlockObjects)
            {
                // Send the location for the approriate trial's bell song stone.
                switch (flintlock.TrialValue)
                {
                    case FlintLock.Trial.paraTrial1: Helpers.CompleteLocationCheck("Paramonian Trial 1 Bell Song Stone"); return;
                    case FlintLock.Trial.paraTrial2: Helpers.CompleteLocationCheck("Paramonian Trial 2 Bell Song Stone"); return;
                    case FlintLock.Trial.paraTrial3: Helpers.CompleteLocationCheck("Paramonian Trial 3 Bell Song Stone"); return;
                    case FlintLock.Trial.paraTrial4: Helpers.CompleteLocationCheck("Paramonian Trial 4 Bell Song Stone"); return;
                    case FlintLock.Trial.paraTrial5: Helpers.CompleteLocationCheck("Paramonian Trial 5 Bell Song Stone"); return;
                    case FlintLock.Trial.paraTrial6: Helpers.CompleteLocationCheck("Paramonian Trial 6 Bell Song Stone"); return;

                    case FlintLock.Trial.scrabTrial1: Helpers.CompleteLocationCheck("Scrabanian Trial 1 Bell Song Stone"); return;
                    case FlintLock.Trial.scrabTrial2: Helpers.CompleteLocationCheck("Scrabanian Trial 2 Bell Song Stone"); return;
                    case FlintLock.Trial.scrabTrial3: Helpers.CompleteLocationCheck("Scrabanian Trial 3 Bell Song Stone"); return;
                    case FlintLock.Trial.scrabTrial4: Helpers.CompleteLocationCheck("Scrabanian Trial 4 Bell Song Stone"); return;
                    case FlintLock.Trial.scrabTrial5: Helpers.CompleteLocationCheck("Scrabanian Trial 5 Bell Song Stone"); return;
                    case FlintLock.Trial.scrabTrial6: Helpers.CompleteLocationCheck("Scrabanian Trial 6 Bell Song Stone"); return;
                    case FlintLock.Trial.scrabTrial7: Helpers.CompleteLocationCheck("Scrabanian Trial 7 Bell Song Stone"); return;
                    case FlintLock.Trial.scrabTrial8: Helpers.CompleteLocationCheck("Scrabanian Trial 8 Bell Song Stone"); return;

                    default: Plugin.consoleLog.LogError($"Flint Lock type {flintlock.TrialValue} not handled for Bell Song Items! Acting as if the option isn't enabled."); return;
                }
            }

            // Assume this is in the Monsaic Lines and send that location.
            // TODO: Make sure this doesn't blow up in my face horribly.
            Helpers.CompleteLocationCheck("Monsaic Lines Bell Song Stone");
        }
    }
}
