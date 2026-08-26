using BepInEx;
using NNT_Archipealgo.CustomData;

namespace NNT_Archipealgo.Patchers
{
    internal class MudokonSlavePatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MudokonSlave), "Start")]
        static void CreateAPSprite(MudokonSlave __instance)
        {
            // Check that this Mudokon hasn't already been rescued.
            if (Helpers.CheckLocationExists(__instance.ID) && !Plugin.session.Locations.AllLocationsChecked.Contains(__instance.ID))
            {
                // Create a GameObject called AP Indicator.
                GameObject apSprite = new("AP Indicator");

                // Parent the GameObject to this Mudokon's and shift it to be above their head.
                apSprite.transform.position = __instance.transform.position;
                apSprite.transform.parent = __instance.transform;
                apSprite.transform.localPosition = new(0, 2.2f, 0);

                // Add a Sprite Renderer with the Archipelago logo in it.
                SpriteRenderer renderer = apSprite.AddComponent<SpriteRenderer>();
                renderer.sprite = Helpers.GetCustomSprite($@"{Paths.GameRootPath}\mod_overrides\Archipelago\ap_logo.png");

                // Add the billboard script so the sprite doesn't flip around.
                apSprite.AddComponent<Billboard>();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonSlave), "DeathBackEnter")]
        static void DeathBackEnter() => RingLinkLoss();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonSlave), "DeathChokeEnter")]
        static void DeathChokeEnter() => RingLinkLoss();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonSlave), "DeathFrontEnter")]
        static void DeathFrontEnter() => RingLinkLoss();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonSlave), "DeathGibEnter")]
        static void DeathGibEnter() => RingLinkLoss();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonSlave), "DeathZapEnter")]
        static void DeathZapEnter() => RingLinkLoss();
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonSlave), "DeathZapGibEnter")]
        static void DeathZapGibEnter() => RingLinkLoss();

        /// <summary>
        /// Sends out a single negative RingLink packet if a Mudokon dies.
        /// </summary>
        static void RingLinkLoss()
        {
            if ((long)Plugin.slotData["ring_link"] != 0)
                Plugin.RingLinkMudokonCount--;
        }
    }
}
