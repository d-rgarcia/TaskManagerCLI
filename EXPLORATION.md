# Technical Exploration — TaskManagerCLI

## Project Structure

```
TaskManagerCLI/
├── Program.cs           — Entry point, CLI setup via System.CommandLine
├── Tool/Operation.cs    — Command definitions (add, list, complete, delete, clear)
├── Core/TaskManager.cs  — Business logic + JSON persistence
├── Models/ToDoTask.cs   — Task model
```

## Program.cs Analysis

- Uses `System.CommandLine` with `CommandLineBuilder` + `UseDefaults()`.
- Middleware block is a no-op (`await next(context)` only) — can be removed or used for future cross-cutting concerns (logging, error handling).
- All commands delegated to `Operation` static factory methods, keeping entry point clean.

## TaskManager.cs Key Points

| Aspect | Detail |
|---|---|
| Persistence | JSON via `System.Text.Json`, stored in `%AppData%\ToDoTaskManagerCLI\tasks.json` |
| UI | `Spectre.Console` for colored table output |
| Load strategy | Eager load on construction, in-memory list during session |
| Save strategy | Full file rewrite on every mutation |

## Identified Issues

### 1. Frágil generación de IDs (`Core/TaskManager.cs:125`) — **Prioritario**
```csharp
// Actual — asume que el último elemento tiene el ID más alto
return _tasks.Count > 0 ? _tasks[^1].Id + 1 : 1;

// Correcto — funciona incluso tras eliminar tareas
return _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
```
Si se eliminan tareas intermedias y se reordenan en disco, el índice `[^1]` puede devolver un ID menor al máximo existente, causando duplicados.

### 2. Middleware vacío (`Program.cs:20-23`)
El bloque de middleware no hace nada. Es un buen punto para añadir manejo global de excepciones en el futuro, pero actualmente genera ruido.

### 3. `clear` sin confirmación (`Tool/Operation.cs:53-59`)
El comando `clear` elimina todas las tareas sin ningún prompt de confirmación. Riesgo de pérdida de datos accidental.

## Recommended Next Action

Fix ID generation: cambiar `_tasks[^1].Id + 1` por `_tasks.Max(t => t.Id) + 1` en `Core/TaskManager.cs:125`.
