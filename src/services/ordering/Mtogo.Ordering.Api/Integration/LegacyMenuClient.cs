using System.Net;

namespace Mtogo.Ordering.Api.Integration;

public sealed class LegacyMenuClient : ILegacyMenuClient
{
    private readonly HttpClient _http;

    public LegacyMenuClient(HttpClient http) => _http = http;

    public async Task<bool> RestaurantExistsAsync(Guid restaurantId, CancellationToken ct)
    {
        var resp = await _http.GetAsync($"/api/legacy/menu/{restaurantId}", ct);

        if (resp.StatusCode == HttpStatusCode.NotFound) return false;
        if (!resp.IsSuccessStatusCode) throw new HttpRequestException("Legacy menu unavailable", null, resp.StatusCode);

        return true;
    }
}
