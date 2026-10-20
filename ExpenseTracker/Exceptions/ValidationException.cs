namespace ExpenseTracker.Exceptions;

public class ValidationException(IDictionary<string, string[]> errors): Exception(DefaultMessage)
{
    public const string DefaultMessage = "The request contains invalid data.";

    public IDictionary<string, string[]> Errors { get; } = errors;

    public ValidationException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] })
    {
    }
}
