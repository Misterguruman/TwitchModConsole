using Spectre.Console;
using TwitchLib.Client.Events;

namespace TwitchModConsole;

public static class Program {
    static async Task Main(string[] args)
    {
        AnsiConsole.WriteLine("Welcome to TwitchModConsole :)");
        
        string? token = null;
        if (File.Exists("token.txt"))
        {
            token = File.ReadAllText("token.txt");
        }

        if (token is null) {
            token = await TwitchOAuth.GetOAuthToken();
            if (token is null)
            {
                //TODO: Create Custom Error
                AnsiConsole.MarkupLine("[red]Error: Unable to get OAuth token. Exiting...[/]");
                Environment.Exit(1);
            }

            await File.WriteAllTextAsync("token.txt", token);
        }

        var username = AnsiConsole.Ask<string>("Enter your [purple]Twitch username[/] : ");
        
        List<string> channels = [];
        string? selectedChannel = null;
        if (File.Exists("channels.txt"))
        {
            channels = File.ReadAllLines("channels.txt").ToList();
        }

        if (channels.Count == 0)
        {
            var channel = AnsiConsole.Ask<string>("Enter the [purple]Twitch channel[/] you want to join : ");
            selectedChannel = channel;
            channels.Add(channel);
            await File.WriteAllLinesAsync("channels.txt", channels);
        }
        else
        {
            channels.Add("Add Channel +");
            selectedChannel = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select a channel to join")
                    .PageSize(10)
                    .AddChoices(channels)
                );

        }

        if (selectedChannel == "Add Channel +")
        {
            var channel = AnsiConsole.Ask<string>("Enter the [purple]Twitch channel[/] you want to join : ");
            selectedChannel = channel;
            channels.Remove("Add Channel +");
            channels.Add(channel);
            await File.WriteAllLinesAsync("channels.txt", channels);
        }

        Twitch.StartTwitch(token, username, selectedChannel!);
        CommandHandler.StartCommandCapture();

        while (true)
        {
            if (Twitch.CommandQueue.TryDequeue(out var key))
            {
                if (key == ConsoleKey.Q)
                    break;
                else if (key == ConsoleKey.S)
                {
                    var message = AnsiConsole.Ask<string>("[bold green]Send[/] :");
                    Twitch.TwitchClient!.SendMessage(selectedChannel, message);
                    AnsiConsole.MarkupLineInterpolated($"[bold red]{username}[/] : {message}");
                }

                CommandHandler.IsCommandQueued = false;
            }
            else if (Twitch.ChatEntries.TryDequeue(out OnMessageReceivedArgs? messageEvent))
            {
                if (messageEvent is null)
                    continue;

                AnsiConsole.MarkupLineInterpolated($"[{messageEvent.ChatMessage.ColorHex}] {messageEvent.ChatMessage.Username} [/] : {messageEvent.ChatMessage.Message.EscapeMarkup()}");
            }

            


        }
        
        Twitch.StopTwitch();
        CommandHandler.StopCommandCapture();
    }
}
