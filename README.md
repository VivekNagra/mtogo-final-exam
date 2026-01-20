# MTOGO Final Exam Project (2nd Semester) — Software Development Top-Up

This repo contains my final exam project for the Software Development Top-Up (2nd semester).

The project covers three course areas:
- **Software Quality** (tests, CI checks, formatting, coverage, CodeQL)
- **System Integration** (a modern service calling a legacy service)
- **Udvikling af Store Systemer** (repo structure, CI, collaboration setup)

The idea is simple on purpose: a legacy menu system already exists, and I add a small ordering service on top.

## What is in the solution

- **Legacy service:** `Mtogo.LegacyMenu.Api`  
  Stores restaurants + menu items (legacy boundary)

- **Ordering service:** `Mtogo.Ordering.Api`  
  Creates orders and applies pricing rules

- **Gateway:** `Mtogo.Gateway.Yarp`  
  One entry point that routes requests to the services

I worked alone, but I still keep the repo structured like a normal team repo (CI, PR flow, docs, etc.).

---

## Examiner Quick Start

### 1) Run the system (Docker Compose)

From repo root:

```bash
docker compose -f docker-compose.exam.yml up --build
```

### 2) Check that services are up (through the gateway)

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/health
Invoke-RestMethod http://localhost:8080/ordering/health
```

Expected: both return something like:

```json
{ "status": "ok", "service": "..." }
```

### 3) Verify Prometheus metrics

```powershell
Invoke-RestMethod http://localhost:8080/metrics
```

Expected: Prometheus text output (e.g., `http_requests_received_total`).

### 4) Call the legacy menu endpoint (through the gateway)

```powershell
Invoke-RestMethod http://localhost:8080/legacy-menu/api/legacy/menu/11111111-1111-1111-1111-111111111111
```

### 5) Create an order (through the gateway)

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

* HTTP **202 Accepted**
* Response includes `orderId` and `totalPrice`

---

## Software Quality (how to verify)

### Run tests locally

```bash
dotnet test mtogo-final-exam.sln
```

### Run Release tests + coverage in the same SDK container (evidence command)

```powershell
docker run --rm -v ${PWD}:/src -w /src mcr.microsoft.com/dotnet/sdk:10.0 `
  dotnet test mtogo-final-exam.sln -c Release `
    /p:CollectCoverage=true `
    /p:CoverletOutput=TestResults/ `
    /p:CoverletOutputFormat="cobertura,json" `
    /p:ExcludeByAttribute="Obsolete,GeneratedCodeAttribute,CompilerGeneratedAttribute" `
    /p:ExcludeByFile="**/Migrations/*.cs,**/Program.cs,**/OpenApi*.cs,**/obj/**,**/*.g.cs,**/*.g.i.cs,**/*AssemblyInfo*.cs"
```

### CI checks

CI runs on pushes to `main` and on pull requests:

* restore + build (Release)
* formatting check (`dotnet format --verify-no-changes`)
* tests + coverage gate (**Ordering 30% line**, **LegacyMenu 25% line**)
* CodeQL scan

Workflow file: `.github/workflows/ci.yml`

More details: `docs/software-quality.md`

---

## System Integration (what I show)

The Ordering service depends on the LegacyMenu service:

* Ordering calls the legacy service through `ILegacyMenuClient`
* It checks if the restaurant exists before accepting an order
* If the legacy service is down or times out, Ordering returns **503** instead of crashing with a raw 500

Docs: `docs/system-integration.md`

---

## Repo structure

```text
docs/
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
coverlet.runsettings
Directory.Build.props
```

---

## Notes

License: MIT (see `LICENSE`).

