param(
    [string]$solutionDir = "."
)

# Find all .csproj files
$projects = Get-ChildItem -Path $solutionDir -Filter *.csproj -Recurse

foreach ($proj in $projects) {
    $projDir = $proj.Directory.FullName
    $projName = [System.IO.Path]::GetFileNameWithoutExtension($proj.Name)
    
    # Find all .cs files in this project (excluding bin, obj)
    $csFiles = Get-ChildItem -Path $projDir -Filter *.cs -Recurse | Where-Object {
        $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" -and $_.FullName -notmatch "\\Migrations\\"
    }
    
    foreach ($cs in $csFiles) {
        $relPath = $cs.Directory.FullName.Substring($projDir.Length).TrimStart('\')
        $expectedNs = $projName
        if (![string]::IsNullOrEmpty($relPath)) {
            $expectedNs += "." + $relPath.Replace("\", ".")
        }
        
        $content = Get-Content $cs.FullName -Raw
        
        # Regex to find namespace declaration
        $newContent = [regex]::Replace($content, '(?m)^namespace\s+[a-zA-Z0-9_.]+', "namespace $expectedNs")
        
        if ($content -ne $newContent) {
            Write-Host "Updating namespace in $($cs.Name) to $expectedNs"
            Set-Content -Path $cs.FullName -Value $newContent -NoNewline
        }
    }
}
