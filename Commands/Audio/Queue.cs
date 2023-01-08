using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiiBot
{
    [SlashCommandGroup("Queue", "All MiiBot Queue Commands")]
    public class Queue : ApplicationCommandModule
    {
        // Contains the tracks in the queue for each guild
        public static Dictionary<ulong, LinkedList<LavalinkTrack>> trackQueues = new Dictionary<ulong, LinkedList<LavalinkTrack>>();

        
        public static async Task<bool> PlayNextSong(LavalinkGuildConnection voiceConnection, TrackFinishEventArgs e = null)
        {   
            // Return if MiiBot isn't connected to a VC
            if (voiceConnection == null) return false;

            LavalinkTrack loopTrack = null;

            // Get lavalinkstates for the guild
            bool isQueueLooped = Player.lavalinkStates[voiceConnection.Guild.Id]["isQueueLooped"];
            bool isSongLooped = Player.lavalinkStates[voiceConnection.Guild.Id]["isSongLooped"];
            bool isSkipping = Player.lavalinkStates[voiceConnection.Guild.Id]["isSkipping"];

            
            // Get track for looping purposes
            if ((isSongLooped || isQueueLooped) && e != null)
            {
                loopTrack = (await voiceConnection.GetTracksAsync(uri: e.Track.Uri)).Tracks.First();
            }

            // After side case is solved, handle looping
            if (isSongLooped && !isSkipping) trackQueues[voiceConnection.Guild.Id].AddFirst(loopTrack);
            else if (isQueueLooped) trackQueues[voiceConnection.Guild.Id].AddLast(loopTrack);
            
            if (trackQueues[voiceConnection.Guild.Id].Count() >= 1)
            {
                // Get the next song, and then remove it from the queue
                LavalinkTrack track = trackQueues[voiceConnection.Guild.Id].First();
                trackQueues[voiceConnection.Guild.Id].RemoveFirst();

                // Play song and unpause if paused
                await voiceConnection.PlayAsync(track);
                Player.lavalinkStates[voiceConnection.Guild.Id]["isPlayerPaused"] = false;

                return true;
            }

            // No longer skipping
            Player.lavalinkStates[voiceConnection.Guild.Id]["isSkipping"] = false;

            return false;
        }
        
        
        [SlashCommand("list", "List the tracks in the queue")]
        public async Task List(InteractionContext ctx)
        {   
            // If the queue is empty
            if (trackQueues[ctx.Guild.Id].Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "The queue is empty", DiscordColor.Red);
                return;
            }
            
            // Epic Embed Fail
            string description = "";
            TimeSpan totalLength = new TimeSpan();

            foreach (var it in trackQueues[ctx.Guild.Id].Select((x, i) => new {track = x, index = i}))
            {
                string trackLength;
                if (it.track.Length.Hours > 0) trackLength = $"{it.track.Length.ToString(@"hh\:mm\:ss")}";
                else trackLength = $"{it.track.Length.ToString(@"mm\:ss")}";

                description += $"`[{it.index + 1}]` **[{it.track.Title}]({it.track.Uri.AbsoluteUri})** `[{trackLength}]`\n";
                totalLength += it.track.Length;
            }
            description += $"\nTotal Tracks: **{trackQueues[ctx.Guild.Id].Count()}**\nTotal Length: **{totalLength.ToString(@"hh\:mm\:ss")}**";

            // Send the constructed embed with the queue contents
            await Embeds.SendEmbed(ctx, "Current Queue", description, DiscordColor.Azure);
        }

        
        [SlashCommand("clear", "Clear the current queue")]
        public async Task Clear(InteractionContext ctx)
        {
            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "clear")) return;
            
            if (trackQueues[ctx.Guild.Id].Count() > 0)
            {
                // Clear the queue
                trackQueues[ctx.Guild.Id].Clear();

                // Let the user know that the queue was cleared
                await Embeds.SendEmbed(ctx, "Queue Cleared", "MiiBot has cleared the queue", DiscordColor.Green);
            }

            // Let the user know that the queue is already empty
            else await Embeds.SendEmbed(ctx, "Queue Already Empty", "MiiBot has nothing to clear", DiscordColor.Red);
        }

        
        [SlashCommand("play", "Play a song from the queue using its index")]
        public async Task Play(InteractionContext ctx,
            [Option("Index", "Item index", true)] long? itemIndex = null
        )
        {   
            // No index provided
            if (itemIndex == null)
            {
                await Embeds.SendEmbed(ctx, "No Index Provided", "MiiBot couldn't figure out which song to play", DiscordColor.Red);
                return;
            }

            // Queue is empty
            if (trackQueues[ctx.Guild.Id].Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "MiiBot has no songs to pick from", DiscordColor.Red);
                return;
            }
            
            (var lava, var node, var voiceConnection) = await Player.GetVoiceConnection(ctx);

            // Checks for valid voice connection
            if (!await Validation.CheckConnection(ctx, voiceConnection, "Play")) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "play in")) return;

            // Invalid index
            if (itemIndex > trackQueues[ctx.Guild.Id].Count() || itemIndex < 0)
            {
                await Embeds.SendEmbed(ctx, "Item Does Not Exist", "MiiBot couldn't get the song at this position", DiscordColor.Red);
            }
            else
            {
                // Get the track, while also deleting it from the queue
                var track = trackQueues[ctx.Guild.Id].ElementAt((int)itemIndex - 1);
                trackQueues[ctx.Guild.Id].Remove(trackQueues[ctx.Guild.Id].ElementAt((int)itemIndex - 1));
                trackQueues[ctx.Guild.Id].AddFirst(track);

                // Play the queued song
                await voiceConnection.StopAsync();

                // Let the user know that the song is now playing
                await Embeds.SendEmbed(ctx, "Playing " + track.Title, "By: " + track.Author + "\nLength: " + track.Length, DiscordColor.Azure, url: track.Uri.ToString());
            }
        }


        [SlashCommand("move", "Move a song from the queue using its index to another index")]
        public async Task Move(InteractionContext ctx,
            [Option("SongIndex", "The item's current index", true)] long? itemStartIndex = null,
            [Option("NewIndex", "The item's new index")] long? itemEndIndex = null
        )
        {   
            // No index provided
            if (itemStartIndex == null && itemEndIndex == null)
            {
                await Embeds.SendEmbed(ctx, "No Index Provided", "MiiBot couldn't figure out which song to move", DiscordColor.Red);
                return;
            }

            // If they are the same, then nothing will be moved
            if (itemStartIndex == itemEndIndex)
            {
                await Embeds.SendEmbed(ctx, "Nothing To Move", "MiiBot can't move the song to the same location", DiscordColor.Red);
                return;
            }

            // Queue is empty - this also covers the case of Miibot not being in VC
            if (trackQueues[ctx.Guild.Id].Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "MiiBot has no songs to move", DiscordColor.Red);
                return;
            }

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "move in")) return;

            // Convert long to byte for min and max funcs
            byte itemStartIndexByte = (byte)itemStartIndex;
            byte itemEndIndexByte = (byte)itemEndIndex;

            ulong id = ctx.Guild.Id;
            // Handle every invalid case with provided indices
            if (Math.Max(itemStartIndexByte, itemEndIndexByte) > trackQueues[id].Count() || Math.Min(itemStartIndexByte, itemEndIndexByte) < 0)
            {
                await Embeds.SendEmbed(ctx, "Invalid Positions", "Miibot could not move song to that position", DiscordColor.Red);
                return;
            }

            // Get the track and delete it from the queue
            var track = trackQueues[id].ElementAt((int)itemStartIndex - 1);
            trackQueues[ctx.Guild.Id].Remove(track);

            // Get the current amount of songs in the queue for the guild
            int queueTracksCount = trackQueues[id].Count();
            
            // If the user wants to move it to be at the end of the queue, or not
            if (itemEndIndex == queueTracksCount + 1) trackQueues[id].AddAfter(trackQueues[id].Find(trackQueues[id].ElementAt((int)itemEndIndex - 2)), track);
            else trackQueues[id].AddBefore(trackQueues[id].Find(trackQueues[id].ElementAt((int)itemEndIndex - 1)), track);

            // Let the user know that the song has been moved
            await Embeds.SendEmbed(ctx, "Song Moved", $"[{track.Title}]({track.Uri}) has been moved from #{itemStartIndex} to #{itemEndIndex}", DiscordColor.Green);
        }


        [SlashCommand("remove", "Remove a song from the queue using its index")]
        public async Task Remove(InteractionContext ctx,
            [Option("StartIndex", "Item start index", true)] long? itemStartIndex = null,
            [Option("EndIndex", "Item end index")] long? itemEndIndex = null
        )
        {   
            // No index provided
            if (itemStartIndex == null && itemEndIndex == null)
            {
                await Embeds.SendEmbed(ctx, "No Index Provided", "MiiBot couldn't figure out which song to remove", DiscordColor.Red);
                return;
            }

            // Queue is empty
            if (trackQueues[ctx.Guild.Id].Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "MiiBot has no songs to remove", DiscordColor.Red);
                return;
            }
            
            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "remove from")) return;
            

            // If only one is supplied, set them both to the one with the given value
            if (itemStartIndex != null && itemEndIndex == null) itemEndIndex = itemStartIndex;
            if (itemStartIndex == null && itemEndIndex != null) itemStartIndex = itemEndIndex;

            // Convert long to byte for min and max funcs
            byte itemStartIndexByte = (byte)itemStartIndex;
            byte itemEndIndexByte = (byte)itemEndIndex;

            // Handle every invalid case with provided indices
            if (Math.Max(itemStartIndexByte, itemEndIndexByte) > trackQueues[ctx.Guild.Id].Count() 
                    || Math.Min(itemStartIndexByte, itemEndIndexByte) < 0 
                    || itemEndIndex < itemStartIndex
                )
            { 
                if (itemStartIndex == itemEndIndex)
                {
                    await Embeds.SendEmbed(ctx, "Item Does Not Exist", "MiiBot couldn't get the song at this position", DiscordColor.Red);
                }
                else
                {
                    await Embeds.SendEmbed(ctx, "Invalid Range", "MiiBot couldn't get the songs in this range", DiscordColor.Red);
                }
            }
            else
            {
                // Create the embed
                // The for loop works for any amount of songs provided
                string description = "Your song(s) have been removed from the queue:\n";
                for (int i = 0; i <= itemEndIndex - itemStartIndex; i++)
                {
                    var trackI = trackQueues[ctx.Guild.Id].ElementAt((int)itemStartIndex - 1); 
                    description += $"[{trackI.Title}]({trackI.Uri})\n";
                    trackQueues[ctx.Guild.Id].Remove(trackI);
                }

                // Send the embed
                await Embeds.SendEmbed(ctx, "Song(s) Removed", description, DiscordColor.Green);
            }
        }


        [SlashCommand("save", "Save the current queue to be used later")]
        public async Task Save(InteractionContext ctx,
            [Option("SaveCurrentSong", "Save the currently playing song as well")] bool saveCurrent = true,
            [Option("QueueName", "Name your queue", true)] string queueName = null
        )
        {   
            // Defers response
            await ctx.DeferAsync();
            
            // No name provided
            if (queueName == null)
            {
                await Embeds.EditEmbed(ctx, "No Name Provided", "MiiBot couldn't figure out what to name your queue", DiscordColor.Red);
                return;
            }
            
            // Queue is empty - also covers the case of Miibot being outside of VC
            if (trackQueues[ctx.Guild.Id].Count() == 0)
            {
                await Embeds.EditEmbed(ctx, "Empty Queue", "MiiBot has no songs to save", DiscordColor.Red);
                return;
            }

            // Attempt to get the user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (voiceChannel == null)
            {
                await Embeds.EditEmbed(ctx, "Please Join a VC First", "MiiBot couldn't figure out which VC to save", DiscordColor.Red);
                return;
            }

            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await Player.GetVoiceConnection(ctx);

            // Outer Dictionary - Maps guild ID : Dictionary of guild saved queues
            // Inner Dictionary - Maps queuename : List of Uris
            Dictionary<ulong, Dictionary<string, List<string>>> queueDictionary = await Database.ReadDB();

            // Read Operation Failed
            if (queueDictionary.Count() == 0)
            {
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot is having trouble getting the queues", DiscordColor.Red);
                return;
            }
            
            // Create the array to save with all contents
            List<string> queueArray = new List<string>();
            
            if (saveCurrent)
            {
                // Checks for valid voice connection
                if (voiceConnection == null)
                {
                    await Embeds.EditEmbed(ctx, "Not in VC", "MiiBot wasn't able to get the current song", DiscordColor.Red);
                    return;
                }
                queueArray.Add(voiceConnection.CurrentState.CurrentTrack.Uri.ToString());
            }
            
            // Add all song urls afterwards
            foreach (var track in trackQueues[ctx.Guild.Id]) queueArray.Add(track.Uri.ToString());

            // If the guild isn't in the dicationary, add it
            if (!queueDictionary.ContainsKey(ctx.Guild.Id))
            {
                queueDictionary.Add(ctx.Guild.Id, new Dictionary<string, List<string>>());
            }

            // If the queue name isn't in the dictionary, add it
            if (!queueDictionary[ctx.Guild.Id].ContainsKey(queueName))
            {
                queueDictionary[ctx.Guild.Id].Add(queueName, new List<string>());
            }
            else 
            {
                // Setup client interactivity
                var interactivity = ctx.Client.GetInteractivity();

                // Create a message to inform the user of this with
                // Confirm and deny buttons to react accordingly
                var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(
                    new DiscordEmbedBuilder
                    {
                        Title = "Queue Already Exists",
                        Description = "Miibot found another queue with this name. Would you like to overwrite it?",
                        Color = DiscordColor.Azure
                    }
                )
                .AddComponents(new DiscordComponent[] {
                    new DiscordButtonComponent(
                        ButtonStyle.Secondary,
                        $"MiiBotSaveQueueButtonConfirm",
                        $"Yes"
                    ),
                    new DiscordButtonComponent(
                        ButtonStyle.Secondary,
                        $"MiiBotSaveQueueButtonDeny",
                        $"No"
                    )
                });

                // Send the message and store it
                var overwriteMessage = await messageBuilder.SendAsync(ctx.Channel);

                // Wait for a button to be selected
                var interactionResult = await interactivity.WaitForButtonAsync(overwriteMessage, ctx.User);

                if (interactionResult.Result.Id == "MiiBotSaveQueueButtonConfirm")
                {
                    queueDictionary[ctx.Guild.Id][queueName] = queueArray;
                    return;
                }
                else if (interactionResult.Result.Id == "MiiBotSaveQueueButtonDeny")
                {
                    await Embeds.EditEmbed(ctx, "Save Queue Cancelled", "MiiBot has canceled the save operation", DiscordColor.Azure);
                    return;
                }
            }

            // If no overwrite, just add the new entry
            queueDictionary[ctx.Guild.Id][queueName] = queueArray;

            // Writes new queue to DB
            if (!await Database.WriteDB(queueDictionary))
            {
                await Embeds.EditEmbed(ctx, "Something Went Wrong", "MiiBot was unable to save your queue", DiscordColor.Green);
                return;
            }
            
            // Let the user know that their queue has been saved
            await Embeds.EditEmbed(ctx, "Queue Saved", "MiiBot has saved your queue", DiscordColor.Green);
        }
        

        [SlashCommand("load", "Load a saved queue and play it")]
        public async Task Load(InteractionContext ctx, 
            [Option("QueueName", "Select queue", true)] string queueName = null,
            [Option("Overwrite", "Overwrite current queue")] bool overwriteQueue = false
        )
        {
            // Defers response
            await ctx.DeferAsync();

            // No name provided
            if (queueName == null)
            {
                await Embeds.EditEmbed(ctx, "No Name Provided", "MiiBot couldn't figure out what to name your queue", DiscordColor.Red);
                return;
            }

            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await Player.GetVoiceConnection(ctx);

            // Checks for valid voiceConnection
            if (voiceConnection == null) 
            {
                await Embeds.EditEmbed(ctx, "Not in Voice Channel", "MiiBot isn't playing anything", DiscordColor.Red);
                return;
            }

            // Attempt to get the user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (voiceChannel == null)
            {
                await Embeds.EditEmbed(ctx, "Please Join a VC First", "MiiBot couldn't figure out which VC to load into", DiscordColor.Red);
                return;
            }

            // Outer Dictionary - Maps guild ID : Dictionary of guild saved queues
            // Inner Dictionary - Maps queuename : List of Uris
            Dictionary<ulong, Dictionary<string, List<string>>> queueDictionary = await Database.ReadDB();

            // Read Operation Failed
            if (queueDictionary.Count() == 0)
            {
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot is having trouble getting the queues", DiscordColor.Red);
                return;
            }
   
            // If the guild isn't in the dictionary
            if (!queueDictionary.ContainsKey(ctx.Guild.Id))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // If the queue name isn't in the dictionary
            if (!queueDictionary[ctx.Guild.Id].ContainsKey(queueName))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // Prepare to overwrite the queue if demanded
            if (overwriteQueue)
            {   
                // Clear the queue
                trackQueues[ctx.Guild.Id].Clear();

                // Stop if something is playing
                if (voiceConnection.CurrentState.CurrentTrack != null) await voiceConnection.StopAsync();
            }
            
            int invalidUrls = 0;
            foreach (string url in queueDictionary[ctx.Guild.Id][queueName])
            {
                // Get a list of available tracks from search query
                if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                {
                    var loadResult = await node.Rest.GetTracksAsync(new Uri(url));
                    trackQueues[ctx.Guild.Id].AddLast(loadResult.Tracks.First());
                }
                else invalidUrls++;
            }

            // If MiiBot found invalid URLs
            if (invalidUrls > 0) 
            {
                var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder
                    {
                        Title = "Invalid URLs Found",
                        Description = $"MiiBot found {invalidUrls} invalid URLs in the saved queue",
                        Color = DiscordColor.Red
                    });
                var songRequestMessage = await messageBuilder.SendAsync(ctx.Channel);
            }

            // Number of tracks in queue
            var trackCount = trackQueues[ctx.Guild.Id].Count();
            
            // If no song is currently playing - queue was empty originally
            if (voiceConnection.CurrentState.CurrentTrack == null)
            {
                var track = trackQueues[ctx.Guild.Id].ElementAt(trackQueues[ctx.Guild.Id].Count() - trackCount);
                
                // Formats track length
                string length;
                if (track.Length.Hours >= 1) length = track.Length.ToString(@"hh\:mm\:ss");
                else length = track.Length.ToString(@"mm\:ss");

                
                // Let the user know that the song is now playing
                await Embeds.EditEmbed(ctx, "Playing " + track.Title, "By: " + track.Author + "\nLength: " + length, DiscordColor.Azure,
                    url: track.Uri.ToString(),
                    footer: (trackQueues[ctx.Guild.Id].Count() - trackCount > 1 ? $"Found {trackQueues[ctx.Guild.Id].Count() - trackCount} songs" : null)
                );
                
                // Play song
                await PlayNextSong(voiceConnection);
    
                // The player is not paused now that a song is playing
                Player.lavalinkStates[ctx.Guild.Id]["isPlayerPaused"] = false;
                return;
            }
            await Embeds.EditEmbed(ctx, "Queue Loaded", "Your selected queue has been loaded", DiscordColor.Green);
        }

        
        [SlashCommand("remove", "Remove a saved queue from the database")]
        public async Task Remove(InteractionContext ctx, 
            [Option("QueueName", "Select queue", true)] string queueName = null
        )
        {
            // Defers response
            await ctx.DeferAsync();

            // No name provided
            if (queueName == null)
            {
                await Embeds.EditEmbed(ctx, "No Name Provided", "MiiBot couldn't figure out what queue to look for", DiscordColor.Red);
                return;
            }

            // Outer Dictionary - Maps guild ID : Dictionary of guild saved queues
            // Inner Dictionary - Maps queuename : List of Uris
            Dictionary<ulong, Dictionary<string, List<string>>> queueDictionary = await Database.ReadDB();

            // Read Operation Failed
            if (queueDictionary.Count() == 0)
            {
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot is having trouble getting the queues", DiscordColor.Red);
                return;
            }
            
            // If the guild isn't in the dictionary, error
            if (!queueDictionary.ContainsKey(ctx.Guild.Id))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // If the queue name isn't in the dictionary, error
            if (!queueDictionary[ctx.Guild.Id].ContainsKey(queueName))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // Remove the entry matching the queue name that was provided
            queueDictionary[ctx.Guild.Id].Remove(queueName);

            bool result = await Database.WriteDB(queueDictionary);

            if (!result)
            {
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot could not save your queue to the database", DiscordColor.Red);
                return;
            }
            
            await Embeds.EditEmbed(ctx, "Queue Removed", "Your selected queue has been removed from the database", DiscordColor.Green);
        }

        [SlashCommand("view", "View the saved queues in the database")]
        public async Task View(InteractionContext ctx)
        {
            // Defers response
            await ctx.DeferAsync();
            (var lava, var node, var voiceConnection) = await Player.GetVoiceConnection(ctx);

            string description = "";

            // Outer Dictionary - Maps guild ID : Dictionary of guild saved queues
            // Inner Dictionary - Maps queuename : List of Uris
            Dictionary<ulong, Dictionary<string, List<string>>> queueDictionary = await Database.ReadDB();

            // Read Operation Failed
            if (queueDictionary.Count() == 0)
            {
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot is having trouble getting the queues", DiscordColor.Red);
                return;
            }
            
            foreach (KeyValuePair<string, List<string>> queueName in queueDictionary[ctx.Guild.Id])
            {
                TimeSpan length = new TimeSpan();
                foreach (string url in queueName.Value)
                {
                    TimeSpan trackLength = (await voiceConnection.GetTracksAsync(new Uri(url))).Tracks.First().Length;
                    length += trackLength;
                }
                string queueLength;
                if (length.Hours > 0) queueLength = length.ToString(@"hh\:mm\:ss");
                else queueLength = length.ToString(@"mm\:ss");
                
                description += $"`[{queueName.Key}]` : {queueName.Value.Count()} `[{queueLength}]`\n";                
            }
            
            await Embeds.EditEmbed(ctx, "Saved Queues", description, DiscordColor.Azure);
        }
    }
}