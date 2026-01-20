# Udvikling af Store Systemer (DLS) — Repo Documentation

This file is the **repo-side documentation** for the DLS part of my final exam project (MTOGO).  
The **main DLS deliverable is a separate Word report (max 10 pages)**. This document exists so the repository has clear, concrete evidence: where things live, what is implemented, and what is design-only.

---

## Scope and honesty rule

This repo contains an **exam-friendly slice** of a larger “national provider” scenario. I kept the solution small on purpose so it is easy to run, inspect, and explain.

Implemented in this repo:

- API Gateway (YARP) as a single entry point
- Legacy Menu service (legacy boundary)
- Ordering service (modern service)
- Synchronous HTTP integration: Ordering → LegacyMenu
- Local orchestration with Docker Compose
- CI pipeline with quality gates
- Observability baseline: Prometheus + Grafana (metrics + dashboard provisioning)

Anything outside this slice is marked clearly as **Design (not implemented)**.

---

## Collaboration setup (template-based foundation)

Even though I worked alone, I wanted the repo to feel like a real team repository. I started from my earlier **large-systems collaboration template** so I did not “hand-wave” collaboration practices.

This shows up as:

- standard repo structure (`docs/`, `src/`, `tests/`, `deploy/`)
- PR flow conventions and templates
- CI workflows and automated checks

Evidence lives primarily under `.github/`.

---

## Architecture overview (implemented)

The system is split into three main runtime parts:

- **Gateway**: routes incoming requests to the right service.
- **Ordering service**: owns the “place order” flow and validates against legacy menu data.
- **LegacyMenu service**: owns restaurant/menu data and represents a legacy boundary.

The important integration in this slice is a synchronous HTTP call from Ordering to LegacyMenu. The goal is to show a realistic dependency where a modern service must call a legacy service and handle failure in a controlled way.

---

## Core flow: Place order (implemented)

The main scenario I demonstrate is placing an order through the gateway:

1. Client calls the gateway endpoint for orders
2. Gateway forwards the request to Ordering
3. Ordering calls LegacyMenu to validate restaurant + menu items
4. Ordering returns either:
   - success (accepted order)
   - validation error
   - dependency failure (503) if legacy is unavailable

The detailed sequence is documented with Mermaid:

- `docs/diagrams/sequence-order-flow.mmd`

---

## Error handling (implemented baseline)

In distributed systems, dependency failures are normal. In this slice, the most important failure mode is the legacy service being unavailable or slow when Ordering needs to validate an order.

The implemented goal is to return a clear, controlled response (503) instead of crashing with a random 500. This keeps the failure obvious and makes it easier to operate the system even when one dependency is unstable.

---

## Observability baseline (implemented)

I added a minimal but realistic observability setup:

- Each service exposes Prometheus-compatible metrics at `/metrics`
- Prometheus scrapes the services using Docker Compose service names (container network)
- Grafana provisions:
  - a Prometheus datasource
  - an overview dashboard (JSON in repo)
- Evidence screenshots are stored in `docs/evidence/observability/`

This is intentionally “small but real”: enough to show request rate, latency, and error responses (including 503 when legacy is down).

---

## How to run (local)

I keep the single source of truth for run steps in the root `README.md`.

Main entry points:

- `README.md`
- `docker-compose.exam.yml`

---

## CI/CD (implemented)

This repository includes CI that runs on pushes and pull requests. The goal is to keep changes honest: if CI fails, the change is not “done”.

Evidence:

- `.github/workflows/ci.yml`
- `.github/workflows/codeql.yml`

More detailed notes (SQ part):

- `docs/software-quality.md`

---

## Scalability and deployment (Design — not implemented)

This exam scenario describes a national provider, so scalability and deployment concerns matter. My implemented baseline is Docker Compose for local demo. For production-style deployment, the next step would be Kubernetes (and possibly infrastructure-as-code), but that is **not implemented here unless `deploy/k8s/` exists and is runnable**.

Planned repo locations (only relevant if added later):

- `deploy/k8s/` (Kubernetes manifests)
- `deploy/swarm/` (Docker Swarm stack file)
- `docs/scalability.md` (short explanation + how to run)

If these folders/files are not present, they should be treated as **Design (not implemented)** in the Word report.

---

## Diagrams (Mermaid)

All architecture diagrams are stored under `docs/diagrams/` and rendered in GitHub.

Recommended entry point:

- `docs/diagrams/README.md`

Key diagram files:

- `docs/diagrams/c4-context.mmd`
- `docs/diagrams/c4-container.mmd`
- `docs/diagrams/component-ordering.mmd`
- `docs/diagrams/sequence-order-flow.mmd`

---

## Evidence map (quick pointers)

If you want to verify the repo evidence quickly, these are the main entry points:

- System overview + run steps: `README.md`
- DLS repo doc (this file): `docs/udvikling-af-store-systemer.md`
- DLS diagrams: `docs/diagrams/README.md`
- Service boundaries:
  - `src/gateway/Mtogo.Gateway.Yarp/`
  - `src/services/ordering/Mtogo.Ordering.Api/`
  - `src/legacy-menu/Mtogo.LegacyMenu.Api/`
- Local orchestration: `docker-compose.exam.yml`
- Observability setup:
  - `deploy/observability/prometheus/prometheus.yml`
  - `deploy/observability/grafana/`
  - `docs/evidence/observability/`
- CI/CD: `.github/workflows/ci.yml`, `.github/workflows/codeql.yml`
- Integration notes: `docs/system-integration.md`
- Quality notes: `docs/software-quality.md`



