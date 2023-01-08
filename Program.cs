using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    // Add the server IDs that use MiiBot here
    private readonly static ulong[] whitelistedGuilds = {
        1041829398904586262, // Bot Testing
        1042904993382027315 // Badge Server
    };

    // Lavalink server process
    private static Process LLServer = new Process();

    // Application Entry Point
    static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    static async Task MainAsync()
    {
        // MiiBot configuration
        DiscordClient bot = new DiscordClient(new DiscordConfiguration()
        {
            Token = System.Environment.GetEnvironmentVariable("token"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged,
        });

        // Lavalink server connection information
        var endPoint = new ConnectionEndpoint
        {
            Hostname = "127.0.0.1",
            Port = 235
        };

        // Lavalink server configuration
        var lavaLinkConfig = new LavalinkConfiguration
        {
            Password = "MiiBot",
            RestEndpoint = endPoint,
            SocketEndpoint = endPoint
        };

        // Extra MiiBot dependencies
        LavalinkExtension lavaLink = bot.UseLavalink();
        SlashCommandsExtension slashCommands = bot.UseSlashCommands();
        bot.UseInteractivity(new InteractivityConfiguration()
        {
            Timeout = TimeSpan.FromSeconds(60)
        });

        // Register slash commands from the following classes (Guilds)
        for (int i = 0; i < whitelistedGuilds.Count(); i++)
        {
            slashCommands.RegisterCommands<MiiBot.Player>(whitelistedGuilds[i]);
            slashCommands.RegisterCommands<MiiBot.Queue>(whitelistedGuilds[i]);
        }

        // Register slash commands from the following classes (Global)
        slashCommands.RegisterCommands<MiiBot.Info>();
        
        // Set MiiBot's status
        DiscordActivity activity = new DiscordActivity("Wii Music", ActivityType.Playing);
        activity.StreamUrl = "https://www.youtube.com/watch?v=64akWe7eFzQ";

        // Connect MiiBot to Discord
        await bot.ConnectAsync(activity);

        // Starts Lavalink server
        LLServer.StartInfo.FileName = "java";
        LLServer.StartInfo.Arguments = "-Xms1g -Xmx2g -jar Lavalink.jar";
        LLServer.Start();

        // Delay first connection (saves time)
        System.Threading.Thread.Sleep(8000);

        // Connect MiiBot to the LLServer
        await lavaLink.ConnectAsync(lavaLinkConfig);

        // Stop from exiting
        await Task.Delay(-1);
    }
}