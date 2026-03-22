using Models;
using Spectre.Console;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;

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
            .AddColumn("[b]Due Date[/]")
            .AddColumn("[b]Completed at[/]")
            .Alignment(Justify.Left)
            .BorderColor(Color.CadetBlue_1);

        foreach (var task in _tasks.OrderBy(t => t.TaskStatus).ThenByDescending(t => t.Id))
        {
            string rowColor = getTaskRowColor(task);
            string dueDateDisplay = formatDueDateDisplay(task);
            table.AddRow(
                $"[{rowColor}]{task.Id}[/]",
                $"[{rowColor}]{task.Description}[/]",
                $"[{rowColor}]{task.TaskStatus}[/]",
                dueDateDisplay,
                $"[{rowColor}]{task.CompletedAt}[/]"
            );
        }

        AnsiConsole.Write(table);
    }

    public void AddTask(string description, string? dueDate = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            AnsiConsole.MarkupLine("[yellow]Provide a task description.[/]");
            return;
        }

        if (dueDate is not null && !DateOnly.TryParseExact(dueDate, "dd/MM/yyyy", null, DateTimeStyles.None, out _))
        {
            AnsiConsole.MarkupLine("[red]Invalid date format. Use DD/MM/YYYY.[/]");
            return;
        }

        _tasks.Add(new ToDoTask
        {
            Id = generateNextId(),
            Description = description,
            TaskStatus = Models.TaskStatus.Pending,
            DueDate = dueDate
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

    public void RemoveCompletedTasks()
    {
        var completedTasks = _tasks.Where(t => t.TaskStatus == Models.TaskStatus.Completed).ToList();
        foreach (var task in completedTasks)
        {
            _tasks.Remove(task);
        }

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
        return _tasks.Any() ? _tasks.Max(t => t.Id) + 1 : 1;
    }

    private string getTaskRowColor(ToDoTask task) => task.TaskStatus == Models.TaskStatus.Completed ?
        "green" :
        "yellow";

    private string formatDueDateDisplay(ToDoTask task)
    {
        if (string.IsNullOrEmpty(task.DueDate))
            return string.Empty;

        string color = getTaskRowColor(task);

        if (task.TaskStatus == Models.TaskStatus.Pending && isDueToday(task.DueDate))
            color = "red";

        return $"[{color}]{task.DueDate}[/]";
    }

    private static bool isDueToday(string dueDate)
    {
        if (!DateOnly.TryParseExact(dueDate, "dd/MM/yyyy", null, DateTimeStyles.None, out var parsed))
            return false;

        return parsed <= DateOnly.FromDateTime(DateTime.Today);
    }

    #endregion

    public void NotifyTasksDueToday()
    {
        var dueTodayTasks = _tasks
            .Where(t => t.TaskStatus == Models.TaskStatus.Pending && !string.IsNullOrEmpty(t.DueDate) && isDueToday(t.DueDate))
            .ToList();

        if (!dueTodayTasks.Any())
            return;

        try
        {
            foreach (var task in dueTodayTasks)
                sendWindowsToast("Task Manager — Due Today", task.Description);
        }
        catch
        {
            // Notifications are best-effort; silent fail if the OS doesn't support them.
        }
    }

    private static void sendWindowsToast(string title, string message)
    {
        // Escape single quotes for PowerShell string literals
        string safeTitle = title.Replace("'", "''");
        string safeMessage = message.Replace("'", "''");

        string script = $"""
            [void][Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType=WindowsRuntime]
            [void][Windows.UI.Notifications.ToastNotification, Windows.UI.Notifications, ContentType=WindowsRuntime]
            [void][Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType=WindowsRuntime]
            $xml = '<toast><visual><binding template="ToastText02"><text id="1">{safeTitle}</text><text id="2">{safeMessage}</text></binding></visual></toast>'
            $doc = [Windows.Data.Xml.Dom.XmlDocument]::new()
            $doc.LoadXml($xml)
            $toast = [Windows.UI.Notifications.ToastNotification]::new($doc)
            [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('TaskManagerCLI').Show($toast)
            """;

        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell",
            Arguments = $"-NoProfile -NonInteractive -WindowStyle Hidden -Command \"{script.Replace("\"", "\\\"")}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        });
    }
}