namespace Mtogo.Ordering.Api.Integration;

public interface ILegacyMenuClient
{
  Task<bool> RestaurantExistsAsync(Guid restaurantId, CancellationToken ct);
}
