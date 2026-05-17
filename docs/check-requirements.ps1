# ReunionWelcomeApp - Verificación de Requisitos
# Ejecutar en PowerShell: .\check-requirements.ps1

$ErrorActionPreference = "Continue"
$appPath = $PSScriptRoot

Write-Host "`n=== Verificación de Requisitos - ReunionWelcomeApp ===" -ForegroundColor Cyan
Write-Host ""

$issues = @()
$warnings = @()

# 1. Sistema Operativo
Write-Host "[1/6] Verificando Sistema Operativo..." -ForegroundColor Yellow
$os = Get-CimInstance Win32_OperatingSystem
$osVersion = [System.Version]$os.Version
$build = [int]$os.BuildNumber

if ($osVersion.Major -ge 10 -and $build -ge 19041) {
    Write-Host "  OK: Windows $($osVersion.Major).$($osVersion.Minor) Build $build" -ForegroundColor Green
} elseif ($osVersion.Major -eq 10 -and $build -ge 18362) {
    Write-Host "  OK: Windows 10 Build $build" -ForegroundColor Green
} else {
    $issues += "Windows 10/11 requerido (actual: $($os.Caption))"
    Write-Host "  ERROR: Windows 10/11 requerido" -ForegroundColor Red
}

# 2. Arquitectura
Write-Host "[2/6] Verificando Arquitectura..." -ForegroundColor Yellow
$arch = $env:PROCESSOR_ARCHITECTURE
if ($arch -eq "AMD64") {
    Write-Host "  OK: Arquitectura 64-bit (AMD64)" -ForegroundColor Green
} else {
    $issues += "Sistema 64-bit requerido (actual: $arch)"
    Write-Host "  ERROR: Se requiere 64-bit" -ForegroundColor Red
}

# 3. Memoria RAM
Write-Host "[3/6] Verificando Memoria RAM..." -ForegroundColor Yellow
$totalRam = [math]::Round($os.TotalVisibleMemorySize / 1MB, 2)
if ($totalRam -ge 2) {
    Write-Host "  OK: $totalRam GB RAM" -ForegroundColor Green
} else {
    $issues += "Mínimo 2GB RAM requerido (actual: ${totalRam}GB)"
    Write-Host "  ERROR: Mínimo 2GB requerido" -ForegroundColor Red
}

# 4. Espacio en Disco
Write-Host "[4/6] Verificando Espacio en Disco..." -ForegroundColor Yellow
$drive = Split-Path $appPath -Qualifier
$disk = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='$drive'"
$freeSpace = [math]::Round($disk.FreeSpace / 1GB, 2)
if ($freeSpace -ge 0.2) {
    Write-Host "  OK: $freeSpace GB disponible en unidad $drive" -ForegroundColor Green
} else {
    $issues += "Mínimo 200MB requerido (actual: ${freeSpace}GB)"
    Write-Host "  ERROR: Espacio insuficiente" -ForegroundColor Red
}

# 5. Archivos Requeridos
Write-Host "[5/6] Verificando Archivos Requeridos..." -ForegroundColor Yellow
$requiredFiles = @(
    "Resources\config.json",
    "Resources\Sounds\start.wav",
    "Resources\Sounds\typing.wav",
    "Resources\Sounds\confirm.wav",
    "Resources\Images\Impsat ThinkAhead1.jpg",
    "Resources\Images\Impsat-FondoBlanco.jpg"
)

$missingFiles = @()
foreach ($file in $requiredFiles) {
    $fullPath = Join-Path $appPath $file
    if (Test-Path $fullPath) {
        Write-Host "  OK: $file" -ForegroundColor Green
    } else {
        $missingFiles += $file
        Write-Host "  FALTA: $file" -ForegroundColor Red
    }
}

if ($missingFiles.Count -gt 0) {
    $issues += "Faltan archivos: $($missingFiles -join ', ')"
}

# 6. Permisos de Escritura
Write-Host "[6/6] Verificando Permisos de Escritura..." -ForegroundColor Yellow
$testDirs = @("data", "logs")
foreach ($dir in $testDirs) {
    $dirPath = Join-Path $appPath $dir
    if (Test-Path $dirPath) {
        $canWrite = $false
        try {
            $testFile = Join-Path $dirPath "test_$(Get-Random).tmp"
            [System.IO.File]::WriteAllText($testFile, "test")
            Remove-Item $testFile -Force
            $canWrite = $true
        } catch {}
        
        if ($canWrite) {
            Write-Host "  OK: $dir/ escribible" -ForegroundColor Green
        } else {
            $issues += "Sin permisos de escritura en $dir/"
            Write-Host "  ERROR: $dir/ sin permisos" -ForegroundColor Red
        }
    } else {
        try {
            New-Item -ItemType Directory -Path $dirPath -Force | Out-Null
            Write-Host "  OK: $dir/ creado" -ForegroundColor Green
        } catch {
            $issues += "No se puede crear $dir/"
            Write-Host "  ERROR: No se puede crear $dir/" -ForegroundColor Red
        }
    }
}

# Resumen
Write-Host "`n=== RESULTADO ===" -ForegroundColor Cyan
if ($issues.Count -eq 0) {
    Write-Host "TODOS LOS REQUISITOS CUMPLIDOS" -ForegroundColor Green
    Write-Host "La aplicación debería funcionar correctamente." -ForegroundColor Green
    exit 0
} else {
    Write-Host "SE ENCONTRARON $($issues.Count) PROBLEMA(S):" -ForegroundColor Red
    foreach ($issue in $issues) {
        Write-Host "  - $issue" -ForegroundColor Red
    }
    Write-Host "`nPor favor, resuelva los problemas antes de ejecutar la aplicación." -ForegroundColor Yellow
    exit 1
}