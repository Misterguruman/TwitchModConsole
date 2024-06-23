using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchModConsole;

internal static class CommandHandler
{
    private static Task? _commandCaptureTask;
    private static readonly CancellationTokenSource CancellationTokenSource = new();

    internal static void StartCommandCapture()
    {
        _commandCaptureTask = Task.Run( () =>
        {
            while (!CancellationTokenSource.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    Twitch.CommandQueue.Enqueue(key.Key);
                }
            }
        });

        AnsiConsole.MarkupLineInterpolated($"[bold lightcoral]Command Capture[/] is [bold green]running[/]");
    }

    public static void StopCommandCapture()
    {
        AnsiConsole.MarkupLineInterpolated($"Stopping [bold lightcoral]Command Capture[/]...");
        CancellationTokenSource.Cancel();
        _commandCaptureTask?.Wait();
    }
}
