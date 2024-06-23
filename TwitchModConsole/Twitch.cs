using System.Collections.Concurrent;
using Spectre.Console.Rendering;
using Spectre.Console;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;

namespace TwitchModConsole;

internal static class Twitch
{
    private static Task? _twitchTask;
    private static readonly CancellationTokenSource CancellationTokenSource = new();
    internal static readonly ConcurrentQueue<OnMessageReceivedArgs> ChatEntries = new();
    internal static readonly ConcurrentQueue<ConsoleKey> CommandQueue = new();
    internal static TwitchClient? TwitchClient { get; set; }

    internal static void StartTwitch(string token, string username, string channel)
    {
        
        _twitchTask = Task.Run( () =>
        {
            var credentials = new ConnectionCredentials(username, token);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),

            };
            
            var customClient = new WebSocketClient(clientOptions);
            TwitchClient = new TwitchClient(customClient);
            TwitchClient.Initialize(credentials, channel);

            TwitchClient.OnJoinedChannel += Client_OnJoinedChannel;
            TwitchClient.OnMessageReceived += Client_OnMessageReceived;
            TwitchClient.OnConnected += Client_OnConnected;

            TwitchClient.Connect();
        }, CancellationTokenSource.Token);
        
        AnsiConsole.MarkupLineInterpolated($"[bold purple]Twitch[/] client is [bold green]running[/]");
    }

    private static void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold green]Connected[/]");
    }

    private static void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        AnsiConsole.MarkupLineInterpolated($"[bold green]Success![/] Connected to [bold purple]Twitch[/]");
    }

    private static void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs messageEvent)
    {
        ChatEntries.Enqueue(messageEvent);
        //AnsiConsole.MarkupLineInterpolated($"[{messageEvent.ChatMessage.ColorHex}] {messageEvent.ChatMessage.Username} [/]: {messageEvent.ChatMessage.Message}");
    }
    public static void StopTwitch()
    {
        AnsiConsole.MarkupLineInterpolated($"Stopping [bold purple]Twitch[/] client...");
        CancellationTokenSource.Cancel();
        _twitchTask?.Wait();
    }

}
