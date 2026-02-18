namespace NNT_Archipealgo.Patchers
{
    internal class MudokonSlavePatcher
    {
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
