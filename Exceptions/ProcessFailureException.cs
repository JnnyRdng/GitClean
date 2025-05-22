namespace GitClean.Exceptions;

public class ProcessFailureException : Exception
{
    public ProcessFailureException(string message) : base(message)
    {
    }
}