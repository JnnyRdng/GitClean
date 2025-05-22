namespace GitClean.Models;

public class CmdResult
{
    // public record CmdResult(string Output, string Error);
    public string Output { get; }
    public string Error { get; }
    public bool IsError => Error.Length > 0;


    public CmdResult(string output, string error)
    {
        Output = output.Trim();
        Error = error.Trim();
    }
}