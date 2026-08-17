FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/MedPath.Domain/MedPath.Domain.csproj backend/MedPath.Domain/
COPY backend/MedPath.Application/MedPath.Application.csproj backend/MedPath.Application/
COPY backend/MedPath.Infrastructure/MedPath.Infrastructure.csproj backend/MedPath.Infrastructure/
COPY backend/MedPath.Api/MedPath.Api.csproj backend/MedPath.Api/
RUN dotnet restore backend/MedPath.Api/MedPath.Api.csproj
COPY backend/ backend/
RUN dotnet publish backend/MedPath.Api/MedPath.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MedPath.Api.dll"]
