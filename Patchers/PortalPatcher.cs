namespace NNT_Archipealgo.Patchers
{
    internal class PortalPatcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Portal), "OpenExecute")]
        static void RescueMudokon(Portal __instance, ref List<Transform> ___m_lstEnteringCharacters)
        {
            // Don't bother doing this if we're in The Boardroom.
            if (App.getInstance().GetCurrentLevelName().Contains("Boardroom")) return;

            // Check that this portal is processing a rescuable Mudokon and that Abe is... Alive.
            if (__instance.type == Portal.Type.Slave && !App.abe.Dead)
            {
                // Loop through each character entering this portal.
                for (int enteringCharacterIndex = ___m_lstEnteringCharacters.Count - 1; enteringCharacterIndex >= 0; enteringCharacterIndex--)
                {
                    // Get this character's MudokonSlave component.
                    MudokonSlave component = ___m_lstEnteringCharacters[enteringCharacterIndex].GetComponent<MudokonSlave>();

                    // Check that this character actually HAS a MudokonSlave component.
                    if (component != null)
                    {
                        // Check that this Mudokon's bounding box is overlapping the portal's position.
                        if (__instance.m_cOOBB.Contains(component.transform.position))
                        {
                            // Send the location for this Mudokon.
                            Helpers.CompleteLocationCheck(component.ID);

                            // Send a RingLink if we have our joke RingLink option on.
                            if ((long)Plugin.slotData["ring_link"] != 0)
                                Plugin.RingLinkMudokonCount++;
                        }
                    }
                }
            }
        }
    }
}
