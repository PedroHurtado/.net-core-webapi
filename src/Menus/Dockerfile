# --- Build stage ---
ARG PROJECT_DIR=webapi
ARG FIRESTORE_DIR=Firestore

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG PROJECT_DIR
ARG FIRESTORE_DIR
WORKDIR /src

COPY ${PROJECT_DIR}/src/Menus/Menus.csproj ${PROJECT_DIR}/src/Menus/
COPY ${PROJECT_DIR}/src/Fudie/Fudie.csproj ${PROJECT_DIR}/src/Fudie/
COPY ${PROJECT_DIR}/src/Fudie.Generator/Fudie.Generator.csproj ${PROJECT_DIR}/src/Fudie.Generator/
COPY ${FIRESTORE_DIR}/src/Fudie.Firestore.EntityFrameworkCore/Fudie.Firestore.EntityFrameworkCore.csproj ${FIRESTORE_DIR}/src/Fudie.Firestore.EntityFrameworkCore/

RUN dotnet restore ${PROJECT_DIR}/src/Menus/Menus.csproj --runtime linux-musl-x64

COPY ${PROJECT_DIR}/src/Menus/ ${PROJECT_DIR}/src/Menus/
COPY ${PROJECT_DIR}/src/Fudie/ ${PROJECT_DIR}/src/Fudie/
COPY ${PROJECT_DIR}/src/Fudie.Generator/ ${PROJECT_DIR}/src/Fudie.Generator/
COPY ${FIRESTORE_DIR}/src/Fudie.Firestore.EntityFrameworkCore/ ${FIRESTORE_DIR}/src/Fudie.Firestore.EntityFrameworkCore/

RUN dotnet publish ${PROJECT_DIR}/src/Menus/Menus.csproj -c Release -o /app \
    --runtime linux-musl-x64 \
    --self-contained false \
    --no-restore

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
ARG PROJECT_DIR
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app .
COPY --from=build /src/${PROJECT_DIR}/src/Menus/OpenApi ./OpenApi

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Menus.dll"]
