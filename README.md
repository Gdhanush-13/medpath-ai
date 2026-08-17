# MedPathAI

MedPathAI is a production-shaped clinical learning platform portfolio project. It demonstrates a .NET 8 API, React + TypeScript client, role-based access, learning progress, assessment submission, audit logging, and a provider-agnostic AI Study Assistant.

The AI feature is intentionally backed by a clearly labelled mock provider. It is suitable for demos and local development; it does not provide medical advice and it is not a substitute for clinical supervision.

## Run locally

Prerequisites: .NET 8 SDK and Node.js 20+.

```powershell
dotnet restore .\MedPathAI.sln
dotnet run --project .\backend\MedPath.Api\MedPath.Api.csproj --urls http://localhost:5076
```

In another terminal:

```powershell
cd .\frontend
npm install
npm run dev
```

Open the Vite URL and use one of the seeded local accounts:

| Role | Email | Password |
| --- | --- | --- |
| Student | student@medpath.local | MedPath123!Local |
| Educator | educator@medpath.local | MedPath123!Local |
| Administrator | admin@medpath.local | MedPath123!Local |

These credentials are development-only and must not be used in a deployed environment.

## Verify

```powershell
dotnet test .\MedPathAI.sln --no-restore -m:1
cd .\frontend
npm run build
```

The API exposes `/health` and Swagger at `/swagger` in Development. The default database is EF Core InMemory for a zero-setup demo. Set `Database__Provider=SqlServer` and a real connection string for SQL Server/Azure SQL.

## Repository map

- `backend/MedPath.Domain`: entities and domain enums.
- `backend/MedPath.Application`: DTOs and service contracts.
- `backend/MedPath.Infrastructure`: EF Core context, seed data, password/JWT/AI/audit services.
- `backend/MedPath.Api`: HTTP controllers, auth, policies, ProblemDetails, Swagger and health checks.
- `frontend`: React/Vite client with role-aware navigation and TanStack Query data access.
- `docs`: architecture, demo, interview and learning notes.

See `docs/ARCHITECTURE.md` for boundaries, security decisions and the Azure-ready deployment path.
