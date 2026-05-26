$knownExternalMappings = @{
    "Fact" = "Xunit"
    "FactAttribute" = "Xunit"
    "Theory" = "Xunit"
    "InlineData" = "Xunit"
    "Mock" = "Moq"
    "Mock`1" = "Moq"
    "Test" = "NUnit.Framework"
    "Controller" = "Microsoft.AspNetCore.Mvc"
    "ControllerBase" = "Microsoft.AspNetCore.Mvc"
    "IActionResult" = "Microsoft.AspNetCore.Mvc"
    "ActionResult" = "Microsoft.AspNetCore.Mvc"
    "HttpGet" = "Microsoft.AspNetCore.Mvc"
    "HttpPost" = "Microsoft.AspNetCore.Mvc"
    "HttpPut" = "Microsoft.AspNetCore.Mvc"
    "HttpDelete" = "Microsoft.AspNetCore.Mvc"
    "Route" = "Microsoft.AspNetCore.Mvc"
    "FromBody" = "Microsoft.AspNetCore.Mvc"
    "FromQuery" = "Microsoft.AspNetCore.Mvc"
    "FromServices" = "Microsoft.AspNetCore.Mvc"
    "AllowAnonymous" = "Microsoft.AspNetCore.Authorization"
    "Authorize" = "Microsoft.AspNetCore.Authorization"
    "DbContext" = "Microsoft.EntityFrameworkCore"
    "DbSet" = "Microsoft.EntityFrameworkCore"
    "ObservableCollection" = "System.Collections.ObjectModel"
    "ObservableObject" = "CommunityToolkit.Mvvm.ComponentModel"
    "RelayCommand" = "CommunityToolkit.Mvvm.Input"
    "ICommand" = "System.Windows.Input"
    "NotifyIcon" = "H.NotifyIcon"
    "ValidationResult" = "System.ComponentModel.DataAnnotations"
    "ValidationAttribute" = "System.ComponentModel.DataAnnotations"
    "Required" = "System.ComponentModel.DataAnnotations"
    "Key" = "System.ComponentModel.DataAnnotations"
    "Column" = "System.ComponentModel.DataAnnotations.Schema"
    "Table" = "System.ComponentModel.DataAnnotations.Schema"
    "JsonIgnore" = "System.Text.Json.Serialization"
    "JsonPropertyName" = "System.Text.Json.Serialization"
    "JsonSerializer" = "System.Text.Json"
    "JsonDocument" = "System.Text.Json"
    "HttpClient" = "System.Net.Http"
    "HttpResponseMessage" = "System.Net.Http"
    "HttpRequestMessage" = "System.Net.Http"
    "Task" = "System.Threading.Tasks"
    "Task`1" = "System.Threading.Tasks"
    "List`1" = "System.Collections.Generic"
    "IEnumerable`1" = "System.Collections.Generic"
    "DateTime" = "System"
    "Guid" = "System"
    "String" = "System"
    "Exception" = "System"
    "ArgumentNullException" = "System"
    "ArgumentException" = "System"
    "InvalidOperationException" = "System"
    "NotImplementedException" = "System"
    "Console" = "System"
}

# 1. Build an index of all internal types
$typeIndex = @{}
$csFiles = Get-ChildItem -Path . -Filter *.cs -Recurse | Where-Object { $_.FullName -notmatch "\\bin\\" -and $_.FullName -notmatch "\\obj\\" }

Write-Host "Building type index..."
foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $ns = $null
    if ($content -match '(?m)^namespace\s+([a-zA-Z0-9_.]+)') {
        $ns = $matches[1]
    }
    
    if ($ns) {
        # Find classes, interfaces, enums, records
        $matchesList = [regex]::Matches($content, '(?m)\b(?:class|interface|enum|record|struct)\s+([A-Za-z0-9_]+)')
        foreach ($m in $matchesList) {
            $typeName = $m.Groups[1].Value
            if (-not $typeIndex.ContainsKey($typeName)) {
                $typeIndex[$typeName] = $ns
            }
        }
    }
}

Write-Host "Found $($typeIndex.Count) internal types."

# 2. Iterate build and fix
$maxIterations = 5
for ($i = 0; $i -lt $maxIterations; $i++) {
    Write-Host "Running build iteration $($i+1)..."
    $buildOutput = dotnet build 2>&1 | Out-String
    
    # Extract CS0246 errors (missing type or namespace)
    # Format: Path(Line,Col): error CS0246: The type or namespace name 'TypeName' could not be found
    $errorMatches = [regex]::Matches($buildOutput, '(?m)^(.+?\.cs)\(\d+,\d+\):\s*error\s+CS0246.*?name\s+''([^'']+)''')
    
    $fixes = 0
    $filesToUpdate = @{}
    
    foreach ($err in $errorMatches) {
        $path = $err.Groups[1].Value.Trim()
        if (-not [System.IO.Path]::IsPathRooted($path)) {
            $path = Join-Path (Get-Location) $path
        }
        $typeName = $err.Groups[2].Value
        
        # Remove generic part if present
        $baseTypeName = $typeName -replace '<.*>', ''
        
        $nsToImport = $null
        if ($knownExternalMappings.ContainsKey($baseTypeName)) {
            $nsToImport = $knownExternalMappings[$baseTypeName]
        } elseif ($typeIndex.ContainsKey($baseTypeName)) {
            $nsToImport = $typeIndex[$baseTypeName]
        }
        
        if ($nsToImport) {
            if (-not $filesToUpdate.ContainsKey($path)) {
                $filesToUpdate[$path] = @{}
            }
            if (-not $filesToUpdate[$path].ContainsKey($nsToImport)) {
                $filesToUpdate[$path][$nsToImport] = $true
            }
        }
    }
    
    foreach ($path in $filesToUpdate.Keys) {
        if (Test-Path $path) {
            $content = Get-Content $path -Raw
            $added = $false
            foreach ($ns in $filesToUpdate[$path].Keys) {
                # Check if using already exists
                if ($content -notmatch "(?m)^using\s+$ns;") {
                    $content = "using $ns;`r`n" + $content
                    $added = $true
                    $fixes++
                }
            }
            if ($added) {
                Set-Content -Path $path -Value $content -NoNewline
            }
        }
    }
    
    Write-Host "Applied $fixes fixes in iteration $($i+1)."
    if ($fixes -eq 0) {
        Write-Host "No more fixes can be applied."
        break
    }
}
