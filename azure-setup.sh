#!/bin/bash

# AudiSoft School - Azure Setup Script
# Automatiza la creación de recursos en Azure

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== AudiSoft School - Azure Setup ===${NC}\n"

# Variables
RESOURCE_GROUP="rg-audisoft-school"
LOCATION="eastus2"
RANDOM_SUFFIX=$(date +%s)
SQL_SERVER_NAME="servidor-audisoft-${RANDOM_SUFFIX}"
SQL_DB_NAME="AudiSoftSchoolDb"
SQL_ADMIN_USER="adminuser"
BACKEND_APP_SERVICE="app-audisoft-api"
FRONTEND_APP_SERVICE="app-audisoft-web"
APP_SERVICE_PLAN_BACKEND="plan-audisoft-api"
APP_SERVICE_PLAN_FRONTEND="plan-audisoft-web"

# Read from user or use defaults
read -p "Ingresa el nombre del resource group (default: $RESOURCE_GROUP): " input
RESOURCE_GROUP=${input:-$RESOURCE_GROUP}

read -p "Ingresa la región de Azure (default: eastus2): " input
LOCATION=${input:-eastus2}

read -p "Ingresa la contraseña SQL (min 12 chars, mayús, minús, números, símbolos): " SQL_ADMIN_PASSWORD
if [ ${#SQL_ADMIN_PASSWORD} -lt 12 ]; then
    echo -e "${RED}❌ La contraseña debe tener al menos 12 caracteres${NC}"
    exit 1
fi

echo -e "\n${BLUE}Autenticando en Azure...${NC}"
az login

SUBSCRIPTION_ID=$(az account show --query id -o tsv)
echo -e "${GREEN}✅ Suscripción: $SUBSCRIPTION_ID${NC}\n"

# 1. Create Resource Group
echo -e "${BLUE}1. Creando Resource Group: $RESOURCE_GROUP${NC}"
az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --output table

# 2. Create App Service Plans
echo -e "\n${BLUE}2. Creando App Service Plans${NC}"

echo "   → Plan para Backend (Basic B1)..."
az appservice plan create \
  --name "$APP_SERVICE_PLAN_BACKEND" \
  --resource-group "$RESOURCE_GROUP" \
  --sku B1 \
  --is-linux \
  --output table

echo "   → Plan para Frontend (Basic B1)..."
az appservice plan create \
  --name "$APP_SERVICE_PLAN_FRONTEND" \
  --resource-group "$RESOURCE_GROUP" \
  --sku B1 \
  --is-linux \
  --output table

# 3. Create SQL Server and Database
echo -e "\n${BLUE}3. Creando SQL Server y Base de Datos${NC}"

echo "   → SQL Server: $SQL_SERVER_NAME..."
if ! az sql server create \
  --name "$SQL_SERVER_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD" \
  --output table; then
    echo -e "${RED}❌ Error creando SQL Server. Verifica:${NC}"
    echo -e "   - Contraseña: debe tener mayús, minús, números, símbolos"
    echo -e "   - El nombre del servidor debe ser único globalmente"
    echo -e "   - Cuota de recursos disponible en la región"
    exit 1
fi

echo "   → Configurando Firewall (permitir servicios de Azure)..."
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output table

echo "   → Base de Datos: $SQL_DB_NAME (Basic)..."
az sql db create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DB_NAME" \
  --edition Basic \
  --collation SQL_Latin1_General_CP1_CI_AS \
  --output table

SQL_SERVER_FQDN=$(az sql server show \
  --name "$SQL_SERVER_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query fullyQualifiedDomainName \
  --output tsv)

# 4. Create Backend App Service
echo -e "\n${BLUE}4. Creando Backend App Service${NC}"
az webapp create \
  --name "$BACKEND_APP_SERVICE" \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_SERVICE_PLAN_BACKEND" \
  --runtime "DOTNETCORE:8.0" \
  --output table

BACKEND_URL="https://${BACKEND_APP_SERVICE}.azurewebsites.net"

# 5. Create Frontend App Service
echo -e "\n${BLUE}5. Creando Frontend App Service${NC}"
az webapp create \
  --name "$FRONTEND_APP_SERVICE" \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_SERVICE_PLAN_FRONTEND" \
  --runtime "NODE:20-lts" \
  --output table

FRONTEND_URL="https://${FRONTEND_APP_SERVICE}.azurewebsites.net"

# 6. Configure Backend App Settings
echo -e "\n${BLUE}6. Configurando App Settings del Backend${NC}"

CONNECTION_STRING="Server=tcp:${SQL_SERVER_FQDN},1433;Initial Catalog=${SQL_DB_NAME};Persist Security Info=False;User ID=${SQL_ADMIN_USER};Password=${SQL_ADMIN_PASSWORD};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

az webapp config connection-string set \
  --name "$BACKEND_APP_SERVICE" \
  --resource-group "$RESOURCE_GROUP" \
  --settings DefaultConnection="$CONNECTION_STRING" \
  --connection-string-type SQLAzure \
  --output table

az webapp config appsettings set \
  --name "$BACKEND_APP_SERVICE" \
  --resource-group "$RESOURCE_GROUP" \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_URLS="http://+:80" \
  --output table

# 7. Configure Frontend App Settings
echo -e "\n${BLUE}7. Configurando App Settings del Frontend${NC}"

az webapp config appsettings set \
  --name "$FRONTEND_APP_SERVICE" \
  --resource-group "$RESOURCE_GROUP" \
  --settings \
    API_BASE_URL_PRODUCTION="${BACKEND_URL}/api/v1" \
    DEBUG_MODE=false \
    LOG_LEVEL=info \
  --output table

# 8. Create Service Principal for CI/CD
echo -e "\n${BLUE}8. Creando Service Principal para GitHub Actions${NC}"

SERVICE_PRINCIPAL_JSON=$(az ad sp create-for-rbac \
  --name "AudiSoftDeployment" \
  --role Contributor \
  --scopes "/subscriptions/${SUBSCRIPTION_ID}" \
  --sdk-auth)

echo -e "\n${GREEN}✅ Service Principal creado${NC}"

# Output Summary
echo -e "\n${GREEN}=== RESUMEN DE CONFIGURACIÓN ===${NC}"
echo -e "${BLUE}Resource Group:${NC} $RESOURCE_GROUP"
echo -e "${BLUE}Región:${NC} $LOCATION"
echo -e "${BLUE}SQL Server:${NC} $SQL_SERVER_FQDN"
echo -e "${BLUE}Base de Datos:${NC} $SQL_DB_NAME"
echo -e "${BLUE}Backend URL:${NC} $BACKEND_URL"
echo -e "${BLUE}Frontend URL:${NC} $FRONTEND_URL"

echo -e "\n${BLUE}=== SECRETOS PARA GITHUB ===${NC}"
echo -e "Agrega los siguientes secrets en GitHub (Repo > Settings > Secrets > Actions):\n"

echo -e "${BLUE}AZURE_CREDENTIALS:${NC}"
echo "$SERVICE_PRINCIPAL_JSON"

echo -e "\n${BLUE}AZURE_SUBSCRIPTION_ID:${NC}"
echo "$SUBSCRIPTION_ID"

echo -e "\n${BLUE}AZURE_BACKEND_RESOURCE_GROUP:${NC}"
echo "$RESOURCE_GROUP"

echo -e "\n${BLUE}AZURE_BACKEND_APP_NAME:${NC}"
echo "$BACKEND_APP_SERVICE"

echo -e "\n${BLUE}AZURE_FRONTEND_RESOURCE_GROUP:${NC}"
echo "$RESOURCE_GROUP"

echo -e "\n${BLUE}AZURE_FRONTEND_APP_NAME:${NC}"
echo "$FRONTEND_APP_SERVICE"

echo -e "\n${BLUE}FRONTEND_API_BASE_URL:${NC}"
echo "${BACKEND_URL}/api/v1"

echo -e "\n${GREEN}✅ Setup completado${NC}\n"
