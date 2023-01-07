using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using System.Linq;
using System.Threading.Tasks;

namespace MiiBot
{
    public class Validation
    {
        public static async Task<bool> CheckConnection(InteractionContext ctx, LavalinkExtension lava)
        {
            if (lava.ConnectedNodes.Any()) return true;
            await Embeds.SendEmbed(ctx, "Problem!", "LavaLink is not configured properly", DiscordColor.Red);
            return false;
        }


        public static async Task<bool> CheckConnection(InteractionContext ctx, DiscordChannel voiceChannel, string command)
        {
            if (voiceChannel != null) return true;
            await Embeds.SendEmbed(ctx, "Please join a VC first", "MiiBot couldn't figure out which VC to " + command, DiscordColor.Red);
            return false;
        }


        public static async Task<bool> CheckConnection(InteractionContext ctx, LavalinkGuildConnection voiceConnection, string command = null)
        {
            if (voiceConnection != null && voiceConnection.CurrentState.CurrentTrack != null) return true;
            else if (voiceConnection == null) await Embeds.SendEmbed(ctx, "Not in Voice Channel", "MiiBot isn't playing anything", DiscordColor.Red);
            else await Embeds.SendEmbed(ctx, "Nothing To " + command, "MiiBot isn't playing anything", DiscordColor.Red);
            return false;
        }

        
        public static async Task<bool> CheckConnection(InteractionContext ctx, LavalinkNodeConnection node, bool connecting)
        {
            if (connecting != (node.GetGuildConnection(ctx.Guild) != null)) return true;
            if (connecting) await Embeds.SendEmbed(ctx, "Already Connected!", "MiiBot is already connected to your VC", DiscordColor.Red);
            else await Embeds.SendEmbed(ctx, "Already Disconnected!", "MiiBot didn't find any VC to disconnect from", DiscordColor.Red);
            return false;
        }

        
        public static async Task<bool> CheckPause(InteractionContext ctx, bool pausing)
        {
            // Check if the player is paused for this guild
            bool isPlayerPaused = Player.lavalinkStates[ctx.Guild.Id]["isPlayerPaused"];
            if (pausing != isPlayerPaused) return true;
            if (pausing && isPlayerPaused) await Embeds.SendEmbed(ctx, "Already Paused", "MiiBot has already paused playback", DiscordColor.Red);
            if (!pausing && !isPlayerPaused) await Embeds.SendEmbed(ctx, "Already Playing", "MiiBot is already playing", DiscordColor.Red);
            return false;
        }
    }
}