# Observability (Prometheus + Grafana)

This project includes a small observability setup to support the DLS requirement about error handling/logging and demonstrating with tools such as Prometheus and Grafana.

The goal is not a full production monitoring platform. It is an exam-friendly setup that makes failures visible.

## What is included

- Prometheus scraping metrics from:
  - `gateway` (YARP)
  - `ordering`
  - `legacy-menu`
- Grafana with a pre-provisioned dashboard:
  - requests/sec
  - 5xx errors/sec
  - latency p95
  - ordering 503 dependency failures/sec

## How to run

Start the system with both compose files:

```bash
docker compose \
  -f deploy/compose/docker-compose.yml \
  -f deploy/observability/docker-compose.observability.yml \
  up --build
```
### Open:
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (admin / admin)

