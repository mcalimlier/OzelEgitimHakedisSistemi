[CmdletBinding()]
param(
    [string]$OutputDirectory = ".\artifacts\OEHS-PDF-Smoke"
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $repoRoot "tests\OzelEgitimHakedis.Wpf.Tests\OzelEgitimHakedis.Wpf.Tests.csproj"

if (-not (Test-Path -LiteralPath $testProject)) {
    throw "WPF PDF smoke test project was not found: $testProject"
}

if ([System.IO.Path]::IsPathRooted($OutputDirectory)) {
    $resolvedOutputDirectory = [System.IO.Path]::GetFullPath($OutputDirectory)
}
else {
    $resolvedOutputDirectory = [System.IO.Path]::GetFullPath((Join-Path $repoRoot $OutputDirectory))
}

New-Item -ItemType Directory -Path $resolvedOutputDirectory -Force | Out-Null

$oldOutputDirectory = $env:OEHS_PDF_SMOKE_OUTPUT_DIR
$testExitCode = 0

try {
    $env:OEHS_PDF_SMOKE_OUTPUT_DIR = $resolvedOutputDirectory
    Push-Location $repoRoot
    try {
        dotnet test $testProject --configuration Release
        $testExitCode = $LASTEXITCODE
    }
    finally {
        Pop-Location
    }
}
finally {
    if ($null -eq $oldOutputDirectory) {
        Remove-Item Env:\OEHS_PDF_SMOKE_OUTPUT_DIR -ErrorAction SilentlyContinue
    }
    else {
        $env:OEHS_PDF_SMOKE_OUTPUT_DIR = $oldOutputDirectory
    }
}

if ($testExitCode -ne 0) {
    throw "PDF smoke tests failed with exit code $testExitCode."
}

$requiredFiles = @(
    "01_Hakedis_Raporu.pdf",
    "02_Hizmet_Isleri_Hakedis_Raporu.pdf",
    "03_Hizmet_Isleri_Kabul_Tutanagi.pdf",
    "04_Hakedis_Icmali.pdf"
)

$missingFiles = @($requiredFiles | Where-Object {
    -not (Test-Path -LiteralPath (Join-Path $resolvedOutputDirectory $_))
})

if ($missingFiles.Count -gt 0) {
    throw "PDF smoke tests passed but expected output files are missing: $($missingFiles -join ', ')"
}

Write-Host ""
Write-Host "Four official A4 PDF smoke files are ready for visual review:" -ForegroundColor Green
Get-ChildItem -LiteralPath $resolvedOutputDirectory -Filter "*.pdf" -File |
    Sort-Object Name |
    Select-Object Name, Length, FullName |
    Format-Table -AutoSize
Write-Host "Upload these four PDFs for the final visual review."
