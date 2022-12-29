using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.Lavalink;

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
            [Option("Youtube", "Enter Youtube Link")] string search = null
        )
        {
            // Get user's VC
            DiscordChannel voiceChannel = ctx.Member?.VoiceState?.Channel;

            if (voiceChannel == null)
            {
                await Embeds.SendEmbed(ctx, "Please join a VC first", "MiiBot couldn't figure out which VC to join", DiscordColor.Red);
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Guild);

            if (conn == null)
            {
                // Connect the bot first manually
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed 
                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches
            )
            {
                await Embeds.SendEmbed(ctx, "Nothing Found", "MiiBot couldn't gather any results from your search query", DiscordColor.Red);
                return;
            }

            var track = loadResult.Tracks.First();

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
                await Embeds.SendEmbed(ctx, "Nothing To Stop", "MiiBot isn't already playing anything", DiscordColor.Red);
                return;
            }

            await conn.PauseAsync();

            await Embeds.SendEmbed(ctx, "Song Paused", "MiiBot has paused the current song", DiscordColor.Green);
        }


        [SlashCommand("stop", "Stop The Currently Playing Song")]
        public async Task Stop(InteractionContext ctx)
        {
            
        }
    }
}
