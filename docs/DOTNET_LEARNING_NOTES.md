# .NET learning notes

- Minimal hosting in `Program.cs` keeps composition at the API boundary.
- EF Core migrations should be added once the SQL schema is finalized; the current InMemory default keeps the first run frictionless.
- `ProblemDetails` gives clients a consistent HTTP error envelope.
- Policy-based authorization is more maintainable than scattering role checks through every controller.
- `CancellationToken` is passed through database and provider calls so requests can stop cleanly.
