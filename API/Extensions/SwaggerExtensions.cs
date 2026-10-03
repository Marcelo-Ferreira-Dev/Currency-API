using Microsoft.OpenApi;

namespace API.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.OperationFilter<SwaggerExamples>();
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Prueba Técnica API",
                Version = "v1"
            });
            options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Name = "X-API-KEY",
                Description = "Clave de prueba: API-KEY-prueba"
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
            });
        });
        return services;
    }
}
