using Spectre.Console;

public static class Help
{
    public static void Print()
    {
        AnsiConsole.MarkupLine($"Usage: [yellow]todo {Markup.Escape("[command]")} {Markup.Escape("[attribute]")}[/]");
        AnsiConsole.MarkupLine("Commands:");
        AnsiConsole.MarkupLine($"  add {Markup.Escape("[description]")}");
        AnsiConsole.MarkupLine("  list");
        AnsiConsole.MarkupLine($"  complete {Markup.Escape("[id]")}");
        AnsiConsole.MarkupLine($"  delete {Markup.Escape("[id]")}");
        AnsiConsole.MarkupLine("  clear");
        AnsiConsole.MarkupLine("  help");
    }
}