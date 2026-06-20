param(
    [string]$HostName = "localhost",
    [int]$Port = 5432,
    [string]$User = "postgres",
    [string]$PsqlPath = ""
)

$ErrorActionPreference = "Stop"

function Resolve-Psql {
    param([string]$ExplicitPath)

    if ($ExplicitPath -and (Test-Path $ExplicitPath)) {
        return (Resolve-Path $ExplicitPath).Path
    }

    $command = Get-Command psql -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $candidates = Get-ChildItem "C:\Program Files\PostgreSQL" -Filter "psql.exe" -Recurse -ErrorAction SilentlyContinue |
        Sort-Object FullName -Descending

    if ($candidates -and $candidates.Count -gt 0) {
        return $candidates[0].FullName
    }

    throw "psql.exe not found. Add PostgreSQL bin folder to PATH or pass -PsqlPath `"C:\Path\to\psql.exe`"."
}

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$psql = Resolve-Psql $PsqlPath

Write-Host "Using psql: $psql"
Write-Host "Connection: ${User}@${HostName}:${Port}"
Write-Host "If PostgreSQL asks for a password, enter the password you set during installation."

& $psql -h $HostName -p $Port -U $User -d postgres -f (Join-Path $root "00_create_databases.sql")
& $psql -h $HostName -p $Port -U $User -d merch_catalog -f (Join-Path $root "01_merch_catalog.sql")
& $psql -h $HostName -p $Port -U $User -d merch_store -f (Join-Path $root "02_merch_store.sql")
& $psql -h $HostName -p $Port -U $User -d merch_sales -f (Join-Path $root "03_merch_sales.sql")

Write-Host "Done. Databases created: merch_catalog, merch_store, merch_sales."
