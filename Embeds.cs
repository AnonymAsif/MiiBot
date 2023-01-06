using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace MiiBot
{
    public class Embeds
    {
        public static async Task SendEmbed(
            InteractionContext ctx,
            string title,
            string desc,
            DiscordColor color,
            DiscordEmbedBuilder.EmbedAuthor author = null,
            DiscordEmbedBuilder.EmbedFooter footer = null,
            DiscordEmbedBuilder.EmbedThumbnail thumbnail = null,
            string url = null,
            int fieldCount = 0,
            string[][] fieldArray = null,
            bool ephemeral = false
        )
        {
            // Create the embed using passed in arguments
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = desc,
                Color = color,
                Author = author,
                Footer = footer,
                Thumbnail = thumbnail,
                Url = url
            };

            // Create the fields
            for (int i = 0; i < fieldCount; i++) embed.AddField(fieldArray[i][0], fieldArray[i][1], true);

            // Send the embed
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(ephemeral).AddEmbed(embed));
        }
        

        public static async Task EditEmbed(
            InteractionContext ctx,
            string title,
            string desc,
            DiscordColor color,
            DiscordEmbedBuilder.EmbedAuthor author = null,
            DiscordEmbedBuilder.EmbedThumbnail thumbnail = null,
            string footer = null,
            string url = null,
            int fieldCount = 0,
            string[][] fieldArray = null,
            bool ephemeral = false
        )
        {
            // Create the embed using passed in arguments
            var embed = new DiscordEmbedBuilder{
                Title = title,
                Description = desc,
                Color = color,
                Author = author,
                Thumbnail = thumbnail
            }.WithUrl(url).WithFooter(footer);
            
            // Create the fields
            for (int i = 0; i < fieldCount; i++) embed.AddField(fieldArray[i][0], fieldArray[i][1], true);

            // Create the webhook using the built embed
            var builder = new DiscordWebhookBuilder().AddEmbed(embed);

            // Edit the response to the embed
            await ctx.EditResponseAsync(builder);
        }
    }
}