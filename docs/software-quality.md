# Software Quality Evidence (Final Exam Project)

This document summarizes how software quality is demonstrated in this repository through testing, quality gates, static analysis, and secure coding practices. The goal is to provide clear, verifiable evidence that can be inspected quickly by an examiner.

---

## 1. Quality goals and approach

The project applies a pragmatic quality strategy:

- **Fast feedback in CI:** build, tests, formatting verification, and a coverage threshold gate.
- **Layered tests:** unit tests for domain rules and service logic, plus lightweight API integration tests.
- **Static analysis:** CodeQL is enabled to detect common vulnerability patterns and insecure flows.
- **Security mindset:** input validation, safe logging, and explicit handling of downstream dependency failure.

---

## 2. Test strategy

### 2.1 Unit tests
Unit tests focus on deterministic logic and edge cases:

- **Ordering domain pricing rules** (`OrderPricingRulesTests`)
  - Delivery fee boundary around the subtotal threshold
  - Bulk discount boundary around the total quantity threshold
  - Rounding rules for money values

- **Ordering service logic** (`OrderServiceTests`)
  - Validation: missing restaurantId, empty items, invalid quantity
  - Integration boundary behavior: unknown restaurant rejected
  - Dependency resilience: legacy service unavailable → mapped response (503)

- **Payment consumer logic** (`OrderPlacedConsumerTests`)
  - Demo rule: totals > 500 publish `PaymentFailedEvent`
  - Totals ≤ 500 do not publish failure

- **Legacy menu service logic** (`MenuServiceTests`)
  - Restaurant existence true/false
  - Menu retrieval: items returned / empty menu
  - Menu item lookup: missing returns null

### 2.2 Integration tests (in-process API)
Both APIs include lightweight integration tests using `WebApplicationFactory` to validate the API surface without requiring external infrastructure:

- **Ordering API** (`OrderingApiTests`)
  - `POST /api/orders` returns **202 Accepted** for a valid payload
  - The legacy dependency is replaced with a deterministic fake (`ILegacyMenuClient`) for stable test execution

- **Legacy Menu API** (`LegacyMenuApiTests`)
  - `GET /api/legacy/menu/{restaurantId}` returns **200 OK** and items when seeded data exists
  - `GET /api/legacy/restaurants/{restaurantId}/exists` returns **200 OK** with `exists=true`

This provides confidence that dependency injection, routing, and request/response shapes work as intended.

---

## 3. CI quality gates

The CI pipeline runs on every push to `main` and on pull requests:

- **Restore + Build** (`Release`)
- **Format verification**
  - `dotnet format mtogo-final-exam.sln --verify-no-changes`
- **Test execution + coverage gate**
  - CI runs coverage gates per test project (see `.github/workflows/ci.yml`).
  - Coverage threshold is enforced (Ordering **65% line**, LegacyMenu **65% line**, Payment **65% line**).
  - Note: the gate uses **total line coverage** (`ThresholdStat=total`) so shared libraries (like `Mtogo.Contracts`) do not fail the gate on their own.

Coverage reports (`coverage.cobertura.xml`) are uploaded as workflow artifacts so an examiner can inspect them from GitHub Actions.

---

## 4. Coverage evidence (Docker-run, CI-equivalent)

To ensure reproducibility (and to avoid Windows policies that can block running locally), the repo can run tests + coverage in the official .NET SDK container. This matches CI more closely.

**Commands (PowerShell):**
```powershell
docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 `
  dotnet test tests/ordering.tests/Mtogo.Ordering.Tests/Mtogo.Ordering.Tests.csproj -c Release `
    /p:CollectCoverage=true /p:CoverletOutput=TestResults/ordering- /p:Threshold=65 /p:ThresholdType=line /p:ThresholdStat=total

docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 `
  dotnet test tests/legacy-menu.tests/Mtogo.LegacyMenu.Tests/Mtogo.LegacyMenu.Tests.csproj -c Release `
    /p:CollectCoverage=true /p:CoverletOutput=TestResults/legacy- /p:Threshold=65 /p:ThresholdType=line /p:ThresholdStat=total

docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 `
  dotnet test tests/payment.tests/Mtogo.Payment.Tests/Mtogo.Payment.Tests.csproj -c Release `
    /p:CollectCoverage=true /p:CoverletOutput=TestResults/payment- /p:Threshold=65 /p:ThresholdType=line /p:ThresholdStat=total
````

**Evidence (latest run output):**

```text
Passed!  - Failed:     0, Passed:    18, Skipped:     0, Total:    18, Duration: 646 ms - Mtogo.Ordering.Tests.dll (net10.0)

