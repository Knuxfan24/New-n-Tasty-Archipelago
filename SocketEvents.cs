using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Models;
using NNT_Archipealgo.CustomData;
using NNT_Archipealgo.Patchers;

namespace NNT_Archipealgo
{
    internal class SocketEvents
    {
        private static readonly string[] GenericDeathLinkReasons =
        [
            "$ had an oopsie.",
            "$ didn't find their Photochad.", // Reference to Balatro.
            "$ tripped.",
            "$ died. Sadge.",
            "$'s princess is in another castle.", // Reference to Super Mario Brothers.
            "$ got bodied.",
            "$ hit the ground too hard.", // Reference to Minecraft.
            "$ should have played on Drizzle.", // Reference to Risk of Rain.
            "$ didn't make it to 6AM.", // Reference to Five Nights at Freddy's.
            "$ had a Station Brakes Failure.", // Reference to RollerCoaster Tycoon.
            "$'s heart belongs to darkness.", // Reference to Kingdom Hearts.
            "$ had vapor for brains.", // Reference to Metroid Prime.
            "$ passed out at 2AM.", // Reference to Stardew Valley.
            "$ didn't follow the damn train.", // Reference to Grand Theft Auto: San Andreas.
            "Cranky was right about $.", // Reference to Donkey Kong Country.
            "$ forgot to rip and tear.", // Reference to Doom.
            "$ missed 1 box.", // Reference to Crash Bandicoot.
            "$ had a bad time.", // Reference to Undertale.
            "$ forgot the real superpower of teamwork.", // Reference to Sonic Heroes.
            "$ is a horrible goose.", // Reference to Untilted Goose Game.
            "$ got splatted.", // Reference to Splatoon.
            "$ misread their tracker client.",
        ];

        /// <summary>
        /// Event handler to update the remaining location count upon carrying out a check.
        /// </summary>
        public static void Socket_UpdateRemainingLocationsCount(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations) => Plugin.save.RemainingLocations = Plugin.session.Locations.AllMissingLocations.Count;

        /// <summary>
        /// Event handler for when we receive an item from the multiworld.
        /// </summary>
        public static void Socket_ReceiveItem(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper)
        {
            // Get the item the multiworld sent us and handle adding it to our queue.
            SetUpQueue(helper.PeekItem());

            // Dequeue this item.
            helper.DequeueItem();
        }

        /// <summary>
        /// Event handler for when we receive a DeathLink from the multiworld.
        /// </summary>
        public static void Socket_ReceiveDeathLink(DeathLink deathLink)
        {
            // Set up the message showing our DeathLink source.
            string notifyMessage = string.Empty;

            // Present the cause and source of the DeathLink, assuming we actually have one.
            if (deathLink.Cause != null)
                if (deathLink.Cause != "")
                    notifyMessage = $"{deathLink.Cause}";

            // If we still don't have a notify message set, then pull a generic one.
            if (notifyMessage == string.Empty)
                notifyMessage = GenericDeathLinkReasons[Plugin.rng.Next(GenericDeathLinkReasons.Length)].Replace("$", deathLink.Source);

            // Add our message to the info string queue.
            Plugin.infoStringQueue.Add(notifyMessage);

            // Set the flag that a DeathLink is waiting.
            AbePatcher.hasBufferedDeathLink = true;
        }

        /// <summary>
        /// Sets up the queue of items to be received.
        /// </summary>
        /// <param name="item">The item we're handling.</param>
        public static void SetUpQueue(ItemInfo item)
        {
            // Set up a queued item entry for this item.
            ArchipelagoItem queuedItem = new(item.ItemName, item.Player.Name);

            // Set up a boolean to check if we've got an item of the same type and source in the queue already.
            bool foundExistingInQueue = false;

            // Loop through each queued item.
            foreach (ArchipelagoItem itemInQueue in Plugin.itemQueue.Keys)
            {
                // Check if this item's name and source matches this dictionary entry.
                if (itemInQueue.ItemName == queuedItem.ItemName && itemInQueue.Source == queuedItem.Source)
                {
                    // Increment this dictionary entry's count.
                    Plugin.itemQueue[itemInQueue]++;

                    // Set our flag.
                    foundExistingInQueue = true;

                    // Stop the rest of the foreach loop, as its a pointless waste of time now.
                    break;
                }
            }

            // If we haven't found a version of this item in the queue already, then add one.
            if (!foundExistingInQueue)
                Plugin.itemQueue.Add(queuedItem, 1);
        }
    }
}
