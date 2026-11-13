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

        /// <summary>
        /// Handles Mudokons dying.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MudokonSlave), "Kill")]
        static void MudokonDeath(MudokonSlave __instance)
        {
            // Check if we're doing a genocide run and send the location for this Mudokon if so.
            if ((long)Plugin.slotData["genocide"] != 0)
            {
                // Send the location for this Mudokon.
                Helpers.CompleteLocationCheck(__instance.ID);

                // Loop through this Mudokon's object to find the Archipelago indicator and destroy it if it exists (as it otherwise persists through checkpoint reloads).
                for (int childIndex = __instance.transform.childCount - 1; childIndex >= 0; childIndex--)
                {
                    if (__instance.transform.GetChild(childIndex).name == "AP Indicator")
                    {
                        GameObject.Destroy(__instance.transform.GetChild(childIndex).gameObject);
                        break;
                    }
                }
            }

            // Send out a single negative RingLink packet.
            if ((long)Plugin.slotData["ring_link"] != 0)
                Plugin.RingLinkMudokonCount--;
        }
    }
}
