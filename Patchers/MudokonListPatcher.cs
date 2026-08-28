using static LevelList;

namespace NNT_Archipealgo.Patchers
{
    internal class MudokonListPatcher
    {
        /// <summary>
        /// Changes the rescued percentage on the Chapter Select to read from the AP server.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MudokonList), "MudsStatesForChapter")]
        static void RecalculateMudokonPercentages(LevelList.Chapters eChapter, ref int nRescued)
        {
            // Don't do this for The Boardroom or Alf's Escape.
            if (eChapter == Chapters.TheBoardroom || eChapter == Chapters.Alf)
                return;

            // Reset the rescued count.
            nRescued = 0;

            // Get the Mudokon IDs.
            List<int> mudIDsForChapter = MudokonList.GetMudIDsForChapter(eChapter);

            // Loop through each Mudokon ID, check if the location has been checked and increment the rescued count if so.
            for (int mudokonID = 0; mudokonID < mudIDsForChapter.Count; mudokonID++)
                if (Helpers.CheckLocationExists(mudIDsForChapter[mudokonID]) && Plugin.session.Locations.AllLocationsChecked.Contains(mudIDsForChapter[mudokonID]))
                    nRescued++;
        }
    }
}
