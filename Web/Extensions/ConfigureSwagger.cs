using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace Web.Extensions;

public static class ConfigureSwagger
{
    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("admin", new OpenApiInfo
            {
                Version = "v1",
                Title = "Admin APIs",
                Description = "Administrator related interfaces"
            });

            options.SwaggerDoc("common", new OpenApiInfo
            {
                Version = "v1",
                Title = "Common APIs",
                Description = "General public interfaces"
            });

            options.SwaggerDoc("auth", new OpenApiInfo
            {
                Version = "v1",
                Title = "Auth APIs",
                Description = "Authorization interfaces"
            });

            options.SwaggerDoc("blog", new OpenApiInfo
            {
                Version = "v1",
                Title = "Blog APIs",
                Description = "Blog management interfaces"
            });

            options.SwaggerDoc("test", new OpenApiInfo
            {
                Version = "v1",
                Title = "Test APIs",
                Description = "Test interfaces"
            });

            // Enable little green lock
            var security = new OpenApiSecurityScheme
            {
                Description = "JWT authentication mode, please enter \"Bearer {Token}\" for authentication",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            };
            options.AddSecurityDefinition("oauth2", security);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement { { security, new List<string>() } });
            options.OperationFilter<AddResponseHeadersFilter>();
            options.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            options.OperationFilter<SecurityRequirementsOperationFilter>();

            // XML comments
            var filePath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(filePath, true);
        });
    }
}