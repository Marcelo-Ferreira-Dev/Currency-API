using System.Text.Json;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Extensions;

public class SwaggerExamples : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.RequestBody?.Content is not { } contentTypes
            || !contentTypes.TryGetValue("application/json", out var content)
            || content is null)
        {
            return;
        }

        var path = context.ApiDescription.RelativePath?.Replace(":int", "").Trim('/');
        var examples = (context.ApiDescription.HttpMethod, path) switch
        {
            ("POST", "users") => Examples(("Crear usuario", new
            {
                name = "Marcelo Ferreira", email = "marcelod.ferreira.dev@gmail.com", password = "P@sswordsegura"
            })),
            ("PUT", "users/{id}") => Examples(("Actualizar usuario", new
            {
                name = "Marcelo Daniel Ferreira", email = "marcelod.ferreira.dev@gmail.com", isActive = true
            })),
            ("POST", "users/{userId}/addresses") => Examples(("Crear dirección", new
            {
                street = "Av. Mariscal López 1234", city = "Asunción", country = "Paraguay", zipCode = "1209"
            })),
            ("PUT", "addresses/{id}") => Examples(("Actualizar dirección", new
            {
                street = "Av. España 456", city = "Asunción", country = "Paraguay", zipCode = "1209", isActive = true
            })),
            ("POST", "currencies") => Examples(
                ("Crear PYG", new { code = "PYG", name = "Guaraní", rateToBase = 1 }),
                ("Crear USD", new { code = "USD", name = "Dólar estadounidense", rateToBase = 6000 })),
            ("POST", "currency/convert") => Examples(
                ("USD a PYG", new { fromCurrencyCode = "USD", toCurrencyCode = "PYG", amount = 100 }),
                ("PYG a USD", new { fromCurrencyCode = "PYG", toCurrencyCode = "USD", amount = 600000 })),
            _ => null
        };

        if (examples is not null)
        {
            content.Examples = examples;
        }
    }

    private static Dictionary<string, IOpenApiExample> Examples(params (string Name, object Value)[] examples) =>
        examples.ToDictionary(example => example.Name, example => (IOpenApiExample)new OpenApiExample
        {
            Summary = example.Name,
            Value = JsonSerializer.SerializeToNode(example.Value)
        });
}
