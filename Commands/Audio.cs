using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;

namespace MiiBot
{
    public class Commands : ApplicationCommandModule
    {
        //[Option("Title", "Enter Embed Title")] string title = "Hello World",
        //[Option("Description", "Enter Embed Description")] string description = "Good to see you!"
        private VoiceNextConnection? voiceConnection = null;


        [SlashCommand("connect", "Connect To A Voice Channel")]
        public async Task Join(InteractionContext ctx)
        {
            var voiceNext = ctx.Client.GetVoiceNext();

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

            DiscordChannel? voiceChannel = null;
            foreach (KeyValuePair<ulong, DiscordChannel> channel in ctx.Guild.Channels)
            {
                if (channel.Value.Type == ChannelType.Voice)
                {
                    if (channel.Value.Users.Contains(ctx.User))
                    {
                        voiceChannel = channel.Value;
                    }
                }
            }

            if (voiceChannel == null)
            {
                await Embeds.SendEmbed(ctx, "Please join a VC first", "MiiBot couldn't figure out which VC to join", DiscordColor.Red);
                return;
            }

            // Try to join VC
            voiceConnection = await voiceChannel.ConnectAsync();
        }


        [SlashCommand("disconnect", "Disconnect From Voice Channel")]
        public async Task Disconnect(InteractionContext ctx)
        {
            if (voiceConnection == null)
                return;
            voiceConnection.Disconnect();
            voiceConnection = null;
        }

        [SlashCommand("play", "Play A Song From Youtube")]
        public async Task Play(InteractionContext ctx)
        {
            var filePath = "ImageMaterial.mp3";
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $@"-i ""{filePath}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            Stream pcm = ffmpeg.StandardOutput.BaseStream;

            VoiceTransmitSink transmit = voiceConnection.GetTransmitSink();
            await pcm.CopyToAsync(transmit);
            await pcm.DisposeAsync();

            await Embeds.SendEmbed(ctx, "Playing Song", "Playing requested song!", DiscordColor.Red);
        }
    }
}