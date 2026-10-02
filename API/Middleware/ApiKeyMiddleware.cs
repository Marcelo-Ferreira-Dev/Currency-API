namespace API.Middleware;

public class ApiKeyMiddleware
{
    private const string HeaderName = "X-API-KEY";
    private readonly RequestDelegate next;
    private readonly string apiKey;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        this.next = next;
        apiKey = configuration["ApiKey"]
            ?? throw new InvalidOperationException("No se configuró ApiKey.");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("ApiKey no puede estar vacía.");
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var providedKey)
            || providedKey.Count != 1
            || !string.Equals(providedKey[0], apiKey, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "API Key ausente o incorrecta." });
            return;
        }

        await next(context);
    }
}
