# ensure-azurite.ps1
# Verifica si Azurite (emulador de Azure Blob Storage) está escuchando en el
# puerto 10000 y, si no, lo inicia en segundo plano. Reutilizado por
# start-dev.ps1 y por el build de la API (ProductApp.Api.csproj) para no
# depender de acordarse de levantarlo a mano.

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

$azuritePort = Get-NetTCPConnection -LocalPort 10000 -State Listen -ErrorAction SilentlyContinue

if ($azuritePort) {
    Write-Host "Azurite ya estaba corriendo." -ForegroundColor Green
    exit 0
}

Write-Host "Azurite no está corriendo. Iniciando..." -ForegroundColor Yellow

$azuriteDir = Join-Path $root ".azurite"
New-Item -ItemType Directory -Force -Path $azuriteDir | Out-Null

try {
    # "azurite" a secas resuelve ambiguo: Get-Command lo hace matchear con el
    # wrapper azurite.ps1 (que Start-Process no puede lanzar como proceso) en
    # vez del azurite.cmd real. Se fuerza la extensión .cmd para desambiguar.
    $azuriteCmd = (Get-Command azurite.cmd -ErrorAction Stop).Source

    Start-Process -FilePath $azuriteCmd `
        -ArgumentList "--silent", "--location", $azuriteDir, "--debug", (Join-Path $azuriteDir "debug.log") `
        -WindowStyle Hidden
} catch {
    Write-Warning "No se pudo iniciar Azurite. Verificá que esté instalado (npm i -g azurite)."
    exit 0
}

# Sondeo corto en vez de un solo sleep fijo: el tiempo de arranque de
# Azurite varía (arranque en frío vs. con datos ya cargados).
$arriba = $false
for ($intento = 0; $intento -lt 10; $intento++) {
    Start-Sleep -Seconds 1
    if (Get-NetTCPConnection -LocalPort 10000 -State Listen -ErrorAction SilentlyContinue) {
        $arriba = $true
        break
    }
}

if ($arriba) {
    Write-Host "Azurite arriba en el puerto 10000." -ForegroundColor Green
} else {
    Write-Warning "Azurite no respondió en el puerto 10000 tras 10s."
}
