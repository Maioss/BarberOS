using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;

namespace BarberOS.Api.IntegrationTests;

[Collection(ApiCollection.Name)]
public class SessionTests(ApiFixture api)
{
    private sealed record Session(string Token, string RefreshToken, ApiFixture.UserData User);

    private async Task<Session> Register()
    {
        var response = await api.FromNewIp().PostAsJsonAsync("/api/auth/register", new
        {
            email = $"sesion-{Guid.NewGuid():N}@barberos.com",
            password = "Passw0rd123",
            fullName = "Cliente Sesion",
            phone = (string?)null
        });

        return await response.ReadData<Session>();
    }

    private HttpClient WithToken(string accessToken)
    {
        var client = api.FromNewIp();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    private Task<HttpResponseMessage> Refresh(string refreshToken) =>
        api.FromNewIp().PostAsJsonAsync("/api/auth/refresh", new { refreshToken });

    [Fact]
    public async Task El_registro_entrega_los_dos_tokens()
    {
        var session = await Register();

        Assert.False(string.IsNullOrWhiteSpace(session.Token));
        Assert.False(string.IsNullOrWhiteSpace(session.RefreshToken));
    }

    [Fact]
    public async Task El_refresh_entrega_una_sesion_utilizable()
    {
        var session = await Register();

        var renewed = await (await Refresh(session.RefreshToken)).ReadData<Session>();

        Assert.NotEqual(session.RefreshToken, renewed.RefreshToken);
        (await WithToken(renewed.Token).GetAsync("/api/users/me")).EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task El_token_de_refresh_no_sirve_dos_veces()
    {
        var session = await Register();
        (await Refresh(session.RefreshToken)).EnsureSuccessStatusCode();

        var reused = await Refresh(session.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);
    }

    [Fact]
    public async Task Reusar_un_token_rotado_cierra_todas_las_sesiones()
    {
        var session = await Register();
        var renewed = await (await Refresh(session.RefreshToken)).ReadData<Session>();

        var reused = await Refresh(session.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);

        // El token vigente tambien cae: si hay una copia circulando, no sabemos cual es la buena.
        var afterTheft = await Refresh(renewed.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, afterTheft.StatusCode);
    }

    [Fact]
    public async Task El_logout_invalida_el_token_de_refresh()
    {
        var session = await Register();

        var logout = await api.FromNewIp().PostAsJsonAsync("/api/auth/logout",
            new { refreshToken = session.RefreshToken });
        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await Refresh(session.RefreshToken)).StatusCode);
    }

    [Fact]
    public async Task El_logout_de_una_sesion_ya_cerrada_no_falla()
    {
        var session = await Register();
        await api.FromNewIp().PostAsJsonAsync("/api/auth/logout", new { refreshToken = session.RefreshToken });

        var again = await api.FromNewIp().PostAsJsonAsync("/api/auth/logout",
            new { refreshToken = session.RefreshToken });

        Assert.Equal(HttpStatusCode.NoContent, again.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("no-es-un-token")]
    public async Task Un_token_de_refresh_invalido_no_abre_sesion(string token)
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Refresh(token)).StatusCode);
    }

    [Fact]
    public async Task El_token_de_refresh_se_guarda_hasheado()
    {
        var session = await Register();

        using var scope = api.Services.CreateScope();
        var db = scope.ServiceProvider
            .GetRequiredService<BarberOS.Infrastructure.Persistence.BarberOSDbContext>();
        var stored = db.RefreshTokens.Select(t => t.TokenHash).ToList();

        Assert.DoesNotContain(session.RefreshToken, stored);
    }
}
