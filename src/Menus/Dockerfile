# --- Build stage ---
ARG PROJECT_DIR=webapi
ARG FIRESTORE_DIR=Firestore

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
ARG PROJECT_DIR
ARG FIRESTORE_DIR
WORKDIR /src

COPY ${PROJECT_DIR}/Directory.Build.props webapi/
COPY ${PROJECT_DIR}/src/Menus/Menus.csproj webapi/src/Menus/
COPY ${PROJECT_DIR}/src/Fudie/Fudie.csproj webapi/src/Fudie/
COPY ${PROJECT_DIR}/src/Fudie.Generator/Fudie.Generator.csproj webapi/src/Fudie.Generator/
COPY ${FIRESTORE_DIR}/src/Fudie.Firestore.EntityFrameworkCore/Fudie.Firestore.EntityFrameworkCore.csproj Firestore/src/Fudie.Firestore.EntityFrameworkCore/

RUN dotnet restore webapi/src/Menus/Menus.csproj

COPY ${PROJECT_DIR}/src/Menus/ webapi/src/Menus/
COPY ${PROJECT_DIR}/src/Fudie/ webapi/src/Fudie/
COPY ${PROJECT_DIR}/src/Fudie.Generator/ webapi/src/Fudie.Generator/
COPY ${FIRESTORE_DIR}/src/Fudie.Firestore.EntityFrameworkCore/ Firestore/src/Fudie.Firestore.EntityFrameworkCore/

RUN dotnet publish webapi/src/Menus/Menus.csproj -c Release -o /app

# --- Runtime stage ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

COPY --from=build /app .
COPY --from=build /src/webapi/src/Menus/OpenApi ./OpenApi

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Menus.dll"]
