# System Integration (SI) — Ordering ↔ LegacyMenu (+ async Payment)

This slice is intentionally small, but it demonstrates two realistic integration types:

- **Sync HTTP**: Ordering calls LegacyMenu to validate restaurant/menu references.
- **Async messaging (RabbitMQ)**: Ordering publishes an `OrderPlacedEvent`, and Payment consumes it.

## 1) Runtime services involved

- **Gateway (YARP)** routes requests to services: `src/gateway/Mtogo.Gateway.Yarp/`
- **Ordering API**: `src/services/ordering/Mtogo.Ordering.Api/`
- **Legacy Menu API**: `src/legacy-menu/Mtogo.LegacyMenu.Api/`
- **Payment API**: `src/services/payment/Mtogo.Payment.Api/`
- **RabbitMQ** used for async messaging (OrderPlaced → Payment consumer)

## 2) Sync HTTP integration (Ordering → LegacyMenu)

When a client places an order:

1. Client calls **Gateway** `/ordering/api/orders`
2. Gateway forwards to **Ordering**
3. Ordering calls LegacyMenu via `ILegacyMenuClient.RestaurantExistsAsync(...)`
4. Ordering returns:
   - **202 Accepted** when valid
   - **400** for validation errors
   - **503** if LegacyMenu is down/slow (controlled dependency failure)

## 3) Async integration (Ordering → RabbitMQ → Payment)

After a valid order is created, Ordering publishes:

- `OrderPlacedEvent` (in `src/services/shared/Mtogo.Contracts/`)

Payment consumes `OrderPlacedEvent` and, for the demo, publishes:

- `PaymentFailedEvent` when `TotalPrice > 500`

This makes it easy to demonstrate “something happens after the order” without blocking the HTTP request.

## 4) Test evidence (no external infra)

To keep tests deterministic:

- Ordering API integration tests replace the legacy dependency with a fake `ILegacyMenuClient`.
- Ordering tests run in the `Testing` environment so they do not require RabbitMQ.
- Payment tests are unit tests of the consumer logic (`OrderPlacedConsumerTests`) and do not require RabbitMQ.

Relevant projects:
- `tests/ordering.tests/Mtogo.Ordering.Tests/`
- `tests/payment.tests/Mtogo.Payment.Tests/`