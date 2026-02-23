# --- Build stage ---
ARG PROJECT_DIR=webapi

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG PROJECT_DIR
WORKDIR /src

COPY ${PROJECT_DIR}/Directory.Build.props webapi/
COPY ${PROJECT_DIR}/src/Fudie.Gateway/Fudie.Gateway.csproj webapi/src/Fudie.Gateway/

RUN dotnet restore webapi/src/Fudie.Gateway/Fudie.Gateway.csproj

COPY ${PROJECT_DIR}/src/Fudie.Gateway/ webapi/src/Fudie.Gateway/

RUN dotnet publish webapi/src/Fudie.Gateway/Fudie.Gateway.csproj -c Release -o /app

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Fudie.Gateway.dll"]
