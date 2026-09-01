using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace BarberOS.Api.IntegrationTests;

/// <summary>
/// Postgres real, no un proveedor en memoria: media de lo que se prueba lo impone la base.
/// </summary>
public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        StartApi();
    }

    private void StartApi() => _ = Server;

    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        await _postgres.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Secret", "secreto-de-pruebas-con-mas-de-treinta-y-dos-bytes-1234567890");
    }

    public HttpClient Anonymous() => CreateClient();

    public async Task<HttpClient> As(string email, string password = "Pitch2026!")
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<Envelope<LoginData>>(Json);
        var token = body?.Data?.Token
            ?? throw new InvalidOperationException($"No se pudo autenticar a {email}.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public Task<HttpClient> AsSuperAdmin() => As("samin@barberos.com");
    public Task<HttpClient> AsAdmin() => As("admin.pitch@barberos.com");
    public Task<HttpClient> AsClient() => As("cliente.pitch@barberos.com");
    public Task<HttpClient> AsOtherClient() => As("cliente2@barberos.com");
    public Task<HttpClient> AsBarber() => As("barbero.pitch@barberos.com");

    public sealed record Envelope<T>(bool Success, T? Data, string? Message, List<string>? Errors);
    public sealed record LoginData(string Token, UserData User);
    public sealed record UserData(Guid Id, string Email, string FullName, string Role, Guid? BarbershopId, string? PhotoUrl);
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "api";
}
