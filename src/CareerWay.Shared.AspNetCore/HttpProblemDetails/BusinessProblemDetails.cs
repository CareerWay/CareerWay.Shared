using Microsoft.AspNetCore.Mvc;

namespace CareerWay.Shared.AspNetCore.HttpProblemDetails;

public class BusinessProblemDetails : ProblemDetails
{
    public BusinessProblemDetails(string? detail, string? instance)
    {
        Title = "Rule Violation";
        Detail = detail;
        Status = StatusCodes.Status422UnprocessableEntity;
        Type = "https://datatracker.ietf.org/doc/html/rfc4918#section-11.2";
        Instance = instance;
    }
}
