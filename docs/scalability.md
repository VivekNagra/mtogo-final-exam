# Scalability (Kubernetes + Docker Swarm + Terraform discussion)

This repo runs locally with Docker Compose, which is the best fit for an exam demo and development.

The DLS brief also expects:
- a scalability discussion (Terraform is mentioned)
- code evidence using tools such as Kubernetes and Docker Swarm

## Docker Compose (implemented baseline)

Compose is used for local orchestration:
- easy startup
- predictable demo
- good enough for a small slice

See: `docker-compose.exam.yml`

## Docker Swarm (code evidence)

A minimal Swarm stack file exists to show how the same services could run with replicas and rolling updates.

File:
- `deploy/swarm/stack.yml`

Example (local):
```bash
docker swarm init
docker stack deploy -c deploy/swarm/stack.yml mtogo
```

## Kubernetes (code evidence)
A minimal Kubernetes setup exists to show deployability and horizontal scaling.
Files:
- deploy/k8s/namespace.yml
- deploy/k8s/postgres.yml
- etc.

Example:
```bash
kubectl apply -f deploy/k8s/namespace.yml
kubectl apply -f deploy/k8s/postgres.yml
```

## Terraform (Design, not implemented)
Terraform is discussed in the Word report as Infrastructure as Code for a real MTOGO deployment (networking, managed databases, cluster provisioning, etc.).