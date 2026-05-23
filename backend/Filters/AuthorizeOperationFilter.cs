using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace backend.Filters;

public class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authorizeAttrs = context.MethodInfo.DeclaringType!
            .GetCustomAttributes(true).OfType<AuthorizeAttribute>()
            .Concat(context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>())
            .ToList();

        if (authorizeAttrs.Count == 0) return;

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };
        operation.Security = [new OpenApiSecurityRequirement { { scheme, [] } }];

        var roles = authorizeAttrs
            .Where(a => !string.IsNullOrEmpty(a.Roles))
            .SelectMany(a => a.Roles!.Split(','))
            .Select(r => r.Trim())
            .Distinct()
            .ToList();

        if (roles.Count > 0)
        {
            var roleList = string.Join(", ", roles);
            operation.Description = string.IsNullOrEmpty(operation.Description)
                ? $"**Required role(s):** {roleList}"
                : $"**Required role(s):** {roleList}\n\n{operation.Description}";
        }
    }
}
