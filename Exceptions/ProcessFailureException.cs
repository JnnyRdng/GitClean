using GitClean.Models;

namespace GitClean.Exceptions;

public class ProcessFailureException : Exception
{
    public ProcessFailureException(string message) : base(message)
    {
    }

    public ProcessFailureException(CmdResult result) : base($"Git command exited with code {result.ExitCode}: {result.Error}")
    {
    }
}