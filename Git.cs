using System.Diagnostics;
using GitClean.Exceptions;
using GitClean.Models;

namespace GitClean;

public class Git
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

        await p.WaitForExitAsync();
        var output = await p.StandardOutput.ReadToEndAsync();
        var error = await p.StandardError.ReadToEndAsync();
        return new CmdResult(output, error);
    }

    public static async Task<RepoDetails> GetTopLevelOfRepo(string currentDirectory)
    {
        var res = await RunGit("rev-parse --show-toplevel", currentDirectory);
        return RepoDetails.FromCmdResult(res);
    }

    public static async Task<List<string>> GetRemovedBranches(string workingDirectory, bool allBranches)
    {
        var res = await RunGit("branch -vv", workingDirectory);
        var lines = res.Output.Split('\n');
        var removedLines = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => allBranches || line.Contains(": gone]"));
        var branchNames = removedLines.Select(line => line.Trim().Split(' ')[0]);
        return branchNames.ToList();
    }
}