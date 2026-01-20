# MTOGO Final Exam Project (2nd Semester) — Software Development Top-Up

This repository contains my **final exam project** for the **2nd semester (of 3)** in the **Software Development Top-Up** program.

The solution demonstrates outcomes across three course areas:
- **Software Quality** (testing, quality gates, static analysis, coverage)
- **System Integration** (integration between a legacy component and a modern service)
- **Udvikling af Store Systemer** (collaboration setup, CI, automation, lightweight metrics)

The core story is intentionally simple and exam-friendly:

- **Legacy component:** `Mtogo.LegacyMenu.Api` (legacy menu + restaurant data; “legacy boundary”)
- **Modern service:** `Mtogo.Ordering.Api` (new order creation logic with pricing rules)
- **Gateway:** `Mtogo.Gateway.Yarp` (single entry-point routing to services)

> I worked alone. Collaboration mechanics (PR discipline, CODEOWNERS review, etc.) are still demonstrated through GitHub workflows and rules.

---

````md
---

## Examiner Quick Start (verify the system)

### A) Run with Docker Compose
From repo root:

```bash
docker compose -f ./deploy/compose/docker-compose.yml up --build
````

### B) Verify services via the gateway (health)

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/health
Invoke-RestMethod http://localhost:8080/ordering/health
```

Expected: both return `{ status: "ok", service: "..." }`.

### C) Demo: Legacy menu endpoint (via gateway)

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/api/legacy/menu/11111111-1111-1111-1111-111111111111
```

### D) Demo: Create an order (via gateway)

```powershell
$body = @{
  restaurantId = "11111111-1111-1111-1111-111111111111"
  items = @(
    @{ menuItemId = "22222222-2222-2222-2222-222222222222"; quantity = 1 },
    @{ menuItemId = "33333333-3333-3333-3333-333333333333"; quantity = 1 }
  )
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Method Post -Uri "http://localhost:8080/ordering/api/orders" -ContentType "application/json" -Body $body
```

Expected: `202 Accepted` and a JSON response containing `orderId` and `totalPrice`.

### E) Run tests (local)

```bash
dotnet test mtogo-final-exam.sln
```

For Software Quality evidence details, see: `docs/software-quality.md`.

---

## Architecture Overview

### Runtime services
- **Gateway** routes external traffic to internal services:
  - `GET /legacy-menu/...` → `Mtogo.LegacyMenu.Api`
  - `GET /ordering/...` → `Mtogo.Ordering.Api`
- **Ordering → Legacy integration:**
  - Ordering validates restaurant existence via `ILegacyMenuClient` calling LegacyMenu HTTP endpoints.
  - Dependency failures are handled explicitly (503 mapping).

### High-level diagram (mental model)
Client → **Gateway (YARP)** → **Ordering API** → (HTTP) → **Legacy Menu API**

---

## Repository Structure

```text
docs/  # Course evidence docs (short, focused)
  software-quality.md
  system-integration.md
  udvikling-af-store-systemer.md
src/
  gateway/
    Mtogo.Gateway.Yarp/
  legacy-menu/
    Mtogo.LegacyMenu.Api/
  services/
    ordering/
      Mtogo.Ordering.Api/
    shared/
      Mtogo.Contracts/
tests/
  legacy-menu.tests/
    Mtogo.LegacyMenu.Tests/
  ordering.tests/
    Mtogo.Ordering.Tests/
deploy/
  compose/
    docker-compose.yml
.github/
  workflows/
    ci.yml
    codeql.yml
    ...
coverlet.runsettings
Directory.Build.props

---

## Run the system (Docker Compose)

From repo root:

```bash
docker compose -f ./deploy/compose/docker-compose.yml up --build
```

### Health checks (via gateway)

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/health
Invoke-RestMethod http://localhost:8080/ordering/health
```

Expected:

* legacy-menu: `{ status: "ok", service: "legacy-menu" }`
* ordering: `{ status: "ok", service: "ordering" }`

---

## Demo endpoints (via gateway)

### 1) Legacy Menu: list menu items for a restaurant

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/api/legacy/menu/11111111-1111-1111-1111-111111111111
```

### 2) Ordering: create an order (valid request)

```powershell
$body = @{
  restaurantId = "11111111-1111-1111-1111-111111111111"
  items = @(
    @{ menuItemId = "22222222-2222-2222-2222-222222222222"; quantity = 1 },
    @{ menuItemId = "33333333-3333-3333-3333-333333333333"; quantity = 1 }
  )
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Method Post -Uri "http://localhost:8080/ordering/api/orders" -ContentType "application/json" -Body $body
```

Expected:

* Status 202
* response contains `orderId` and `totalPrice`

### 3) Ordering validation examples (bad input → 400)

* Missing restaurantId
* Empty items
* Quantity <= 0

---

## Software Quality (how to verify)

### Run tests locally

```bash
dotnet test mtogo-final-exam.sln
```

### Run Release tests + coverage using the same SDK container as CI

```powershell
docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 `
  dotnet test mtogo-final-exam.sln -c Release --settings coverlet.runsettings /p:CollectCoverage=true
```

### Quality gates in CI

* Build + test
* Formatting verification (`dotnet format` in verify mode)
* Coverage threshold gate (staged; current threshold is conservative to prevent regression)
* CodeQL scanning (static analysis)

Evidence writeup:

* `docs/software-quality.md`

---

## System Integration (how it is demonstrated)

Integration scenario:

* Ordering service must validate the restaurant by calling the legacy menu service.
* The integration boundary is explicit (`ILegacyMenuClient`).
* Dependency failure handling is implemented:

  * If the legacy service is down or times out, Ordering returns **503 Service Unavailable** (not a raw 500).

This creates a clear exam narrative:

* “We are integrating a modern ordering capability into an existing legacy menu system.”

(Integration writeup can be added later under `docs/system-integration.md`.)

---

## Udvikling af Store Systemer (collaboration setup)

This repo reuses patterns from my earlier collaboration template:

* Issue templates + PR templates
* Labels + label automation
* CI workflow
* Code scanning
* Branch protection/ruleset (where applicable)
* Optional lightweight metrics (if kept)

The intention is to demonstrate professional collaboration and delivery practices even in a solo exam project.

---

## Documentation index (exam evidence)

* `docs/software-quality.md` — testing strategy, gates, static analysis, and execution commands
* `docs/system-integration.md`
* `docs/udvikling-af-store-systemer.md`

---

## License

MIT (see `LICENSE`).

