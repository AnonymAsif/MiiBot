using Discord.Net;
using Discord.WebSocket;

public class Program
{
    public static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

     
    public async Task MainAsync()
    {
        DiscordSocketClient _client = new DiscordSocketClient();

        Console.WriteLine("Hello, Mii!");
    }
}