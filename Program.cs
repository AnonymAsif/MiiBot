using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;

class Program
{
    // Add server ID's for which are being used by MiiBot
    private readonly static ulong[] whitelistedGuilds = {
        1041829398904586262, // Bot Testing
    };

    static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

    static async Task MainAsync()
    {
        var bot = new DiscordClient(new DiscordConfiguration()
        {
            Token = "MTA1Njc3NDc2NjI1OTg4MDAzNg.GVmVMO.b72pjlVw27wcZG-8dj5Gj4ZkZ-hCb-nsi6gpLs",
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged
        });

        Process myProcess = new Process();
        myProcess.StartInfo.UseShellExecute = false;
        myProcess.StartInfo.FileName = "java";
        myProcess.StartInfo.Arguments = "-jar Lavalink.jar";
        Console.WriteLine("Starting LavaLink Server...");
        myProcess.Start();
        System.Threading.Thread.Sleep(8000);

        var endpoint = new ConnectionEndpoint
        {
            Hostname = "127.0.0.1",
            Port = 235
        };

        var lavalinkConfig = new LavalinkConfiguration
        {
            Password = "MiiBot",
            RestEndpoint = endpoint,
            SocketEndpoint = endpoint
        };

        // Regular text commands (we're not using this for now - or ever?)
        // get with e.Message
        //bot.MessageCreated += async (s, e) => {};

        var slashCommands = bot.UseSlashCommands();
        var lavalink = bot.UseLavalink();

        for (int i = 0; i < whitelistedGuilds.Count(); i++)
        {
            slashCommands.RegisterCommands<MiiBot.Audio>(whitelistedGuilds[i]);
        }

        await bot.ConnectAsync();
        await lavalink.ConnectAsync(lavalinkConfig);

        await Task.Delay(-1);
    }
}