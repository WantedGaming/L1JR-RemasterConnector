# Launcher Monitoring Script
# Monitors process creation and file changes during launcher execution

$clientPath = "D:\L1R Project\L1R-Client"
$logFile = "$PSScriptRoot\launcher_analysis.log"

Write-Host "=== L1R Launcher Monitoring Started ===" -ForegroundColor Green
Write-Host "Monitoring client directory: $clientPath" -ForegroundColor Yellow
Write-Host "Log file: $logFile" -ForegroundColor Yellow
Write-Host ""

# Clear previous log
"=== Launcher Monitoring - $(Get-Date) ===" | Out-File $logFile

# Capture initial state
Write-Host "[1/4] Capturing initial file state..." -ForegroundColor Cyan
$beforeFiles = Get-ChildItem -Path $clientPath -Recurse -File | Select-Object FullName, Length, LastWriteTime
"Initial file count: $($beforeFiles.Count)" | Tee-Object -FilePath $logFile -Append

# Monitor for new processes
Write-Host "[2/4] Monitoring for new processes..." -ForegroundColor Cyan
$beforeProcesses = Get-Process | Select-Object -ExpandProperty Name

Write-Host ""
Write-Host ">>> LAUNCH THE OTHER LAUNCHER NOW <<<" -ForegroundColor Green -BackgroundColor Black
Write-Host ">>> Press ENTER when launcher has finished starting Lin.bin <<<" -ForegroundColor Green -BackgroundColor Black
Read-Host

# Capture after state
Write-Host ""
Write-Host "[3/4] Analyzing changes..." -ForegroundColor Cyan

# Check new processes
$afterProcesses = Get-Process | Select-Object -ExpandProperty Name
$newProcesses = Compare-Object -ReferenceObject $beforeProcesses -DifferenceObject $afterProcesses | Where-Object { $_.SideIndicator -eq "=>" } | Select-Object -ExpandProperty InputObject

if ($newProcesses) {
    Write-Host ""
    Write-Host "NEW PROCESSES DETECTED:" -ForegroundColor Yellow
    "`nNEW PROCESSES:" | Add-Content $logFile
    foreach ($proc in ($newProcesses | Select-Object -Unique)) {
        try {
            $procInfo = Get-Process -Name $proc -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($procInfo) {
                $info = "  - $proc (PID: $($procInfo.Id), Path: $($procInfo.Path))"
                Write-Host $info -ForegroundColor Green
                $info | Add-Content $logFile

                # Get command line if possible
                try {
                    $cmdLine = (Get-WmiObject Win32_Process -Filter "ProcessId = $($procInfo.Id)").CommandLine
                    if ($cmdLine) {
                        $cmdInfo = "    Command: $cmdLine"
                        Write-Host $cmdInfo -ForegroundColor Gray
                        $cmdInfo | Add-Content $logFile
                    }
                } catch {}
            }
        } catch {
            "  - $proc (process ended)" | Tee-Object -FilePath $logFile -Append | Write-Host -ForegroundColor DarkGray
        }
    }
}

# Check file changes
Write-Host ""
Write-Host "[4/4] Checking file system changes..." -ForegroundColor Cyan
$afterFiles = Get-ChildItem -Path $clientPath -Recurse -File | Select-Object FullName, Length, LastWriteTime

# Find new files
$newFiles = $afterFiles | Where-Object { $_.FullName -notin $beforeFiles.FullName }
if ($newFiles) {
    Write-Host ""
    Write-Host "NEW FILES CREATED:" -ForegroundColor Yellow
    "`nNEW FILES:" | Add-Content $logFile
    foreach ($file in $newFiles) {
        $info = "  + $($file.FullName) ($('{0:N0}' -f $file.Length) bytes)"
        Write-Host $info -ForegroundColor Green
        $info | Add-Content $logFile
    }
}

# Find modified files
$modifiedFiles = @()
foreach ($after in $afterFiles) {
    $before = $beforeFiles | Where-Object { $_.FullName -eq $after.FullName }
    if ($before -and $before.LastWriteTime -ne $after.LastWriteTime) {
        $modifiedFiles += $after
    }
}

if ($modifiedFiles) {
    Write-Host ""
    Write-Host "MODIFIED FILES:" -ForegroundColor Yellow
    "`nMODIFIED FILES:" | Add-Content $logFile
    foreach ($file in $modifiedFiles) {
        $info = "  * $($file.FullName) ($('{0:N0}' -f $file.Length) bytes, modified: $($file.LastWriteTime))"
        Write-Host $info -ForegroundColor Cyan
        $info | Add-Content $logFile
    }
}

# Check for specific files of interest
Write-Host ""
Write-Host "CHECKING FOR KEY FILES:" -ForegroundColor Yellow
"`nKEY FILES:" | Add-Content $logFile

$keyFiles = @(
    "$clientPath\bin64\Login.ini",
    "$clientPath\bin64\Lineage.ini",
    "$clientPath\bin64\210916.asi",
    "$clientPath\bin64\boxer.dll",
    "$clientPath\bin64\libcocos2d.dll",
    "$clientPath\bin32\Login.ini",
    "$clientPath\bin32\Lineage.ini"
)

foreach ($keyFile in $keyFiles) {
    if (Test-Path $keyFile) {
        $info = "  [FOUND] $keyFile"
        Write-Host $info -ForegroundColor Green
        $info | Add-Content $logFile

        # If it's an INI file, show contents
        if ($keyFile -match '\.ini$') {
            $content = Get-Content $keyFile -Raw
            "`n    Contents:`n$content" | Add-Content $logFile
            Write-Host "    (Contents saved to log)" -ForegroundColor Gray
        }
    }
}

Write-Host ""
Write-Host "=== Monitoring Complete ===" -ForegroundColor Green
Write-Host "Full report saved to: $logFile" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press ENTER to exit..."
Read-Host
