using CareerWay.Shared.AspNetCore.Handlers;
using CareerWay.Shared.AspNetCore.Middlewares;
using CareerWay.Shared.AspNetCore.Security.Claims;
using CareerWay.Shared.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddEnableRequestBuffering(this IServiceCollection services)
    {
        return services.AddScoped<EnableRequestBufferingMiddleware>();
    }

    public static IServiceCollection AddRequestTime(this IServiceCollection services)
    {
        return services.AddScoped<RequestTimeMiddleware>();
    }

    public static IServiceCollection AddHttpContextCurrentPrincipalAccessor(this IServiceCollection services)
    {
        return services.AddScoped<ICurrentPrincipalAccessor, HttpContextCurrentPrincipalAccessor>();
    }

    public static IServiceCollection AddPushSerilogProperties(this IServiceCollection services)
    {
        return services.AddScoped<PushSerilogPropertiesMiddleware>();
    }

    public static IServiceCollection AddCustomExceptionHandler(this IServiceCollection services)
    {
        return services.AddExceptionHandler<CustomExceptionHandler>();
    }

    public static IServiceCollection AddCustomAuthentication(
      this IServiceCollection services,
      string audience,
      string issuer,
      string issuerSigningKey)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IServiceCollection AddCustomAuthentication(
        this IServiceCollection services,
        string token,
        string challangeRedirect,
        string forbiddenRedirect,
        string audience,
        string issuer,
        string issuerSigningKey)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[token];
                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect(challangeRedirect);
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.Redirect(forbiddenRedirect);
                        return Task.CompletedTask;
                    }
                };

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = audience,
                    ValidIssuer = issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(issuerSigningKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }
}
