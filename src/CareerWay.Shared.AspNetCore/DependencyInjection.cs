using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using CareerWay.Shared.AspNetCore.Handlers;
using CareerWay.Shared.AspNetCore.Middlewares;
using CareerWay.Shared.AspNetCore.Security.Claims;
using CareerWay.Shared.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection EnableRequestBuffering(this IServiceCollection services)
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

    public static IServiceCollection AddCorrelationIdProvider(this IServiceCollection services)
    {
        return services.AddScoped<CorrelationIdMiddleware>();
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

    public static IServiceCollection AddCustomOpenApi(
        this IServiceCollection services,
        string version)
    {
        return services.AddOpenApi(version);
    }

    public static IServiceCollection AddCustomOpenApi(
        this IServiceCollection services,
        string version,
        string title,
        string description,
        string projectName,
        string email,
        string fullName)
    {
        services.AddOpenApi(version, o =>
        {
            o.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                var versionedDescriptionProvider = context.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
                var apiDescription = versionedDescriptionProvider?.ApiVersionDescriptions.SingleOrDefault(description => description.GroupName == context.DocumentName);
                if (apiDescription is null)
                {
                    return Task.CompletedTask;
                }
                document.Info.Version = apiDescription.ApiVersion.ToString();
                document.Info.Title = $"{projectName} Open API | " + document.Info.Version;
                document.Info.Description = BuildDescription(apiDescription, description);
                document.Info.Contact = new OpenApiContact()
                {
                    Email = email,
                    Name = fullName
                };
                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static string BuildDescription(ApiVersionDescription api, string description)
    {
        var text = new StringBuilder(description);

        if (api.IsDeprecated)
        {
            if (text.Length > 0)
            {
                if (text[^1] != '.')
                {
                    text.Append('.');
                }

                text.Append(' ');
            }

            text.Append("This API version has been deprecated.");
        }

        if (api.SunsetPolicy is { } policy)
        {
            if (policy.Date is { } when)
            {
                if (text.Length > 0)
                {
                    text.Append(' ');
                }

                text.Append("The API will be sunset on ")
                    .Append(when.Date.ToShortDateString())
                    .Append('.');
            }

            if (policy.HasLinks)
            {
                text.AppendLine();

                var rendered = false;

                foreach (var link in policy.Links.Where(l => l.Type == "text/html"))
                {
                    if (!rendered)
                    {
                        text.Append("<h4>Links</h4><ul>");
                        rendered = true;
                    }

                    text.Append("<li><a href=\"");
                    text.Append(link.LinkTarget.OriginalString);
                    text.Append("\">");
                    text.Append(
                        StringSegment.IsNullOrEmpty(link.Title)
                        ? link.LinkTarget.OriginalString
                        : link.Title.ToString());
                    text.Append("</a></li>");
                }

                if (rendered)
                {
                    text.Append("</ul>");
                }
            }
        }

        return text.ToString();
    }

    public static IServiceCollection AddCustomApiVersioning(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    public static IServiceCollection ConfigureCustomRouteOptions(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options =>
        {
            options.LowercaseUrls = true;
            options.LowercaseQueryStrings = true;
        });
        return services;
    }
}
