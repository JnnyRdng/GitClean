namespace GitClean.Models;

public class CmdResult
{
    public string Output { get; }
    public string Error { get; }
    
    public int ExitCode { get; }
    public bool IsError => ExitCode != 0 || Error.Length > 0;

    public CmdResult(string output, string error, int exitCode)
    {
        Output = output.Trim();
        Error = error.Trim();
        ExitCode = exitCode;
    }
}