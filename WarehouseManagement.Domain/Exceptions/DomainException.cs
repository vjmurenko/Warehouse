namespace WarehouseManagement.Domain.Exceptions;

/// <summary>
/// Base domain exception class
/// </summary>
public abstract class DomainException : Exception
{
    public string Code { get; }
    public object[] Parameters { get; }

    protected DomainException(string code, string message, params object[] parameters) : base(message)
    {
        Code = code;
        Parameters = parameters;
    }

    protected DomainException(string code, string message, Exception innerException, params object[] parameters) 
        : base(message, innerException)
    {
        Code = code;
        Parameters = parameters;
    }
}