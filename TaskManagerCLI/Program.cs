using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

internal class Program
{
    private static async Task Main(string[] args)
    {
        TaskManager taskManager = new TaskManager();

        var rootCommand = new RootCommand("Task Manager CLI");

        rootCommand.AddCommand(Operation.Add(taskManager));
        rootCommand.AddCommand(Operation.List(taskManager));
        rootCommand.AddCommand(Operation.Remove(taskManager));
        rootCommand.AddCommand(Operation.Complete(taskManager));
        rootCommand.AddCommand(Operation.RemoveCompleted(taskManager));
        rootCommand.AddCommand(Operation.Clear(taskManager));

        var parser = new CommandLineBuilder(rootCommand)
            .AddMiddleware(async (context, next) =>
            {
                await next(context);
            })
            .UseDefaults()
            .Build();

        await parser.InvokeAsync(args);
    }
}