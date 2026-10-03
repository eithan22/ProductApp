<#
.SYNOPSIS
    Crea los recursos de Azure para una instancia nueva de ProductApp (single-tenant).

.DESCRIPTION
    Punto de partida para automatizar el alta de un cliente, no una solución llave en
    mano: crea los recursos vacíos (resource group, SQL Database, Storage Account, los
    dos App Services) y deja las cadenas de conexión configuradas. Después de correrlo
    todavía hay que:
      - Generar y configurar Jwt:Key y las variables Seed:* (ver DEPLOYMENT.md)
      - Desplegar el código a los dos App Services (manual o con un pipeline de CI/CD
        propio para este cliente)

.PARAMETER Cliente
    Nombre corto del cliente, usado como prefijo de todos los recursos (ej. "acme").
    Debe ser válido como parte de un nombre de dominio (minúsculas, sin espacios).

.PARAMETER SqlAdminUser
.PARAMETER SqlAdminPassword
    Credenciales del administrador de la SQL Database que se crea.

.EXAMPLE
    ./alta-cliente.ps1 -Cliente "acme" -SqlAdminUser "sqladmin" -SqlAdminPassword (Read-Host -AsSecureString)
#>
param(
    [Parameter(Mandatory)] [string]$Cliente,
    [Parameter(Mandatory)] [string]$SqlAdminUser,
    [Parameter(Mandatory)] [SecureString]$SqlAdminPassword,
    [string]$Location = "eastus"
)

$ErrorActionPreference = "Stop"

$resourceGroup = "rg-productapp-$Cliente"
$appServicePlan = "asp-productapp-$Cliente"
$apiAppName = "productapp-$Cliente-api"
$webAppName = "productapp-$Cliente-web"
$sqlServerName = "sql-productapp-$Cliente"
$sqlDbName = "productapp-$Cliente"
$storageAccountName = ("stproductapp$Cliente" -replace '[^a-z0-9]', '').Substring(0, [Math]::Min(24, ("stproductapp$Cliente").Length))

$sqlAdminPasswordPlain = [System.Net.NetworkCredential]::new("", $SqlAdminPassword).Password

Write-Host "Creando resource group $resourceGroup..."
az group create --name $resourceGroup --location $Location | Out-Null

Write-Host "Creando App Service Plan $appServicePlan..."
az appservice plan create --name $appServicePlan --resource-group $resourceGroup --sku B1 --is-linux | Out-Null

Write-Host "Creando App Service de la API ($apiAppName)..."
az webapp create --name $apiAppName --resource-group $resourceGroup --plan $appServicePlan --runtime "DOTNETCORE:10.0" | Out-Null

Write-Host "Creando App Service de la Web ($webAppName)..."
az webapp create --name $webAppName --resource-group $resourceGroup --plan $appServicePlan --runtime "DOTNETCORE:10.0" | Out-Null

Write-Host "Creando SQL Server $sqlServerName y base $sqlDbName..."
az sql server create --name $sqlServerName --resource-group $resourceGroup --location $Location `
    --admin-user $SqlAdminUser --admin-password $sqlAdminPasswordPlain | Out-Null
az sql server firewall-rule create --resource-group $resourceGroup --server $sqlServerName `
    --name "AllowAzureServices" --start-ip-address "0.0.0.0" --end-ip-address "0.0.0.0" | Out-Null
az sql db create --resource-group $resourceGroup --server $sqlServerName --name $sqlDbName --service-objective Basic | Out-Null

Write-Host "Creando Storage Account $storageAccountName y contenedores..."
az storage account create --name $storageAccountName --resource-group $resourceGroup --location $Location --sku Standard_LRS | Out-Null
$storageKey = az storage account keys list --account-name $storageAccountName --resource-group $resourceGroup --query "[0].value" -o tsv
az storage container create --name "productos-imagenes" --account-name $storageAccountName --account-key $storageKey --public-access off | Out-Null
az storage container create --name "facturas" --account-name $storageAccountName --account-key $storageKey --public-access off | Out-Null

$sqlConnectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Database=$sqlDbName;User ID=$SqlAdminUser;Password=$sqlAdminPasswordPlain;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
$storageConnectionString = az storage account show-connection-string --name $storageAccountName --resource-group $resourceGroup --query connectionString -o tsv

Write-Host "Configurando variables del App Service de la API..."
az webapp config appsettings set --name $apiAppName --resource-group $resourceGroup --settings `
    "ConnectionStrings__DefaultConnection=$sqlConnectionString" `
    "ConnectionStrings__AzureStorage=$storageConnectionString" `
    "Jwt__Issuer=ProductApp" `
    "Jwt__Audience=ProductAppUsers" | Out-Null

Write-Host "Configurando variables del App Service Web..."
az webapp config appsettings set --name $webAppName --resource-group $resourceGroup --settings `
    "ApiSettings__BaseUrl=https://$apiAppName.azurewebsites.net/api/" | Out-Null

Write-Host ""
Write-Host "Recursos creados. Pendiente manual (ver DEPLOYMENT.md):"
Write-Host "  - Configurar Jwt__Key en $apiAppName (secreto, no lo genera este script)"
Write-Host "  - Configurar Seed__AdminUsername / Seed__AdminEmail / Seed__AdminPassword en $apiAppName"
Write-Host "  - Desplegar el código a $apiAppName y $webAppName"
