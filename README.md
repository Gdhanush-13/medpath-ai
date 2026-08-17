# MedPathAI

MedPathAI is a production-shaped clinical learning platform portfolio project. It combines a .NET 8 REST API with a React + TypeScript learning workspace for students, educators, and administrators.

> **Safety boundary:** MedPathAI is an educational software demo. It does not diagnose, prescribe, triage, or replace clinical supervision. The AI Study Assistant is deliberately a labelled mock provider in this repository.

## Live deployment

| Surface | URL | Hosting | Status |
| --- | --- | --- | --- |
| Web application | [medpath-ai-web.vercel.app](https://medpath-ai-web.vercel.app) | Vercel | Production build deployed |
| API | [medpath-ai-api.onrender.com](https://medpath-ai-api.onrender.com) | Render | Docker web service |
| API health | [/health](https://medpath-ai-api.onrender.com/health) | Render | Returns `200` when awake |
| Source | [github.com/Gdhanush-13/medpath-ai](https://github.com/Gdhanush-13/medpath-ai) | GitHub | `main` branch |

The Render service uses the free plan and may sleep after inactivity. The first request after sleeping can take up to a minute. The deployed demo currently uses EF Core InMemory storage, so its data resets when the service restarts.

## What is implemented

### Student experience

- JWT sign-in, refresh, logout, and current-user session restoration.
- Dashboard metrics for enrollments, completed lessons, assessments, and average score.
- Course catalog and course detail pages.
- Lesson completion and progress tracking.
- Assessment submission API and result model.
- AI Study Assistant endpoint with educational-only response marking.

### Educator experience

- Role-protected course creation.
- Module creation, publishing, and learner enrollment APIs.
- Educator analytics endpoint.
- Course builder screen in the React client.

### Administrator experience

- User directory and user creation APIs.
- User activation/deactivation.
- Administrator analytics.
- Recent audit-log review.

### Engineering and operations

- Clean backend boundaries: Domain, Application, Infrastructure, and API.
- EF Core with InMemory demo mode and SQL Server/Azure SQL configuration.
- PBKDF2-SHA256 password hashing with per-password salts.
- Short-lived JWT access tokens and hashed, rotated refresh tokens.
- Policy-based Student/Educator/Administrator authorization.
- ProblemDetails and centralized exception handling.
- Swagger in Development and `/health` for deployment checks.
- Audit events for security-relevant changes.
- Dockerfile, Docker Compose, Render Blueprint, and GitHub Actions CI.
- Frontend type-checking, Vite production build, and Vitest coverage.

## Repository structure

```text
MedPathAI/
├── backend/
│   ├── MedPath.Domain/          # Entities and domain enums
│   ├── MedPath.Application/     # DTOs and service contracts
│   ├── MedPath.Infrastructure/  # EF Core, seed data, auth, AI and audit adapters
│   ├── MedPath.Api/             # HTTP pipeline, policies, controllers and Swagger
│   └── MedPath.Tests/           # Backend unit tests
├── frontend/                   # React, TypeScript, Vite and TanStack Query
│   └── vercel.json             # SPA history fallback for direct routes
├── docs/                       # Architecture, API, deployment, operations and demo notes
├── .github/workflows/ci.yml    # Backend and frontend CI
├── docker-compose.yml          # Local API + frontend containers
└── render.yaml                 # Render API Blueprint
```

## Run locally

### Prerequisites

- .NET 8 SDK
- Node.js 20 or newer
- npm
- Docker Desktop is optional

### Start the API

From the repository root:

```powershell
dotnet restore .\MedPathAI.sln
dotnet run --project .\backend\MedPath.Api\MedPath.Api.csproj --urls http://localhost:5076
```

The API is available at `http://localhost:5076`. In Development, Swagger is at `http://localhost:5076/swagger` and health is at `http://localhost:5076/health`.

### Start the web app

In a second terminal:

```powershell
cd .\frontend
npm install
npm run dev
```

The Vite client defaults to `http://localhost:5173` and calls the API at `http://localhost:5076`. Override this with `frontend/.env`:

```text
VITE_API_URL=http://localhost:5076
```

### Local demo accounts

The development seed creates these accounts with the same local-only password:

| Role | Email | Password |
| --- | --- | --- |
| Student | `student@medpath.local` | `MedPath123!Local` |
| Educator | `educator@medpath.local` | `MedPath123!Local` |
| Administrator | `admin@medpath.local` | `MedPath123!Local` |

These credentials are for local demos only. Change the seed and provide secrets from a managed secret store before any non-local deployment.

## Configuration

Copy `.env.example` into the environment that starts the API. ASP.NET Core maps double underscores to configuration sections:

```text
Database__Provider=InMemory
ConnectionStrings__DefaultConnection=Server=localhost;Database=MedPathAI;Trusted_Connection=True;TrustServerCertificate=True
Jwt__Issuer=MedPathAI
Jwt__Audience=MedPathAI.Web
Jwt__Key=replace-with-a-random-32-character-secret
```

Use `Database__Provider=SqlServer` with a real SQL Server/Azure SQL connection string for persistence. Do not commit `.env`, passwords, JWT keys, connection strings, or provider API keys.

## Verification commands

From the repository root:

```powershell
dotnet restore .\MedPathAI.sln
dotnet build .\backend\MedPath.Api\MedPath.Api.csproj -m:1
dotnet test .\MedPathAI.sln --no-restore -m:1
```

For the frontend:

```powershell
cd .\frontend
npm install
npm run lint
npm run build
npm run test
npm audit --omit=dev
```

The checked production dependency audit is clean. Development tooling may report advisories inherited from test/build packages; review those separately before hardening a CI policy.

## API surface

All routes are rooted at `/api`:

| Area | Routes | Authorization |
| --- | --- | --- |
| Auth | `POST /auth/login`, `/auth/refresh`, `/auth/logout`, `GET /auth/me` | Login/refresh public; others authenticated |
| Courses | `GET /courses`, `GET /courses/{id}` | Public/read; detail includes learner completion when signed in |
| Course management | `POST /courses`, `POST /courses/{id}/modules`, `/publish`, `/enrollments/{studentId}` | Educator or Administrator |
| Learning | `GET /learning/dashboard`, `/course/{id}`, `POST /lessons/{id}/complete` | Student, Educator, Administrator |
| Assessments | `POST /learning/assessments/{id}/submit` | Student, Educator, Administrator |
| Users | `GET/POST /users`, `PATCH /users/{id}/status` | Administrator |
| Analytics | `GET /analytics/student`, `/educator`, `/admin` | Role policy |
| AI study | `POST /ai-study` | Authenticated learning role |
| Audit | `GET /audit-logs` | Administrator |

Request and response records are defined in `backend/MedPath.Application/Contracts.cs`. See [API_REFERENCE.md](docs/API_REFERENCE.md) for examples and status-code expectations.

## Deployment summary

The repository is connected to the deployed surfaces as follows:

1. GitHub stores the `main` branch and the CI workflow.
2. Render reads `render.yaml`, builds `backend/MedPath.Api/Dockerfile`, and exposes `/health`.
3. Vercel builds `frontend` with Vite and uses `VITE_API_URL=https://medpath-ai-api.onrender.com`.

Detailed instructions, environment setup, rollback guidance, and production hardening are in [DEPLOYMENT.md](docs/DEPLOYMENT.md) and [OPERATIONS.md](docs/OPERATIONS.md).

## Production hardening still required

This repository is honest about its demo defaults. Before calling it a production clinical system:

- Replace InMemory with SQL Server/Azure SQL and add reviewed EF migrations.
- Restrict CORS to the deployed web origin.
- Store JWT keys and provider credentials in Key Vault or another managed secret store.
- Persist ASP.NET Core Data Protection keys across API replicas.
- Replace `MockAiStudyService` with a reviewed provider adapter, safety filters, timeouts, quotas, and human-review policy.
- Add distributed rate limiting, centralized telemetry, backup/restore, alerting, and a formal threat model.
- Replace local demo credentials and define account recovery/MFA requirements.

## Documentation

- [Architecture](docs/ARCHITECTURE.md)
- [Deployment](docs/DEPLOYMENT.md)
- [Operations runbook](docs/OPERATIONS.md)
- [API reference](docs/API_REFERENCE.md)
- [Demo script](docs/DEMO_SCRIPT.md)
- [Interview guide](docs/INTERVIEW_GUIDE.md)
- [.NET learning notes](docs/DOTNET_LEARNING_NOTES.md)

## License and attribution

This portfolio repository does not currently declare an open-source license. Add an explicit license before accepting external contributions or redistributing it.
