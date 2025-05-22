namespace GitClean.Models;

public class RepoDetails
{
    public bool IsGitRepo { get; private init; }
    public string TopLevelDirectory { get; private init; } = string.Empty;

    public bool IsInvalid()
    {
        return !IsGitRepo || string.IsNullOrWhiteSpace(TopLevelDirectory) || !Directory.Exists(TopLevelDirectory);
    }

    public static RepoDetails FromCmdResult(CmdResult res)
    {
        return new RepoDetails
        {
            IsGitRepo = !res.IsError,
            TopLevelDirectory = res.Output
        };
    }
}