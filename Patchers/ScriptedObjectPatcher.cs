namespace NNT_Archipealgo.Patchers
{
    internal class ScriptedObjectPatcher
    {
        /// <summary>
        /// Stops the intro cutscene from playing in the Rupture Farms Escape.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ScriptedObject), "Start")]
        static void KillIntro(ScriptedObject __instance)
        {
            if (__instance.gameObject.name == "__IntroCutscene") __instance.Active = false;
        }
    }
}
