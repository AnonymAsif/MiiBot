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
    [SlashCommandGroup("Player", "All MiiBot Music Player Commands")]
    public class Player : ApplicationCommandModule
    {   
        // Lavalink States for each guild 
        public static Dictionary<ulong, Dictionary<string, bool>> lavalinkStates = new Dictionary<ulong, Dictionary<string, bool>>();

        public async Task<Tuple<LavalinkExtension, LavalinkNodeConnection, LavalinkGuildConnection>> GetVoiceConnection(InteractionContext ctx)
        {
            // Get the voice connection object
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Return the items in a tuple
            return new Tuple<LavalinkExtension, LavalinkNodeConnection, LavalinkGuildConnection>(lava, node, voiceConnection);
        }
        
        [SlashCommand("connect", "Connect to a voice channel")]
        public async Task Connect(InteractionContext ctx,
            [Option("Hidden", "Hide connect message")] bool isHidden = false
        )
        {
            // Get lava client
            var lava = ctx.Client.GetLavalink();
            if (!await Validation.CheckConnection(ctx, lava)) return;

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();
            if (!await Validation.CheckConnection(ctx, node, true)) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "join")) return;

            // Connect MiiBot to VC
            await node.ConnectAsync(voiceChannel);

            // Let the user know that the bot connected
            if (!isHidden) await Embeds.SendEmbed(ctx, "Connected", "MiiBot successfully joined the VC", DiscordColor.Green);

            // Get the voice connection object
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Register the playback finished event
            voiceConnection.PlaybackFinished += async (LavalinkGuildConnection _, TrackFinishEventArgs e) => await Queue.PlayNextSong(voiceConnection, e);

            if (!lavalinkStates.ContainsKey(ctx.Guild.Id))
            {
                lavalinkStates.Add(ctx.Guild.Id, new Dictionary<string, bool>
                {
                    {"isPlayerPaused", false},
                    {"isQueueLooped", false},
                    {"isSongLooped", false},
                    {"isSkipping", false}
                });
                
                // Register a queue list for the guild
                Queue.trackQueues.Add(ctx.Guild.Id, new LinkedList<LavalinkTrack>());
            }
        }


        [SlashCommand("disconnect", "Disconnect from a voice channel")]
        public async Task Disconnect(InteractionContext ctx)
        {   
            // Get lava client
            var lava = ctx.Client.GetLavalink();
            if (!await Validation.CheckConnection(ctx, lava)) return;

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();
            if (!await Validation.CheckConnection(ctx, node, false)) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "leave")) return;

            // Gets the active voice connection and disconnects
            LavalinkGuildConnection voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Remove the playback finished event
            voiceConnection.PlaybackFinished -= async (LavalinkGuildConnection _, TrackFinishEventArgs e) => await Queue.PlayNextSong(voiceConnection, e);

            // Disconnect MiiBot from VC
            await voiceConnection.DisconnectAsync();

            // Let the user know that MiiBot disconnected
            await Embeds.SendEmbed(ctx, "Disconnected", "MiiBot successfully left the VC", DiscordColor.Green);

            // Reset the queue
            Queue.trackQueues[ctx.Guild.Id].Clear();

            // Reset the lavalink states
            lavalinkStates[ctx.Guild.Id]["isPlayerPaused"] = false;
            lavalinkStates[ctx.Guild.Id]["isQueueLooped"] = false;
            lavalinkStates[ctx.Guild.Id]["isSongLooped"] = false;
            lavalinkStates[ctx.Guild.Id]["isSkipping"] = false;
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
                Queue.trackQueues[ctx.Guild.Id].AddLast(track);
            }
            else
            {
                // Load every URL result into the queue
                foreach (LavalinkTrack trackI in loadResult.Tracks) Queue.trackQueues[ctx.Guild.Id].AddLast(trackI);

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
                await Queue.PlayNextSong(voiceConnection);
                
                // The player is not paused now that a song is playing
                lavalinkStates[ctx.Guild.Id]["isPlayerPaused"] = false;
                return;
            }

            // Get the current amount of songs in the queue for the guild
            int queueTracksCount = Queue.trackQueues[ctx.Guild.Id].Count();

            if (!isURL || loadResult.Tracks.Count() == 1)
            {
                // Let the user know that a single song has been queued
                await Embeds.EditEmbed(ctx, "Song Queued", $"[{track.Title}]({track.Uri}) has been queued\nPosition #{queueTracksCount}", DiscordColor.Azure);
            }
            else
            {
                // Let the user know that their playlist has been queued
                await Embeds.EditEmbed(ctx, 
                    "Playlist Queued",
                    $"Your [playlist]({query}) has been queued\nPositions #{queueTracksCount - loadResult.Tracks.Count() + 1} - #{queueTracksCount}", 
                    DiscordColor.Azure
                );
            }
        }


        [SlashCommand("pause", "Pause the currently playing song")]
        public async Task Pause(InteractionContext ctx)
        {
            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await GetVoiceConnection(ctx);

            // Checks for valid voiceConnection || Checks if already paused
            if (!await Validation.CheckConnection(ctx, voiceConnection, "Pause") || !await Validation.CheckPause(ctx, true)) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "pause")) return;

            // Pause the currently playing song
            await voiceConnection.PauseAsync();

            // Set the 'isPlayerPaused' variable to true
            lavalinkStates[ctx.Guild.Id]["isPlayerPaused"] = true;

            // Lets the user know that the song has been paused
            await Embeds.SendEmbed(ctx, "Song Paused", "MiiBot has paused the current song", DiscordColor.Green);
        }


        [SlashCommand("resume", "Resume the currently paused song")]
        public async Task Resume(InteractionContext ctx)
        {
            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await GetVoiceConnection(ctx);

            // Checks for valid voiceConnection || Checks if already playing
            if (!await Validation.CheckConnection(ctx, voiceConnection, "Resume") || !await Validation.CheckPause(ctx, false)) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "resume")) return;

            // Resume the paused song
            await voiceConnection.ResumeAsync();

            // Set the 'isPlayerPaused' variable to true
            lavalinkStates[ctx.Guild.Id]["isPlayerPaused"] = false;

            // Let the user know that the song has been resumed
            await Embeds.SendEmbed(ctx, "Song Resumed", "MiiBot has resumed the current song", DiscordColor.Green);
        }


        [SlashCommand("stop", "Stop the currently playing song")]
        public async Task Stop(InteractionContext ctx)
        {
            
            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await GetVoiceConnection(ctx);

            // Checks for valid voiceConnection
            if (!await Validation.CheckConnection(ctx, voiceConnection, "Stop")) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "stop")) return;

            // Reset the queue
            Queue.trackQueues[ctx.Guild.Id].Clear();

            // Stops the song
            await voiceConnection.StopAsync();

            // Lets the user know that the song has been stopped
            await Embeds.SendEmbed(ctx, "Song Stopped", "MiiBot has stopped the current song", DiscordColor.Green);
        }


        [SlashCommand("skip", "Skip the currently playing song")]
        public async Task Skip(InteractionContext ctx)
        {
            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await GetVoiceConnection(ctx);

            // Checks for valid voiceConnection
            if (!await Validation.CheckConnection(ctx, voiceConnection, "Skip")) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "skip")) return;
            
            // Let the 'Queue.PlayNextSong' function know that skipping is intended
            lavalinkStates[ctx.Guild.Id]["isSkipping"] = true;

            // Stop the player
            await voiceConnection.StopAsync();

            // Lets the user know that the song has been skipped
            await Embeds.SendEmbed(ctx, "Song Skipped", "Miibot has skipped the current song", DiscordColor.Green);
        }

        
        [SlashCommand("current", "Display the currently playing song")]
        public async Task NowPlaying(InteractionContext ctx,
            [Option("Hidden","Hide message from other users")] bool isHidden = false
        )
        {
            // Defers response
            await ctx.DeferAsync(ephemeral: isHidden);

            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await GetVoiceConnection(ctx);

            // Checks for valid voiceConnection
            if (voiceConnection == null) await Embeds.EditEmbed(ctx, "Not in Voice Channel", "MiiBot isn't playing anything", DiscordColor.Red);
            if (voiceConnection.CurrentState.CurrentTrack == null) await Embeds.EditEmbed(ctx, "Nothing Playing", "MiiBot isn't playing anything", DiscordColor.Red);

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

            // Get the voice connection object
            (var lava, var node, var voiceConnection) = await GetVoiceConnection(ctx);

            // If MiiBot isn't in a VC
            if (voiceConnection == null) await Embeds.SendEmbed(ctx, "Not In VC", "MiiBot isn't in a voice channel", DiscordColor.Red);

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Validation.CheckConnection(ctx, voiceChannel, "loop")) return;

            if (loopType == "Song")
            {
                // Toggles song loop and lets the user know the song loop state
                lavalinkStates[ctx.Guild.Id]["isSongLooped"] = !lavalinkStates[ctx.Guild.Id]["isSongLooped"];
                if (lavalinkStates[ctx.Guild.Id]["isSongLooped"]) await Embeds.SendEmbed(ctx, "Song Loop Enabled", "MiiBot has enabled song looping", DiscordColor.Azure);
                else await Embeds.SendEmbed(ctx, "Song Loop Disabled", "MiiBot has disabled song looping", DiscordColor.Azure);
            }
            else if (loopType == "Queue")
            {
                // Toggles queue loop and lets the user know the queue loop state
                lavalinkStates[ctx.Guild.Id]["isQueueLooped"] = !lavalinkStates[ctx.Guild.Id]["isQueueLooped"];
                if (lavalinkStates[ctx.Guild.Id]["isQueueLooped"]) await Embeds.SendEmbed(ctx, "Queue Loop Enabled", "MiiBot has enabled queue looping", DiscordColor.Azure);
                else await Embeds.SendEmbed(ctx, "Queue Loop Disabled", "MiiBot has disabled queue looping", DiscordColor.Azure);
            }
        }
    }
}