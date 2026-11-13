using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace NNT_Archipealgo.Patchers
{
    internal class JokeMessages
    {
        /// <summary>
        /// Valid placeholder types for replacing parts of a string in the message body.
        /// </summary>
        public enum PlaceholderTypes
        {
            RandomName, // Picks any name from the server.
            RandomNameNoServer, // Excludes the word "Server".
            RandomNameNotOurs, // Excludes our own name.
            RandomNameNotOursOrServer, // Excludes both "Server" and our own name.
            OurName, // Shows our own name.
            RandomString // Picks a random string from a provided set.
        }

        /// <summary>
        /// Whether this message can appear exclusively on Rupture Farms signs, exclusively on Fireflies or on either.
        /// </summary>
        public enum Type
        {
            Both,
            Board,
            Fireflies
        }

        public class JokeMessage(string message)
        {
            /// <summary>
            /// Text shown in the message body.
            /// </summary>
            public string Message { get; set; } = message;

            /// <summary>
            /// Placeholder types for replacing text in the message body.
            /// </summary>
            public List<PlaceholderTypes> Placeholders = [];

            /// <summary>
            /// What this message can appear on.
            /// </summary>
            public Type Type { get; set; } 

            /// <summary>
            /// Strings that can be used to fill in the RandomString placeholder type.
            /// </summary>
            public List<string> PlaceholderStrings = [];

            // Initialiser that includes placeholders or type.
            public JokeMessage(string message, List<PlaceholderTypes> placeholders) : this(message) => Placeholders = placeholders;
            public JokeMessage(string message, Type type) : this(message)
            {
                Type = type;
            }
            public JokeMessage(string message, List<PlaceholderTypes> placeholders, List<string> placeholderStrings) : this(message)
            {
                Placeholders = placeholders;
                PlaceholderStrings = placeholderStrings;
            }
        }

        // The various messages that can be picked for display. Currently these are all taken from Freedom Planet 2's Spam Trap in some form.
        public static readonly JokeMessage[] messages =
        [
            new("We've been trying to reach you regarding your Mine Car's extended warranty."), // The "car's extended warranty" meme, but referencing the Mine Car from Exoddus.
            new("Half price entry to Factory Capers.    Avaliable while stocks last.", Type.Board), // Reference to OpenRCT2, using a different scenario name to the one in FP2.
            new("Half price entry to Factory Capers. Avaliable while stocks last.", Type.Fireflies), // Reference to OpenRCT2, using a different scenario name to the one in FP2.
            new("You won't get tired of my voice will you? You won't get tired of my voice will you? You won't get tired of my voice will you? You won't get tired of my voice will you?", Type.Board), // Reference to FNaF World.
            new("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA!", Type.Board), // Reference to Freedom Planet 2
            new("75% off your next purchase at JojaMart."), // Reference to Stardew Valley.
            new("Reading this scrolling sign...    It fills you with determination.", Type.Board), // Reference to Undertale.
            new("Reading this scrolling sign... It fills you with determination.", Type.Fireflies), // Reference to Undertale.
            new("You feel an evil presence watching you..."), // Reference to Terraria.
            new("A MYURRDERRRR?!    ON MY OWL EXPRESS?!", Type.Board), // Reference to A Hat in Time.
            new("A MYURRDERRRR?! ON MY OWL EXPRESS?!", Type.Fireflies), // Reference to A Hat in Time.
            new("eastmost peninsula is the secret"), // Reference to The Legend of Zelda.
            new("The word {$}, they stole it too!", [PlaceholderTypes.RandomName]), // Reference to Kingdom Hearts 2.
            new("i showed you my cacodemon plz respond"), // Reference to Doom.
            new("Message from Ghandi: Our words are backed by nuclear weapons!"), // Reference to Civilization.
            new("You offer to the shrine, but gain nothing."), // Reference to Risk of Rain 2.
            new("AURORA BOREALIS?!    At this time of year? At this time of day? In this part of the mutliworld? Localised entirely within your slot data?!", Type.Board), // Reference to that Simpsons meme.
            new("AURORA BOREALIS?! At this time of year? At this time of day? In this part of the mutliworld? Localised entirely within your slot data?!", Type.Board), // Reference to that Simpsons meme.
            new("Dear {$}. Please come to the castle. I've baked a cake for you. Yours truly-- Princess Toadstool", [PlaceholderTypes.OurName]), // Reference to Super Mario 64.
            new("{$} has died in an accident on Steeplechase 1!", [PlaceholderTypes.RandomName]), // Reference to OpenRCT2.
            new("Adam has yet to authorise reading of this sign."), // Reference to Metroid: Other M.
            new("The square root of rope is string."), // Reference to Portal 2.
            new("What is a man? A miserable little pile of secrets!"), // Reference to Castlevania: Symphony of the Night.
            new("I'm sorry, {$}, but you seem to be playing a hacked version of this game.", [PlaceholderTypes.OurName]), // Reference to Spyro 3.
            new("IT'S JUST A BIG NOSE BUSH"), // Reference to Rayman 2.
            new("Local boy discovers friends are power. Sword responds with confusion."), // Reference to Kingdom Hearts.
            new("\"Barrier continues to hold\" reports frustrated conductor."), // Reference to The Legend of Zelda: Wind Waker.
            new("The train headed for the Mystic Ruins will be departing soon."), // Reference to Sonic Adventure.
            new("Local resident dies due to mysterious pool ladder related incident. {$} denies involvement.", [PlaceholderTypes.RandomNameNoServer]), // Reference to The Sims.
            new("According to all known laws of aviation, there is no way a Slig should be able to fly."), // Reference to the Bee Movie copy pasta, using the Flying Sligs from Exoddus.
            new("Welcome. Welcome to City 17. You have chosen, or been chosen, to relocate to one of our finest remaining urban centers."), // Reference to Half-Life 2.
            new("How are you gentlemen !! All your base are belong to us."), // Reference to Zero Wing.
            new("Boy gets beaten in foot race by one second despite speedrunner techniques."), // Reference to The Legend of Zelda: Ocarina of Time.
            new("{$} would just love it if there was a Vending Machine right here!", [PlaceholderTypes.RandomNameNotOursOrServer]), // Reference to Tomadachi Life.
            new("Local \"More Gun\" advocate caught appreciating \"A Little Less Gun\""), // Reference to Team Fortress 2.
            new("Supposed \"Greatest Plan\" turned out to be not so great. Pilot unavailable for comment."), // Reference to The Henry Stickmin Collection.
            new("{$} wins by doing absolutely nothing.", [PlaceholderTypes.RandomNameNotOursOrServer]), // Reference to the Luigi Wins meme.
            new("Squids continue to argue over mundane choices. Newly arriving Octopi left confused."), // Reference to Splatoon.
            new("\"They just weren't protected at all\", claims Literature Club president upon deleting critical CHR files."), // Reference to Doki Doki Literature Club.
            new("Hedgehog shows up late after being lost in maze."), // Reference to Super Smash Brothers Brawl.
            new("Monster Truck sightings increase. Souls reported stolen."), // Reference to Sonic Racing CrossWorlds.
            new("Reading this sign crashes Paper Mario."), // Reference to old "Doing [x] crashes Paper Mario" videos.
            new("Grandma allegedly discovers quantum technology while baking cookies."), // Reference to Cookie Clicker.
            new("Local officer reportedly almost a sandwich."), // Reference to Resident Evil.
            new("There was a hole here. It's gone now."), // Reference to Silent Hill 2.
            new("Glory to Arstotzka"), // Reference to Papers, Please.
            new("An archipelago.gg account is required to play this title."), // Reference to the bethesda.net requirement in the 25th anniversary Doom rereleases.
            new("Hear the words of O-Lir, last Sentinel of the Fortress Temple. May they serve you well."), // Reference to Metroid Prime 2: Echoes.
            new("you got games on your phone?"),
            new("It was foretold by Gyromancy!"), // Reference to Silent Hill.
            new("It's a lovely morning on Oddworld, and you are a horrible goose."), // Reference to Untitled Goose Game.
            new("Look at you, hacker: a pathetic creature of meat and bone, panting and sweating as you run through my corridors. How can you challenge a perfect, immortal machine?"), // Reference to System Shock.
            new("Still can't cross the Sahara Desert? Try camels."), // Reference to The History of the Entire World.
            new("BREAKING NEWS! Guardian genie fired.                                Again...", Type.Board), // Reference to the Shantae series.
            new("BREAKING NEWS! Guardian genie fired. Again...", Type.Fireflies), // Reference to the Shantae series.
            new("What is the square root of a fish? Now I'm sad."), // Reference to Skullgirls.
            new("Momentum, a function of mass and velocity, is conserved between portals. In layman's terms, speedy thing goes in, speedy thing comes out."), // Reference to Portal.
            new("y cant metroid crawl?"), // Reference to the "y cant metroid crawl" meme.
        ];

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ChantFlies), "Awake")]
        [HarmonyPatch(typeof(ChantFlies), "StopFormingWords")]
        static void Fireflies(ref string[] ___m_words)
        {
            // Only do this if we pass the config option.
            if (!Plugin.configJokeSigns.Value)
                return;

            // Pick a message, making sure it isn't exclusive to the Boards.
            JokeMessage selectedMessage = messages[UnityEngine.Random.Range(0, messages.Length)];
            while (selectedMessage.Type == Type.Board)
                selectedMessage = messages[UnityEngine.Random.Range(0, messages.Length)];

            // Run the chosen message through the replace placeholders method, force it to uppercase (as all the firefly messages are in all caps) and split it into the words array.
            ___m_words = ReplacePlaceholders(selectedMessage.Message, selectedMessage.Placeholders, selectedMessage.PlaceholderStrings).ToUpper().Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LEDScreen), "OnApplicationFocus")]
        static bool StopFocusRefresh() => false;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LEDScreen), "RenderMessage")]
        static void ReplaceMessage(ref bool ___isRandomMessage, ref string ___curtext, ref List<string> ___m_lstStringIDs)
        {
            // Only do this if we pass the config options.
            if (!Plugin.configJokeSigns.Value)
                return;
            if (!___isRandomMessage && !Plugin.configJokeSignsTutorials.Value)
                return;

            // If this sign has less than two random IDs, then add the first two messages as dummy entries so the game doesn't lock up in an infinite loop.
            if (___m_lstStringIDs.Count < 2)
            {
                ___m_lstStringIDs.Add("led_misc_1");
                ___m_lstStringIDs.Add("led_misc_2");
            }

            // Set the Random Message flag so we actually pick a random one.
            ___isRandomMessage = true;

            // Pick a message, making sure it isn't exclusive to the Fireflies.
            JokeMessage selectedMessage = messages[UnityEngine.Random.Range(0, messages.Length)];
            while (selectedMessage.Type == Type.Fireflies)
                selectedMessage = messages[UnityEngine.Random.Range(0, messages.Length)];

            // Run the chosen message through the replace placeholders method before setting the sign's text value to it.
            ___curtext = ReplacePlaceholders(selectedMessage.Message, selectedMessage.Placeholders, selectedMessage.PlaceholderStrings);
        }

        private static string ReplacePlaceholders(string text, List<PlaceholderTypes> placeholders, List<string> placeholderStrings = null)
        {
            // If we only have at most two players (likely our own name and the server), then force replace RandomNameNotOursOrServer with RandomName.
            if (Plugin.session.Players.AllPlayers.Count() <= 2)
                for (int overwriteIndex = 0; overwriteIndex < placeholders.Count; overwriteIndex++)
                    if (placeholders[overwriteIndex] == PlaceholderTypes.RandomNameNotOursOrServer)
                        placeholders[overwriteIndex] = PlaceholderTypes.RandomName;

            int placeholderIndex = 0;

            // Split the string on the {$} indicators.
            string[] split = Regex.Split(text, "({\\$})");

            // Loop through each split.
            for (int splitIndex = 0; splitIndex < split.Length; splitIndex++)
            {
                // Check that this split is a placeholder one.
                if (split[splitIndex] == "{$}")
                {
                    // Check that we haven't got more placeholders than we actually called for.
                    if (placeholderIndex >= placeholders.Count)
                    {
                        Plugin.consoleLog.LogError($"Spam Trap value '{text}' had more placeholders than defined!");
                        break;
                    }

                    // Determine what to do based on our current placeholder's type.
                    switch (placeholders[placeholderIndex])
                    {
                        // Pick a random name from the player list.
                        case PlaceholderTypes.RandomName:
                            split[splitIndex] = Plugin.session.Players.AllPlayers.ToArray()[Plugin.rng.Next(Plugin.session.Players.AllPlayers.ToArray().Length)].Name;
                            break;

                        // Force our split to "Server", then select from the player list until we pick something else.
                        case PlaceholderTypes.RandomNameNoServer:
                            split[splitIndex] = "Server";

                            while (split[splitIndex] == "Server")
                                split[splitIndex] = Plugin.session.Players.AllPlayers.ToArray()[Plugin.rng.Next(Plugin.session.Players.AllPlayers.ToArray().Length)].Name;
                            break;

                        // Force our split to our slot name, then select from the player list until we pick something valid.
                        case PlaceholderTypes.OurName:
                        case PlaceholderTypes.RandomNameNotOurs:
                        case PlaceholderTypes.RandomNameNotOursOrServer:
                            split[splitIndex] = Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot);

                            if (placeholders[placeholderIndex] is PlaceholderTypes.RandomNameNotOurs)
                                while (split[splitIndex] == Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot))
                                    split[splitIndex] = Plugin.session.Players.AllPlayers.ToArray()[Plugin.rng.Next(Plugin.session.Players.AllPlayers.ToArray().Length)].Name;

                            if (placeholders[placeholderIndex] is PlaceholderTypes.RandomNameNotOursOrServer)
                                while (split[splitIndex] == Plugin.session.Players.GetPlayerName(Plugin.session.ConnectionInfo.Slot) || split[splitIndex] == "Server")
                                    split[splitIndex] = Plugin.session.Players.AllPlayers.ToArray()[Plugin.rng.Next(Plugin.session.Players.AllPlayers.ToArray().Length)].Name;
                            break;


                        case PlaceholderTypes.RandomString:
                            split[splitIndex] = placeholderStrings[Plugin.rng.Next(placeholderStrings.Count)];
                            break;
                        // Log an error if we haven't handled this placeholder type.
                        default: Plugin.consoleLog.LogError($"Placeholder type {placeholders[placeholderIndex]} not handled!"); break;
                    }

                    // Increment our placeholder index.
                    placeholderIndex++;
                }
            }

            // Return our edited string.
            return String.Join("", split);
        }

    }
}
