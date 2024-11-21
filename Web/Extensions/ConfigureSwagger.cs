using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerUI;
using Web.Models;

namespace Web.Extensions;

public static class ApiGroups
{
    public const string Admin = "admin";
    public const string Auth = "auth";
    public const string Blog = "blog";
    public const string Comment = "comment";
    public const string Common = "common";
    public const string Link = "link";
    public const string Photo = "photo";
    public const string Test = "test";
}

public static class ConfigureSwagger
{
    public static readonly List<SwaggerGroup> Groups = new()
    {
        new SwaggerGroup(ApiGroups.Admin, "Admin APIs", "Administrator-related interfaces"),
        new SwaggerGroup(ApiGroups.Auth, "Auth APIs", "Authentication interfaces"),
        new SwaggerGroup(ApiGroups.Blog, "Blog APIs", "Blog management interfaces"),
        new SwaggerGroup(ApiGroups.Comment, "Comment APIs", "Comment interfaces"),
        new SwaggerGroup(ApiGroups.Common, "Common APIs", "Common public interfaces"),
        new SwaggerGroup(ApiGroups.Link, "Link APIs", "Friendly link interfaces"),
        new SwaggerGroup(ApiGroups.Photo, "Photo APIs", "Image Management APIs"),
        new SwaggerGroup(ApiGroups.Test, "Test APIs", "Test interfaces")
    };

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            Groups.ForEach(group => options.SwaggerDoc(group.Name, group.ToOpenApiInfo()));

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

    public static void UseSwaggerPkg(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(opt =>
        {
            opt.RoutePrefix = "api-docs/swagger";
            // Default expansion depth of models, set to -1 to completely hide models
            opt.DefaultModelsExpandDepth(-1);
            // Only expand marked API documents
            opt.DocExpansion(DocExpansion.List);
            opt.DocumentTitle = "Blog APIs";
            // Grouping
            Groups.ForEach(group => opt.SwaggerEndpoint($"/swagger/{group.Name}/swagger.json", group.Name));
        });
    }
}