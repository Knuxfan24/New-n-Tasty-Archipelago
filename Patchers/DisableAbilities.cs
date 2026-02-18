namespace NNT_Archipealgo.Patchers
{
    internal class DisableAbilities
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Abe), "Update")]
        static void DisablePossession(Abe __instance)
        {
            if (!Plugin.save.CanPosses) __instance.m_lstPossessionTargets.Clear();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Abe), "Update")]
        static void DisableShrykull(ref int ___m_nShrykullCharges)
        {
            if (!Plugin.save.CanUseShrykull) ___m_nShrykullCharges = 0;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GrenadeDispenser), "Dispense")]
        static bool DisableBoomMachine() => Plugin.save.HasGrenades;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RockBag), "OnTriggerEnter")]
        static bool DisableRockAndMeatSack(ref Spawner ___m_cSpawner)
        {
            // Check if this is a Rock Bag or Meat Sack.
            bool isMeat = ___m_cSpawner.Spawned.name.Contains("Meat");

            // Disable depending on the type and whether or not we've received the ability to use them.
            if (!isMeat && !Plugin.save.HasRocks) return false;
            if (isMeat && !Plugin.save.CanUseMeatSacks) return false;

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Lever), "LeverPulled")]
        static bool DisableLevers() => Plugin.save.HasLevers;

        // TODO: Doesn't stop the cargo lift in Zulag 1.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LiftController), "StartMoving")]
        static bool StopLifts() => Plugin.save.CanUseLifts;

        // TODO: The first time a Mudokon Native interacts with Abe ignores this and still gives a Spirit Ring?
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonNative), "TransferSpiritRingsEnter")]
        static bool StopSpiritRingsEnter() => Plugin.save.CanUseSpiritRings;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonNative), "TransferSpiritRingsExit")]
        static bool StopSpiritRingsExit(MudokonNative __instance, ref bool ___m_bChantAudioPlaying)
        {
            // If we can't use Spirit Rings, then at least stop the Mudokon's chant sound.
            if (!Plugin.save.CanUseSpiritRings)
            {
                ___m_bChantAudioPlaying = false;
                AkSoundEngine.PostEvent("Stop_vox_mudnative_chant", __instance.gameObject);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ToggleMine), "ActivatedChangeColour")]
        static bool StopUXBCycle(ref Material ___m_cButtonMaterial, ref bool ___m_bGreen, ref Timer ___m_cPatternTimer, ref DifficultyValuesListFloat ___m_cPattern, ref int ___m_nPatternIndex)
        {
            // Check if we can defuse UXBs.
            if (!Plugin.save.CanDefuseUXBs)
            {
                // Make sure this UXB isn't green.
                ___m_bGreen = false;
                ___m_cButtonMaterial.color = Color.red;

                // Make sure this UXB's pattern is running.
                ___m_cPatternTimer.Start(___m_cPattern.GetDifficultyValue()[___m_nPatternIndex]);

                // Don't run the original function.
                return false;
            }

            return true;
        }
    }
}
