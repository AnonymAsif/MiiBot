using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MiiBot
{
    // [SlashCommandGroup("Music", "All MiiBot Music Commands")]
    public class Audio : ApplicationCommandModule
    {
        // Contains the tracks in the queue
        private static LinkedList<LavalinkTrack> trackQueue = new LinkedList<LavalinkTrack>();
        

        // Lavalink States
        private static bool isPlayerPaused, isQueueLooped, isSongLooped, isSkipping = false;


        private async Task<bool> ConnectionChecks(InteractionContext ctx, LavalinkExtension lava)
        {
            if (lava.ConnectedNodes.Any()) return true;
            await Embeds.SendEmbed(ctx, "Problem!", "LavaLink is not configured properly", DiscordColor.Red);
            return false;
        }


        private async Task<bool> ConnectionChecks(InteractionContext ctx, DiscordChannel voiceChannel)
        {
            if (voiceChannel != null) return true;
            await Embeds.SendEmbed(ctx, "Please join a VC first", "MiiBot couldn't figure out which VC to join", DiscordColor.Red);
            return false;
        }


        private async Task<bool> ConnectionChecks(InteractionContext ctx, LavalinkGuildConnection voiceConnection, string command = null)
        {
            if (voiceConnection != null && voiceConnection.CurrentState.CurrentTrack != null) return true;
            else if (voiceConnection == null) await Embeds.SendEmbed(ctx, "Not in Voice Channel", "MiiBot isn't playing anything", DiscordColor.Red);
            else await Embeds.SendEmbed(ctx, "Nothing To " + command, "MiiBot isn't playing anything", DiscordColor.Red);
            return false;
        }

        
        private async Task<bool> ConnectionChecks(InteractionContext ctx, LavalinkNodeConnection node, bool connecting)
        {
            if (connecting != (node.GetGuildConnection(ctx.Guild) != null)) return true;
            if (connecting) await Embeds.SendEmbed(ctx, "Already Connected!", "MiiBot is already connected to your VC", DiscordColor.Red);
            else await Embeds.SendEmbed(ctx, "Already Disconnected!", "MiiBot didn't find any VC to disconnect from", DiscordColor.Red);
            return false;
        }

        
        private async Task<bool> PauseCheck(InteractionContext ctx, bool pausing)
        {
            if (pausing != isPlayerPaused) return true;
            if (pausing && isPlayerPaused) await Embeds.SendEmbed(ctx, "Already Paused", "MiiBot has already paused playback", DiscordColor.Red);
            if (!pausing && !isPlayerPaused) await Embeds.SendEmbed(ctx, "Already Playing", "MiiBot is already playing", DiscordColor.Red);
            return false;
        }


        public async Task<bool> PlayFromQueue(LavalinkGuildConnection voiceConnection, TrackFinishEventArgs e = null)
        {
            // Return if MiiBot isn't connected to a VC
            if (voiceConnection == null) return false;

            LavalinkTrack loopTrack = null;
            // Get track for looping purposes
            if ((isSongLooped || isQueueLooped) && e != null)
            {
                loopTrack = (await voiceConnection.GetTracksAsync(uri: e.Track.Uri)).Tracks.First();
            }

            // After side case is solved, handle looping
            if (isSongLooped && !isSkipping) trackQueue.AddFirst(loopTrack);
            else if (isQueueLooped) trackQueue.AddLast(loopTrack);
            
            if (trackQueue.Count() >= 1)
            {
                Console.WriteLine("PlayFromQueue found a valid track count");
                
                // Get the next song, and then remove it from the queue
                LavalinkTrack track = trackQueue.First();
                trackQueue.RemoveFirst();

                Console.WriteLine("PlayFromQueue got the tracks and removed one");

                // Play song and unpause if paused
                await voiceConnection.PlayAsync(track);
                isPlayerPaused = false;

                Console.WriteLine("PlayFromQueue played the track");

                return true;
            }

            // No longer skipping
            isSkipping = false;

            return false;
        }

        
        [SlashCommand("connect", "Connect to a voice channel")]
        public async Task Connect(InteractionContext ctx,
            [Option("Hidden", "Hide connect message")] bool isHidden = false
        )
        {
            // Get lava client
            var lava = ctx.Client.GetLavalink();
            if (!await ConnectionChecks(ctx, lava)) return;

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();
            if (!await ConnectionChecks(ctx, node, true)) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await ConnectionChecks(ctx, voiceChannel)) return;

            // Connect bot to VC
            await node.ConnectAsync(voiceChannel);

            // Let the user know that the bot connected
            if (!isHidden) await Embeds.SendEmbed(ctx, "Connected", "MiiBot successfully joined the VC", DiscordColor.Green);

            // Get the voice connection object
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Register the playback finished event
            voiceConnection.PlaybackFinished += async (LavalinkGuildConnection _, TrackFinishEventArgs e) => await PlayFromQueue(voiceConnection, e);
        }


        [SlashCommand("disconnect", "Disconnect from a voice channel")]
        public async Task Disconnect(InteractionContext ctx)
        {
            // Get lava client
            var lava = ctx.Client.GetLavalink();
            if (!await ConnectionChecks(ctx, lava)) return;

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();
            if (!await ConnectionChecks(ctx, node, false)) return;

            // Gets the active voice connection and disconnects
            LavalinkGuildConnection voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Remove the playback finished event
            voiceConnection.PlaybackFinished -= async (LavalinkGuildConnection _, TrackFinishEventArgs e) => await PlayFromQueue(voiceConnection, e);

            // Disconnect MiiBot from VC
            await voiceConnection.DisconnectAsync();

            // Let the user know that MiiBot disconnected
            await Embeds.SendEmbed(ctx, "Disconnected", "MiiBot successfully left the VC", DiscordColor.Green);

            // Reset the queue
            trackQueue.Clear();
        }


        [SlashCommand("play", "Play a song using a query")]
        public async Task Play(InteractionContext ctx,
            [Option("Search", "Enter Search Query")] string query = "null",
            [Option("Platform", "Platform to search"), ChoiceAttribute("Youtube", "Youtube"), ChoiceAttribute("SoundCloud", "SoundCloud")] string platform = "Youtube"
        )
        {
            // If nothing was provided
            if (query == "null")
            {
                await Embeds.SendEmbed(ctx, "No Search Query Provided", "MiiBot doesn't have any query to work with", DiscordColor.Red);
                return;
            }

            // Defers response
            await ctx.DeferAsync();

            // Attempt to get the LavaLink connection
            var lava = ctx.Client.GetLavalink();
            
            // If there's a problem with lavalink
            if (!lava.ConnectedNodes.Any())
            {
                await Embeds.EditEmbed(ctx, "Problem!", "Lavalink is not configured properly", DiscordColor.Red);
                return;
            }

            // Attempt to get the user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (voiceChannel == null)
            {
                await Embeds.EditEmbed(ctx, "Please Join a VC First", "MiiBot couldn't figure out which VC to join", DiscordColor.Red);
                return;
            }

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();

            // Attempt to establish the voice connection - if none, connects
            if (node.GetGuildConnection(ctx.Guild) == null) await Connect(ctx, true);
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Stores all tracks from query
            LavalinkLoadResult loadResult;

            // Figure out if the query is a URL
            bool isURL = Uri.IsWellFormedUriString(query, UriKind.Absolute);

            // Gets the platform to search from

            LavalinkSearchType plat;
            if (platform == "Youtube") plat = LavalinkSearchType.Youtube;
            else plat = LavalinkSearchType.SoundCloud;
            
            // Get a list of available tracks from search query
            if (isURL) loadResult = await node.Rest.GetTracksAsync(new Uri(query));
            else loadResult = await node.Rest.GetTracksAsync(query, plat);

            // Error loading track results
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await Embeds.EditEmbed(ctx, "Something Went Wrong", "MiiBot couldn't load results", DiscordColor.Red);
                return;
            }

            // Nothing was found using the query
            if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await Embeds.EditEmbed(ctx, "Nothing Found", "MiiBot couldn't gather any results from your search query", DiscordColor.Red);
                return;
            }

            // Setup client interactivity
            var interactivity = ctx.Client.GetInteractivity();

            // Current track is stored here
            DSharpPlus.Lavalink.LavalinkTrack track;

            // Using custom search
            if (!isURL)
            {
                // Gets first 'trackCount' track results (or however many found)
                int trackCount = 5;
                var trackList = loadResult.Tracks.Take(trackCount);
                trackCount = trackList.Count();

                // Creates embed buttons for tracks
                DiscordComponent[] buttonList = new DiscordComponent[trackCount];
                for (int i = 0; i < trackCount; i++)
                {
                    buttonList[i] = new DiscordButtonComponent(
                        ButtonStyle.Secondary,
                        $"MiiBotPlayButton{i}",
                        $"{i + 1}"
                    );
                }

                // Create descriptions for each selected track 
                string embedDescription = "";
                int trackIndex = 1;
                string length;
                foreach (var trackI in trackList)
                {
                    // Formats track length
                    if (trackI.Length.Hours >= 1) length = trackI.Length.ToString(@"hh\:mm\:ss");
                    else length = trackI.Length.ToString(@"mm\:ss");

                    // Adds to description
                    embedDescription += $"{trackIndex++}: {trackI.Title} ({length})\n";
                }

                // Create the message with everything and send it
                // Done this way because 'songRequestMessage' is needed
                var messageBuilder = new DiscordMessageBuilder()
                .AddEmbed(
                    new DiscordEmbedBuilder
                    {
                        Title = "Select A Song",
                        Description = embedDescription,
                        Color = DiscordColor.Azure
                    }
                )
                .AddComponents(buttonList);
                var songRequestMessage = await messageBuilder.SendAsync(ctx.Channel);

                // Wait for a button to be selected
                var interactionResult = await interactivity.WaitForButtonAsync(songRequestMessage, ctx.User);

                // Gets the selected track (by index)
                track = trackList.ElementAt(interactionResult.Result.Id.Last() - '0');

                // Delete the sent message
                await ctx.Channel.DeleteMessageAsync(songRequestMessage);

                // Queues the selected song
                trackQueue.AddLast(track);
            }
            else
            {
                // Load every URL result into the queue
                foreach (LavalinkTrack trackI in loadResult.Tracks) trackQueue.AddLast(trackI);

                // Set the track to be played to the first in the playlist
                track = loadResult.Tracks.First();
            }

            // If no song is currently playing - queue was empty originally
            if (voiceConnection.CurrentState.CurrentTrack == null)
            {
                // Formats track length
                string length;
                if (track.Length.Hours >= 1) length = track.Length.ToString(@"hh\:mm\:ss");
                else length = track.Length.ToString(@"mm\:ss");
                
                // Let the user know that the song is now playing
                await Embeds.EditEmbed(ctx, "Playing " + track.Title, "By: " + track.Author + "\nLength: " + length, DiscordColor.Azure,
                    url: track.Uri.ToString(),
                    footer: (isURL && loadResult.Tracks.Count() > 1 ? $"Found {loadResult.Tracks.Count()} songs" : null)
                );
                
                // Play song
                await PlayFromQueue(voiceConnection);

                // The player is not paused now that a song is playing
                isPlayerPaused = false;
                return;
            }

            if (!isURL || loadResult.Tracks.Count() == 1)
            {
                // Let the user know that a single song has been queued
                await Embeds.EditEmbed(ctx, "Song Queued", $"[{track.Title}]({track.Uri}) has been queued\nPosition #{trackQueue.Count()}", DiscordColor.Azure);
            }
            else
            {
                // Let the user know that their playlist has been queued
                await Embeds.EditEmbed(ctx, 
                    "Playlist Queued",
                    $"Your [playlist]({query}) has been queued\nPositions #{trackQueue.Count() - loadResult.Tracks.Count() + 1} - #{trackQueue.Count()}", 
                    DiscordColor.Azure
                );
            }
        }


        [SlashCommand("pause", "Pause the currently playing song")]
        public async Task Pause(InteractionContext ctx)
        {
            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection || Checks if already paused
            if (!await ConnectionChecks(ctx, voiceConnection, "Pause") || !await PauseCheck(ctx, true)) return;

            // Pause the currently playing song
            await voiceConnection.PauseAsync();

            // Set the 'isPlayerPaused' variable to true
            isPlayerPaused = true;

            // Lets the user know that the song has been paused
            await Embeds.SendEmbed(ctx, "Song Paused", "MiiBot has paused the current song", DiscordColor.Green);
        }


        [SlashCommand("resume", "Resume the currently paused song")]
        public async Task Resume(InteractionContext ctx)
        {
            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection || Checks if already playing
            if (!await ConnectionChecks(ctx, voiceConnection, "Resume") || !await PauseCheck(ctx, false)) return;

            // Resume the paused song
            await voiceConnection.ResumeAsync();

            // Set the 'isPlayerPaused' variable to true
            isPlayerPaused = false;

            // Let the user know that the song has been resumed
            await Embeds.SendEmbed(ctx, "Song Resumed", "MiiBot has resumed the current song", DiscordColor.Green);
        }


        [SlashCommand("stop", "Stop the currently playing song")]
        public async Task Stop(InteractionContext ctx)
        {
            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection
            if (!await ConnectionChecks(ctx, voiceConnection, "Stop")) return;

            // Reset the queue
            trackQueue.Clear();

            // Stops the song
            await voiceConnection.StopAsync();

            // Lets the user know that the song has been stopped
            await Embeds.SendEmbed(ctx, "Song Stopped", "MiiBot has stopped the current song", DiscordColor.Green);
        }


        [SlashCommand("skip", "Skip the currently playing song")]
        public async Task Skip(InteractionContext ctx)
        {
            // Defers response
            await ctx.DeferAsync();

            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection
            if (!await ConnectionChecks(ctx, voiceConnection, "Skip")) return;

            // Let the 'PlayFromQueue' function know that skipping is intended
            isSkipping = true;

            // Stop the player
            await voiceConnection.StopAsync();

            // Lets the user know that the song has been skipped
            await Embeds.EditEmbed(ctx, "Song Skipped", "Miibot has skipped the current song", DiscordColor.Green);
        }

        
        [SlashCommand("nowplaying", "Display the currently playing song")]
        public async Task NowPlaying(InteractionContext ctx,
            [Option("Hidden","Hide message from other users")] bool isHidden = false
        )
        {
            // Defers response
            await ctx.DeferAsync(ephemeral: isHidden);

            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection
            if (!await ConnectionChecks(ctx, voiceConnection, "Loop")) return;

            // Get the currently playing track
            LavalinkTrack track = voiceConnection.CurrentState.CurrentTrack;

            // Formats track length
            string length;
            if (track.Length.Hours >= 1) length = track.Length.ToString(@"hh\:mm\:ss");
            else length = track.Length.ToString(@"mm\:ss");

            // Let's the user know which song is playing
            await Embeds.EditEmbed(ctx, "Playing " + track.Title, "By: " + track.Author + "\nLength: " + length, DiscordColor.Azure, url: track.Uri.ToString());
        }


        [SlashCommand("loop", "Toggle loop states for song and queue")]
        public async Task Loop(InteractionContext ctx,
            [Option("Type", "Loop type"), ChoiceAttribute("Song", "Song"), ChoiceAttribute("Queue", "Queue")] string loopType = null
        )
        {
            // Nothing was provided
            if (loopType == null) await Embeds.SendEmbed(ctx, "Nothing Provided", "MiiBot couldn't figure out what to loop", DiscordColor.Red);

            if (loopType == "Song")
            {
                // Toggles song loop and lets the user know the song loop state
                isSongLooped = !isSongLooped;
                if (isSongLooped) await Embeds.SendEmbed(ctx, "Song Loop Enabled", "MiiBot has enabled song looping", DiscordColor.Azure);
                else await Embeds.SendEmbed(ctx, "Song Loop Disabled", "MiiBot has disabled song looping", DiscordColor.Azure);
            }
            else if (loopType == "Queue")
            {
                // Toggles queue loop and lets the user know the queue loop state
                isQueueLooped = !isQueueLooped;
                if (isQueueLooped) await Embeds.SendEmbed(ctx, "Queue Loop Enabled", "MiiBot has enabled queue looping", DiscordColor.Azure);
                else await Embeds.SendEmbed(ctx, "Queue Loop Disabled", "MiiBot has disabled queue looping", DiscordColor.Azure);
            }
        }


        [SlashCommand("queue", "List the tracks in the queue")]
        public async Task ListQueue(InteractionContext ctx)
        {
            // If the queue is empty
            if (trackQueue.Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "The queue is empty", DiscordColor.Red);
                return;
            }
            
            // Epic Embed Fail
            string description = "";
            TimeSpan totalLength = new TimeSpan();

            foreach (var it in trackQueue.Select((x, i) => new {track = x, index = i}))
            {
                string trackLength;
                if (it.track.Length.Hours > 0) trackLength = $"{it.track.Length.ToString(@"hh\:mm\:ss")}";
                else trackLength = $"{it.track.Length.ToString(@"mm\:ss")}";

                description += $"`[{it.index + 1}]` **[{it.track.Title}]({it.track.Uri.AbsoluteUri})** `[{trackLength}]`\n";
                totalLength += it.track.Length;
            }
            description += $"\nTotal Tracks: **{trackQueue.Count()}**\nTotal Length: **{totalLength.ToString(@"hh\:mm\:ss")}**";

            // Send the constructed embed with the queue contents
            await Embeds.SendEmbed(ctx, "Current Queue", description, DiscordColor.Azure);
        }

        
        [SlashCommand("clear", "Clear the current queue")]
        public async Task Clear(InteractionContext ctx)
        {
            if (trackQueue.Count() > 0)
            {
                // Clear the queue
                trackQueue.Clear();

                // Let the user know that the queue was cleared
                await Embeds.SendEmbed(ctx, "Queue Cleared", "MiiBot has cleared the queue", DiscordColor.Green);
            }

            // Let the user know that the queue is already empty
            else await Embeds.SendEmbed(ctx, "Queue Already Empty", "MiiBot has nothing to clear", DiscordColor.Red);
        }

        
        [SlashCommand("playindex", "Play a song from the queue using its index")]
        public async Task PlayIndex(InteractionContext ctx,
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
            if (trackQueue.Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "MiiBot has no songs to pick from", DiscordColor.Red);
                return;
            }
            
            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voice connection
            if (!await ConnectionChecks(ctx, voiceConnection, "Play")) return;

            // Invalid index
            if (itemIndex > trackQueue.Count() || itemIndex < 0)
            {
                await Embeds.SendEmbed(ctx, "Item Does Not Exist", "MiiBot couldn't get the song at this position", DiscordColor.Red);
            }
            else
            {
                // Get the track, while also deleting it from the queue
                var track = trackQueue.ElementAt((int)itemIndex - 1);
                trackQueue.Remove(trackQueue.ElementAt((int)itemIndex - 1));
                trackQueue.AddFirst(track);

                // Play the queued song
                await voiceConnection.StopAsync();

                // Let the user know that the song is now playing
                await Embeds.SendEmbed(ctx, "Playing " + track.Title, "By: " + track.Author + "\nLength: " + track.Length, DiscordColor.Azure, url: track.Uri.ToString());
            }
        }


        [SlashCommand("moveindex", "Move a song from the queue using its index to another index")]
        public async Task MoveIndex(InteractionContext ctx,
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

            // Queue is empty
            if (trackQueue.Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "MiiBot has no songs to move", DiscordColor.Red);
                return;
            }

            // Convert long to byte for min and max funcs
            byte itemStartIndexByte = (byte)itemStartIndex;
            byte itemEndIndexByte = (byte)itemEndIndex;
            
            // Handle every invalid case with provided indices
            if (Math.Max(itemStartIndexByte, itemEndIndexByte) > trackQueue.Count() || Math.Min(itemStartIndexByte, itemEndIndexByte) < 0)
            {
                await Embeds.SendEmbed(ctx, "Invalid Positions", "Miibot could not move song to that position", DiscordColor.Red);
                return;
            }

            // Get the track and delete it from the queue
            var track = trackQueue.ElementAt((int)itemStartIndex - 1);
            trackQueue.Remove(track);

            // If the user wants to move it to be at the end of the queue, or not
            if (itemEndIndex == trackQueue.Count() + 1) trackQueue.AddAfter(trackQueue.Find(trackQueue.ElementAt((int)itemEndIndex - 2)), track);
            else trackQueue.AddBefore(trackQueue.Find(trackQueue.ElementAt((int)itemEndIndex - 1)), track);

            // Let the user know that the song has been moved
            await Embeds.SendEmbed(ctx, "Song Moved", $"[{track.Title}]({track.Uri}) has been moved from #{itemStartIndex} to #{itemEndIndex}", DiscordColor.Green);
        }


        [SlashCommand("removeindex", "Remove a song from the queue using its index")]
        public async Task RemoveIndex(InteractionContext ctx,
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
            if (trackQueue.Count() == 0)
            {
                await Embeds.SendEmbed(ctx, "Empty Queue", "MiiBot has no songs to remove", DiscordColor.Red);
                return;
            }

            // If only one is supplied, set them both to the one with the given value
            if (itemStartIndex != null && itemEndIndex == null) itemEndIndex = itemStartIndex;
            if (itemStartIndex == null && itemEndIndex != null) itemStartIndex = itemEndIndex;

            // Convert long to byte for min and max funcs
            byte itemStartIndexByte = (byte)itemStartIndex;
            byte itemEndIndexByte = (byte)itemEndIndex;

            // Handle every invalid case with provided indices
            if (Math.Max(itemStartIndexByte, itemEndIndexByte) > trackQueue.Count() || Math.Min(itemStartIndexByte, itemEndIndexByte) < 0 || itemEndIndex < itemStartIndex)
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
                    var trackI = trackQueue.ElementAt((int)itemStartIndex - 1); 
                    description += $"[{trackI.Title}]({trackI.Uri})\n";
                    trackQueue.Remove(trackI);
                }

                // Send the embed
                await Embeds.SendEmbed(ctx, "Song(s) Removed", description, DiscordColor.Green);
            }
        }


        [SlashCommand("savequeue", "Save the current queue to be used later")]
        public async Task SaveQueue(InteractionContext ctx,
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
            
            // Queue is empty
            if (trackQueue.Count() == 0)
            {
                await Embeds.EditEmbed(ctx, "Empty Queue", "MiiBot has no songs to save", DiscordColor.Red);
                return;
            }

            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Outer Dictionary - Maps guild ID : Dictionary of guild saved queues
            // Inner Dictionary - Maps queuename : List of Uris
            Dictionary<string, Dictionary<string, List<string>>> queueDictionary;

            try
            {
                FileStream fileStream = new FileStream("Data/Audio/Queues.json", FileMode.Open);
                using (StreamReader reader = new StreamReader (fileStream))
                {
                    // Get the text as a string from the file
                    string text = reader.ReadToEnd();

                    // Convert json string from file to the queueDictionary type and set it to it
                    queueDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(text);
                }
                fileStream.Close();
            }
            catch (Exception)
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
            foreach (var track in trackQueue) queueArray.Add(track.Uri.ToString());

            // If the guild isn't in the dicationary, add it
            if (!queueDictionary.ContainsKey(ctx.Guild.Id.ToString()))
            {
                queueDictionary.Add(ctx.Guild.Id.ToString(), new Dictionary<string, List<string>>());
            }

            // If the queue name isn't in the dictionary, add it
            if (!queueDictionary[ctx.Guild.Id.ToString()].ContainsKey(queueName))
            {
                queueDictionary[ctx.Guild.Id.ToString()].Add(queueName, new List<string>());
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
                    queueDictionary[ctx.Guild.Id.ToString()][queueName] = queueArray;
                    return;
                }
                else if (interactionResult.Result.Id == "MiiBotSaveQueueButtonDeny")
                {
                    await Embeds.EditEmbed(ctx, "Save Queue Cancelled", "MiiBot has canceled the save operation", DiscordColor.Azure);
                    return;
                }
            }

            // If no overwrite, just add the new entry
            queueDictionary[ctx.Guild.Id.ToString()][queueName] = queueArray;

            try
            {
                // Create a file if it doesn't exist, or overwrite if it does
                FileStream fileStream = new FileStream("Data/Audio/Queues.json", FileMode.Create);
                using (StreamWriter writer = new StreamWriter (fileStream))
                {
                    // Convert the 'queueDictionary' object to json text
                    var json = JsonConvert.SerializeObject(queueDictionary, Formatting.Indented);

                    // Update the file with the new text
                    writer.Write(json);
                }
                fileStream.Close();
            }
            catch (Exception)
            {
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot could not save your queue to the database", DiscordColor.Red);
                return;
            }
            
            // Let the user know that their queue has been saved
            await Embeds.EditEmbed(ctx, "Queue Saved", "MiiBot has saved your queue", DiscordColor.Green);
        }

        [SlashCommand("loadqueue", "Load a saved queue and play it")]
        public async Task LoadQueue(InteractionContext ctx, 
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

            // Outer Dictionary - Maps guild ID : Dictionary of guild saved queues
            // Inner Dictionary - Maps queuename : List of Uris
            Dictionary<string, Dictionary<string, List<string>>> queueDictionary;

            try
            {
                FileStream fileStream = new FileStream("Data/Audio/Queues.json", FileMode.Open);
                using (StreamReader reader = new StreamReader (fileStream))
                {
                    // Get the text as a string from the file
                    string text = reader.ReadToEnd();

                    // Convert json string from file to the queueDictionary type and set it to it
                    queueDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(text);
                }
                fileStream.Close();
            }
            catch (Exception)
            {   
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot is having trouble getting the queues from the database", DiscordColor.Red);
                return;
            }
   
            // If the guild isn't in the dictionary, error
            if (!queueDictionary.ContainsKey(ctx.Guild.Id.ToString()))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // If the queue name isn't in the dictionary, add it
            if (!queueDictionary[ctx.Guild.Id.ToString()].ContainsKey(queueName))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Prepare to overwrite the queue if demanded
            if (overwriteQueue)
            {
                // Checks for valid voice connection
                if (voiceConnection == null)
                {
                    await Embeds.EditEmbed(ctx, "Not in VC", "MiiBot wasn't able to overwrite the queue", DiscordColor.Red);
                    return;
                }
                
                // Clear the queue
                trackQueue.Clear();

                // Only stop if something is playing
                if (voiceConnection.CurrentState.CurrentTrack != null) await voiceConnection.StopAsync();
            }

            var trackCount = trackQueue.Count();
            foreach (string url in queueDictionary[ctx.Guild.Id.ToString()][queueName])
            {
                // Get a list of available tracks from search query
                var loadResult = await node.Rest.GetTracksAsync(new Uri(url));
                trackQueue.AddLast(loadResult.Tracks.First());
            }
            
            // If no song is currently playing - queue was empty originally
            if (voiceConnection.CurrentState.CurrentTrack == null)
            {
                var track = trackQueue.ElementAt(trackQueue.Count() - trackCount);
                
                // Let the user know that the song is now playing
                await Embeds.EditEmbed(ctx, "Playing " + track.Title, "By: " + track.Author + "\nLength: " + track.Length, DiscordColor.Azure,
                    url: track.Uri.ToString(),
                    footer: (trackQueue.Count() - trackCount > 1 ? $"Found {trackQueue.Count() - trackCount} songs" : null)
                );
                
                // Play song
                await PlayFromQueue(voiceConnection);
    
                // The player is not paused now that a song is playing
                isPlayerPaused = false;
                return;
            }
            
            await Embeds.EditEmbed(ctx, "Queue Loaded", "Your selected queue has been loaded", DiscordColor.Green);
        }

        
        [SlashCommand("removequeue", "Remove a saved queue from the database")]
        public async Task RemoveQueue(InteractionContext ctx, 
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
            Dictionary<string, Dictionary<string, List<string>>> queueDictionary;

            try
            {
                FileStream fileStream = new FileStream("Data/Audio/Queues.json", FileMode.Open);
                using (StreamReader reader = new StreamReader (fileStream))
                {
                    // Get the text as a string from the file
                    string text = reader.ReadToEnd();

                    // Convert json string from file to the queueDictionary type and set it to it
                    queueDictionary = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, List<string>>>>(text);
                }
                fileStream.Close();
            }
            catch (Exception)
            {
                await Embeds.EditEmbed(ctx, "Unable to Access Database", "MiiBot is having trouble getting the queues from the database", DiscordColor.Red);
                return;
            }
   
            // If the guild isn't in the dictionary, error
            if (!queueDictionary.ContainsKey(ctx.Guild.Id.ToString()))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // If the queue name isn't in the dictionary, error
            if (!queueDictionary[ctx.Guild.Id.ToString()].ContainsKey(queueName))
            {
                await Embeds.EditEmbed(ctx, "Queue Not Found", "Your selected queue does not exist in the database", DiscordColor.Red);
                return;
            }

            // Remove the entry matching the queue name that was provided
            queueDictionary[ctx.Guild.Id.ToString()].Remove(queueName);
            
            await Embeds.EditEmbed(ctx, "Queue Removed", "Your selected queue has been removed from the database", DiscordColor.Green);
        }
    }
}