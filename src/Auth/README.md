# Auth - Gestión de Secretos

Este proyecto usa **git-crypt** para almacenar secretos OAuth de Google encriptados en el repositorio. Al hacer `git pull`, los archivos se desencriptan automáticamente en local.

## Archivos encriptados

| Archivo | Contenido |
|---------|-----------|
| `src/Auth/google-oauth.json` | Credenciales OAuth de Google |

## Requisitos previos

### Instalar git-crypt

**Windows**
```bash
scoop install git-crypt
```

**macOS**
```bash
brew install git-crypt
```

**Linux (Debian/Ubuntu)**
```bash
sudo apt install git-crypt
```

## Setup inicial (solo una vez por máquina)

### 1. Obtener la clave simétrica

Solicita el archivo `git-crypt-key` a un compañero del equipo. Este archivo se comparte por canal seguro (Slack DM, email directo, etc.). **Nunca se sube al repositorio.**

### 2. Desbloquear el repositorio

```bash
git-crypt unlock /ruta/al/git-crypt-key
```

A partir de este momento, `git pull` y `git push` encriptan y desencriptan automáticamente. No necesitas hacer nada más.

### 3. Configurar user-secrets

Ejecuta la herramienta de setup que genera las claves JWT y carga las credenciales OAuth en user-secrets:

```bash
dotnet run --project tools/SetupDevCerts
```

Esto configura:
- `Jwt:PrivateKey` - Clave privada ES256 para firmar tokens
- `Jwt:Kid` - Key ID del JWT
- `Google:Oauth` - Credenciales OAuth de Google (leídas de `google-oauth.json`)

## Verificar que funciona

```bash
# Ver solo archivos encriptados
git-crypt status -e

# Verificar user-secrets
dotnet user-secrets list --project src/Auth
```

## Cómo funciona internamente

git-crypt usa filtros de Git (`clean`/`smudge`) definidos en `.gitattributes`:

```
src/Auth/google-oauth.json filter=git-crypt diff=git-crypt
```

- **Al hacer commit/push**: el filtro `clean` encripta el archivo antes de subirlo
- **Al hacer checkout/pull**: el filtro `smudge` desencripta el archivo en tu working directory
- **En GitHub**: el archivo aparece como binario ilegible
- **En local**: el archivo aparece en texto plano

## Preguntas frecuentes

### ¿Qué pasa si clono sin hacer unlock?

`google-oauth.json` aparecerá como un archivo binario ilegible. Ejecuta `git-crypt unlock` con la clave y se desencriptará.

### ¿Necesito hacer algo especial en cada pull/push?

No. Una vez hecho el `git-crypt unlock`, todo es transparente.

### ¿Puedo ver si un archivo está encriptado?

```bash
git-crypt status -e
```

### ¿Cómo añado un nuevo archivo encriptado?

Añade una línea en `.gitattributes`:

```
ruta/al/archivo.json filter=git-crypt diff=git-crypt
```
