using System.Collections.Concurrent;
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
    public const string Password = "Pitch2026!";
    public const string SuperAdminEmail = "samin@barberos.com";
    public const string AdminEmail = "admin.pitch@barberos.com";
    public const string ClientEmail = "cliente.pitch@barberos.com";
    public const string OtherClientEmail = "cliente2@barberos.com";
    public const string BarberEmail = "barbero.pitch@barberos.com";

    private static readonly string[] SeededUsers =
        [SuperAdminEmail, AdminEmail, ClientEmail, OtherClientEmail, BarberEmail];

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16-alpine").Build();

    private readonly ConcurrentDictionary<string, string> _tokens = new();

    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        StartApi();

        // Todos los inicios de sesion ocurren aqui: el limitador de /api/auth/login
        // cuenta por IP y los tests comparten la del servidor de pruebas.
        foreach (var email in SeededUsers)
            _tokens[email] = await RequestToken(email, Password);
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
        builder.UseSetting("Database:ApplyMigrationsOnStartup", "true");
        builder.UseSetting("Database:SeedDemoData", "true");
    }

    public HttpClient Anonymous() => CreateClient();

    private static int _clientCounter;

    /// <summary>
    /// El limitador de /api/auth particiona por IP y el servidor de pruebas no tiene una:
    /// sin esto los tests se ahogan entre ellos.
    /// </summary>
    public HttpClient FromNewIp()
    {
        var n = Interlocked.Increment(ref _clientCounter);
        var client = CreateClient();
        client.DefaultRequestHeaders.Add("X-Forwarded-For", $"10.{n / 65536 % 256}.{n / 256 % 256}.{n % 256}");
        return client;
    }

    public HttpClient As(string email)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokens[email]);
        return client;
    }

    public HttpClient AsSuperAdmin() => As(SuperAdminEmail);
    public HttpClient AsAdmin() => As(AdminEmail);
    public HttpClient AsClient() => As(ClientEmail);
    public HttpClient AsOtherClient() => As(OtherClientEmail);
    public HttpClient AsBarber() => As(BarberEmail);

    public async Task<string> RequestToken(string email, string password)
    {
        using var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<Envelope<LoginData>>(Json);
        return body?.Data?.Token
            ?? throw new InvalidOperationException($"No se pudo autenticar a {email}.");
    }

    public sealed record Envelope<T>(bool Success, T? Data, string? Message, List<string>? Errors);
    public sealed record LoginData(string Token, UserData User);
    public sealed record UserData(Guid Id, string Email, string FullName, string Role, Guid? BarbershopId, string? PhotoUrl);
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "api";
}
