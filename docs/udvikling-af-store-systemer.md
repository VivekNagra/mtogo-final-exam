# Udvikling af Store Systemer (DLS) — Repo Documentation

This document is the **repository-side documentation** for the DLS part of my final exam project (MTOGO).  
The **Word report** is the main deliverable for DLS. This file exists to keep the repo evidence tidy: links, diagrams, and where the “proof” lives.

## Scope (what this repo actually contains)

This repository is an **exam-friendly slice** of the bigger MTOGO scenario:

- **API Gateway (YARP)** as a single entry point
- **Legacy Menu service** (legacy boundary)
- **Ordering service** (modern service)
- **Sync HTTP integration**: Ordering → LegacyMenu (validation)
- **Docker Compose** run setup
- **CI pipeline** + tests (quality gate)

Anything outside this slice is either **out of scope** or explicitly marked as **Design (not implemented)**.

## Collaboration foundation (template-based)

Even though I worked alone, the repository is set up like a team repo. I started from my earlier **large-systems collaboration template** to get a realistic baseline: structure, automation, and repo conventions.

You can see this in the `.github/` setup and supporting files (workflows, templates, and contribution guidance).

## Diagrams (Mermaid)

All architecture diagrams are stored under `docs/diagrams/` and rendered in GitHub.

- `docs/diagrams/README.md` (recommended entry point)
- `docs/diagrams/c4-context.mmd`
- `docs/diagrams/c4-container.mmd`
- `docs/diagrams/component-ordering.mmd`
- `docs/diagrams/sequence-order-flow.mmd`

## Architecture summary (implemented)

At a high level, the system is split into clear subsystems:

- **Gateway**: one entry point for clients, routes requests to services
- **Ordering service**: owns the “place order” use case and business validation logic
- **LegacyMenu service**: owns menu data (legacy boundary)

The key integration is **synchronous HTTP** from Ordering to LegacyMenu, used to validate restaurant/menu item references during ordering.

This is intentionally simple so it is easy to run and demonstrate, and it still reflects a realistic “legacy + modern” setup often seen in large systems.

## Core flow: Place order (implemented)

The main documented flow is placing an order through the gateway:

1. Client calls the gateway endpoint for orders
2. Gateway forwards to Ordering
3. Ordering calls LegacyMenu to validate menu references
4. Ordering returns either a success response, a validation error, or a controlled dependency failure response

The detailed sequence is documented as a Mermaid diagram:

- `docs/diagrams/sequence-order-flow.mmd`

## Error handling (implemented baseline)

Because services are distributed, dependency failures are expected.  
The most important failure mode in this slice is **LegacyMenu being unavailable or slow** while Ordering is trying to validate an order.

The project is documented to handle this with a controlled response (instead of “random 500 errors”), so it is clear for the client when the issue is a temporary dependency problem.

## How to run (local)

For the actual run commands, I keep the single source of truth in the root `README.md`.

In general, the repo is designed so you can:

- start the system with Docker Compose
- call endpoints through the gateway
- use health endpoints to verify services are running

See:
- `README.md`
- `docker-compose.exam.yml`

## CI/CD (implemented)

This repo includes a CI pipeline that runs on pushes and pull requests. The goal is to keep development honest: if CI fails, the change is not “done”.

See:
- `.github/workflows/ci.yml`
- `.github/workflows/codeql.yml`
- `docs/software-quality.md` (notes about tests/quality gates)

## Scalability & infrastructure (DLS Req 5)

**Design (not implemented yet):** Terraform / Kubernetes / Docker Swarm

The DLS brief expects scalability discussion (Terraform) and code evidence using tools such as Kubernetes and Docker Swarm.  
My implemented baseline is Docker Compose, which is enough for local demo, but not “national scale”.

Planned repo locations (to be added if missing):
- `deploy/k8s/` (Kubernetes manifests)
- `deploy/swarm/stack.yml` (Docker Swarm stack file)
- `docs/scalability.md` (short explanation + how to run)

## Observability (DLS Req 7)

**Design (not implemented yet):** Prometheus / Grafana

The DLS brief expects error handling and logging, and it mentions demonstrating with tools such as Prometheus and Grafana. The intention is to make failures visible (latency/errors/dependency problems), not to build a full monitoring platform.

Planned repo locations (to be added if missing):
- `deploy/observability/` (Prometheus + Grafana compose + configs)
- `docs/observability.md` (how to run + what to show at the exam)

## Evidence pointers (quick map)

If you want to check the repo evidence quickly, these are the main entry points:

- System overview + run steps: `README.md`
- DLS diagrams: `docs/diagrams/README.md`
- Service boundaries: `src/gateway/`, `src/services/ordering/`, `src/legacy-menu/`
- Local orchestration: `docker-compose.exam.yml`
- CI/CD: `.github/workflows/ci.yml`, `.github/workflows/codeql.yml`
- Integration notes: `docs/system-integration.md`
- Quality notes: `docs/software-quality.md`
