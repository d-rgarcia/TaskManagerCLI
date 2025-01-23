using System.Text.Json;
using Models;
using Spectre.Console;

public class TaskManager
{
    private readonly string _filePath;
    private IList<ToDoTask> _tasks;

    public TaskManager()
    {
        var directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ToDoTaskManagerCLI");

        if (!Directory.Exists(directoryPath))
            Directory.CreateDirectory(directoryPath);

        _filePath = Path.Combine(directoryPath, "tasks.json");

        _tasks = loadTasksFromFile();
    }

    public void ListTasks()
    {
        if (!_tasks.Any())
        {
            AnsiConsole.MarkupLine("[green]There are no tasks![/]");
            return;
        }

        var table = new Table()
            .RoundedBorder()
            .AddColumn("[b]ID[/]")
            .AddColumn("[b]Description[/]")
            .AddColumn("[b]Status[/]")
            .AddColumn("[b]Completed at[/]")
            .Alignment(Justify.Left)
            .BorderColor(Color.CadetBlue_1);

        foreach (var task in _tasks.OrderBy(t => t.TaskStatus).ThenByDescending(t => t.Id))
        {
            string rowColor = getTaskRowColor(task);
            table.AddRow(
                $"[{rowColor}]{task.Id}[/]",
                $"[{rowColor}]{task.Description}[/]",
                $"[{rowColor}]{task.TaskStatus}[/]",
                $"[{rowColor}]{task.CompletedAt}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    public void AddTask(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            AnsiConsole.MarkupLine("[yellow]Provide a task description.[/]");
            return;
        }

        _tasks.Add(new ToDoTask
        {
            Id = generateNextId(),
            Description = description,
            TaskStatus = Models.TaskStatus.Pending
        });

        saveTasksToFile();
    }

    public void CompleteTask(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
        {
            AnsiConsole.MarkupLine($"[red]Task '{id}' not found.[/]");
            return;
        }

        task.TaskStatus = Models.TaskStatus.Completed;
        task.CompletedAt = DateTime.Now.ToShortDateString();
        saveTasksToFile();
    }

    public void RemoveTask(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task is null)
            return;

        _tasks.Remove(task);

        saveTasksToFile();
    }

    public void ClearAll()
    {
        _tasks.Clear();

        saveTasksToFile();
    }

    #region private methods

    private IList<ToDoTask> loadTasksFromFile()
    {
        if (!File.Exists(_filePath))
            return new List<ToDoTask>();

        var fileContent = File.ReadAllText(_filePath);
        if (string.IsNullOrWhiteSpace(fileContent))
            return new List<ToDoTask>();

        return JsonSerializer.Deserialize<List<ToDoTask>>(fileContent) ?? new List<ToDoTask>();
    }

    private void saveTasksToFile()
    {
        var content = JsonSerializer.Serialize(_tasks.ToList(), new JsonSerializerOptions { WriteIndented = true });

        File.WriteAllText(_filePath, content);
    }

    private int generateNextId()
    {
        return _tasks.Count > 0 ? _tasks[^1].Id + 1 : 1;
    }

    private string getTaskRowColor(ToDoTask task) => task.TaskStatus == Models.TaskStatus.Completed ? "green" : "yellow";

    #endregion
}