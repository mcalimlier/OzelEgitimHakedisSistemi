param(
    [string]$WorkbookPath = (Join-Path ([Environment]::GetFolderPath('Desktop')) 'SonDosyalar\02-Özel Eğitim Hakediş Ödeme SON.xlsm'),
    [string]$ModulePath = (Join-Path ([Environment]::GetFolderPath('Desktop')) 'SonDosyalar\Özel Eğitim VBA Kodları\Module1.bas')
)

$ErrorActionPreference = 'Stop'
[System.Text.Encoding]::RegisterProvider([System.Text.CodePagesEncodingProvider]::Instance)
$turkishWindowsEncoding = [System.Text.Encoding]::GetEncoding(1254)

if (-not (Test-Path -LiteralPath $WorkbookPath -PathType Leaf)) { throw "XLSM bulunamadı: $WorkbookPath" }
if (-not (Test-Path -LiteralPath $ModulePath -PathType Leaf)) { throw "VBA Module1.bas bulunamadı: $ModulePath" }

$sourceText = [IO.File]::ReadAllText($ModulePath, $turkishWindowsEncoding)
$codeLines = $sourceText -split "`r?`n"
if ($codeLines.Count -gt 0 -and $codeLines[0] -match '^Attribute VB_Name\s*=') {
    $codeLines = $codeLines | Select-Object -Skip 1
}
$codeBody = $codeLines -join "`r`n"

$testRoot = Join-Path ([IO.Path]::GetTempPath()) ('oehs-vba-month-test-' + [guid]::NewGuid().ToString('N'))
$copyPath = Join-Path $testRoot ([IO.Path]::GetFileName($WorkbookPath))
$originalHash = (Get-FileHash -LiteralPath $WorkbookPath -Algorithm SHA256).Hash
$excel = $null
$workbook = $null
$component = $null
$codeModule = $null
$results = [System.Collections.Generic.List[object]]::new()

try {
    New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
    Copy-Item -LiteralPath $WorkbookPath -Destination $copyPath

    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    $excel.EnableEvents = $false
    $excel.AutomationSecurity = 3 # msoAutomationSecurityForceDisable while replacing the VBA module

    $workbook = $excel.Workbooks.Open($copyPath, 0, $false)
    $component = $workbook.VBProject.VBComponents.Item('Module1')
    $codeModule = $component.CodeModule
    if ($codeModule.CountOfLines -gt 0) {
        $codeModule.DeleteLines(1, $codeModule.CountOfLines)
    }
    $codeModule.AddFromString($codeBody)
    $workbook.Save()
    $workbook.Close($false)
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($codeModule)
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($component)
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($workbook)
    $codeModule = $null
    $component = $null
    $workbook = $null

    # Enable only the test copy's VBA project, with workbook events still suppressed.
    $excel.AutomationSecurity = 1 # msoAutomationSecurityLow
    $workbook = $excel.Workbooks.Open($copyPath, 0, $false)

    $months = @('Eylül', 'Ekim', 'Kasım', 'Aralık', 'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran', 'Temmuz')
    $monthIndex = 0
    foreach ($month in $months) {
        foreach ($firstColumn in @(4, 16, 28, 40)) {
            $expected = $firstColumn + $monthIndex
            $actual = [int]$excel.Run("'$(Split-Path -Leaf $copyPath)'!Module1.GetIcmalMonthTargetColumn", $month, $firstColumn)
            if ($actual -ne $expected) {
                throw "$month / başlangıç sütunu ${firstColumn}: beklenen $expected, dönen $actual"
            }
            $results.Add([pscustomobject]@{ Month = $month; FirstColumn = $firstColumn; TargetColumn = $actual; Status = 'PASS' })
        }
        $monthIndex++
    }

    foreach ($invalidMonth in @('', 'Ağustos', 'Bilinmeyen')) {
        foreach ($firstColumn in @(4, 16, 28, 40)) {
            $actual = [int]$excel.Run("'$(Split-Path -Leaf $copyPath)'!Module1.GetIcmalMonthTargetColumn", $invalidMonth, $firstColumn)
            if ($actual -ne 0) {
                throw "Geçersiz ay '$invalidMonth' için hedef sütun 0 olmalı; $actual döndü."
            }
            $results.Add([pscustomobject]@{ Month = $invalidMonth; FirstColumn = $firstColumn; TargetColumn = $actual; Status = 'PASS' })
        }
    }

    $results | Format-Table -AutoSize
    Write-Output "VBA ay eşleme testi geçti: $($results.Count) kontrol. Test geçici XLSM kopyasında çalıştı."
}
finally {
    if ($workbook) {
        try { $workbook.Close($false) } catch { }
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($workbook)
    }
    if ($codeModule) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($codeModule) }
    if ($component) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($component) }
    if ($excel) {
        $excel.Quit()
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
    }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()

    if (Test-Path -LiteralPath $testRoot) {
        $resolvedTempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
        $resolvedTestRoot = [IO.Path]::GetFullPath($testRoot)
        if (-not $resolvedTestRoot.StartsWith($resolvedTempRoot, [StringComparison]::OrdinalIgnoreCase)) {
            throw "Geçici test dizini beklenen Temp klasöründe değil: $resolvedTestRoot"
        }
        Remove-Item -LiteralPath $resolvedTestRoot -Recurse -Force
    }
    if ((Get-FileHash -LiteralPath $WorkbookPath -Algorithm SHA256).Hash -ne $originalHash) {
        throw 'Kaynak XLSM değişti; test kaynağını değiştirmemeliydi.'
    }
}
