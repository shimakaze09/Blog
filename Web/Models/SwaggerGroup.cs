using Microsoft.OpenApi.Models;

namespace Web.Models;

/// <summary>
///     API Group
///     <para>Group design reference: https://wangyou233.wang/archives/73</para>
/// </summary>
public class SwaggerGroup
{
    public SwaggerGroup(string name, string? title = null, string? description = null)
    {
        Name = name;
        Title = title;
        Description = description;
    }

    /// <summary>
    ///     Group name (also used as URL prefix)
    /// </summary>
    public string Name { get; set; }

    public string? Title { get; set; }
    public string? Description { get; set; }

    /// <summary>
    ///     Generates <see cref="Microsoft.OpenApi.Models.OpenApiInfo" />
    /// </summary>
    public OpenApiInfo ToOpenApiInfo(string version = "1.0")
    {
        var item = new OpenApiInfo();
        Title ??= Name;
        Description ??= Name;
        return new OpenApiInfo { Title = Title, Description = Description, Version = version };
    }
}