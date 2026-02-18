using System.Reflection;
using static JAWStateMachine;

namespace NNT_Archipealgo.Patchers
{
    internal class AbePatcher
    {
        private static readonly MethodInfo dropItem = typeof(Abe).GetMethod("DropPickUp", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Holds a reference to the player's object.
        /// </summary>
        public static Abe player;

        /// <summary>
        /// Whether or not we have a DeathLink queued.
        /// </summary>
        public static bool hasBufferedDeathLink;

        /// <summary>
        /// Whether or not we can send a DeathLink out.
        /// </summary>
        public static bool canSendDeathLink = true;

        /// <summary>
        /// How much amnesty we have before we send out a DeathLink.
        /// </summary>
        public static int deathLinkAmnesty = 10;

        /// <summary>
        /// Grabs a reference to the player's object.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Abe), "Start")]
        private static void Setup(Abe __instance) => player = __instance;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Abe), "RespawnStartEnter")]
        private static void EnableDeathLinkOnRespawn() => canSendDeathLink = true;
        [HarmonyPostfix]
        [HarmonyPatch(typeof(App), "LoadQuickSave")]
        private static void EnableDeathLinkOnSaveLoad(ref bool ___m_bQuickLoadPending)
        {
            if (___m_bQuickLoadPending) canSendDeathLink = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Abe), "Update")]
        static void ReceiveDeathLink(Abe __instance)
        {
            // Check that we have a DeathLink waiting to come in.
            if (hasBufferedDeathLink)
            {
                // Remove the DeathLink flag.
                hasBufferedDeathLink = false;

                // Stop any of Abe's voice lines.
                AkSoundEngine.PostEvent("Stop_abe_vo", __instance.gameObject);

                // Set Abe to his death explosion state.
                __instance.ReInitStateMachine(JAWStateMachine.SMStates.AbeDeathExplosion);

                // If any Mudokons are following Abe, then have the first one make a voice line.
                if (__instance.Followers.Count > 0)
                    AkSoundEngine.PostEvent("Play_vox_mudslave_death_response_abe", __instance.Followers[0].gameObject);

                // Play the death jingle.
                App.musicsystem.PostMusicTrigger("trigger_death");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Abe), "Kill")]
        static void SendDeathLink(ref TakeDamageMessage cTakeDamageMessage)
        {
            // Temporarily display the death damage type so we can figure out what maps to what and make messages for them.
            Plugin.consoleLog.LogDebug(cTakeDamageMessage.Type);

            // Only do any of this if we can send a DeathLink and have it enabled.
            if (!canSendDeathLink || (long)Plugin.slotData["death_link"] == 0)
                return;

            // Disable the send flag so we don't send multiple (mines were especially bad with this).
            canSendDeathLink = false;

            // If we have any amnesty left, then decrement the counter and stop here.
            if (deathLinkAmnesty != 0)
            {
                deathLinkAmnesty--;
                return;
            }

            // Add a message for this DeathLink to our message queue.
            Plugin.infoStringQueue.Add("Sending death to your friends!");

            // Set up a generic reason for the DeathLink.
            string reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} died.";

            // Replace the reason depending on the source of the damage.
            switch (cTakeDamageMessage.Type)
            {
                case TakeDamageMessage.Types.Explosion: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} blew up."; break;
                case TakeDamageMessage.Types.Shot: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} got shot."; break;
                case TakeDamageMessage.Types.Fall: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} fell in a hole."; break;
                case TakeDamageMessage.Types.Bees: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} got stung."; break;
                case TakeDamageMessage.Types.Bat: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} got bit."; break;
                case TakeDamageMessage.Types.Zap: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} got electrocuted."; break;
                case TakeDamageMessage.Types.Grinder: case TakeDamageMessage.Types.UnderfloorMeatGrinder: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} became a Mudokon Pop."; break;
                case TakeDamageMessage.Types.Scrab: reason = $"A Scrab got revenge on {Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)}."; break;
                case TakeDamageMessage.Types.Slog: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} became Slog food."; break;
                case TakeDamageMessage.Types.Paramite: reason = $"A Paramite got revenge on {Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)}."; break;
                case TakeDamageMessage.Types.DeathPlane: reason = $"{Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot)} fell in a hole."; break;
                default: Plugin.consoleLog.LogWarning($"Death Type {cTakeDamageMessage.Type} not handled for unique message!"); break;
            }

            // Send a DeathLink with our reason.
            Plugin.DeathLink.SendDeathLink(new Archipelago.MultiClient.Net.BounceFeatures.DeathLink.DeathLink(Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot), reason));
            
            // Throw the reason into the log.
            Plugin.consoleLog.LogDebug($"Sending DeathLink with reason:\r\n\t{reason}");

            // Reset the DeathLink amnesty.
            deathLinkAmnesty = (int)(long)Plugin.slotData["death_link_amnesty"];
        }

        /// <summary>
        /// Forces Abe's state into the provided one.
        /// Wrapped in a try catch block as poorly timed traps could completely break item receiving due to a null reference exception in the animator, so this gets around that.
        /// </summary>
        public static void SetTrapState(SMStates state)
        {
            // TODO: Receiving a state based trap can cause some weirdness (like regaining control of Abe during a possession).
            try
            {
                player?.ReInitStateMachine(state);
            }
            catch
            {

            }
        }

        /// <summary>
        /// Forces Abe to drop whatever he's carrying (usually a bottlecap).
        /// </summary>
        public static void DropTrap() => dropItem.Invoke(player, new object[] { });

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(Abe), "Update")]
        //private static void StateTester(Abe __instance)
        //{
        //    if (Input.GetKeyDown(KeyCode.F9))
        //        dropItem.Invoke(player, new object[] { });
        //}
    }
}
