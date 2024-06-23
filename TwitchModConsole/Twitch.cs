using System.Collections.Concurrent;
using Spectre.Console.Rendering;
using Spectre.Console;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

namespace TwitchModConsole;

internal static class Twitch
{
    private static Task? _twitchTask;
    private static readonly CancellationTokenSource CancellationTokenSource = new();
    private static readonly ManualResetEventSlim CompletionEvent = new();
    private static readonly ConcurrentQueue<Tuple<string, string>> ChatEntries = new();
    private static TwitchClient? TwitchClient { get; set; }

    internal static void StartTwitch(string token, string username, string channel)
    {
        _twitchTask = Task.Run(async () =>
        {
            ConnectionCredentials credentials = new ConnectionCredentials(username, token);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30),

            };
            WebSocketClient customClient = new WebSocketClient(clientOptions);
            TwitchClient = new TwitchClient(customClient);
            TwitchClient.Initialize(credentials, channel);

            TwitchClient.OnJoinedChannel += Client_OnJoinedChannel;
            TwitchClient.OnMessageReceived += Client_OnMessageReceived;
            TwitchClient.OnConnected += Client_OnConnected;

            TwitchClient.Connect();
        }, CancellationTokenSource.Token);
    }

    private static void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        ChatEntries.Enqueue(new Tuple<string, string>($"[bold green]Connected[/]", "\n"));
    }

    private static void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        ChatEntries.Enqueue(new Tuple<string, string>($"[bold green]Success! Connected to Twitch[/]", "\n"));
    }

    private static void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs messageEvent)
    {
        ChatEntries.Enqueue(new Tuple<string, string>($"[{messageEvent.ChatMessage.ColorHex}] {messageEvent.ChatMessage.Username} [/]", $"{messageEvent.ChatMessage.Message}\n"));
        //AnsiConsole.MarkupLineInterpolated($"[{messageEvent.ChatMessage.ColorHex}] {messageEvent.ChatMessage.Username} [/]: {messageEvent.ChatMessage.Message}");
    }
    public static void StopTwitch()
    {
        CancellationTokenSource.Cancel();
        _twitchTask?.Wait();
    }

    public static void WaitForCompletion()
    {
        CompletionEvent.Wait();
    }
}
