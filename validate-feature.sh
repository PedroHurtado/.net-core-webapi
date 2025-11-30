#!/bin/bash

# 🤖 Script de Validación Automática para Features
# Uso: ./validate-feature.sh [NombreEntidad]
# Ejemplo: ./validate-feature.sh Pedido

set -e  # Salir si hay errores

# Colores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
ENTITY=$1
ENTITY_LOWER=$(echo "$ENTITY" | tr '[:upper:]' '[:lower:]')
ENTITY_PLURAL="${ENTITY}s"  # Simplificado, ajustar si es necesario

if [ -z "$ENTITY" ]; then
    echo -e "${RED}❌ Error: Debes proporcionar el nombre de la entidad${NC}"
    echo "Uso: ./validate-feature.sh [NombreEntidad]"
    echo "Ejemplo: ./validate-feature.sh Pedido"
    exit 1
fi

echo -e "${BLUE}🔍 Validando feature: ${ENTITY}${NC}"
echo ""

# Función para verificar archivo
check_file() {
    local file=$1
    local description=$2
    
    if [ -f "$file" ]; then
        echo -e "${GREEN}✅ ${description}${NC}"
        return 0
    else
        echo -e "${RED}❌ ${description} - NO ENCONTRADO${NC}"
        echo "   Esperado: $file"
        return 1
    fi
}

# Función para ejecutar comando y verificar
run_command() {
    local description=$1
    shift
    local command="$@"
    
    echo -e "${YELLOW}⏳ ${description}...${NC}"
    
    if eval "$command" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ ${description}${NC}"
        return 0
    else
        echo -e "${RED}❌ ${description} - FALLÓ${NC}"
        echo "   Comando: $command"
        return 1
    fi
}

# Contador de errores
ERRORS=0

echo -e "${BLUE}📂 Verificando estructura de archivos...${NC}"
echo ""

# Dominio
check_file "src/webapi/features/${ENTITY_LOWER}/models/${ENTITY}.cs" "Clase de dominio" || ((ERRORS++))

# Queries
check_file "src/webapi/features/${ENTITY_LOWER}/queries/Get${ENTITY}.cs" "Query Get${ENTITY}" || ((ERRORS++))
check_file "src/webapi/features/${ENTITY_LOWER}/queries/Get${ENTITY_PLURAL}.cs" "Query Get${ENTITY_PLURAL}" || ((ERRORS++))

# Commands
check_file "src/webapi/features/${ENTITY_LOWER}/commands/Create${ENTITY}.cs" "Command Create${ENTITY}" || ((ERRORS++))
check_file "src/webapi/features/${ENTITY_LOWER}/commands/Update${ENTITY}.cs" "Command Update${ENTITY}" || ((ERRORS++))

# Persistencia
check_file "src/webapi/infrastructure/Configurations/${ENTITY}Configuration.cs" "Configuración EF Core" || ((ERRORS++))

# Tests Unitarios
check_file "tests/WebApi.UnitTests/Features/${ENTITY}/${ENTITY}Tests.cs" "Tests unitarios" || ((ERRORS++))

# Tests de Integración
check_file "tests/WebApi.IntegrationTests/Features/${ENTITY}/Create${ENTITY}Tests.cs" "Tests Create" || ((ERRORS++))
check_file "tests/WebApi.IntegrationTests/Features/${ENTITY}/Get${ENTITY}Tests.cs" "Tests Get por ID" || ((ERRORS++))
check_file "tests/WebApi.IntegrationTests/Features/${ENTITY}/Get${ENTITY_PLURAL}Tests.cs" "Tests Get lista" || ((ERRORS++))
check_file "tests/WebApi.IntegrationTests/Features/${ENTITY}/Update${ENTITY}Tests.cs" "Tests Update" || ((ERRORS++))

echo ""
echo -e "${BLUE}🔨 Verificando compilación...${NC}"
echo ""

# Limpiar y compilar
run_command "Limpieza de proyecto" "dotnet clean" || ((ERRORS++))
run_command "Compilación" "dotnet build" || ((ERRORS++))

echo ""
echo -e "${BLUE}🧪 Ejecutando tests...${NC}"
echo ""

# Tests unitarios
run_command "Tests unitarios de ${ENTITY}" "dotnet test tests/WebApi.UnitTests/Features/${ENTITY}/${ENTITY}Tests.cs" || ((ERRORS++))

# Tests de integración
run_command "Tests de integración de ${ENTITY}" "dotnet test --filter \"FullyQualifiedName~${ENTITY}\"" || ((ERRORS++))

# Todos los tests
run_command "Todos los tests del proyecto" "dotnet test" || ((ERRORS++))

echo ""
echo -e "${BLUE}🚀 Verificando aplicación...${NC}"
echo ""

# Verificar que la aplicación inicia (timeout de 10 segundos)
echo -e "${YELLOW}⏳ Iniciando aplicación (timeout 10s)...${NC}"
timeout 10s dotnet run --project src/webapi > /dev/null 2>&1 &
APP_PID=$!
sleep 5

if ps -p $APP_PID > /dev/null; then
    echo -e "${GREEN}✅ Aplicación inicia correctamente${NC}"
    kill $APP_PID 2>/dev/null || true
else
    echo -e "${RED}❌ Aplicación no inicia correctamente${NC}"
    ((ERRORS++))
fi

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

if [ $ERRORS -eq 0 ]; then
    echo -e "${GREEN}✅ ¡VALIDACIÓN EXITOSA!${NC}"
    echo -e "${GREEN}   Feature ${ENTITY} está completo y funcional${NC}"
    echo ""
    echo -e "${BLUE}📋 Próximos pasos:${NC}"
    echo "   1. Probar endpoints en Swagger: https://localhost:5001/swagger"
    echo "   2. Revisar código con Prompt 9 (AI_PROMPTS.md)"
    echo "   3. Hacer commit de los cambios"
    echo ""
    exit 0
else
    echo -e "${RED}❌ VALIDACIÓN FALLÓ${NC}"
    echo -e "${RED}   Se encontraron ${ERRORS} errores${NC}"
    echo ""
    echo -e "${YELLOW}📋 Acciones sugeridas:${NC}"
    echo "   1. Revisa los errores arriba"
    echo "   2. Corrige los problemas"
    echo "   3. Ejecuta nuevamente: ./validate-feature.sh ${ENTITY}"
    echo ""
    exit 1
fi
