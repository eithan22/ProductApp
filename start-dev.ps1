# start-dev.ps1
# Levanta el entorno de desarrollo completo: Azurite (emulador de Azure
# Blob Storage), la API y la Web — en ese orden, cada uno en su propia
# ventana. Azurite solo se inicia si todavía no está corriendo.
#
# Uso (desde la raíz del repo):
#   .\start-dev.ps1

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# ---------- Azurite ----------

$azuritePort = Get-NetTCPConnection -LocalPort 10000 -State Listen -ErrorAction SilentlyContinue

if (-not $azuritePort) {
    Write-Host "Azurite no está corriendo. Iniciando..." -ForegroundColor Yellow

    $azuriteDir = Join-Path $root ".azurite"
    New-Item -ItemType Directory -Force -Path $azuriteDir | Out-Null

    Start-Process -FilePath "azurite" `
        -ArgumentList "--silent", "--location", $azuriteDir, "--debug", (Join-Path $azuriteDir "debug.log") `
        -WindowStyle Hidden

    Start-Sleep -Seconds 2

    if (Get-NetTCPConnection -LocalPort 10000 -State Listen -ErrorAction SilentlyContinue) {
        Write-Host "Azurite arriba en el puerto 10000." -ForegroundColor Green
    } else {
        Write-Warning "Azurite no respondió en el puerto 10000. Verificá que esté instalado (npm i -g azurite)."
    }
} else {
    Write-Host "Azurite ya estaba corriendo." -ForegroundColor Green
}

# ---------- API ----------

Write-Host "Iniciando API (https://localhost:7197)..." -ForegroundColor Cyan
Start-Process -FilePath "dotnet" `
    -ArgumentList "run", "--project", "ProductApp/ProductApp.Api.csproj", "--launch-profile", "https" `
    -WorkingDirectory $root

# ---------- Web ----------

Write-Host "Iniciando Web (https://localhost:7141)..." -ForegroundColor Cyan
Start-Process -FilePath "dotnet" `
    -ArgumentList "run", "--project", "Web/Web.csproj", "--launch-profile", "https" `
    -WorkingDirectory $root

Write-Host "`nListo. Azurite, API y Web arrancando cada uno en su ventana." -ForegroundColor Green
Write-Host "Cerrá las ventanas de la API y la Web para detenerlas; Azurite queda corriendo en segundo plano." -ForegroundColor DarkGray
