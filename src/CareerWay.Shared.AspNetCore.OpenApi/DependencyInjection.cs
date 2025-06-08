using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
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
}

