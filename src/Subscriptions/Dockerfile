# --- Build stage ---
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

COPY webapi/src/Subscriptions/Subscriptions.csproj webapi/src/Subscriptions/
COPY webapi/src/Fudie/Fudie.csproj webapi/src/Fudie/
COPY webapi/src/Fudie.Generator/Fudie.Generator.csproj webapi/src/Fudie.Generator/
COPY Firestore/src/Fudie.Firestore.EntityFrameworkCore/Fudie.Firestore.EntityFrameworkCore.csproj Firestore/src/Fudie.Firestore.EntityFrameworkCore/

RUN dotnet restore webapi/src/Subscriptions/Subscriptions.csproj --runtime linux-musl-x64

COPY webapi/src/Subscriptions/ webapi/src/Subscriptions/
COPY webapi/src/Fudie/ webapi/src/Fudie/
COPY webapi/src/Fudie.Generator/ webapi/src/Fudie.Generator/
COPY Firestore/src/Fudie.Firestore.EntityFrameworkCore/ Firestore/src/Fudie.Firestore.EntityFrameworkCore/

RUN dotnet publish webapi/src/Subscriptions/Subscriptions.csproj -c Release -o /app \
    --runtime linux-musl-x64 \
    --self-contained false \
    --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app .
COPY --from=build /src/webapi/src/Subscriptions/OpenApi ./OpenApi

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Subscriptions.dll"]
