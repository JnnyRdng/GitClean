using System.Diagnostics;
using GitClean.Exceptions;
using GitClean.Models;

namespace GitClean;

public static class Git
{
    public static async Task<CmdResult> RunGit(string args, string workingDirectory)
    {
        var psi = new ProcessStartInfo("git", args)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            WorkingDirectory = workingDirectory,
        };
        using var p = Process.Start(psi);
        if (p == null)
        {
            throw new ProcessFailureException("Failed to start git process");
        }

        var outputTask = p.StandardOutput.ReadToEndAsync();
        var errorTask = p.StandardError.ReadToEndAsync();
        await p.WaitForExitAsync();
        var output = await outputTask;
        var error = await errorTask;
        return new CmdResult(output, error, p.ExitCode);
    }

    public static async Task<RepoDetails> GetTopLevelOfRepo(string currentDirectory)
    {
        var res = await RunGit("rev-parse --show-toplevel", currentDirectory);
        return RepoDetails.FromCmdResult(res);
    }

    public static async Task<CmdResult> GetCurrentBranchName(string currentDirectory)
    {
        return await RunGit("rev-parse --abbrev-ref HEAD", currentDirectory);
    }

    public static async Task<List<string>> GetBranches(string workingDirectory, bool allBranches)
    {
        var res = await RunGit("branch -vv", workingDirectory);
        var lines = res.Output.Split('\n');
        var branchNames = lines
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith('*'))
            .Where(line => allBranches || line.Contains(": gone]"))
            .Select(line => line.Trim().Split(' ')[0]);
        return branchNames.ToList();
    }
}