using CareerWay.Shared.Validation.Validation;

namespace CareerWay.Shared.AspNetCore.Models;

public class ValidationErrorApiResponse : ErrorApiResponse
{
    public List<ValidationFailureResult> Errors { get; set; } = [];

    public ValidationErrorApiResponse(string? detail, string? instance, List<ValidationFailureResult> errors)
    {
        Title = "Validation Failure";
        Detail = detail;
        Status = StatusCodes.Status400BadRequest;
        Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        Instance = instance;
        Errors = errors;
    }
}
