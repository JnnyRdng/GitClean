namespace GitClean.Models;

public class CmdResult
{
    // public record CmdResult(string Output, string Error);
    public string Output { get; }
    public string Error { get; }
    
    public int ExitCode { get; }
    public bool IsError => Error.Length > 0 || ExitCode != 0;

    public CmdResult(string output, string error, int exitCode)
    {
        Output = output.Trim();
        Error = error.Trim();
        ExitCode = exitCode;
    }
}