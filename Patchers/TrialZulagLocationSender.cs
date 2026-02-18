using static ZulagLockHub;

namespace NNT_Archipealgo.Patchers
{
    internal class TrialZulagLocationSender
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlintLockHub), "Start")]
        static void SendTrialLocations(ref bool ___m_isParamonia, ref FlintLockTrialEffect[] ___m_trialEffects)
        {
            // Get the Scrabania trials and set our temple string to Scrabanian.
            bool[] trials = App.getInstance().m_lstScrabHubTrials;
            string temple = "Scrabanian";

            // If this Flint Lock HUB is for Paramonia instead, then get the Paramonia Trials and string.
            if (___m_isParamonia)
            {
                trials = App.getInstance().m_lstParaHubTrials;
                temple = "Paramonian";
            }

            // Loop through each trial, check if its completed and send the location if so.
            for (int trialIndex = 0; trialIndex < ___m_trialEffects.Length; trialIndex++)
                if (trials[trialIndex])
                    Helpers.CompleteLocationCheck($"{temple} Trial {trialIndex + 1}");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZulagLockHub), "Start")]
        static void SendZulagLocations(ref Hub ___m_hub, ref ZulagLockEffect[] ___m_lockEffects)
        {
            // Get the Zulag 2 doors.
            bool[] doors = App.getInstance().m_lstZulag2Lock;

            // Handle Zulag 3 if that's the one we're in.
            if (___m_hub == Hub.zulag3)
            {
                // Get the Zulag 3 doors instead.
                doors = App.getInstance().m_lstZulag3Lock;

                // Loop through each door, check if its completed and send the location if so.
                for (int doorIndex = 0; doorIndex < ___m_lockEffects.Length; doorIndex++)
                    if (doors[doorIndex])
                        Helpers.CompleteLocationCheck($"Zulag 3 Door {doorIndex + 1}");

                // Don't run the Zulag 2 checks.
                return;
            }

            // If we're in Zulag 2, then handle them differently, as the flags for Doors 2 and 3 are swapped.
            if (doors[0]) Helpers.CompleteLocationCheck($"Zulag 2 Door 1");
            if (doors[2]) Helpers.CompleteLocationCheck($"Zulag 2 Door 2");
            if (doors[1]) Helpers.CompleteLocationCheck($"Zulag 2 Door 3");
        }
    }
}
