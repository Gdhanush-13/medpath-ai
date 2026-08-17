# Operations runbook

## Health check

```powershell
Invoke-RestMethod https://medpath-ai-api.onrender.com/health
```

Expected response:

```json
{"status":"ok","service":"medpath-api"}
```

## First-response troubleshooting

1. Check Render service status and recent deploy logs.
2. Request `/health` to distinguish a sleeping instance from an application failure.
3. Inspect the API logs for startup, database, JWT, or seed errors.
4. Verify the Vercel `VITE_API_URL` value and redeploy after changing it.
5. Reproduce locally with the same environment variables before changing code.

## Common demo issues

### The first request is slow

The Render free instance sleeps after inactivity. Wait for the service to wake, then retry.

### Login returns 401

Confirm the local demo credentials are being used and that the API has completed startup/seed. In a deployed environment, never assume the local seed password is appropriate.

### The frontend calls localhost

The Vite API URL is a build-time value. Set `VITE_API_URL` in Vercel and create a new production deployment.

### Data disappeared

The current demo uses EF Core InMemory. Any restart or redeploy recreates the seed state. Use SQL Server/Azure SQL plus migrations for persistence.

## Security response

If a secret is exposed, rotate it immediately, invalidate refresh tokens, remove it from the repository, and review GitHub/Vercel/Render logs. Do not rely on deleting a commit as the only remediation.

## Observability roadmap

For production, add structured correlation IDs, OpenTelemetry traces, Application Insights, alerting on 5xx/latency, database health checks, and a durable audit-log retention policy.
