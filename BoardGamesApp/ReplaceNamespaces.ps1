param(
    [string]$solutionDir = "."
)

$replacements = @{
    "BoardRentAndProperty\.Api" = "BoardGames.Api"
    "BoardRentAndProperty\.Contracts" = "BoardGames.Shared"
    "BoardRentAndProperty\.Tests" = "BoardGames.Tests"
    "BoardRentAndProperty\.ViewModels" = "BoardGames.Desktop.ViewModels"
    "BoardRentAndProperty\.Views" = "BoardGames.Desktop.Views"
    "BoardRentAndProperty\.Constants" = "BoardGames.Shared.Common"
    "BoardRentAndProperty\.Services" = "BoardGames.Desktop.Services"
    "BoardRentAndProperty\.Utilities" = "BoardGames.Desktop.Helpers"
    "BoardRentAndProperty\.ApiClient" = "BoardGames.ApiClient"
    "BoardRentAndProperty" = "BoardGames.Desktop"
    "BookingBoardGames\.Data\.Enum" = "BoardGames.Data.Enums"
    "BookingBoardGames\.Data\.Interfaces" = "BoardGames.Data.Repositories"
    "BookingBoardGames\.Data" = "BoardGames.Data"
    "BookingBoardGames\.Sharing" = "BoardGames.Shared"
    "BookingBoardGames\.Src" = "BoardGames.Desktop"
    "BookingBoardGames" = "BoardGames"
    "GUI_BRAP" = "BoardGames.Web"
    "BoardGames\.Data\.Enum;" = "BoardGames.Data.Enums;"
    "BoardGames\.Data\.Interfaces;" = "BoardGames.Data.Repositories;"
    "BoardGames\.Data\.Repositpries;" = "BoardGames.Data.Repositories;"
}

$files = Get-ChildItem -Path $solutionDir -Include *.cs, *.cshtml, *.xaml -Recurse | Where-Object {
    $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\"
}

foreach ($file in $files) {
    $content = Get-Content $file.FullName -Raw
    $newContent = $content
    
    foreach ($key in $replacements.Keys) {
        $value = $replacements[$key]
        $newContent = [regex]::Replace($newContent, "(?m)^(using\s+|namespace\s+|xmlns:local=`"using:)$key", "`${1}$value")
        # Also replace inside tags and strings where applicable
        $newContent = $newContent -replace "\b$key\b", $value
    }
    
    if ($content -ne $newContent) {
        Write-Host "Updating references in $($file.Name)"
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
    }
}
