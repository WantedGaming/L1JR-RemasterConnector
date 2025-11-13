# Build Test Script for L1R Custom Launcher
# This script attempts to build the solution and capture results

Write-Host "=== L1R Custom Launcher Build Test ===" -ForegroundColor Green
Write-Host ""

$solutionPath = "D:\L1R Project\l1r-customlauncher\LineageLauncher.sln"

# Check if solution exists
if (!(Test-Path $solutionPath)) {
    Write-Host "ERROR: Solution file not found at $solutionPath" -ForegroundColor Red
    exit 1
}

Write-Host "Solution found: $solutionPath" -ForegroundColor Green
Write-Host ""

# Try to find dotnet
Write-Host "Searching for dotnet SDK..." -ForegroundColor Yellow
$dotnetPath = Get-Command dotnet -ErrorAction SilentlyContinue

if ($dotnetPath) {
    Write-Host "Found dotnet at: $($dotnetPath.Source)" -ForegroundColor Green

    # Show dotnet version
    Write-Host ""
    Write-Host "dotnet version:" -ForegroundColor Cyan
    & dotnet --version

    Write-Host ""
    Write-Host "Building solution..." -ForegroundColor Yellow
    Write-Host ""

    # Build the solution
    & dotnet build $solutionPath --configuration Release

    $buildResult = $LASTEXITCODE

    Write-Host ""
    if ($buildResult -eq 0) {
        Write-Host "=== BUILD SUCCEEDED ===" -ForegroundColor Green
    } else {
        Write-Host "=== BUILD FAILED ===" -ForegroundColor Red
    }

    exit $buildResult
} else {
    Write-Host "ERROR: dotnet SDK not found in PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please install .NET 8.0 SDK from:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Or add dotnet to your PATH if already installed." -ForegroundColor Yellow
    exit 1
}
