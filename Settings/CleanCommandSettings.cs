using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace GitClean.Settings;

public class CleanCommandSettings : CommandSettings
{
    [Description("Optional target directory. If omitted, the current working directory is used. Must be within a git repository.")]
    [CommandArgument(0, "[DIRECTORY]")]
    public string WorkingDir { get; private set; } = Directory.GetCurrentDirectory();

    [Description("Dry run. No branches will be deleted")]
    [CommandOption("-d|--dry-run")]
    public bool DryRun { get; private set; }
    
    [Description("Dangerous. Select from all branches.")]
    [CommandOption("-a|--all")]
    public bool AllBranches { get; private set; }

    public override ValidationResult Validate()
    {
        if (!Directory.Exists(WorkingDir))
        {
            return ValidationResult.Error("Directory not found");
        }

        var repoDetails = Git.GetTopLevelOfRepo(WorkingDir).GetAwaiter().GetResult();
        if (repoDetails.IsInvalid())
        {
            return ValidationResult.Error($"'{WorkingDir}' is not in a git repository.");
        }

        WorkingDir = repoDetails.TopLevelDirectory;

        return ValidationResult.Success();
    }
}