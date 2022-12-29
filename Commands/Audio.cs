using DSharpPlus;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiiBot
{
    public class Audio : ApplicationCommandModule
    {
        [SlashCommand("connect", "Connect To A Voice Channel")]
        public async Task Connect(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await Embeds.SendEmbed(ctx, "Problem!", "LavaLink is not configured properly", DiscordColor.Red);
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (node.GetGuildConnection(ctx.Guild) != null)
            {
                await Embeds.SendEmbed(ctx, "Already Connected!", "MiiBot is already connected to your VC", DiscordColor.Red);
                return;
            }

            // Get user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;

            if (voiceChannel == null)
            {
                await Embeds.SendEmbed(ctx, "Please join a VC first", "MiiBot couldn't figure out which VC to join", DiscordColor.Red);
                return;
            }

            await node.ConnectAsync(voiceChannel);

            await Embeds.SendEmbed(ctx, "Connected", "MiiBot successfully joined the VC", DiscordColor.Green);
        }


        [SlashCommand("disconnect", "Disconnect From A Voice Channel")]
        public async Task Disconnect(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await Embeds.SendEmbed(ctx, "Problem!", "LavaLink is not configured properly", DiscordColor.Red);
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (node.GetGuildConnection(ctx.Guild) == null)
            {
                await Embeds.SendEmbed(ctx, "Already Disconnected!", "MiiBot didn't find any VC to disconnect from", DiscordColor.Red);
                return;
            }

            LavalinkGuildConnection voiceConnection = node.GetGuildConnection(ctx.Guild);
            await voiceConnection.DisconnectAsync();

            await Embeds.SendEmbed(ctx, "Disconnected", "MiiBot successfully left the VC", DiscordColor.Green);
        }


        [SlashCommand("play", "Play A Song From Youtube")]
        public async Task Play(
            InteractionContext ctx,
            [Option("Search", "Enter Search Query")] string search = null
        )
        {
            var lava = ctx.Client.GetLavalink();

            if (!lava.ConnectedNodes.Any())
            {
                await Embeds.SendEmbed(ctx, "Problem!", "LavaLink is not configured properly", DiscordColor.Red);
                return;
            }

            // Get user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;

            if (voiceChannel == null)
            {
                await Embeds.SendEmbed(ctx, "Please join a VC first", "MiiBot couldn't figure out which VC to join", DiscordColor.Red);
                return;
            }

            var node = lava.ConnectedNodes.Values.First();

            if (node.GetGuildConnection(ctx.Guild) == null) await node.ConnectAsync(voiceChannel);
            var conn = node.GetGuildConnection(ctx.Guild);


            var loadResult = await node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await Embeds.SendEmbed(ctx, "Something Went Wrong", "MiiBot couldn't load results", DiscordColor.Red);
                return;
            }

            if (loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await Embeds.SendEmbed(ctx, "Nothing Found", "MiiBot couldn't gather any results from your search query", DiscordColor.Red);
                return;
            }

            var track = loadResult.Tracks.First();
            string description = "by: " + track.Author + "\nLength: " + track.Length + "\n URL: " + track.Uri;






            
            var interactivity = ctx.Client.GetInteractivity();

            DiscordEmoji[] poll_options = {DiscordEmoji.FromName(ctx.Client, ":ok_hand:"), DiscordEmoji.FromName(ctx.Client, ":skull:")};

            // then let's present the poll
            var embed = new DiscordEmbedBuilder
            {
                Title = "Poll time!",
                Description = poll_options[0] + " " + poll_options[1]
            };
            var msg = await ctx.Channel.SendMessageAsync(embed: embed);

            // add the options as reactions
            foreach (var reaction in poll_options)
            {
                System.Threading.Thread.Sleep(300);
                await msg.CreateReactionAsync(reaction);
            }
            var results = await interactivity.WaitForReactionAsync(x => Array.Exists(poll_options, element => element == x.Emoji) , msg, ctx.User);
            // and finally post the results
            //await ctx.CreateResponseAsync(results[0] + " " + results[1]);












            // await Embeds.SendEmbed(ctx,
            //     "Playing " + track.Title,
            //     description,
            //     DiscordColor.Blue
            // );
            Console.WriteLine("Playing Track: " + track.Title);
            await conn.PlayAsync(track);
        }


        [SlashCommand("pause", "Pause The Currently Playing Song")]
        public async Task Pause(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null || conn.CurrentState.CurrentTrack == null)
            {
                await Embeds.SendEmbed(ctx, "Nothing To Pause", "MiiBot isn't playing anything", DiscordColor.Red);
                return;
            }

            await conn.PauseAsync();

            await Embeds.SendEmbed(ctx, "Song Paused", "MiiBot has paused the current song", DiscordColor.Green);
        }


        [SlashCommand("resume", "Resume the paused song")]
        public async Task Resume(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null || conn.CurrentState.CurrentTrack == null)
            {
                await Embeds.SendEmbed(ctx, "Nothing To Resume", "MiiBot isn't playing anything", DiscordColor.Red);
                return;
            }

            await conn.ResumeAsync();

            await Embeds.SendEmbed(ctx, "Song Resumed", "MiiBot has resumed the current song", DiscordColor.Green);
        }


        [SlashCommand("stop", "Stop The Currently Playing Song")]
        public async Task Stop(InteractionContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null || conn.CurrentState.CurrentTrack == null)
            {
                await Embeds.SendEmbed(ctx, "Nothing To Stop", "MiiBot isn't playing anything", DiscordColor.Red);
                return;
            }

            await conn.StopAsync();

            await Embeds.SendEmbed(ctx, "Song Stopped", "MiiBot has stopped the current song", DiscordColor.Green);
        }
    }
}