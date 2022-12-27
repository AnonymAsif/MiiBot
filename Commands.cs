using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;

namespace MiiBot
{
    public class Commands : ApplicationCommandModule
    {
        [SlashCommand("join", "Join Your Voice Channel")]
        public async Task Join(
            InteractionContext ctx
            //[Option("Title", "Enter Embed Title")] string title = "Hello World",
            //[Option("Description", "Enter Embed Description")] string description = "Good to see you!"
        )
        {

            var voiceNext = ctx.Client.GetVoiceNext();

            // need to find a way to check if user is in voice channel with (prefferably):
            //ctx.User.Id
            // and then get that channel as a "DiscordChannel" type

            if (voiceNext == null)
            {
                await Embeds.SendEmbed(ctx, "Problem!", "VoiceNext is not configured properly", DiscordColor.Red);
                return;
            }

            if (voiceNext.GetConnection(ctx.Guild) != null)
            {
                await Embeds.SendEmbed(ctx, "Already Connected!", "Already connected to your VC", DiscordColor.Red);
                return;
            }

            await Embeds.SendEmbed(ctx, "works", "a", DiscordColor.SpringGreen);
        }
    }
}