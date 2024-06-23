
using System.Net;
using Spectre.Console;

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

            File.WriteAllText("token.txt", token);
        }

    }
}
