using System.Net.Http.Json;

namespace BarberOS.Api.IntegrationTests;

internal static class HttpExtensions
{
    public static async Task<T> ReadData<T>(this HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<ApiFixture.Envelope<T>>(ApiFixture.Json);

        Assert.NotNull(body);
        Assert.True(body!.Success, body.Message);
        Assert.NotNull(body.Data);
        return body.Data!;
    }

    public static async Task<string> ReadMessage(this HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<ApiFixture.Envelope<object>>(ApiFixture.Json);
        return body?.Message ?? string.Empty;
    }

    public static async Task<T> GetData<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.ReadData<T>();
    }
}
