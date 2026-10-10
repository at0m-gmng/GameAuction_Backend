using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace GameBackend.ServiceDefaults.Extensions;

/// <summary>
/// OpenAPI-документ со схемой Bearer, чтобы в Scalar можно было вызывать защищённые методы.
/// </summary>
public static class OpenApiExtensions
{
    private const string BearerSchemeName = "Bearer";
    private const string BearerFormat = "JWT";

    /// <summary>
    /// Регистрирует OpenAPI с JWT Bearer-схемой, применённой ко всем операциям.
    /// </summary>
    /// <param name="services">Коллекция сервисов.</param>
    /// <returns>Та же коллекция.</returns>
    public static IServiceCollection AddApiDocumentation(this IServiceCollection services) =>
        services.AddOpenApi(options => options.AddDocumentTransformer((document, _, _) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
            document.Components.SecuritySchemes[BearerSchemeName] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = BearerSchemeName.ToLowerInvariant(),
                BearerFormat = BearerFormat,
                In = ParameterLocation.Header,
            };
            document.Security ??= new List<OpenApiSecurityRequirement>();
            document.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(BearerSchemeName, document)] = new List<string>(),
            });
            return Task.CompletedTask;
        }));
}
