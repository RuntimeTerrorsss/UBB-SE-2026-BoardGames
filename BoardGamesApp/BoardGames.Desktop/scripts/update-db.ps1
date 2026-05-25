dotnet ef database update --project BoardGames.Data --startup-project BookingBoardGamesWeb
param(
    [string]$RepoRoot = "$(Split-Path -Parent $PSScriptRoot)"
)

Write-Host "Applying EF migrations..."

# Move to repository root (script is under scripts/)
Set-Location $RepoRoot

# Ensure dotnet-ef is available
if (-not (Get-Command dotnet-ef -ErrorAction SilentlyContinue)) {
    Write-Host "dotnet-ef not found. Installing..."
    dotnet tool install --global dotnet-ef
}

Write-Host "Resolving project paths..."

# Try to find the data and startup projects by name if the explicit paths are not present
$dataProject = Get-ChildItem -Path $RepoRoot -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*BoardGames.Data*.csproj" } | Select-Object -First 1

$startupProject = Get-ChildItem -Path $RepoRoot -Recurse -Filter "*.csproj" -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -like "*BoardGames.Api*.csproj" -or $_.Name -like "*BoardGamesWeb*.csproj" } | Select-Object -First 1

if (-not $dataProject) {
    Write-Error "Could not find BoardGames.Data project under $RepoRoot"
    exit 1
}

if (-not $startupProject) {
    Write-Error "Could not find BoardGames Web/API project under $RepoRoot"
    exit 1
}

$dataPath = $dataProject.FullName
$startupPath = $startupProject.FullName

Write-Host "Found data project: $dataPath"
Write-Host "Found startup project: $startupPath"

Write-Host "Running: dotnet ef database update --project \"$dataPath\" --startup-project \"$startupPath\""
dotnet ef database update --project "$dataPath" --startup-project "$startupPath"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Database update failed"
    exit $LASTEXITCODE
}

Write-Host "Database updated successfully."
