# Interview guide

## Why these boundaries?

Controllers stay thin and map HTTP to application contracts. Infrastructure owns EF Core and replaceable adapters. The mock AI provider proves the dependency inversion boundary without pretending a model is configured.

## What would change at scale?

Move long-running AI and report work to a queue, add distributed caching, use SQL Server transactions and indexes, centralize telemetry, and deploy multiple API replicas behind a managed ingress. Refresh-token reuse detection and a revocation store would become mandatory for a larger threat model.

## What is intentionally not claimed?

This is not a certified clinical system, does not diagnose or prescribe, and the seeded credentials are not production credentials. The local InMemory provider is a demo default, not a production persistence choice.