+--------------------+--------+--------+--------+
| Module             | Line   | Branch | Method |
+--------------------+--------+--------+--------+
| Mtogo.Ordering.Api | 82.05% | 100%   | 75%    |
+--------------------+--------+--------+--------+

Passed!  - Failed:     0, Passed:    13, Skipped:     0, Total:    13, Duration: 691 ms - Mtogo.LegacyMenu.Tests.dll (net10.0)

+----------------------+--------+--------+--------+
| Module               | Line   | Branch | Method |
+----------------------+--------+--------+--------+
| Mtogo.LegacyMenu.Api | 100%   | 100%   | 100%   |
+----------------------+--------+--------+--------+

Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: 1 s - Mtogo.Payment.Tests.dll (net10.0)

+-------------------+------+--------+--------+
| Module            | Line | Branch | Method |
+-------------------+------+--------+--------+
| Mtogo.Payment.Api | 100% | 100%   | 100%   |
+-------------------+------+--------+--------+
```

---

## 5. Static analysis (CodeQL)

CodeQL is enabled as a repository security scan. It provides static analysis to detect issues such as:

* unsafe data flows
* injection risks (where applicable)
* insecure API patterns
* other common vulnerability classes supported by the CodeQL ruleset

The intent is to demonstrate professional secure development practices (SAST) alongside testing.

---

## 6. Taint analysis demonstration (explicit)

The exam brief requires demonstrating taint analysis. In this project, the Ordering API provides a clear example of tainted data flow across a trust boundary.

### 6.1 Taint sources (untrusted input)

In `Mtogo.Ordering.Api`, the following fields originate from the external client (untrusted):

* `CreateOrderRequest.RestaurantId`
* `CreateOrderRequest.Items[].MenuItemId`
* `CreateOrderRequest.Items[].Quantity`

These values enter the system via the HTTP endpoint:

* `POST /api/orders`

### 6.2 Taint sinks (sensitive operations)

Untrusted values should not flow unchecked into sinks. The main sink in this system is the downstream HTTP call into a legacy dependency:

* `ILegacyMenuClient.RestaurantExistsAsync(restaurantId, ct)`

  * This is an outbound call and a trust boundary crossing.

Secondary sinks include:

* **logging** (risk: sensitive data leakage if full payloads are logged)
* **database operations** (inside the legacy service boundary)

### 6.3 Mitigations (validation + safe logging + failure handling)

The Ordering service applies the following mitigations before using tainted values:

* **Input validation** in `OrderService.CreateOrderAsync`

  * `restaurantId` must not be empty
  * `items` must not be empty
  * `quantity` must be > 0
  * menu item prices must be known (via `IMenuItemPriceProvider`)

* **Safe logging**

  * Logs contain only identifiers (orderId, restaurantId), not full request bodies.

* **Dependency failure handling**

  * If the legacy menu service is unavailable (e.g., `HttpRequestException`, timeouts), Ordering maps this to a controlled response (**503 Service Unavailable**) instead of leaking internals or returning an unhandled 500.

### 6.4 Test evidence for mitigations

The test suite contains explicit examples that correspond to the mitigations above:

* Validation rejects tainted inputs:

  * missing `restaurantId` → 400
  * empty items → 400
  * invalid quantity → 400

* Dependency failure:

  * legacy client throws → Ordering returns 503

This provides a concrete, test-backed demonstration of a taint boundary: untrusted input is validated and controlled before reaching sensitive sinks.

---

## 7. How to run quality checks locally

### 7.1 Run tests (local)

```bash
dotnet test mtogo-final-exam.sln
```

### 7.2 Run coverage using the same SDK container as CI

```powershell
docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 `
  dotnet test mtogo-final-exam.sln -c Release `
    /p:CollectCoverage=true `
    /p:CoverletOutput=TestResults/ `
    /p:CoverletOutputFormat="cobertura,json" `
    /p:ExcludeByAttribute="Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute" `
    /p:ExcludeByFile="**/Migrations/*.cs,**/Program.cs,**/OpenApi*.cs,**/obj/**,**/*.g.cs,**/*.g.i.cs,**/*AssemblyInfo*.cs"
```

Tip: for the exact CI gates (thresholds), use the commands in section 4.

### 7.3 Check formatting locally

```bash
dotnet format mtogo-final-exam.sln
```

---

## 8. Notes on coverage scope
Coverage is intended to measure meaningful code paths (domain rules and service logic). Boilerplate, hosting setup, and infrastructure-heavy code may be excluded from coverage calculations to reduce noise and keep the metric aligned with the learning objective: verifying business logic and integration seams.

