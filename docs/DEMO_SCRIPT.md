# Demo script

1. Open the deployed web app at `https://medpath-ai-web.vercel.app` or start the API and frontend using the README commands.
2. Sign in as `student@medpath.local` and show the seeded course, dashboard metrics, course detail, and lesson completion.
3. Call `POST /api/ai-study` from Swagger with a lesson title/action and point out the `isEducationalOnly` boundary on the mock response.
4. Sign out and sign in as `educator@medpath.local`; create a course from Course builder and explain the server-side policy.
5. Sign in as `admin@medpath.local`; show Users and Audit log. Explain that authorization is enforced server-side, not just hidden in the UI.
6. Open `/swagger` and `/health` on the API to demonstrate the operational surface.
