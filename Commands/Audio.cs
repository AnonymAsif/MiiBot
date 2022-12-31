using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiiBot
{
    public class Audio : ApplicationCommandModule
    {
        // Contains the tracks in the queue
        private static LavalinkTrack[] trackQueue = { };
        private static bool isPlayerPaused = false;

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


        [SlashCommand("connect", "Connect To A Voice Channel")]
        public async Task Connect(InteractionContext ctx)
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
            await Embeds.SendEmbed(ctx, "Connected", "MiiBot successfully joined the VC", DiscordColor.Green);
        }


        [SlashCommand("disconnect", "Disconnect From A Voice Channel")]
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
            await voiceConnection.DisconnectAsync();
            await Embeds.SendEmbed(ctx, "Disconnected", "MiiBot successfully left the VC", DiscordColor.Green);
        }


        [SlashCommand("play", "Play A Song From A Query")]
        public async Task Play(
            InteractionContext ctx,
            [Option("Search", "Enter Search Query")] string search = null,
            [Option("URL", "Enter URL")] string searchURL = null
        )
        {
            if (search == null && search == null)
            {
                await Embeds.SendEmbed(ctx, "No Search Query Provided", "MiiBot doesn't have any query to work with", DiscordColor.Red);
                return;
            }

            // Defers response
            await ctx.DeferAsync();

            // Attempt to get the LavaLink connection
            var lava = ctx.Client.GetLavalink();
            if (!await Checks(ctx, lava)) return;

            // Attempt to get the user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;
            if (!await Checks(ctx, voiceChannel)) return;

            // Get lava connection
            var node = lava.ConnectedNodes.Values.First();

            // Attempt to establish the voice connection if none
            if (node.GetGuildConnection(ctx.Guild) == null) await node.ConnectAsync(voiceChannel);
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
                        Title = "Select a song",
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

            // Edits the defered response to contain Track information
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
                new DiscordEmbedBuilder
                {
                    Title = "Playing " + track.Title,
                    Description = "by: " + track.Author + "\nLength: " + track.Length + "\n URL: " + track.Uri,
                    Color = DiscordColor.Azure
                }
            ));

            // Plays track
            isPlayerPaused = false;
            await voiceConnection.PlayAsync(track);
        }


        [SlashCommand("pause", "Pause The Currently Playing Song")]
        public async Task Pause(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnect || checks if already paused
            if (!await Checks(ctx, voiceConnection, "Pause") || !await Checks(ctx, true)) return;

            await voiceConnection.PauseAsync();
            Console.WriteLine("Player State: " + voiceConnection.CurrentState);

            isPlayerPaused = true;
            await Embeds.SendEmbed(ctx, "Song Paused", "MiiBot has paused the current song", DiscordColor.Green);
        }


        [SlashCommand("resume", "Resume the paused song")]
        public async Task Resume(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            // Checks for valid voiceConnect || checks if already playing
            if (!await Checks(ctx, voiceConnection, "Resume") || !await Checks(ctx, false)) return;

            await voiceConnection.ResumeAsync();

            isPlayerPaused = false;
            await Embeds.SendEmbed(ctx, "Song Resumed", "MiiBot has resumed the current song", DiscordColor.Green);
        }


        [SlashCommand("stop", "Stop The Currently Playing Song")]
        public async Task Stop(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var voiceConnection = node.GetGuildConnection(ctx.Guild);

            if (!await Checks(ctx, voiceConnection, "Stop")) return;

            await voiceConnection.StopAsync();
            
            await Embeds.SendEmbed(ctx, "Song Stopped", "MiiBot has stopped the current song", DiscordColor.Green);
        }
    }
}