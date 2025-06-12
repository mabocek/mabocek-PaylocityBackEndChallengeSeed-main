using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Swagger;

public class VersionSubstitutionDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var pathsToUpdate = new Dictionary<string, OpenApiPathItem>();

        foreach (var path in swaggerDoc.Paths)
        {
            if (path.Key.Contains("{version}"))
            {
                // Replace {version} with v1 (or get the version from context)
                var newPath = path.Key.Replace("{version}", "1");
                pathsToUpdate.Add(newPath, path.Value);
            }
        }

        // Clear the original paths with {version} and add the updated ones
        swaggerDoc.Paths.Clear();
        foreach (var path in pathsToUpdate)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}
