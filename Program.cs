// Hey you,
// You finally decided to open this!
// Before you compile ur probably gonna wanna remake the folder and everything
// because your current thing has Discord.NET libraries which is kinda cringe

// Afterwards, install the following packages through the terminal:
// dotnet add package DSharpPlus --version 4.3.0
// dotnet add package DSharpPlus.SlashCommands --version 5.0.0-nightly-00009
// dotnet add package DSharpPlus.VoiceNext --version 5.0.0-nightly-00009
// Anyway, feel free to delete this when ur done

using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;

namespace MiiBot
{
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

            // Regular text commands (we're not using this for now - or ever?)
            // get with e.Message
            //bot.MessageCreated += async (s, e) => {};

            var slashCommands = bot.UseSlashCommands();
            bot.UseVoiceNext();

            for (int i = 0; i < whitelistedGuilds.Count(); i++)
                slashCommands.RegisterCommands<Commands>(whitelistedGuilds[i]);

            await bot.ConnectAsync();
            await Task.Delay(-1);
        }   
    }
}