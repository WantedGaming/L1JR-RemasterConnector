# PowerShell script to inspect MainLauncherViewModel
$dllPath = "D:\L1R Project\L1R-CustomLauncher\src\LineageLauncher.App\bin\Debug\net8.0-windows\LineageLauncher.dll"

# Load the assembly
$assembly = [System.Reflection.Assembly]::LoadFrom($dllPath)

# Get the ViewModel type
$vmType = $assembly.GetType("LineageLauncher.App.ViewModels.MainLauncherViewModel")

Write-Host "=== MainLauncherViewModel Properties ===" -ForegroundColor Green
$vmType.GetProperties() | Where-Object { $_.Name -like "*Command*" } | ForEach-Object {
    Write-Host "  - $($_.Name) : $($_.PropertyType.Name)"
}

Write-Host "`n=== MainLauncherViewModel Methods ===" -ForegroundColor Green
$vmType.GetMethods([System.Reflection.BindingFlags]::Public -bor [System.Reflection.BindingFlags]::Instance) |
    Where-Object { $_.Name -like "*Game*" } | ForEach-Object {
    Write-Host "  - $($_.Name) : $($_.ReturnType.Name)"
}

Write-Host "`n=== All Public Properties ===" -ForegroundColor Yellow
$vmType.GetProperties() | ForEach-Object {
    Write-Host "  - $($_.Name) : $($_.PropertyType.Name)"
}
