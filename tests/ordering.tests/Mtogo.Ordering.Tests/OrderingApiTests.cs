using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Mtogo.Ordering.Api.Integration;

namespace Mtogo.Ordering.Tests;

public sealed class OrderingApiTests : IClassFixture<WebApplicationFactory<Mtogo.Ordering.Api.Program>>
{
  private readonly WebApplicationFactory<Mtogo.Ordering.Api.Program> _factory;

  public OrderingApiTests(WebApplicationFactory<Mtogo.Ordering.Api.Program> factory)
  {
    _factory = factory.WithWebHostBuilder(builder =>
    {
      builder.ConfigureServices(services =>
      {
        // Replace legacy client with deterministic fake for integration tests
        services.AddSingleton<ILegacyMenuClient>(new FakeLegacyMenuClient(true));
      });
    });
  }

  [Fact]
  public async Task POST_orders_Returns202_WhenValid()
  {
    var client = _factory.CreateClient();

    var req = new
    {
      restaurantId = Guid.NewGuid(),
      items = new[] { new { menuItemId = Guid.NewGuid(), quantity = 1 } }
    };

    var resp = await client.PostAsJsonAsync("/api/orders", req);
    Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);
  }

  private sealed class FakeLegacyMenuClient : ILegacyMenuClient
  {
    private readonly bool _exists;
    public FakeLegacyMenuClient(bool exists) => _exists = exists;
    public Task<bool> RestaurantExistsAsync(Guid restaurantId, CancellationToken ct) => Task.FromResult(_exists);
  }
}
