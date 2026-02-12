using HarmonyLib;
using static ScriptedEvent;
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
                    Plugin.session.Locations.CompleteLocationChecks(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", $"{temple} Trial {trialIndex + 1}"));
        }

        // TODO: Use this to also return to the menu? Might be worth pivoting to using the App's CompletedChapter function instead.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScriptedEvent), "ProcessInput")]
        static bool SendNestLocations(ScriptedEvent __instance)
        {
            foreach (ScriptedInput scriptedInput in __instance.ScriptedInputs)
            {
                foreach (ScriptedInput.ScriptedOutput scriptedOutput in scriptedInput.ScriptedOutputs)
                {
                    if (scriptedOutput.OutputEvent == ScriptedEventMethod.ParaTempleComplete)
                    {
                        Plugin.session.Locations.CompleteLocationChecks(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Paramonian Nests"));
                        return false;
                    }
                    if (scriptedOutput.OutputEvent == ScriptedEventMethod.ScrabTempleComplete)
                    {
                        Plugin.session.Locations.CompleteLocationChecks(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", "Scrabanian Nests"));
                        return false;
                    }

                }
            }

            return true;
        }

        // TODO: Doors 2 and 3 in Zulag 2 are backwards.
        // TODO: Check Zulag 3's doors.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZulagLockHub), "Start")]
        static void SendZulagLocations(ref Hub ___m_hub, ref ZulagLockEffect[] ___m_lockEffects)
        {
            // Get the Zulag 2 doors and set our Zulag index to 2.
            bool[] doors = App.getInstance().m_lstZulag2Lock;
            int zulag = 2;

            // If this Lock HUB is for Zulag 3, then get those doors and set the Zulag index to 3 instead.
            if (___m_hub == Hub.zulag3)
            {
                doors = App.getInstance().m_lstZulag3Lock;
                zulag = 3;
            }

            // Loop through each door, check if its completed and send the location if so.
            for (int doorIndex = 0; doorIndex < ___m_lockEffects.Length; doorIndex++)
                if (doors[doorIndex])
                    Plugin.session.Locations.CompleteLocationChecks(Plugin.session.Locations.GetLocationIdFromName("New 'n' Tasty", $"Zulag {zulag} Door {doorIndex + 1}"));
        }
    }
}
