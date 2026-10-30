param([Parameter(Mandatory)][string]$PackageDirectory)
$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$assets = Get-Content -LiteralPath (Join-Path $repoRoot 'src/OzelEgitimHakedis.Wpf/obj/project.assets.json') -Raw | ConvertFrom-Json
$packageCache = $assets.packageFolders.PSObject.Properties.Name | Select-Object -First 1
$framework = $assets.project.frameworks.PSObject.Properties.Value | Select-Object -First 1
$runtime = $framework.downloadDependencies | Where-Object { $_.name -eq 'Microsoft.NETCore.App.Runtime.win-x64' } | Select-Object -First 1
if (-not $runtime) { throw 'Önce Windows x64 self-contained yayın oluşturun.' }
$runtimeVersion = $runtime.version.Trim('[',']').Split(',')[0].Trim()
$coreRoot = Join-Path $packageCache ('microsoft.netcore.app.runtime.win-x64/' + $runtimeVersion)
$wpfRoot = Join-Path $packageCache ('microsoft.windowsdesktop.app.runtime.win-x64/' + $runtimeVersion)
$notices = Join-Path $PackageDirectory 'Lisanslar'
New-Item -ItemType Directory -Path $notices -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $repoRoot 'docs/PDFSHARP-LICENSE.txt') -Destination (Join-Path $notices 'PDFsharp-LICENSE.txt')
Copy-Item -LiteralPath (Join-Path $coreRoot 'LICENSE.TXT') -Destination (Join-Path $notices 'DOTNET-LICENSE.txt')
Copy-Item -LiteralPath (Join-Path $coreRoot 'THIRD-PARTY-NOTICES.TXT') -Destination (Join-Path $notices 'DOTNET-THIRD-PARTY-NOTICES.txt')
Copy-Item -LiteralPath (Join-Path $wpfRoot 'LICENSE') -Destination (Join-Path $notices 'WPF-LICENSE.txt')
