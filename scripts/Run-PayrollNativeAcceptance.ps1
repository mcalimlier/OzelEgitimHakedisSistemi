param()
$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$fixturePath = Join-Path $repo 'tests/OzelEgitimHakedis.Tests/Fixtures/payroll-2026-10-26.json'
$fixture = Get-Content -LiteralPath $fixturePath -Raw -Encoding utf8 | ConvertFrom-Json
foreach ($source in $fixture.Sources) {
    $path = Join-Path (Join-Path $env:USERPROFILE 'Downloads') $source.File
    if (-not (Test-Path -LiteralPath $path)) { throw "Kaynak dosya bulunamadı: $path" }
    if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne $source.Sha256) {
        throw "Kaynak değişmiş; karşılaştırma verisini yenileyin: $path"
    }
}
$runRoot = Join-Path $repo ('.superpowers/payroll-native-' + (Get-Date -Format 'yyyy-MM-dd-HHmmss'))
$appRoot = Join-Path $runRoot 'app'
$evidenceRoot = Join-Path $runRoot 'evidence'
New-Item -ItemType Directory -Path $appRoot,$evidenceRoot -Force | Out-Null
function Snapshot-NormalData {
    @(Get-ChildItem -LiteralPath (Join-Path $env:LOCALAPPDATA 'OEHS') -File -Recurse -ErrorAction SilentlyContinue |
        ForEach-Object { [pscustomobject]@{Path=$_.FullName;Hash=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash} })
}
$before = Snapshot-NormalData
$before | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $runRoot 'normal-data-before.json') -Encoding utf8
& dotnet build (Join-Path $repo 'tests/OzelEgitimHakedis.NativeAcceptance') -c Release -o $appRoot -p:RunAnalyzers=false
if ($LASTEXITCODE -ne 0) { throw 'Deneme çalıştırıcısı derlenemedi.' }
# The runner marks only its own executable directory as an isolated package before loading App.
& (Join-Path $appRoot 'OzelEgitimHakedis.NativeAcceptance.exe') $fixturePath $evidenceRoot
if ($LASTEXITCODE -ne 0) { throw "Gerçek pencere kabul denemesi başarısız. Kanıt: $evidenceRoot" }
$after = Snapshot-NormalData
$after | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $runRoot 'normal-data-after.json') -Encoding utf8
$beforeKeys = @($before | ForEach-Object { $_.Path + '|' + $_.Hash })
$afterKeys = @($after | ForEach-Object { $_.Path + '|' + $_.Hash })
$difference = @($beforeKeys | Where-Object { $_ -notin $afterKeys }) + @($afterKeys | Where-Object { $_ -notin $beforeKeys })
if ($difference) { throw 'Normal veri dosyaları başlangıçtan farklı; kanıtları inceleyin. Bu betik normal veriyi geri almaz veya değiştirmez.' }
Get-Content -LiteralPath (Join-Path $evidenceRoot 'summary.json') -Encoding utf8
Write-Host "Kanıt klasörü: $evidenceRoot"
