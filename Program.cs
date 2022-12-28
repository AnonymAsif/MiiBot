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
                slashCommands.RegisterCommands<Audio>(whitelistedGuilds[i]);

            await bot.ConnectAsync();
            await Task.Delay(-1);
        }   
    }
}