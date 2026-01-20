
# MTOGO Final Exam Project (2nd Semester) — Software Development Top-Up

This repository contains my **final exam project (2nd semester)** for the Software Development Top-Up.

The exam has three areas, all covered in this repo:

- **Udvikling af Store Systemer (DLS)**: repo structure, documentation, collaboration setup, observability, and (design) deployment considerations.
- **System Integration (SI)**: a modern Ordering service calling a legacy Menu service.
- **Software Quality (SQ)**: tests, CI checks, formatting gate, coverage thresholds, CodeQL.

The implementation is intentionally small and demo-friendly: a legacy menu system already exists, and I add a small ordering service on top, routed through an API Gateway.

---

## What is implemented

- **Gateway (YARP)**: `src/gateway/Mtogo.Gateway.Yarp/`  
  Single entry point and routing to services.

- **Legacy Menu service (legacy boundary)**: `src/legacy-menu/Mtogo.LegacyMenu.Api/`  
  Restaurant + menu data.

- **Ordering service (modern service)**: `src/services/ordering/Mtogo.Ordering.Api/`  
  Place order endpoint + validation against legacy menu.

- **Observability**: Prometheus scraping `/metrics` + Grafana dashboard provisioning (Prometheus datasource + an overview dashboard).

I worked alone, but the repository is structured like a team repository, including PR flow, templates, CI, and code scanning. I started from my earlier **large-systems collaboration template** to get a realistic baseline.

---

## Examiner quick start

### 1) Run the system

From repo root:

```bash
docker compose -f docker-compose.exam.yml up --build
````

### 2) Verify services (through the gateway)

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/health
Invoke-RestMethod http://localhost:8080/ordering/health
```

Expected: both return an OK response (simple status payload).

### 3) Call legacy menu (through gateway)

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/api/legacy/menu/11111111-1111-1111-1111-111111111111
```

### 4) Create an order (through gateway)

```powershell
$body = @{
  restaurantId = "11111111-1111-1111-1111-111111111111"
  items = @(
    @{ menuItemId = "22222222-2222-2222-2222-222222222222"; quantity = 1 },
    @{ menuItemId = "33333333-3333-3333-3333-333333333333"; quantity = 1 }
  )
} | ConvertTo-Json -Depth 10

Invoke-RestMethod -Method Post -Uri "http://localhost:8080/ordering/api/orders" `
  -ContentType "application/json" -Body $body
```

Expected:

* HTTP **202 Accepted**
* Response includes `orderId` and `totalPrice`

---

## Observability (Prometheus + Grafana)

### Key URLs

```text
Gateway metrics:     http://localhost:8080/metrics
Ordering metrics:    http://localhost:8082/metrics
Legacy metrics:      http://localhost:8081/metrics

Prometheus:          http://localhost:9090
Prometheus targets:  http://localhost:9090/targets

Grafana:             http://localhost:3000  (admin / admin)
```

### Where the observability setup lives

* Prometheus config: `deploy/observability/prometheus/prometheus.yml`
* Grafana provisioning:

  * Datasource: `deploy/observability/grafana/provisioning/datasources/datasource.yml`
  * Dashboard provider: `deploy/observability/grafana/provisioning/dashboards/dashboard.yml`
  * Dashboard JSON: `deploy/observability/grafana/dashboards/mtogo-overview.json`
* Evidence screenshots (used for the exam): `docs/evidence/observability/`

---

## System Integration (SI) — what I demonstrate

The Ordering service depends on the LegacyMenu service:

* Ordering calls the legacy service through an abstraction (client interface)
* Ordering validates that restaurant/menu items exist before accepting an order
* If the legacy service is down or times out, Ordering returns **503** (controlled dependency failure) instead of crashing with a raw 500

Docs: `docs/system-integration.md`

---

## Software Quality (SQ) — how to verify

### Run tests locally

```bash
dotnet test mtogo-final-exam.sln
```

### CI checks (runs on push/PR)

CI includes:

* restore + build (Release)
* formatting check (`dotnet format --verify-no-changes`)
* tests + coverage gate (per test project thresholds)
* CodeQL scan

Workflow files:

* `.github/workflows/ci.yml`
* `.github/workflows/codeql.yml`

Docs: `docs/software-quality.md`

---

## DLS documentation entry points

* Main repo-side DLS doc: `docs/udvikling-af-store-systemer.md`
* Architecture diagrams: `docs/diagrams/README.md`
* Collaboration evidence: `.github/` (templates, workflows, repo automation)

---

## Repo structure (overview)

```text
docs/
  software-quality.md
  system-integration.md
  udvikling-af-store-systemer.md
  diagrams/
  evidence/
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
  observability/
  compose/
.github/
  workflows/
    ci.yml
    codeql.yml
coverlet.runsettings
Directory.Build.props
```

---

## Notes
License: MIT (see `LICENSE`).
