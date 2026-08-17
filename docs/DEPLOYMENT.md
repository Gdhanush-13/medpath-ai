# Deployment guide

## Current topology

```text
GitHub main
   ├── Render Blueprint -> Docker .NET API -> https://medpath-ai-api.onrender.com
   └── Vercel -> Vite React app -> https://medpath-ai-web.vercel.app
```

## Render API

`render.yaml` defines the `medpath-ai-api` Docker web service. It uses `backend/MedPath.Api/Dockerfile`, exposes port `8080` inside the container, and checks `/health`.

Required Render environment variables:

```text
ASPNETCORE_ENVIRONMENT=Production
Database__Provider=InMemory
Jwt__Issuer=MedPathAI
Jwt__Audience=MedPathAI.Web
Jwt__Key=<generated-secret>
```

For a real deployment, change the database provider to `SqlServer`, supply `ConnectionStrings__DefaultConnection` from a secret store, and persist Data Protection keys. Render's free plan is suitable for a portfolio demo, not a latency-sensitive production service.

## Vercel frontend

The frontend is a Vite project rooted at `frontend`. Its production variable is:

```text
VITE_API_URL=https://medpath-ai-api.onrender.com
```

Vite embeds `VITE_*` values into the browser bundle. Never put private credentials in a Vite environment variable.

## Manual deployment commands

From the repository root:

```powershell
# API container
docker compose up --build

# Vercel frontend (from frontend/)
vercel --prod --yes --build-env VITE_API_URL=https://medpath-ai-api.onrender.com
```

## Rollback

- Vercel: choose a previous deployment in the Vercel project and promote it to production.
- Render: redeploy the previous known-good Git commit from the service's Deploys page.
- GitHub: keep `main` protected and use a reviewed revert commit; do not rewrite shared history.

## Release checklist

- Confirm backend tests and frontend build pass.
- Confirm no `.env` or secret files are staged.
- Confirm the Render `/health` endpoint returns `200`.
- Confirm browser login, dashboard, course loading, and sign-out.
- Confirm CORS contains only the intended web origin.
- Confirm database migrations/backups and secret rotation for non-demo environments.
