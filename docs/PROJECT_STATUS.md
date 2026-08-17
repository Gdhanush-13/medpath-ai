# Project status

Last verified: 2026-08-18.

## Delivered

- GitHub repository: `https://github.com/Gdhanush-13/medpath-ai`
- Render API: `https://medpath-ai-api.onrender.com`
- Vercel web app: `https://medpath-ai-web.vercel.app`
- GitHub `main` includes the layered backend, frontend, tests, CI, Dockerfile, Compose file, Render Blueprint, and documentation.

## Verification evidence

- Backend build passed with .NET 8.
- Backend test suite passed: 3 tests.
- Frontend TypeScript lint passed.
- Frontend Vite production build passed.
- Frontend Vitest suite passed: 2 tests.
- Production dependency audit passed with zero reported production vulnerabilities.
- Production browser smoke test completed: sign-in redirected to `/dashboard`, loaded the seeded course, and rendered dashboard metrics.
- Render logs showed `/health` returning HTTP 200.

## Explicit limitations

- InMemory persistence is used for the public demo; it is not durable.
- The Render free tier sleeps after inactivity.
- The AI service is a mock and must not be represented as a clinical model.
- Docker was configured but local Docker execution depends on Docker Desktop being installed.
- The project is portfolio-grade and Azure-ready by design; it is not a certified clinical product.
