using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
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
    [SlashCommandGroup("Music", "All MiiBot Music Commands")]
    public class Audio : ApplicationCommandModule
    {
        // Contains the tracks in the queue
        private static LinkedList<LavalinkTrack> trackQueue = new LinkedList<LavalinkTrack>();
        private static bool isPlayerPaused = false;
        private static bool isQueueLooped = false;
        private static bool isSongLooped = false;

        
        private async Task<bool> Checks(InteractionContext ctx, LavalinkExtension lava)
        {
            if (lava.ConnectedNodes.Any()) return true;
            await Embeds.SendEmbed(ctx, "Problem!", "LavaLink is not configured properly", DiscordColor.Red);
            return false;

        }


        private async Task<bool> Checks(InteractionContext ctx, DiscordChannel voiceChannel)
        {
            if (voiceChannel != null) return true;
            await Embeds.SendEmbed(ctx, "Please join a VC first", "MiiBot couldn't figure out which VC to join", DiscordColor.Red);
            return false;
        }


        private async Task<bool> Checks(InteractionContext ctx, LavalinkGuildConnection voiceConnection, string command = null)
        {
            if (voiceConnection == null)
            {
                await Embeds.SendEmbed(ctx, "Not in Voice Channel", "MiiBot isn't playing anything", DiscordColor.Red);
                return false;
            }

            if (voiceConnection.CurrentState.CurrentTrack == null)
            {
                await Embeds.SendEmbed(ctx, "Nothing To " + command, "MiiBot isn't playing anything", DiscordColor.Red);
                return false;
            }

            return true;
        }


        private async Task<bool> Checks(InteractionContext ctx, bool pausing)
        {
            if (pausing)
            {
                if (isPlayerPaused)
                {
                    await Embeds.SendEmbed(ctx, "Already Paused", "MiiBot has already paused playback", DiscordColor.Red);
                    return false;
                }
            }
            else
            {
                if (!isPlayerPaused)
                {
                    await Embeds.SendEmbed(ctx, "Already Playing", "MiiBot is already playing", DiscordColor.Red);
                    return false;
                }
            }
            return true;
        }


        private async Task<bool> Checks(InteractionContext ctx, LavalinkNodeConnection node, bool connecting)
        {
            if (connecting)
            {
                if (node.GetGuildConnection(ctx.Guild) == null) return true;
                await Embeds.SendEmbed(ctx, "Already Connected!", "MiiBot is already connected to your VC", DiscordColor.Red);
                return false;
            }
            else
            {
                if (node.GetGuildConnection(ctx.Guild) != null) return true;
                await Embeds.SendEmbed(ctx, "Already Disconnected!", "MiiBot didn't find any VC to disconnect from", DiscordColor.Red);
                return false;
            }
        }


        [SlashCommand("connect", "Connect to a voice channel")]
        public async Task Connect(InteractionContext ctx, [Option("Hidden", "Hide bot message on connection")] bool isHiddenMessage = false)
        {
            // Get lava client
            var lava = ctx.Client.GetLavalink();
            if (!await Checks(ctx, lava)) return;

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();
            if (!await Checks(ctx, node, true)) return;

            // Get User VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Checks(ctx, voiceChannel)) return;

            // Connect bot to VC
            await node.ConnectAsync(voiceChannel);

            if (!isHiddenMessage) await Embeds.SendEmbed(ctx, "Connected", "MiiBot successfully joined the VC", DiscordColor.Green);
            
            var voiceConnection = node.GetGuildConnection(ctx.Guild);
            
            // Register the playback finished event
            voiceConnection.PlaybackFinished += async (LavalinkGuildConnection _, TrackFinishEventArgs e) => await PlayFromQueue(voiceConnection, e);
        }


        [SlashCommand("disconnect", "Disconnect from a voice channel")]
        public async Task Disconnect(InteractionContext ctx)
        {
            // Get lava client
            var lava = ctx.Client.GetLavalink();
            if (!await Checks(ctx, lava)) return;

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();
            if (!await Checks(ctx, node, false)) return;

            // Gets the active voice connection and disconnects
            LavalinkGuildConnection voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Remove the playback finished event
            voiceConnection.PlaybackFinished -= async (LavalinkGuildConnection _, TrackFinishEventArgs e) => await PlayFromQueue(voiceConnection, e);
            
            await voiceConnection.DisconnectAsync();
            await Embeds.SendEmbed(ctx, "Disconnected", "MiiBot successfully left the VC", DiscordColor.Green);

            // Reset the queue
            trackQueue.Clear();
        }


        [SlashCommand("play", "Play a song using a query")]
        public async Task Play(
            InteractionContext ctx,
            [Option("Search", "Enter Search Query")] string search = null,
            [Option("URL", "Enter URL")] string searchURL = null,
            [Option("Hidden", "Hide Bot Message on Queue")] bool isHidden = false
        )
        {
            if (search == null && searchURL == null)
            {
                await Embeds.SendEmbed(ctx, "No Search Query Provided", "MiiBot doesn't have any query to work with", DiscordColor.Red);
                return;
            }

            // Defers response
            await ctx.DeferAsync(ephemeral: isHidden);

            // Attempt to get the LavaLink connection
            var lava = ctx.Client.GetLavalink();
            // Edit the queueing message
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Title = "Problem!", 
                    Description = "LavaLink is not configured properly",
                    Color = DiscordColor.Red
                }));
                return;
            }

            // Attempt to get the user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (voiceChannel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Title = "Please Join a VC First",
                    Description = "MiiBot couldn't figure out which VC to join",
                    Color = DiscordColor.Red
                }));
                return;
            }

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();

            // Attempt to establish the voice connection - if none, connects
            if (node.GetGuildConnection(ctx.Guild) == null) await Connect(ctx, true);
            var voiceConnection = node.GetGuildConnection(ctx.Guild);
            
            // Results from search Query or URL
            LavalinkLoadResult loadResult;

            // Attemps to get tracks for search query if the URL is valid
            if (searchURL != null)
            {
                Uri trackUri;
                if (!Uri.TryCreate(searchURL, UriKind.Absolute, out trackUri))
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
                        new DiscordEmbedBuilder
                        {
                            Title = "Invalid URL",
                            Description = "Please enter a valid URL",
                            Color = DiscordColor.Red
                        }
                    ));
                    return;
                }

                loadResult = await node.Rest.GetTracksAsync(trackUri);
            }
            // Get a list of available tracks from search query
            else loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
                    new DiscordEmbedBuilder
                    {
                        Title = "Something Went Wrong",
                        Description = "MiiBot couldn't load results",
                        Color = DiscordColor.Red
                    }
                ));
                return;
            }

            if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
                    new DiscordEmbedBuilder
                    {
                        Title = "Nothing Found",
                        Description = "MiiBot couldn't gather any results from your search query",
                        Color = DiscordColor.Red
                    }
                ));
                return;
            }

            // Setup client interactivity
            var interactivity = ctx.Client.GetInteractivity();
            DSharpPlus.Lavalink.LavalinkTrack track;

            // Using custom search
            if (searchURL == null)
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
                foreach (var trackI in trackList)
                {
                    embedDescription += $"{trackIndex++}: {trackI.Title} ({trackI.Length})\n";
                }

                // Create the message with everything and send it
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
            }
            // If a link was used, just get it by link
            else track = loadResult.Tracks.First();  

            // Edits the defered response to let the user know the song has been queued
            trackQueue.AddLast(track);

            // If no song is currently playing - queue was empty originally
            if (voiceConnection.CurrentState.CurrentTrack == null)
            {
                // Edit the queueing message
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder {
                    Title = "Playing " + track.Title,
                    Description = "by: " + track.Author + "\nLength: " + track.Length + "\n URL: " + track.Uri,
                    Color = DiscordColor.Azure
                }));

                // Play song
                await PlayFromQueue(voiceConnection);
                await NowPlaying(ctx);
                
                isPlayerPaused = false;
                return;
            }
            
            var editedMessage = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
                new DiscordEmbedBuilder
                {
                    Title = "Song Queued",
                    Description = $"{track.Title} has been queued\nPosition #{trackQueue.Count() + 1}",
                    Color = DiscordColor.Azure
                }
            ));
        }


        public async Task<bool> PlayFromQueue(LavalinkGuildConnection voiceConnection, TrackFinishEventArgs e = null)
        {   
            if (trackQueue.Count() >= 1)
            {
                LavalinkTrack track = trackQueue.First();
                if (!isSongLooped) 
                {
                    trackQueue.RemoveFirst();
    
                    // Re-Enqueues the song that is about to play    
                    if (isQueueLooped) trackQueue.AddLast(track);
                }
                
                await voiceConnection.PlayAsync(track);
                isPlayerPaused = false;
                
                return true;
            }
            
            return false;
        }


        [SlashCommand("pause", "Pause the currently playing song")]
        public async Task Pause(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection || Checks if already paused
            if (!await Checks(ctx, voiceConnection, "Pause") || !await Checks(ctx, true)) return;

            await voiceConnection.PauseAsync();

            isPlayerPaused = true;
            await Embeds.SendEmbed(ctx, "Song Paused", "MiiBot has paused the current song", DiscordColor.Green);
        }


        [SlashCommand("resume", "Resume the currently paused song")]
        public async Task Resume(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection || Checks if already playing
            if (!await Checks(ctx, voiceConnection, "Resume") || !await Checks(ctx, false)) return;

            await voiceConnection.ResumeAsync();

            isPlayerPaused = false;
            await Embeds.SendEmbed(ctx, "Song Resumed", "MiiBot has resumed the current song", DiscordColor.Green);
        }


        [SlashCommand("stop", "Stop the currently playing song")]
        public async Task Stop(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            if (!await Checks(ctx, voiceConnection, "Stop")) return;

            // Reset the queue
            trackQueue.Clear();
        
            await voiceConnection.StopAsync();

            await Embeds.SendEmbed(ctx, "Song Stopped", "MiiBot has stopped the current song", DiscordColor.Green);
        }
        
        
        [SlashCommand("skip", "Skip the currently playing song")]
        public async Task Skip(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection
            if (!await Checks(ctx, voiceConnection, "Skip")) return;

            // If the song is looped, remove it from the front of the queue
            if(isSongLooped) 
            {
                // If the queue is ALSO looped, re-queues the skipped song
                if (isQueueLooped) trackQueue.AddLast(trackQueue.First());
                trackQueue.RemoveFirst();
            }
            
            await voiceConnection.StopAsync();
            
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
                new DiscordEmbedBuilder
                {
                    Title = "Song Skipped",
                    Description = "MiiBot has skipped the current song",
                    Color = DiscordColor.Green
                }
            ));
        }
        
        
        [SlashCommand("loop", "Loop the current queue")]
        public async Task Loop(
            InteractionContext ctx,
            [Option("Type", "Loop type (song or queue)"), 
                ChoiceAttribute("Song", "Song"), 
                ChoiceAttribute("Queue", "Queue")] 
            string loopType = null
        )
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection || Checks if no song is playing
            if (!await Checks(ctx, voiceConnection, "Loop")) return;
            
            if (loopType == "Song")
            {
                isSongLooped = !isSongLooped;

                if (isSongLooped)
                {
                    await Embeds.SendEmbed(ctx, "Song Loop Enabled", "MiiBot has enabled song looping", DiscordColor.Green);

                    // Enqueues the current playing track (it's not in the queue: Song Edition)
                    if (!isQueueLooped) trackQueue.AddFirst(voiceConnection.CurrentState.CurrentTrack);
                }
                else
                {
                    trackQueue.RemoveFirst();
                    await Embeds.SendEmbed(ctx, "Song Loop Disabled", "MiiBot has disabled song looping", DiscordColor.Green);
                }
            }
            else if (loopType == "Queue")
            {
                isQueueLooped = !isQueueLooped;

                if (isQueueLooped)
                {
                    await Embeds.SendEmbed(ctx, "Queue Loop Enabled", "MiiBot has enabled queue looping", DiscordColor.Green);
    
                    // Enqueues the current playing track (it's not in the queue)
                    if (!isSongLooped) trackQueue.AddLast(voiceConnection.CurrentState.CurrentTrack);
                }
                else
                {
                    trackQueue.RemoveLast();
                    await Embeds.SendEmbed(ctx, "Queue Loop Disabled", "MiiBot has disabled queue looping", DiscordColor.Green);
                }
            }
        }
        
        
        [SlashCommand("nowplaying", "Display the currently playing song")]
        public async Task NowPlaying(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnection || Checks if no song is playing
            if (!await Checks(ctx, voiceConnection, "Loop")) return;

            LavalinkTrack track = voiceConnection.CurrentState.CurrentTrack;

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder {
                Title = "Playing " + track.Title,
                Description = "by: " + track.Author + "\nLength: " + track.Length + "\n URL: " + track.Uri,
                Color = DiscordColor.Azure
            }));
        }

        [SlashCommand("queue", "List the tracks in the queue")]
        public async Task ListQueue(InteractionContext ctx)
        {
            string description = "";
            TimeSpan totalLength = new TimeSpan();
            for (int i = 0; i < trackQueue.Count(); i++)
            {
                var track = trackQueue.ElementAt(i);
                string trackLength;
                
                if (track.Length.Hours > 0) trackLength = $"{track.Length.TotalHours}:{track.Length.Minutes}:{track.Length.Seconds}";
                else trackLength = $"{track.Length.TotalMinutes}:{track.Length.Seconds}";
                
                description += $"`[{i+1}]` **{track.Title}** - {track.Author} `[{trackLength}]`\n";
                totalLength += track.Length;
            }
            description += $"\nTotal Tracks: **{trackQueue.Count()}**\nTotal Length: **{totalLength}**";

            await Embeds.SendEmbed(ctx, "Current Queue", description, DiscordColor.Azure);
        }
    }
}