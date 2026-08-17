# API reference

Base URL locally: `http://localhost:5076`  
Base URL deployed: `https://medpath-ai-api.onrender.com`

Swagger is available at `/swagger` when `ASPNETCORE_ENVIRONMENT=Development`.

## Authentication

### Login

```http
POST /api/auth/login
Content-Type: application/json

{"email":"student@medpath.local","password":"MedPath123!Local"}
```

The response contains `accessToken`, `refreshToken`, `expiresAtUtc`, and the current user. Send the access token as `Authorization: Bearer <token>`.

### Refresh and logout

```http
POST /api/auth/refresh
Content-Type: application/json

{"refreshToken":"<opaque-token>"}
```

```http
POST /api/auth/logout
Authorization: Bearer <access-token>
Content-Type: application/json

{"refreshToken":"<opaque-token>"}
```

Refresh tokens are hashed at rest, rotated on use, and revocable.

## Read APIs

| Method | Route | Purpose |
| --- | --- | --- |
| `GET` | `/health` | Deployment liveness response |
| `GET` | `/api/auth/me` | Resolve the authenticated user |
| `GET` | `/api/courses` | Course summaries |
| `GET` | `/api/courses/{courseId}` | Modules, lessons, and assessment questions |
| `GET` | `/api/learning/dashboard` | Student learning metrics |
| `GET` | `/api/learning/course/{courseId}` | Course progress |
| `GET` | `/api/analytics/student` | Student analytics |
| `GET` | `/api/analytics/educator` | Educator analytics |
| `GET` | `/api/analytics/admin` | Administrator analytics |
| `GET` | `/api/users` | Administrator user list |
| `GET` | `/api/audit-logs` | Last 100 administrator audit entries |

## Write APIs

| Method | Route | Role |
| --- | --- | --- |
| `POST` | `/api/courses` | Educator/Administrator |
| `POST` | `/api/courses/{courseId}/modules` | Educator/Administrator |
| `POST` | `/api/courses/{courseId}/publish` | Educator/Administrator |
| `POST` | `/api/courses/{courseId}/enrollments/{studentId}` | Educator/Administrator |
| `POST` | `/api/learning/lessons/{lessonId}/complete` | Authenticated learning role |
| `POST` | `/api/learning/assessments/{assessmentId}/submit` | Authenticated learning role |
| `POST` | `/api/ai-study` | Authenticated learning role |
| `POST` | `/api/users` | Administrator |
| `PATCH` | `/api/users/{userId}/status` | Administrator |

## Error behavior

Validation and unhandled errors use the ASP.NET Core ProblemDetails pipeline. Typical responses are:

- `400` malformed or invalid request
- `401` missing, expired, or invalid token
- `403` authenticated user lacks the required policy
- `404` resource not found
- `409` uniqueness conflict, such as an existing email
- `500` unexpected server error, logged by the API

## AI safety contract

`POST /api/ai-study` accepts an action, lesson title, and lesson content. The current response includes `isEducationalOnly: true`; callers must keep the content in an educational context and must not present it as medical advice.
