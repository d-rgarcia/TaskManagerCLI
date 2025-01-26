#### Simple console TaskManager to be installed as a CLI tool.

Usage: *task [command] [attribute]*

Type: *```task```* for more info.

For installing the tool, from csproj root folder execute: 

```dotnet tool install --global --add-source ./nupkg TaskManagerCLI```

For uninstalling:

 ```dotnet tool uninstall --global TaskManagerCLI```

 Tool help:

 ```powershell
 Description:
  Task Manager CLI

Usage:
  TaskManagerCLI [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  add <description>  Adds a new task
  list               List all tasks
  delete <ID>        Deletes a task
  complete <ID>      Marks a task as completed
  clear              Clears all tasks
```

The file for managing the tasks is located in: ```C:\Users\Diego\AppData\Roaming\ToDoTaskManagerCLI```

For testing it without installing, execute: dotnet run -- [command] [options]     

Inspired on: karenpayneoregon/command-line-exploration/