using DSharpPlus;
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
    // Add server ID's for which are being used by MiiBot
    private readonly static ulong[] whitelistedGuilds = {
        1041829398904586262, // Bot Testing
    };

    // LavaLink server process
    private static Process LLServer = new Process();

    static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();
    static async Task MainAsync()
    {
        DiscordClient bot = new DiscordClient(new DiscordConfiguration()
        {
            Token = System.Environment.GetEnvironmentVariable("token"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        });

        var endPoint = new ConnectionEndpoint
        {
            Hostname = "127.0.0.1",
            Port = 235
        };

        var lavaLinkConfig = new LavalinkConfiguration
        {
            Password = "MiiBot",
            RestEndpoint = endPoint,
            SocketEndpoint = endPoint
        };

        // Starts Lava Link Server
        LLServer.StartInfo.FileName = "java";
        LLServer.StartInfo.Arguments = "-jar Lavalink.jar";
        LLServer.Start();

        // Wait for server to start
        System.Threading.Thread.Sleep(8000);

        LavalinkExtension lavaLink = bot.UseLavalink();
        SlashCommandsExtension slashCommands = bot.UseSlashCommands();

        bot.UseInteractivity(new InteractivityConfiguration()
        {
            Timeout = TimeSpan.FromSeconds(30)
        });

        for (int i = 0; i < whitelistedGuilds.Count(); i++)
        {
            slashCommands.RegisterCommands<MiiBot.Audio>(whitelistedGuilds[i]);
        }

        await bot.ConnectAsync();
        await lavaLink.ConnectAsync(lavaLinkConfig);

        await Task.Delay(-1);
    }
}