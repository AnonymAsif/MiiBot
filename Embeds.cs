using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace MiiBot
{
    public class Embeds
    {
        public static async Task SendEmbed(InteractionContext ctx, string title, string desc, DiscordColor color)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = title ?? "Unset Title",
                Description = desc ?? "Unset Description",
                Color = color,
                //Author,
                //Fields,
                //Footer,
                //ImageUrl,
                //Url,
                //Thumbnail,
                //Timestamp,
            };

            // When fields become in-use, we will run this
            // function through a for-loop for each field
            //embed.AddField(name, value, inline?)

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }
    }
}