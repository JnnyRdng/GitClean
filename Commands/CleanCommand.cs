using System.Diagnostics;
using GitClean.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitClean.Commands;

public class CleanCommand : AsyncCommand<CleanCommandSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, CleanCommandSettings settings)
    {
        var directory = settings.WorkingDir;
        AnsiConsole.MarkupLine($"Repo: [blue bold]{directory}[/]");
        RenderSettingsTable(settings);
        AnsiConsole.MarkupLine("");
        var branches = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[bold]Finding branches that have been removed on the remote.[/]",
                async ctx =>
                {
                    await Git.RunGit("fetch --prune", directory);
                    return await Git.GetRemovedBranches(directory);
                });

        if (branches.Count == 0)
        {
            AnsiConsole.MarkupLine("No branches to delete.");
            return 0;
        }

        var selected = AnsiConsole.Prompt(
            new MultiSelectionPrompt<string>()
                .Title("Select [bold red]branches to delete[/]:")
                .NotRequired()
                .PageSize(10)
                .MoreChoicesText("[grey](Move up/down to reveal more)[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
                .AddChoices(branches)
        );

        if (selected.Count == 0)
        {
            AnsiConsole.MarkupLine("No branches selected.");
            return 0;
        }

        var concatBranches = string.Join(' ', selected);
        var gitCommand = $"branch -{(settings.Force ? "D" : "d")} {concatBranches}";

        if (settings.DryRun)
        {
            AnsiConsole.MarkupLine("[bold]Dry run mode![/] Would have executed command:");
            AnsiConsole.MarkupLine($"[blue bold]git {gitCommand}[/]");
        }
        else
        {
            var res = await Git.RunGit(gitCommand, directory);
            if (res.IsError)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {res.Error}");
                return 1;
            }

            AnsiConsole.MarkupLine($"[green]Deleted {selected.Count} branches.[/]");
        }

        return 0;
    }


    private static void RenderSettingsTable(CleanCommandSettings settings)
    {
        var table = new Table();
        table.AddColumn("Setting");
        table.AddColumn(new TableColumn("").Centered());
        table.HideHeaders();
        table.Border(TableBorder.Rounded);
        table.AddRow("Force Delete", GetSettingsTableValue(settings.Force));
        table.AddRow("Dry Run Mode", GetSettingsTableValue(settings.DryRun));
        table.Collapse();
        AnsiConsole.Write(table);
    }

    private static string GetSettingsTableValue(bool value)
    {
        return $"[bold]{(value ? "on" : "off")}[/]";
    }
}