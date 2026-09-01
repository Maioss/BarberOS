using System.Net;
using System.Net.Http.Json;

namespace BarberOS.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class OperabilityTests(ApiFixture api)
{
    private sealed record Health(string Status, int DurationMs, Dictionary<string, string> Checks);

    [Fact]
    public async Task El_health_check_consulta_la_base()
    {
        var health = await api.Anonymous().GetData<Health>("/health");

        Assert.Equal("Healthy", health.Status);
        Assert.Equal("Healthy", health.Checks["postgres"]);
    }

    [Fact]
    public async Task El_health_check_no_expone_el_entorno()
    {
        var raw = await (await api.Anonymous().GetAsync("/health")).Content.ReadAsStringAsync();

        Assert.DoesNotContain("nvironment", raw);
        Assert.DoesNotContain("Testing", raw);
    }

    [Fact]
    public async Task El_login_deja_de_aceptar_intentos_tras_pasarse_del_limite()
    {
        var client = api.FromNewIp();
        var attempts = new List<HttpStatusCode>();

        for (var i = 0; i < 15; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email = $"fuerza-bruta-{i}@evil.com", password = "ClaveIncorrecta1" });
            attempts.Add(response.StatusCode);

            if (response.StatusCode == HttpStatusCode.TooManyRequests) break;
        }

        Assert.Contains(HttpStatusCode.TooManyRequests, attempts);
    }

    [Fact]
    public async Task El_rechazo_por_limite_trae_Retry_After_y_el_sobre_de_siempre()
    {
        var client = api.FromNewIp();
        HttpResponseMessage? rejected = null;

        for (var i = 0; i < 15 && rejected is null; i++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { email = $"limite-{i}@evil.com", password = "ClaveIncorrecta1" });

            if (response.StatusCode == HttpStatusCode.TooManyRequests) rejected = response;
        }

        Assert.NotNull(rejected);
        Assert.NotNull(rejected!.Headers.RetryAfter);
        Assert.Contains("Demasiados intentos", await rejected.ReadMessage());
    }
}
