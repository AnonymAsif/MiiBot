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
            string timeStamp = null,
            string url = "",
            int fieldCount = 0,
            string[][] fieldArray = null
        )
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = desc,
                Color = color,
                Author = author,
                Footer = footer,
                Thumbnail = thumbnail,
                //Url,
                //Timestamp,
            };

            for (int i = 0; i < fieldCount; i++)
            {
                // each set
                // name, value, inline
                embed.AddField(fieldArray[i][0], fieldArray[i][1], true);
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }


        public static DiscordEmbedBuilder CreateEmbed(
            InteractionContext ctx,
            string title,
            string desc,
            DiscordColor color,
            DiscordEmbedBuilder.EmbedAuthor author = null,
            DiscordEmbedBuilder.EmbedFooter footer = null,
            DiscordEmbedBuilder.EmbedThumbnail thumbnail = null,
            string timeStamp = null,
            string url = "",
            int fieldCount = 0,
            string[][] fieldArray = null
        )
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = desc,
                Color = color,
                Author = author,
                Footer = footer,
                Thumbnail = thumbnail,
                //Url,
                //Timestamp,
            };

            for (int i = 0; i < fieldCount; i++)
            {
                // each set
                // name, value, inline
                embed.AddField(fieldArray[i][0], fieldArray[i][1], true);
            }

            return embed;
        }
    }
}