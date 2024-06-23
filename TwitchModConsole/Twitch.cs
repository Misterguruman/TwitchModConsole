using Spectre.Console.Rendering;
using Spectre.Console;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Client;
using TwitchLib.Communication.Models;
using TwitchLib.Communication.Clients;

namespace TwitchModConsole;

internal class Twitch
{
    internal TwitchClient client { get; set; }
    internal List<Tuple<string, string>> ChatOutput { get; set; } = [];

    internal Twitch(string token, string username, string channel)
    {
        ConnectionCredentials credentials = new ConnectionCredentials(username, token);
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30),

        };
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        client = new TwitchClient(customClient);
        client.Initialize(credentials, channel);

        client.OnJoinedChannel += Client_OnJoinedChannel;
        client.OnMessageReceived += Client_OnMessageReceived;
        client.OnConnected += Client_OnConnected;

        client.Connect();
    }
    public IRenderable RenderOutput()
    {
        string output = "";
        foreach ((var username, var message) in ChatOutput)
        {
            output += ($"{username}: {message.EscapeMarkup()}");
        }

        return new Markup(output);
    }

    private void Client_OnConnected(object? sender, OnConnectedArgs e)
    {
        ChatOutput.Add(new Tuple<string, string>($"[bold green]Connected[/]", "\n"));
    }

    private void Client_OnJoinedChannel(object? sender, OnJoinedChannelArgs e)
    {
        ChatOutput.Add(new Tuple<string, string>($"[bold green]Success! Connected to Twitch[/]", "\n"));
    }

    private void Client_OnMessageReceived(object? sender, OnMessageReceivedArgs messageEvent)
    {
        ChatOutput.Add(new Tuple<string, string>($"[{messageEvent.ChatMessage.ColorHex}] {messageEvent.ChatMessage.Username} [/]", $"{messageEvent.ChatMessage.Message}\n"));
    }
}
