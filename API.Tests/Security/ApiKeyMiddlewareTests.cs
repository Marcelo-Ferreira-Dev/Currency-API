using API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace API.Tests.Security;

public class ApiKeyMiddlewareTests
{
    private static IConfiguration Configuration(string? key) =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ApiKey"] = key
        }).Build();

    [Fact]
    public async Task Valid_key_executes_next_delegate()
    {
        var executed = false;
        var middleware = new ApiKeyMiddleware(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        }, Configuration("test-key"));
        var context = new DefaultHttpContext();
        context.Request.Headers["X-API-KEY"] = "test-key";

        await middleware.InvokeAsync(context);

        Assert.True(executed);
        Assert.Equal(200, context.Response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("incorrecta")]
    [InlineData("TEST-KEY")]
    [InlineData(" test-key ")]
    public async Task Invalid_key_returns_401_without_executing_endpoint(string? key)
    {
        var executed = false;
        var middleware = new ApiKeyMiddleware(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        }, Configuration("test-key"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        if (key is not null)
        {
            context.Request.Headers["X-API-KEY"] = key;
        }

        await middleware.InvokeAsync(context);

        Assert.False(executed);
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task Duplicate_headers_are_rejected_even_if_key_is_valid()
    {
        var executed = false;
        var middleware = new ApiKeyMiddleware(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        }, Configuration("test-key"));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers["X-API-KEY"] = new StringValues(["test-key", "test-key"]);

        await middleware.InvokeAsync(context);

        Assert.Equal(401, context.Response.StatusCode);
        Assert.False(executed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_configuration_is_rejected(string? key)
    {
        Assert.Throws<InvalidOperationException>(() =>
            new ApiKeyMiddleware(_ => Task.CompletedTask, Configuration(key)));
    }
}
