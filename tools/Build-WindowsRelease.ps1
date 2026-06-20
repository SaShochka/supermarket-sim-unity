# Собирает Windows-билд и упаковывает его в releases/SupermarketSim-Windows.zip

$ErrorActionPreference = 'Stop'

$project = Split-Path -Parent $PSScriptRoot
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.4f1\Editor\Unity.exe'
$buildDir = Join-Path $project 'Build\Windows'
$exe = Join-Path $buildDir 'SupermarketSim.exe'
$log = Join-Path $project 'Build\build-release.log'
$zip = Join-Path $project 'releases\SupermarketSim-Windows.zip'

if (-not (Test-Path $unity)) {
    throw "Unity not found: $unity"
}

New-Item -ItemType Directory -Force -Path $buildDir, (Split-Path $zip) | Out-Null
Remove-Item $zip -Force -ErrorAction SilentlyContinue

& $unity `
    -batchmode `
    -quit `
    -projectPath $project `
    -executeMethod SupermarketSim_BuildPipeline.BuildWindows `
    -buildOutput $exe `
    -logFile $log

if (-not (Test-Path $exe)) {
    throw "Build failed, missing $exe. See $log"
}

Compress-Archive -Path (Join-Path $buildDir '*') -DestinationPath $zip -Force
Write-Host "Done: $zip"
