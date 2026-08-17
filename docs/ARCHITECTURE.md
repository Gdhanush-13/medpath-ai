# Architecture

MedPathAI uses a pragmatic layered architecture:

```text
React/Vite -> REST controllers -> Application contracts -> Infrastructure services -> EF Core -> InMemory or SQL Server
```

`Domain` owns persistence-agnostic entities. `Application` owns use-case contracts and transport DTOs. `Infrastructure` implements persistence and external-service seams. `Api` owns HTTP concerns and authorization policies. This keeps provider, database and UI changes local to their boundaries.

## Security decisions

- Passwords use PBKDF2-SHA256 with per-password random salts and fixed-time comparison.
- Access tokens are short-lived JWTs; refresh tokens are opaque, hashed at rest, rotated on use and revocable.
- Student, Educator and Administrator roles are enforced with named policies.
- Audit events are persisted for course/user security actions.
- No secrets are committed. Production JWT keys and connection strings must come from environment variables or a managed secret store.
- CORS is intentionally permissive for the demo deployment so the Vercel client can call the Render API. Restrict it to the web origin before production use.

## Azure-ready path

Use Azure App Service or Container Apps for the API, Static Web Apps for the client, Azure SQL for persistence, Application Insights/OpenTelemetry for telemetry, and Key Vault for secrets. Replace `MockAiStudyService` with an implementation of `IAiStudyService` using Azure OpenAI after adding content safety, prompt limits, timeout/retry policy and human review requirements.

## Deliberate scope

The repository demonstrates a complete vertical slice: authentication, role-aware dashboards, course catalog, lesson completion, assessment-ready course data, admin users, audit history and a clearly labelled mock AI endpoint. Background jobs, real-time collaboration, file storage and a live AI provider are extension points rather than hidden fake implementations.
