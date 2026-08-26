using BepInEx;
using NNT_Archipealgo.CustomData;

namespace NNT_Archipealgo.Patchers
{
    internal class LockedSprites
    {
        static void CreateLockObject(Transform parentTransform, Vector3 offset)
        {
            // Create a GameObject called AP Indicator.
            GameObject apSprite = new("AP Indicator");

            // Parent the GameObject to the provided transform and shift it by the provided offset.
            apSprite.transform.position = parentTransform.position;
            apSprite.transform.parent = parentTransform;
            apSprite.transform.localPosition = offset;

            // Add a Sprite Renderer with the Lock sprite in it.
            SpriteRenderer renderer = apSprite.AddComponent<SpriteRenderer>();
            renderer.sprite = Helpers.GetCustomSprite($@"{Paths.GameRootPath}\mod_overrides\Archipelago\locked.png");

            // Add the billboard script so the sprite doesn't flip around.
            apSprite.AddComponent<Billboard>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Lever), "Start")]
        private static void LeverLock(Lever __instance)
        {
            if (!Plugin.save.HasLevers) CreateLockObject(__instance.transform, new(0, 1.75f, 0));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Slig), "Start")]
        private static void PossessionLock(Slig __instance)
        {
            if (!Plugin.save.CanPosses) CreateLockObject(__instance.transform, new(0, 1.8f, 0.35f));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GrenadeDispenser), "Awake")]
        private static void BoomMachineLock(GrenadeDispenser __instance)
        {
            if (!Plugin.save.HasGrenades) CreateLockObject(__instance.transform, new(0, 1.2f, 1.3f));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RockBag), "Start")]
        private static void ThrowableBagLock(RockBag __instance, ref Spawner ___m_cSpawner)
        {
            // Check if this is a Rock Bag or Meat Sack.
            bool isMeat = ___m_cSpawner.Spawned.name.Contains("Meat");

            if (!Plugin.save.HasRocks && !isMeat) CreateLockObject(__instance.transform, new(0, -3.1f, 0));
            if (!Plugin.save.CanUseMeatSacks && isMeat) CreateLockObject(__instance.transform, new(0, -2.1f, 0));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ToggleMine), "Start")]
        private static void UXBLock(ToggleMine __instance)
        {
            if (!Plugin.save.CanDefuseUXBs) CreateLockObject(__instance.transform, new(0, 0.75f, 0));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Elevator), "Start")]
        private static void LiftLock(Elevator __instance)
        {
            if (!Plugin.save.CanUseLifts) CreateLockObject(__instance.transform, new(0, 2.65f, 0));
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Elevator), "Start")]
        private static void CargoLiftLock(CargoElevator __instance)
        {
            if (!Plugin.save.CanUseLifts) CreateLockObject(__instance.transform, new(0, 1.5f, 0));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MudokonNative), "Start")]
        private static void SpiritRingLock(MudokonNative __instance, ref MudokonNative.OutcomeType ___m_outcome)
        {
            if (!Plugin.save.CanUseSpiritRings && ___m_outcome == MudokonNative.OutcomeType.SpiritRing) CreateLockObject(__instance.transform, new(0, 2.2f, 0));
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Portal), "Start")]
        private static void ShrykullLock(Portal __instance)
        {
            if (!Plugin.save.CanUseShrykull && __instance.m_bAllowShrykull) CreateLockObject(__instance.transform, new(0, 0, -2));
        }
    }
}
