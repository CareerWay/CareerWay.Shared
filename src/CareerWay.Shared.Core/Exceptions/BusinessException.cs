namespace CareerWay.Shared.Core.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(
        string? message = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
