using Spectre.Console;
using System.CommandLine;

public static class Operation
{
    public static Command Add(TaskManager taskManager)
    {
        var argument = new Argument<string>("description", "The task description");
        var dueOption = new Option<string?>("--due", "Expected due date (DD/MM/YYYY)");
        var command = new Command("add", "Adds a new task")
        {
            argument,
            dueOption
        };

        command.SetHandler(taskManager.AddTask, argument, dueOption);

        return command;
    }

    public static Command List(TaskManager taskManager)
    {
        var command = new Command("list", "List all tasks");

        command.SetHandler(taskManager.ListTasks);

        return command;
    }

    public static Command Complete(TaskManager taskManager)
    {
        var argument = new Argument<int>("ID", "Task ID to be completed");
        var command = new Command("complete", "Marks a task as completed")
        {
            argument
        };

        command.SetHandler(taskManager.CompleteTask, argument);

        return command;
    }

    public static Command Remove(TaskManager taskManager)
    {
        var argument = new Argument<int>("ID", "Task ID to be deleted");
        var command = new Command("delete", "Deletes a task")
        {
            argument
        };

        command.SetHandler(taskManager.RemoveTask, argument);

        return command;
    }

    public static Command RemoveCompleted(TaskManager taskManager)
    {
        var command = new Command("remove-completed", "Removes all compleated tasks");

        command.SetHandler(() =>
        {
            if (AnsiConsole.Confirm("[red]Are you sure you want to delete all completed tasks?[/]", defaultValue: false))
                taskManager.RemoveCompletedTasks();
        });

        return command;
    }

    public static Command Clear(TaskManager taskManager)
    {
        var command = new Command("clear", "Clears all tasks");

        command.SetHandler(() =>
        {
            if (AnsiConsole.Confirm("[red]Are you sure you want to delete all tasks?[/]", defaultValue: false))
                taskManager.ClearAll();
        });

        return command;
    }
}