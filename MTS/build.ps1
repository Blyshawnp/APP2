<#
.SYNOPSIS
    Build, publish, and package Mock Testing Suite as a Windows installer.

.DESCRIPTION
    1. Runs dotnet publish (self-contained single-file, win-x64)
    2. Optionally compiles an Inno Setup installer (.exe)

.PARAMETER Version
    Semantic version to stamp on the build (default: 1.0.0).

.PARAMETER NoInstaller
    Skip the Inno Setup step and only produce the published exe.

.PARAMETER SkipTests
    Skip running the test suite before publishing.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -Version "2.1.0"
    .\build.ps1 -NoInstaller
    .\build.ps1 -Version "1.2.0" -SkipTests
#>

param(
    [string] $Version     = "1.0.0",
    [switch] $NoInstaller,
    [switch] $SkipTests
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$Root       = $PSScriptRoot
$PublishDir = Join-Path $Root "publish\win-x64"
$DistDir    = Join-Path $Root "dist"
$SolutionFile = Join-Path $Root "MTS.sln"

# ============================================================
# Helpers
# ============================================================

function Write-Step([string]$msg) {
    Write-Host "`n=== $msg ===" -ForegroundColor Cyan
}

function Assert-Dotnet {
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Host "[ERROR] 'dotnet' not found. Install .NET 8 SDK from:" -ForegroundColor Red
        Write-Host "        https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Red
        exit 1
    }
    $ver = (dotnet --version 2>&1)
    Write-Host "  dotnet $ver"
}

function Find-InnoSetup {
    $candidates = @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe"
    )
    return $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
}

# ============================================================
# Validate version format
# ============================================================

if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    Write-Host "[ERROR] Version must be in x.y.z format (got: $Version)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "  Mock Testing Suite — Release Build" -ForegroundColor White
Write-Host "  Version  : $Version"
Write-Host "  Output   : $PublishDir"
Write-Host "  Installer: $DistDir"

# ============================================================
# Step 1: Check prerequisites
# ============================================================

Write-Step "Checking prerequisites"
Assert-Dotnet

# ============================================================
# Step 2: Run tests (unless skipped)
# ============================================================

if (-not $SkipTests) {
    Write-Step "Running tests"
    dotnet test $SolutionFile --configuration Release --no-restore --logger "console;verbosity=minimal"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "`n[ERROR] Tests failed — aborting build." -ForegroundColor Red
        exit 1
    }
    Write-Host "  All tests passed." -ForegroundColor Green
}

# ============================================================
# Step 3: Clean publish output
# ============================================================

Write-Step "Cleaning publish directory"
if (Test-Path $PublishDir) {
    Remove-Item $PublishDir -Recurse -Force
    Write-Host "  Removed: $PublishDir"
}
New-Item -ItemType Directory -Path $PublishDir -Force | Out-Null
New-Item -ItemType Directory -Path $DistDir    -Force | Out-Null

# ============================================================
# Step 4: Publish
# ============================================================

Write-Step "Publishing MTS.UI (self-contained, win-x64, single-file)"

dotnet publish "$Root\MTS.UI\MTS.UI.csproj" `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:IncludeAllContentForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:DebugType=embedded `
    -p:PublishTrimmed=false `
    -p:Version=$Version `
    -p:AssemblyVersion="$Version.0" `
    -p:FileVersion="$Version.0" `
    --output $PublishDir

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[ERROR] dotnet publish failed." -ForegroundColor Red
    exit 1
}

$exePath = Join-Path $PublishDir "MTS.UI.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "`n[ERROR] Expected output not found: $exePath" -ForegroundColor Red
    exit 1
}

$sizeMB = [math]::Round((Get-Item $exePath).Length / 1MB, 1)
Write-Host "`n  Published: $exePath ($sizeMB MB)" -ForegroundColor Green

# ============================================================
# Step 5: Build installer (optional)
# ============================================================

if ($NoInstaller) {
    Write-Host "`n  [--NoInstaller] Skipping Inno Setup step." -ForegroundColor Yellow
    Write-Host "`n  Portable exe: $exePath" -ForegroundColor Green
    exit 0
}

$iscc = Find-InnoSetup

if (-not $iscc) {
    Write-Host "`n  [WARNING] Inno Setup 6 not found — installer step skipped." -ForegroundColor Yellow
    Write-Host "            Download from: https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    Write-Host "`n  Portable exe ready: $exePath" -ForegroundColor Green
    exit 0
}

Write-Step "Building installer with Inno Setup 6"
Write-Host "  ISCC: $iscc"

& $iscc `
    "/DAppVersion=$Version" `
    "$Root\installer\MTS.iss"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`n[ERROR] Inno Setup compilation failed." -ForegroundColor Red
    exit 1
}

$installer = Get-ChildItem $DistDir -Filter "MTS-Setup-*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if ($installer) {
    $instMB = [math]::Round($installer.Length / 1MB, 1)
    Write-Host "`n  Installer: $($installer.FullName) ($instMB MB)" -ForegroundColor Green
}

Write-Host "`n  Build complete." -ForegroundColor Green
