using GitClean.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitClean.Commands;

public class CleanCommand : AsyncCommand<CleanCommandSettings>
{
    private CleanCommandSettings Settings { get; set; } = null!;

    public override async Task<int> ExecuteAsync(CommandContext context, CleanCommandSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        var directory = Settings.WorkingDir;
        AnsiConsole.MarkupLine($"Repo: [blue bold]{directory}[/]");
        RenderSettingsTable();
        AnsiConsole.WriteLine();
        var branches = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[bold]Finding branches that have been removed on the remote.[/]",
                async ctx =>
                {
                    await Git.RunGit("fetch --prune", directory);
                    return await Git.GetBranches(directory, Settings.AllBranches);
                });

        if (branches.Count == 0)
        {
            AnsiConsole.MarkupLine("No branches to delete.");
            return 0;
        }

        var selected = await AnsiConsole.PromptAsync(
            new MultiSelectionPrompt<string>()
                .Title("Select [bold red]branches to delete[/]:")
                .NotRequired()
                .PageSize(10)
                .WrapAround(true)
                .MoreChoicesText("[grey](Move up/down to reveal more)[/]")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
                .AddChoices(branches)
        );

        if (selected.Count == 0)
        {
            AnsiConsole.MarkupLine("No branches selected.");
            return 0;
        }

        AnsiConsole.WriteLine("The following branches are to be removed:");
        foreach (var branch in selected)
        {
            AnsiConsole.MarkupLine($"[blue]{branch}[/]");
        }

        var confirm = await AnsiConsole.ConfirmAsync("Continue?", false);
        if (!confirm)
        {
            AnsiConsole.MarkupLine("Cancelled!");
            return 0;
        }

        if (Settings.DryRun)
        {
            AnsiConsole.MarkupLine("[bold]Dry run mode![/] Would have tried to execute:");
            selected.ForEach(b => AnsiConsole.MarkupLine($"[grey]git branch -d {b}[/]"));
            return 0;
        }

        var failed = await TryDeleteBranches(selected);
        if (failed.Count == 0)
        {
            return 0;
        }

        var plural = $"branch{(failed.Count == 1 ? "" : "es")}";
        AnsiConsole.MarkupLine($"{failed.Count} {plural} could not be deleted.");
        var confirmForce = await AnsiConsole.ConfirmAsync($"Force delete the failed {plural}", false);
        if (confirmForce) return await ForceDeleteBranches(failed);
        AnsiConsole.MarkupLine("Cancelled!");
        return 0;
    }

    private async Task<List<string>> TryDeleteBranches(List<string> branches)
    {
        var failed = new List<string>();
        foreach (var branch in branches)
        {
            var command = $"branch -d {branch}";
            var res = await Git.RunGit(command, Settings.WorkingDir);
            if (res.IsError)
            {
                failed.Add(branch);
                AnsiConsole.MarkupLine($"[red]Failed to delete [/] {branch}");
                AnsiConsole.MarkupLine($"[grey]{res.Error}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[green]Deleted {branch}[/]");
            }
        }

        return failed;
    }

    private async Task<int> ForceDeleteBranches(List<string> branches)
    {
        var command = $"branch -D {string.Join(' ', branches)}";
        var res = await Git.RunGit(command, Settings.WorkingDir);
        if (!res.IsError) return 0;
        AnsiConsole.MarkupLine("[red]Failed! {error}", res.Error);
        return 1;
    }

    private void RenderSettingsTable()
    {
        var table = new Table();
        table.AddColumn("Setting");
        table.AddColumn(new TableColumn("").Centered());
        table.HideHeaders();
        table.Border(TableBorder.Rounded);
        table.AddRow("Dry Run Mode", GetSettingsTableValue(Settings.DryRun));
        table.AddRow("All Branches", GetSettingsTableValue(Settings.AllBranches));
        table.Collapse();
        AnsiConsole.Write(table);
    }

    private static string GetSettingsTableValue(bool value)
    {
        return $"[bold]{(value ? "on" : "off")}[/]";
    }
}