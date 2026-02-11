using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace NNT_Archipealgo.Patchers
{
    internal class StatusBoardPatcher
    {
        /// <summary>
        /// Timer to determine when to change the second line.
        /// </summary>
        private static float genericTimer;

        /// <summary>
        /// Value to track what the second line should be displaying.
        /// </summary>
        private static int powerIndex;

        /// <summary>
        /// Set of characters to randomly generate a scrambled string for the sake of a nicer looking transition between values on the second line.
        /// </summary>
        private static readonly string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@£$%^&*(){}[]?#'~,.<>;:-_=+/";

        /// <summary>
        /// What the label part of the second line should read, comprised of 16 random characters.
        /// </summary>
        private static char[] labelChars = new char[16];

        /// <summary>
        /// What the value part of the second line should read, comprised of 4 random characters.
        /// </summary>
        private static char[] valueChars = new char[4];

        /// <summary>
        /// Copy of the TextLine struct from the decompiled source as the original cannot be accessed.
        /// </summary>
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
            // Set the first line to the remaining number of locations.
            ___lines[0].text = "Locations:";
            ___lines[3].text = Plugin.save.RemainingLocations.ToString();

            // Set the third line to the count of rescued Mudokons, using the original Escapees string.
            ___lines[2].text = LanguageManager.GetText("env_statusboard_escapees") + ":";
            ___lines[5].text = Plugin.save.MudokonCount.ToString();

            // If the timer is below 0, then set the second line to our random strings and return.
            if (genericTimer < 0)
            {
                ___lines[1].text = new String(labelChars);
                ___lines[4].text = new String(valueChars);
                return;
            }

            // Set the second line to the approriate ability for the value of powerIndex.
            // Unless its 9, in which case show the deathLinkAmnesty value from the AbePatcher class.
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
            // Increment our timer by the game's delta time.
            genericTimer += Time.deltaTime;

            // Check if the timer has reached 2.
            if (genericTimer >= 2)
            {
                // Decrement the timer by 2.5 (so it goes below 0 and thus triggers the random text to be displayed).
                genericTimer -= 2.5f;

                // Handle incrementing or resetting powerIndex based on whether DeathLink is enabled and has an amnesty value.
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

            // Generate random strings for the line 2 transition.
            for (int i = 0; i < labelChars.Length; i++) labelChars[i] = chars[Plugin.rng.Next(chars.Length)];
            for (int i = 0; i < valueChars.Length; i++) valueChars[i] = chars[Plugin.rng.Next(chars.Length)];
        }
    }
}
