# PowerShell script to list all types in assembly
$dllPath = "D:\L1R Project\L1R-CustomLauncher\src\LineageLauncher.App\bin\Debug\net8.0-windows\LineageLauncher.dll"

# Load the assembly
try {
    $assembly = [System.Reflection.Assembly]::LoadFrom($dllPath)
    Write-Host "Assembly loaded: $($assembly.FullName)" -ForegroundColor Green

    Write-Host "`n=== All Types in Assembly ===" -ForegroundColor Yellow
    $assembly.GetTypes() | Where-Object { $_.IsPublic } | ForEach-Object {
        Write-Host "  - $($_.FullName)"
    }

    Write-Host "`n=== Looking for ViewModel types ===" -ForegroundColor Cyan
    $assembly.GetTypes() | Where-Object { $_.Name -like "*ViewModel*" } | ForEach-Object {
        Write-Host "  - $($_.FullName) (Public: $($_.IsPublic), Namespace: $($_.Namespace))"
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
}
