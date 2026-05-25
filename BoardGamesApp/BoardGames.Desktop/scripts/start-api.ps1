param(
    [string]$ProjectPath = "BoardGames/BoardGames.Api.csproj",
    [int]$Port = 5000
)

$env:ASPNETCORE_ENVIRONMENT = "Development"

Write-Host "Starting API from $ProjectPath on http://localhost:$Port"

# Run the API
dotnet run --project $ProjectPath --urls "http://localhost:$Port"
