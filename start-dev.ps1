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

& (Join-Path $root "scripts\ensure-azurite.ps1")

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
