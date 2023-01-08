using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiiBot
{
    [SlashCommandGroup("Info", "All MiiBot Related Information")]
    public class Info : ApplicationCommandModule
    {
        [SlashCommand("changelog", "View the latest changelog")]
        public async Task Changelog(InteractionContext ctx)
        {
            string changelog = "MiiBot isn't released yet. Please come back when MiiBot is updated!";

            await Embeds.SendEmbed(ctx, "MiiBot Changelog v0.0.0", changelog, DiscordColor.Azure);
        }
    }
}