namespace CareerWay.Shared.AspNetCore.Models;

public class ConflictErrorApiResponse : ErrorApiResponse
{
    public ConflictErrorApiResponse(string? detail, string? instance)
    {
        Title = "Conflict";
        Detail = detail;
        Status = StatusCodes.Status409Conflict;
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";
        Instance = instance;
    }
}
