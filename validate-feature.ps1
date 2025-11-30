# 🤖 Script de Validación Automática para Features (PowerShell)
# Uso: .\validate-feature.ps1 -Entity "Pedido"

param(
    [Parameter(Mandatory=$true)]
    [string]$Entity
)

$ErrorActionPreference = "Continue"

# Variables
$EntityLower = $Entity.ToLower()
$EntityPlural = "${Entity}s"  # Simplificado, ajustar si es necesario
$Errors = 0

# Colores
function Write-Success { Write-Host "✅ $args" -ForegroundColor Green }
function Write-Error-Custom { Write-Host "❌ $args" -ForegroundColor Red }
function Write-Info { Write-Host "ℹ️  $args" -ForegroundColor Blue }
function Write-Warning-Custom { Write-Host "⏳ $args" -ForegroundColor Yellow }

Write-Info "🔍 Validando feature: $Entity"
Write-Host ""

# Función para verificar archivo
function Test-FeatureFile {
    param(
        [string]$FilePath,
        [string]$Description
    )
    
    if (Test-Path $FilePath) {
        Write-Success "$Description"
        return $true
    } else {
        Write-Error-Custom "$Description - NO ENCONTRADO"
        Write-Host "   Esperado: $FilePath" -ForegroundColor Gray
        $script:Errors++
        return $false
    }
}

# Función para ejecutar comando
function Invoke-ValidationCommand {
    param(
        [string]$Description,
        [scriptblock]$Command
    )
    
    Write-Warning-Custom "$Description..."
    
    try {
        $output = & $Command 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "$Description"
            return $true
        } else {
            Write-Error-Custom "$Description - FALLÓ"
            $script:Errors++
            return $false
        }
    } catch {
        Write-Error-Custom "$Description - FALLÓ"
        Write-Host "   Error: $_" -ForegroundColor Gray
        $script:Errors++
        return $false
    }
}

Write-Info "📂 Verificando estructura de archivos..."
Write-Host ""

# Dominio
Test-FeatureFile "src\webapi\features\$EntityLower\models\$Entity.cs" "Clase de dominio"

# Queries
Test-FeatureFile "src\webapi\features\$EntityLower\queries\Get$Entity.cs" "Query Get$Entity"
Test-FeatureFile "src\webapi\features\$EntityLower\queries\Get$EntityPlural.cs" "Query Get$EntityPlural"

# Commands
Test-FeatureFile "src\webapi\features\$EntityLower\commands\Create$Entity.cs" "Command Create$Entity"
Test-FeatureFile "src\webapi\features\$EntityLower\commands\Update$Entity.cs" "Command Update$Entity"

# Persistencia
Test-FeatureFile "src\webapi\infrastructure\Configurations\${Entity}Configuration.cs" "Configuración EF Core"

# Tests Unitarios
Test-FeatureFile "tests\WebApi.UnitTests\Features\$Entity\${Entity}Tests.cs" "Tests unitarios"

# Tests de Integración
Test-FeatureFile "tests\WebApi.IntegrationTests\Features\$Entity\Create${Entity}Tests.cs" "Tests Create"
Test-FeatureFile "tests\WebApi.IntegrationTests\Features\$Entity\Get${Entity}Tests.cs" "Tests Get por ID"
Test-FeatureFile "tests\WebApi.IntegrationTests\Features\$Entity\Get${EntityPlural}Tests.cs" "Tests Get lista"
Test-FeatureFile "tests\WebApi.IntegrationTests\Features\$Entity\Update${Entity}Tests.cs" "Tests Update"

Write-Host ""
Write-Info "🔨 Verificando compilación..."
Write-Host ""

# Limpiar y compilar
Invoke-ValidationCommand "Limpieza de proyecto" { dotnet clean }
Invoke-ValidationCommand "Compilación" { dotnet build }

Write-Host ""
Write-Info "🧪 Ejecutando tests..."
Write-Host ""

# Tests unitarios
Invoke-ValidationCommand "Tests unitarios de $Entity" { 
    dotnet test "tests\WebApi.UnitTests\Features\$Entity\${Entity}Tests.cs" 
}

# Tests de integración
Invoke-ValidationCommand "Tests de integración de $Entity" { 
    dotnet test --filter "FullyQualifiedName~$Entity" 
}

# Todos los tests
Invoke-ValidationCommand "Todos los tests del proyecto" { dotnet test }

Write-Host ""
Write-Info "🚀 Verificando aplicación..."
Write-Host ""

# Verificar que la aplicación inicia
Write-Warning-Custom "Iniciando aplicación (timeout 10s)..."
$appJob = Start-Job -ScriptBlock { 
    dotnet run --project src\webapi 
}

Start-Sleep -Seconds 5

if ($appJob.State -eq "Running") {
    Write-Success "Aplicación inicia correctamente"
    Stop-Job $appJob
    Remove-Job $appJob
} else {
    Write-Error-Custom "Aplicación no inicia correctamente"
    $Errors++
    Remove-Job $appJob
}

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Gray
Write-Host ""

if ($Errors -eq 0) {
    Write-Success "¡VALIDACIÓN EXITOSA!"
    Write-Success "   Feature $Entity está completo y funcional"
    Write-Host ""
    Write-Info "📋 Próximos pasos:"
    Write-Host "   1. Probar endpoints en Swagger: https://localhost:5001/swagger"
    Write-Host "   2. Revisar código con Prompt 9 (AI_PROMPTS.md)"
    Write-Host "   3. Hacer commit de los cambios"
    Write-Host ""
    exit 0
} else {
    Write-Error-Custom "VALIDACIÓN FALLÓ"
    Write-Error-Custom "   Se encontraron $Errors errores"
    Write-Host ""
    Write-Warning-Custom "📋 Acciones sugeridas:"
    Write-Host "   1. Revisa los errores arriba"
    Write-Host "   2. Corrige los problemas"
    Write-Host "   3. Ejecuta nuevamente: .\validate-feature.ps1 -Entity $Entity"
    Write-Host ""
    exit 1
}
