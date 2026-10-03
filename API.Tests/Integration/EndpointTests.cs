using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using API.Contracts;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace API.Tests.Integration;

public class EndpointTests
{
    [Theory]
    [InlineData(true, "omitido")]
    [InlineData(false, "omitido")]
    [InlineData(true, "null")]
    [InlineData(false, "null")]
    [InlineData(true, "true")]
    [InlineData(false, "true")]
    [InlineData(true, "false")]
    [InlineData(false, "false")]
    public async Task Updates_change_active_state_only_when_boolean_is_provided(bool originalState, string input)
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        var user = await CreateUser(client);
        using var created = await client.PostAsJsonAsync($"/users/{user.Id}/addresses", new
        {
            street = "Av. Mariscal López", city = "Asunción", country = "Paraguay"
        });
        created.EnsureSuccessStatusCode();
        var address = (await created.Content.ReadFromJsonAsync<AddressResponse>())!;
        if (!originalState)
        {
            using var userDelete = await client.DeleteAsync($"/users/{user.Id}");
            using var addressDelete = await client.DeleteAsync($"/addresses/{address.Id}");
            userDelete.EnsureSuccessStatusCode();
            addressDelete.EnsureSuccessStatusCode();
        }

        var userBody = new Dictionary<string, object?>
        {
            ["name"] = "Marcelo Daniel Ferreira", ["email"] = user.Email
        };
        var addressBody = new Dictionary<string, object?>
        {
            ["street"] = "Av. España 456", ["city"] = "Asunción", ["country"] = "Paraguay"
        };
        if (input != "omitido")
        {
            object? value = input == "null" ? null : bool.Parse(input);
            userBody["isActive"] = value;
            addressBody["isActive"] = value;
        }
        var expected = input is "omitido" or "null" ? originalState : bool.Parse(input);
        using var userUpdate = await client.PutAsJsonAsync($"/users/{user.Id}", userBody);
        using var addressUpdate = await client.PutAsJsonAsync($"/addresses/{address.Id}", addressBody);
        userUpdate.EnsureSuccessStatusCode();
        addressUpdate.EnsureSuccessStatusCode();
        var updatedUser = (await userUpdate.Content.ReadFromJsonAsync<UserResponse>())!;
        var updatedAddress = (await addressUpdate.Content.ReadFromJsonAsync<AddressResponse>())!;
        Assert.Equal(expected, updatedUser.IsActive);
        Assert.Equal(expected, updatedAddress.IsActive);
        Assert.Equal("Marcelo Daniel Ferreira", updatedUser.Name);
        Assert.Equal("Av. España 456", updatedAddress.Street);
        Assert.Equal(expected, (await client.GetFromJsonAsync<UserResponse>($"/users/{user.Id}"))!.IsActive);
        Assert.Equal(expected, Assert.Single((await client.GetFromJsonAsync<List<AddressResponse>>($"/users/{user.Id}/addresses"))!).IsActive);
    }

    [Fact]
    public async Task Swagger_request_examples_match_postman()
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        response.EnsureSuccessStatusCode();
        using var swagger = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = factory.Services.GetRequiredService<IWebHostEnvironment>().ContentRootPath;
        using var postman = JsonDocument.Parse(await File.ReadAllTextAsync(
            Path.Combine(root, "..", "postman", "API.postman_collection.json")));

        foreach (var folder in postman.RootElement.GetProperty("item").EnumerateArray())
        {
            foreach (var item in folder.GetProperty("item").EnumerateArray())
            {
                var request = item.GetProperty("request");
                if (!request.TryGetProperty("body", out var body))
                {
                    continue;
                }
                var path = request.GetProperty("url").GetString()!
                    .Replace("{{baseUrl}}", "")
                    .Replace("{{userId}}", "{userId}")
                    .Replace("{{addressId}}", "{id}");
                // The users update path uses id, while nested addresses use userId.
                if (path == "/users/{userId}")
                {
                    path = "/users/{id}";
                }
                var method = request.GetProperty("method").GetString()!.ToLowerInvariant();
                var examples = swagger.RootElement.GetProperty("paths").GetProperty(path)
                    .GetProperty(method).GetProperty("requestBody").GetProperty("content")
                    .GetProperty("application/json").GetProperty("examples");
                var expected = JsonNode.Parse(body.GetProperty("raw").GetString()!);
                Assert.Contains(examples.EnumerateObject(), example =>
                    JsonNode.DeepEquals(expected, JsonNode.Parse(example.Value.GetProperty("value").GetRawText())));
            }
        }
    }

    private static object UserBody(string email = "marcelod.ferreira.dev@gmail.com") => new
    {
        name = "Marcelo Ferreira", email, password = "P@sswordsegura"
    };

    private static async Task<UserResponse> CreateUser(HttpClient client)
    {
        using var response = await client.PostAsJsonAsync("/users", UserBody());
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<UserResponse>())!;
    }

    [Theory]
    [InlineData("GET", "/users")]
    [InlineData("POST", "/users")]
    [InlineData("GET", "/users/1")]
    [InlineData("PUT", "/users/1")]
    [InlineData("DELETE", "/users/1")]
    [InlineData("POST", "/users/1/addresses")]
    [InlineData("GET", "/users/1/addresses")]
    [InlineData("PUT", "/addresses/1")]
    [InlineData("DELETE", "/addresses/1")]
    [InlineData("POST", "/currencies")]
    [InlineData("GET", "/currencies")]
    [InlineData("POST", "/currency/convert")]
    public async Task Every_endpoint_requires_valid_api_key(string method, string path)
    {
        using var factory = new ApiFactory();
        using var client = factory.Client(authenticated: false);
        foreach (var key in new string?[] { null, "incorrecta" })
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), path);
            if (key is not null)
            {
                request.Headers.Add("X-API-KEY", key);
            }
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }

    [Fact]
    public async Task Users_crud_preserves_hash_and_soft_deleted_record()
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        using var created = await client.PostAsJsonAsync("/users", UserBody());
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var user = (await created.Content.ReadFromJsonAsync<UserResponse>())!;
        Assert.Equal($"/users/{user.Id}", created.Headers.Location!.ToString());
        using var json = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        Assert.False(json.RootElement.TryGetProperty("password", out _));

        using (var scope = factory.Services.CreateScope())
        {
            var stored = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users.SingleAsync();
            Assert.True(BCrypt.Net.BCrypt.Verify("P@sswordsegura", stored.Password));
        }

        Assert.Equal(user, await client.GetFromJsonAsync<UserResponse>($"/users/{user.Id}"));
        using var duplicate = await client.PostAsJsonAsync("/users", UserBody());
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        using var update = await client.PutAsJsonAsync($"/users/{user.Id}", new
        {
            id = 99999, name = "Marcelo Daniel Ferreira", email = user.Email, isActive = true
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Marcelo Daniel Ferreira", (await update.Content.ReadFromJsonAsync<UserResponse>())!.Name);
        using var delete = await client.DeleteAsync($"/users/{user.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty(await client.GetFromJsonAsync<List<UserResponse>>("/users?isActive=true") ?? []);
        Assert.Single((await client.GetFromJsonAsync<List<UserResponse>>("/users?isActive=false"))!);
        Assert.Single((await client.GetFromJsonAsync<List<UserResponse>>("/users"))!);
        Assert.False((await client.GetFromJsonAsync<UserResponse>($"/users/{user.Id}"))!.IsActive);
        using var repeated = await client.DeleteAsync($"/users/{user.Id}");
        Assert.Equal(HttpStatusCode.NoContent, repeated.StatusCode);
    }

    [Fact]
    public async Task Users_reject_invalid_requests_and_missing_records()
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        using var invalid = await client.PostAsJsonAsync("/users", new { name = " ", email = "invalido", password = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var missing = await client.GetAsync("/users/99999");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var update = await client.PutAsJsonAsync("/users/99999", new { name = "Marcelo", email = "marcelod.ferreira.dev@gmail.com", isActive = true });
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        using var delete = await client.DeleteAsync("/users/99999");
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Addresses_crud_preserves_owner_and_supports_reactivation()
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        var user = await CreateUser(client);
        var route = $"/users/{user.Id}/addresses";
        Assert.Empty((await client.GetFromJsonAsync<List<AddressResponse>>(route))!);
        using var created = await client.PostAsJsonAsync(route, new
        {
            userId = 99999, street = "Av. Mariscal López 1234", city = "Asunción", country = "Paraguay"
        });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var address = (await created.Content.ReadFromJsonAsync<AddressResponse>())!;
        Assert.Equal(user.Id, address.UserId);
        Assert.Null(address.ZipCode);
        var target = $"/addresses/{address.Id}";
        using var delete = await client.DeleteAsync(target);
        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<List<AddressResponse>>(route + "?isActive=true"))!);
        Assert.Single((await client.GetFromJsonAsync<List<AddressResponse>>(route + "?isActive=false"))!);
        using var update = await client.PutAsJsonAsync(target, new
        {
            id = 99999, userId = 99999, street = "Av. España 456", city = "Asunción", country = "Paraguay", isActive = true
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var updated = (await update.Content.ReadFromJsonAsync<AddressResponse>())!;
        Assert.Equal(user.Id, updated.UserId);
        Assert.Equal(address.Id, updated.Id);
        Assert.True(updated.IsActive);
        using var userDelete = await client.DeleteAsync($"/users/{user.Id}");
        Assert.Equal(HttpStatusCode.NoContent, userDelete.StatusCode);
        Assert.Single((await client.GetFromJsonAsync<List<AddressResponse>>(route))!);
    }

    [Fact]
    public async Task Addresses_reject_invalid_data_and_missing_resources()
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        var user = await CreateUser(client);
        var valid = new { street = "Av. España", city = "Asunción", country = "Paraguay" };
        using var invalid = await client.PostAsJsonAsync($"/users/{user.Id}/addresses", new { street = "", city = "", country = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var create = await client.PostAsJsonAsync("/users/99999/addresses", valid);
        Assert.Equal(HttpStatusCode.NotFound, create.StatusCode);
        using var list = await client.GetAsync("/users/99999/addresses");
        Assert.Equal(HttpStatusCode.NotFound, list.StatusCode);
        using var update = await client.PutAsJsonAsync("/addresses/99999", valid);
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        using var delete = await client.DeleteAsync("/addresses/99999");
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task Currencies_and_conversion_validate_rates_codes_and_state()
    {
        using var factory = new ApiFactory();
        using var client = factory.Client();
        using var pyg = await client.PostAsJsonAsync("/currencies", new { code = "PYG", name = "Guaraní", rateToBase = 1 });
        using var usd = await client.PostAsJsonAsync("/currencies", new { code = " usd ", name = "Dólar", rateToBase = 6000 });
        Assert.Equal(HttpStatusCode.Created, pyg.StatusCode);
        Assert.Equal(HttpStatusCode.Created, usd.StatusCode);
        using var duplicate = await client.PostAsJsonAsync("/currencies", new { code = "pyg", name = "Guaraní", rateToBase = 1 });
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        using var invalid = await client.PostAsJsonAsync("/currencies", new { code = "EUR", name = "Euro", rateToBase = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        using var converted = await client.PostAsJsonAsync("/currency/convert", new { fromCurrencyCode = "usd", toCurrencyCode = "PYG", amount = 100 });
        Assert.Equal(HttpStatusCode.OK, converted.StatusCode);
        Assert.Equal(new ConversionResponse("USD", "PYG", 100m, 600000m), await converted.Content.ReadFromJsonAsync<ConversionResponse>());
        using var zero = await client.PostAsJsonAsync("/currency/convert", new { fromCurrencyCode = "USD", toCurrencyCode = "PYG", amount = 0 });
        Assert.Equal(HttpStatusCode.BadRequest, zero.StatusCode);
        using var missing = await client.PostAsJsonAsync("/currency/convert", new { fromCurrencyCode = "USD", toCurrencyCode = "EUR", amount = 100 });
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var eur = await client.PostAsJsonAsync("/currencies", new { code = "EUR", name = "Euro", rateToBase = 7000, isActive = false });
        Assert.Equal(HttpStatusCode.Created, eur.StatusCode);
        using var inactive = await client.PostAsJsonAsync("/currency/convert", new { fromCurrencyCode = "USD", toCurrencyCode = "EUR", amount = 100 });
        Assert.Equal(HttpStatusCode.Conflict, inactive.StatusCode);
        Assert.Equal(3, (await client.GetFromJsonAsync<List<CurrencyResponse>>("/currencies"))!.Count);
        Assert.Equal(2, (await client.GetFromJsonAsync<List<CurrencyResponse>>("/currencies?isActive=true"))!.Count);
        Assert.Single((await client.GetFromJsonAsync<List<CurrencyResponse>>("/currencies?isActive=false"))!);
    }

    [Theory]
    [InlineData("Development", HttpStatusCode.OK)]
    [InlineData("Production", HttpStatusCode.NotFound)]
    public async Task Swagger_is_only_available_in_development(string environment, HttpStatusCode expected)
    {
        using var factory = new ApiFactory(environment);
        using var client = factory.Client();
        if (environment == "Development")
        {
            client.DefaultRequestHeaders.Remove("X-API-KEY");
        }
        using var page = await client.GetAsync("/swagger/index.html");
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(expected, page.StatusCode);
        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.OK)
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("X-API-KEY", document.RootElement.GetProperty("components").GetProperty("securitySchemes").GetProperty("ApiKey").GetProperty("name").GetString());
            Assert.Equal(12, document.RootElement.GetProperty("paths").EnumerateObject().Sum(path => path.Value.EnumerateObject().Count()));
        }
    }

}
