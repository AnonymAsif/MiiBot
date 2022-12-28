using System.Diagnostics;
using System.Threading;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.VoiceNext;

namespace MiiBot
{
    public class Audio : ApplicationCommandModule
    {
        private static VoiceNextConnection? voiceConnection = null;
        private static Thread AudioThread;
        private static Stream pcm;


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

            await Embeds.SendEmbed(ctx, "Connected", "Successfully joined the VC", DiscordColor.Green);
        }


        [SlashCommand("disconnect", "Disconnect From Voice Channel")]
        public async Task Disconnect(InteractionContext ctx)
        {
            if (voiceConnection == null) return;

            voiceConnection.Disconnect();
            voiceConnection = null;

            if (AudioThread.IsAlive)
            {
                AudioThread.Abort();
                await pcm.DisposeAsync();
            }
            await Embeds.SendEmbed(ctx, "Disconnected", "Successfully left the VC", DiscordColor.Green);
        }


        [SlashCommand("play", "Play A Song From Youtube")]
        public async Task Play(
            InteractionContext ctx,
            [Option("Youtube", "Enter Youtube Link")] string link = null
        )
        {
            if (link == null)
            {
                await Embeds.SendEmbed(ctx, "No Song Provided", "MiiBot wasn't given a song to play", DiscordColor.Red);
                return;
            }

            if (AudioThread != null && AudioThread.IsAlive)
            {
                await Embeds.SendEmbed(ctx, "Already Playing", "MiiBot is already playing a song", DiscordColor.Red);
                return;
            }

            string args = $"/C youtube-dl --ignore-errors -o - {link} | ffmpeg -err_detect ignore_err -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1";
            var ffmpeg = Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });
            
            pcm = ffmpeg.StandardOutput.BaseStream;

            VoiceTransmitSink transmit = voiceConnection.GetTransmitSink();

            await Embeds.SendEmbed(ctx, "Playing Song", "Playing requested song!", DiscordColor.Yellow);

            AudioThread = new Thread(() => pcm.CopyToAsync(transmit));

            AudioThread.Start();
            Console.WriteLine("Thread Started");
            await pcm.DisposeAsync();
        }


        [SlashCommand("stop", "Stop the currently playing song")]
        public async Task Stop(InteractionContext ctx)
        {
            if (!AudioThread.IsAlive)
            {
                await Embeds.SendEmbed(ctx, "Stop Right There", "MiiBot found nothing to stop playing!", DiscordColor.Red);
                return;
            }
            AudioThread.Abort();
            await pcm.DisposeAsync();
        }
    }
}