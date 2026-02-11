using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace NNT_Archipealgo.Patchers
{
    internal class StatusBoardPatcher
    {
        private static float genericTimer;
        private static int powerIndex;

        // Copied from the decompiled source as the original cannot be accessed.
        private struct TextLine
        {
            public Material mat;

            public GameObject quad;

            public string text;

            public float minwidth;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StatusBoard), "updateText")]
        static IEnumerable<CodeInstruction> RemoveBoardCount(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int codeIndex = 0; codeIndex <= 66; codeIndex++)
                codes[codeIndex].opcode = OpCodes.Nop;

            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StatusBoard), "Update")]
        static IEnumerable<CodeInstruction> RemoveBoardUpdateOptimisation(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (int codeIndex = 0; codeIndex <= 2; codeIndex++)
                codes[codeIndex].opcode = OpCodes.Nop;

            return codes.AsEnumerable();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatusBoard), "updateText")]
        static void UpdateBoard(ref TextLine[] ___lines)
        {
            ___lines[0].text = "Locations:";
            ___lines[3].text = Plugin.save.RemainingLocations.ToString();

            ___lines[2].text = LanguageManager.GetText("env_statusboard_escapees") + ":";
            ___lines[5].text = Plugin.save.MudokonCount.ToString();

            switch (powerIndex)
            {
                case 0:
                    ___lines[1].text = "Levers:";
                    ___lines[4].text = Plugin.save.HasLevers ? "Yes" : "No";
                    break;
                case 1:
                    ___lines[1].text = "Possession:";
                    ___lines[4].text = Plugin.save.CanPosses ? "Yes" : "No";
                    break;
                case 2:
                    ___lines[1].text = "Grenades:";
                    ___lines[4].text = Plugin.save.HasGrenades ? "Yes" : "No";
                    break;
                case 3:
                    ___lines[1].text = "Rocks:";
                    ___lines[4].text = Plugin.save.HasRocks ? "Yes" : "No";
                    break;
                case 4:
                    ___lines[1].text = "UXB Defusion:";
                    ___lines[4].text = Plugin.save.CanDefuseUXBs ? "Yes" : "No";
                    break;
                case 5:
                    ___lines[1].text = "Lifts:";
                    ___lines[4].text = Plugin.save.CanUseLifts ? "Yes" : "No";
                    break;
                case 6:
                    ___lines[1].text = "Spirit Rings:";
                    ___lines[4].text = Plugin.save.CanUseSpiritRings ? "Yes" : "No";
                    break;
                case 7:
                    ___lines[1].text = "Meat:";
                    ___lines[4].text = Plugin.save.CanUseMeatSacks ? "Yes" : "No";
                    break;
                case 8:
                    ___lines[1].text = "Shrykull:";
                    ___lines[4].text = Plugin.save.CanUseShrykull ? "Yes" : "No";
                    break;
                case 9:
                    ___lines[1].text = "DL Amnesty:";
                    ___lines[4].text = AbePatcher.deathLinkAmnesty.ToString();
                    break;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(StatusBoard), "Update")]
        static void HandleLine2Timer()
        {
            genericTimer += Time.deltaTime;

            if (genericTimer >= 2)
            {
                genericTimer -= 2;

                if ((long)Plugin.slotData["death_link"] == 0 || (long)Plugin.slotData["death_link_amnesty"] == 0)
                {
                    if (powerIndex == 8)
                        powerIndex = 0;
                    else
                        powerIndex++;
                }
                else
                {
                    if (powerIndex == 9)
                        powerIndex = 0;
                    else
                        powerIndex++;
                }
            }
        }
    }
}
