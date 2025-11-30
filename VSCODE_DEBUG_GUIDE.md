# 🚀 Guía Definitiva - VS Code Debug & Tasks

Esta configuración usa `settings.json` para definir en qué proyecto estás trabajando, evitando preguntas repetitivas y permitiendo depuración robusta.

---

## ⚙️ Configuración Inicial (Solo una vez por proyecto)

Edita el archivo `.vscode/settings.json` para definir tu contexto actual:

```json
{
    "active.apiProjectName": "webapi",
    "active.testProjectName": "WebApi.IntegrationTests"
}
```

*Cuando cambies de microservicio, solo actualiza este archivo.*

---

## 🐛 Depuración (Debug)

### 1. Depurar la API
**Atajo:** `F5` (Selecciona "🚀 Debug Active API")

*   Compila y lanza la API definida en `settings.json`.

### 2. Depurar Tests (Alternativa Robusta a CodeLens)
**Atajo:** `F5` (Selecciona "🧪 Debug Active Tests")

*   Si CodeLens ("Debug Test" sobre el código) te falla con "No se pudo iniciar el depurador", usa esta opción.
*   Ejecuta y depura el proyecto de tests definido en `settings.json`.
*   **Nota:** Esto ejecutará todos los tests del proyecto. Para filtrar uno solo, puedes agregar temporalmente `/TestCaseFilter:NombreTest` en `launch.json` o usar `[Fact(Skip="...")]` en los demás.

---

## ⚙️ Tareas (Tasks)

**Atajo:** `Ctrl+Shift+P` → "Run Task"

*   **`build-active-api`**: Compila la API actual.
*   **`build-active-tests`**: Compila los tests actuales.
*   **`build-solution`**: Compila todo (`Ctrl+Shift+B`).

---

## 💡 Solución de Problemas

**CodeLens falla con "No se pudo iniciar el depurador: {0}"**
*   Este es un error conocido de la extensión de C# en algunos entornos.
*   **Solución:** Usa la configuración **"🧪 Debug Active Tests"** desde el menú de depuración (F5) en su lugar. Es más fiable porque invocamos el depurador directamente.

**Error "No se encuentra vstest.console.dll"**
*   Verifica la ruta en `launch.json`. Actualmente apunta a: `C:\Program Files\dotnet\sdk\6.0.404\vstest.console.dll`. Si actualizas tu SDK, tendrás que ajustar esto.
