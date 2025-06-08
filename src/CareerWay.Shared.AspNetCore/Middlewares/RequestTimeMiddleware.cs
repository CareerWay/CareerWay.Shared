using CareerWay.Shared.AspNetCore.HttpFeatures;

namespace CareerWay.Shared.AspNetCore.Middlewares;

public class RequestTimeMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var httpRequestTimeFeature = new HttpRequestTimeFeature();
        context.Features.Set<IHttpRequestTimeFeature>(httpRequestTimeFeature);
        await next(context);
    }
}
