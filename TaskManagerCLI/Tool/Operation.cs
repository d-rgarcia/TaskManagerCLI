using Spectre.Console;

public static class Operation
{
    public static void ExecuteCommands(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            Help.Print();
            return;
        }

        execute(args);
    }

    private static void execute(string[] args)
    {
        var taskManager = new TaskManager();

        switch (args[0].ToLower())
        {
            case "add":
                if (args.Length < 2)
                {
                    AnsiConsole.Markup("[red]Error:[/] Task description is missing.");
                    return;
                }
                taskManager.AddTask(args[1]);
                break;

            case "list":
                taskManager.ListTasks();
                break;

            case "complete":
                if (args.Length < 2 || !int.TryParse(args[1], out int completeId))
                {
                    AnsiConsole.Markup("[red]Error:[/] Invalid task ID.");
                    return;
                }
                taskManager.CompleteTask(completeId);
                break;

            case "delete":
                if (args.Length < 2 || !int.TryParse(args[1], out int deleteId))
                {
                    AnsiConsole.Markup("[red]Error:[/] Invalid task ID.");
                    return;
                }
                taskManager.RemoveTask(deleteId);
                break;
            
            case "clear":
                taskManager.ClearAll();
                break;
                
            case "help":
                Help.Print();
                break;

            default:
                AnsiConsole.Markup($"[red]Unknown command:[/] {args[0]}");
                Help.Print();
                break;
        }
    }
}