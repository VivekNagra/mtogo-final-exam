using System.Net;
using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Tests;

public sealed class LegacyMenuClientTests
{
    [Fact]
    public async Task RestaurantExists_ReturnsFalse_WhenNotFound()
    {
        var client = CreateClient(HttpStatusCode.NotFound);
        var svc = new LegacyMenuClient(client);

        var exists = await svc.RestaurantExistsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.False(exists);
    }

    [Fact]
    public async Task RestaurantExists_ReturnsTrue_WhenSuccess()
    {
        var client = CreateClient(HttpStatusCode.OK);
        var svc = new LegacyMenuClient(client);

        var exists = await svc.RestaurantExistsAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.True(exists);
    }

    [Fact]
    public async Task RestaurantExists_Throws_WhenLegacyUnavailable()
    {
        var client = CreateClient(HttpStatusCode.InternalServerError);
        var svc = new LegacyMenuClient(client);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            svc.RestaurantExistsAsync(Guid.NewGuid(), CancellationToken.None));
    }

    private static HttpClient CreateClient(HttpStatusCode statusCode)
    {
        var handler = new StubHandler(statusCode);
        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;

        public StubHandler(HttpStatusCode statusCode) => _statusCode = statusCode;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(_statusCode));
    }
}
