# Fudie Backend

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

## Project Structure

```
parent/
├── webapi/          # This repository (can be cloned with any name)
│   ├── src/
│   ├── docker-compose.yml
│   └── .env
└── Firestore/       # Must be a sibling directory
```

## Setup

### 1. Clone both repositories as sibling directories

```bash
git clone <webapi-repo-url>
git clone <firestore-repo-url>
```

### 2. Configure `.env`

Copy or edit the `.env` file in the root of this project.

**`PROJECT_DIR`** — Name of the folder where you cloned this repository. Default: `webapi`.

```env
# If you cloned as "webapi" (default), no change needed
PROJECT_DIR=webapi

# If you cloned with a different name, e.g.:
# PROJECT_DIR=net-core-webapi
```

**`USER_SECRETS_PATH`** — Path to .NET User Secrets on your machine.

| OS            | Value                                      |
|---------------|--------------------------------------------|
| macOS / Linux | `~/.microsoft/usersecrets` (default, no change needed) |
| Windows       | `${APPDATA}/Microsoft/UserSecrets`         |

```env
# macOS/Linux — works without setting this variable

# Windows — uncomment this line:
# USER_SECRETS_PATH=${APPDATA}/Microsoft/UserSecrets
```

### 3. Run

```bash
docker compose up
```

The gateway is available at `http://localhost:5176`.

## Environments

| Environment | URL                            |
|-------------|--------------------------------|
| Development | http://localhost:5176           |
| Staging     | https://api-staging.fudie.com  |
| Production  | https://api.fudie.com          |
