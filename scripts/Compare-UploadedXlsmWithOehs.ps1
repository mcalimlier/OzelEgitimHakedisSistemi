param(
    [string]$BasicWorkbookPath = (Join-Path ([Environment]::GetFolderPath('Desktop')) 'SonDosyalar\03-Temel Eğitim Taşıma Ödeme SON.xlsm'),
    [string]$SpecialWorkbookPath = (Join-Path ([Environment]::GetFolderPath('Desktop')) 'SonDosyalar\02-Özel Eğitim Hakediş Ödeme SON.xlsm'),
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [string]$ReportPath
)

$ErrorActionPreference = 'Stop'
$invariant = [Globalization.CultureInfo]::InvariantCulture
$tolerance = [decimal]0.000001
$projectRoot = Split-Path -Parent $PSScriptRoot
$coreProject = Join-Path $projectRoot 'src\OzelEgitimHakedis.Core\OzelEgitimHakedis.Core.csproj'
$coreAssembly = Join-Path $projectRoot "src\OzelEgitimHakedis.Core\bin\$Configuration\net8.0\OzelEgitimHakedis.Core.dll"

if (-not (Test-Path -LiteralPath $BasicWorkbookPath -PathType Leaf)) {
    throw "Temel Eğitim XLSM bulunamadı: $BasicWorkbookPath"
}
if (-not (Test-Path -LiteralPath $SpecialWorkbookPath -PathType Leaf)) {
    throw "Özel Eğitim XLSM bulunamadı: $SpecialWorkbookPath"
}
if (-not (Test-Path -LiteralPath $coreAssembly -PathType Leaf)) {
    & dotnet build $coreProject --configuration $Configuration --verbosity quiet
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $coreAssembly -PathType Leaf)) {
        throw 'OEHS Core derlemesi başarısız oldu.'
    }
}

Add-Type -Path $coreAssembly

function Read-DecimalCell {
    param($Worksheet, [string]$Address)

    $cell = $Worksheet.Range($Address)
    try {
        $value = $cell.Value2
        if ($null -eq $value) {
            return [decimal]0
        }
        if ($value -is [string] -or $value -is [bool]) {
            throw "Sayısal hücre bekleniyordu: $($Worksheet.Name)!$Address"
        }
        $text = [Convert]::ToString($value, $invariant)
        return [decimal]::Parse($text, [Globalization.NumberStyles]::Float, $invariant)
    }
    finally {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($cell)
    }
}

function Read-TextCell {
    param($Worksheet, [string]$Address)

    $cell = $Worksheet.Range($Address)
    try {
        return ([Convert]::ToString($cell.Value2, $invariant)).Trim()
    }
    finally {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($cell)
    }
}

function Add-Comparison {
    param(
        [System.Collections.Generic.List[object]]$Rows,
        [string]$Program,
        $Worksheet,
        [string]$Address,
        [string]$ApplicationField,
        [decimal]$ApplicationValue,
        [string]$ExpectedStatus = 'COMPARE',
        [decimal]$KnownDifference = 0
    )

    $cell = $Worksheet.Range($Address)
    try {
        $formula = [Convert]::ToString($cell.Formula, $invariant)
        $rawValue = $cell.Value2
        if ($null -eq $rawValue -or $rawValue -is [string] -or $rawValue -is [bool]) {
            throw "Karşılaştırma hücresi sayısal değil: $($Worksheet.Name)!$Address"
        }
        $excelValue = [decimal]::Parse(
            [Convert]::ToString($rawValue, $invariant),
            [Globalization.NumberStyles]::Float,
            $invariant)
        $difference = $ApplicationValue - $excelValue
        $status = if ([decimal]::Abs($difference) -le $tolerance) {
            'PASS'
        }
        elseif ($ExpectedStatus -eq 'APPROVED_DIFFERENCE' -and
                [decimal]::Abs([decimal]::Abs($difference) - $KnownDifference) -le $tolerance) {
            'APPROVED_DIFFERENCE'
        }
        else {
            'FAIL'
        }

        $Rows.Add([pscustomobject]@{
            Program = $Program
            ExcelCell = "$($Worksheet.Name)!$Address"
            ExcelFormula = $formula
            ExcelValue = $excelValue
            ApplicationField = $ApplicationField
            ApplicationValue = $ApplicationValue
            DifferenceApplicationMinusExcel = $difference
            Status = $status
        })
    }
    finally {
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($cell)
    }
}

function New-HistoryPayment {
    param(
        [OzelEgitimHakedis.Core.Models.ProgressPaymentInput]$PaymentInput,
        [OzelEgitimHakedis.Core.Models.ProgressPaymentCalculation]$Calculation,
        [int]$PaymentNumber,
        [DateOnly]$Period,
        [string]$TenderNumber,
        [string]$ContractNumber,
        [string]$RouteNumber
    )

    return [OzelEgitimHakedis.Core.Models.ProgressPaymentHistoryData]::new(
        [guid]::NewGuid(),
        [DateTime]::SpecifyKind($Period.ToDateTime([TimeOnly]::MinValue), [DateTimeKind]::Utc),
        $PaymentInput,
        $Calculation,
        $PaymentNumber,
        [Nullable[DateOnly]]$Period,
        $RouteNumber,
        'QA Route',
        'QA Work',
        'QA Contractor',
        $TenderNumber,
        $ContractNumber,
        $null,
        $null,
        $null,
        $null,
        $null,
        'QA-IDENTITY')
}

function Compare-Workbook {
    param(
        $Excel,
        [string]$Path,
        [string]$Program,
        [System.Collections.Generic.List[object]]$Rows
    )

    $workbook = $null
    $worksheets = $null
    $calculationSheet = $null
    $reportSheet = $null
    $icmalSheet = $null
    $homeSheet = $null
    try {
        # Open read-only and force-disable macros. FullRebuild runs Excel formulas only.
        $workbook = $Excel.Workbooks.Open($Path, 0, $true)
        $Excel.CalculateFullRebuild()
        $worksheets = $workbook.Worksheets
        $homeSheet = $worksheets.Item('Anasayfa')
        $reportSheet = $worksheets.Item('Hakediş Raporu')
        $icmalSheet = $worksheets.Item('Hakediş İcmali')

        if ($Program -eq 'Temel Eğitim') {
            $calculationSheet = $worksheets.Item('Hesaplama Bilgileri')
            $periodText = Read-TextCell $homeSheet 'E4'
            $programType = [OzelEgitimHakedis.Core.Models.TransportationProgramType]::BasicOrSecondary
            $workDays = Read-DecimalCell $calculationSheet 'G4'
            $dailyPrice = Read-DecimalCell $calculationSheet 'H4'
            $priceDifference = Read-DecimalCell $calculationSheet 'I4'
            $discount = Read-DecimalCell $calculationSheet 'J4'
            $vatExempt = (Read-TextCell $calculationSheet 'E4') -eq 'Evet'
            $taxDebt = Read-DecimalCell $calculationSheet 'P4'
            $inspectionPenalty = Read-DecimalCell $calculationSheet 'Q4'
            $socialSecurityDebt = Read-DecimalCell $calculationSheet 'R4'
            $minimumWageSupport = Read-DecimalCell $calculationSheet 'S4'
            $paymentNumberCell = 'K16'
            $checks = @(
                @{ Cell = 'K4'; Field = 'TransportNetAmount'; Property = 'TransportNetAmount' },
                @{ Cell = 'L4'; Field = 'TransportVat'; Property = 'TransportVat' },
                @{ Cell = 'M4'; Field = 'TransportWithholding'; Property = 'TransportWithholding' },
                @{ Cell = 'O4'; Field = 'GrossAmount'; Property = 'GrossAmount' },
                @{ Cell = 'T4'; Field = 'AdditionalGuarantee'; Property = 'AdditionalGuarantee' },
                @{ Cell = 'U4'; Field = 'StampTax'; Property = 'StampTax' },
                @{ Cell = 'V4'; Field = 'TotalDeductions'; Property = 'TotalDeductions' },
                @{ Cell = 'W4'; Field = 'NetPayable'; Property = 'NetPayable' }
            )
        }
        else {
            $calculationSheet = $worksheets.Item('Hesaplama')
            $periodText = Read-TextCell $homeSheet 'E5'
            $programType = [OzelEgitimHakedis.Core.Models.TransportationProgramType]::SpecialEducation
            $workDays = Read-DecimalCell $calculationSheet 'G4'
            $dailyPrice = Read-DecimalCell $calculationSheet 'H4'
            $priceDifference = Read-DecimalCell $calculationSheet 'I4'
            $discount = Read-DecimalCell $calculationSheet 'J4'
            $guideWorkDays = Read-DecimalCell $calculationSheet 'P4'
            $guideDailyPrice = Read-DecimalCell $calculationSheet 'Q4'
            $guidePriceDifference = Read-DecimalCell $calculationSheet 'R4'
            $guideDiscount = Read-DecimalCell $calculationSheet 'S4'
            $taxDebt = Read-DecimalCell $calculationSheet 'Y4'
            $inspectionPenalty = Read-DecimalCell $calculationSheet 'Z4'
            $socialSecurityDebt = Read-DecimalCell $calculationSheet 'AA4'
            $minimumWageSupport = Read-DecimalCell $calculationSheet 'AB4'
            $vatExempt = (Read-TextCell $calculationSheet 'E4') -eq 'Evet'
            $paymentNumberCell = 'K17'
            $checks = @(
                @{ Cell = 'K4'; Field = 'TransportNetAmount'; Property = 'TransportNetAmount' },
                @{ Cell = 'L4'; Field = 'TransportVat'; Property = 'TransportVat' },
                @{ Cell = 'M4'; Field = 'TransportWithholding'; Property = 'TransportWithholding' },
                @{ Cell = 'T4'; Field = 'GuideNetAmount'; Property = 'GuideNetAmount' },
                @{ Cell = 'U4'; Field = 'GuideVat'; Property = 'GuideVat' },
                @{ Cell = 'V4'; Field = 'GuideWithholding'; Property = 'GuideWithholding' },
                @{ Cell = 'X4'; Field = 'GuideGrossAmount'; Property = 'GuideGrossAmount' },
                @{ Cell = 'AC4'; Field = 'AdditionalGuarantee'; Property = 'AdditionalGuarantee' },
                @{ Cell = 'AD4'; Field = 'StampTax'; Property = 'StampTax' },
                @{ Cell = 'AE4'; Field = 'TotalDeductions'; Property = 'TotalDeductions' },
                @{ Cell = 'AF4'; Field = 'GrossAmount'; Property = 'GrossAmount' },
                @{ Cell = 'AG4'; Field = 'NetPayable'; Property = 'NetPayable' }
            )
        }

        $period = [DateOnly]::new(([DateTime]::Parse("1 $periodText", [Globalization.CultureInfo]::GetCultureInfo('tr-TR'))).Year,
            ([DateTime]::Parse("1 $periodText", [Globalization.CultureInfo]::GetCultureInfo('tr-TR'))).Month,
            [DateTime]::DaysInMonth(([DateTime]::Parse("1 $periodText", [Globalization.CultureInfo]::GetCultureInfo('tr-TR'))).Year,
                ([DateTime]::Parse("1 $periodText", [Globalization.CultureInfo]::GetCultureInfo('tr-TR'))).Month))

        $paymentInput = [OzelEgitimHakedis.Core.Models.ProgressPaymentInput]::new(
            $workDays, $dailyPrice, $discount, $priceDifference, [decimal]100000,
            [bool]$vatExempt, $taxDebt, $socialSecurityDebt, $minimumWageSupport,
            $inspectionPenalty,
            $(if ($Program -eq 'Özel Eğitim') { $guideWorkDays } else { [decimal]0 }),
            $(if ($Program -eq 'Özel Eğitim') { $guideDailyPrice } else { [decimal]0 }),
            $(if ($Program -eq 'Özel Eğitim') { $guideDiscount } else { [decimal]0 }),
            $(if ($Program -eq 'Özel Eğitim') { $guidePriceDifference } else { [decimal]0 }),
            $programType)
        $calculation = [OzelEgitimHakedis.Core.Services.ProgressPaymentCalculator]::Calculate($paymentInput)

        foreach ($check in $checks) {
            Add-Comparison $Rows $Program $calculationSheet $check.Cell $check.Field $calculation.($check.Property)
        }

        $currentRouteNumber = Read-TextCell $calculationSheet 'A4'
        $current = New-HistoryPayment $paymentInput $calculation 3 $period 'QA-TENDER' 'QA-CONTRACT' $currentRouteNumber

        # Seed the prior cumulative total from the workbook's actual D10 cell so the current
        # report fields D6/D8/D10/D11 can be compared using the production report service.
        $previousTotal = Read-DecimalCell $reportSheet 'D10'
        $previousPeriod = $period.AddMonths(-1)
        $previousInput = [OzelEgitimHakedis.Core.Models.ProgressPaymentInput]::new(
            [decimal]1, $previousTotal, [decimal]0, [decimal]0, [decimal]100000,
            $true, [decimal]0, [decimal]0, [decimal]0, [decimal]0,
            [decimal]0, [decimal]0, [decimal]0, [decimal]0, $programType)
        $previousCalculation = [OzelEgitimHakedis.Core.Services.ProgressPaymentCalculator]::Calculate($previousInput)
        $previous = New-HistoryPayment $previousInput $previousCalculation 2 $previousPeriod 'QA-TENDER' 'QA-CONTRACT' $currentRouteNumber
        $payments = [OzelEgitimHakedis.Core.Models.ProgressPaymentHistoryData[]]@($previous, $current)
        $totals = [OzelEgitimHakedis.Core.Services.ProgressPaymentReportService]::Calculate($current, $payments)
        $reportDeductions = [OzelEgitimHakedis.Core.Services.ProgressPaymentReportService]::CalculateReportTotalDeductions($calculation, $programType)
        $reportNet = [OzelEgitimHakedis.Core.Services.ProgressPaymentReportService]::CalculateReportNetPayable($current, $totals)

        Add-Comparison $Rows $Program $reportSheet 'D6' 'ReportTotals.CumulativeServiceAmount' $totals.CumulativeServiceAmount
        Add-Comparison $Rows $Program $reportSheet 'D7' 'ReportTotals.CurrentPriceDifference' $totals.CurrentPriceDifference
        Add-Comparison $Rows $Program $reportSheet 'D8' 'ReportTotals.CumulativeTotal' $totals.CumulativeTotal
        Add-Comparison $Rows $Program $reportSheet 'D10' 'Prior cumulative total (read from Excel D10)' $previousTotal
        Add-Comparison $Rows $Program $reportSheet 'D11' 'ReportTotals.CurrentTotal' $totals.CurrentTotal
        Add-Comparison $Rows $Program $reportSheet 'D15' 'ReportStampTax' ([OzelEgitimHakedis.Core.Services.ProgressPaymentReportService]::CalculateReportStampTax($calculation, $programType))
        Add-Comparison $Rows $Program $reportSheet 'D26' 'ReportTotalDeductions' $reportDeductions
        Add-Comparison $Rows $Program $reportSheet 'D28' 'ReportNetPayable' $reportNet

        $serviceBase = [OzelEgitimHakedis.Core.Services.ProgressPaymentReportService]::CalculateWorkbookMonthlyPaymentAmount($current)
        $icmalDifference = [OzelEgitimHakedis.Core.Services.ProgressPaymentReportService]::CalculateWorkbookPriceDifference($current)
        if ($programType -eq [OzelEgitimHakedis.Core.Models.TransportationProgramType]::BasicOrSecondary) {
            $icmalDifference += $calculation.TotalDeductions
        }
        $icmalTotal = $serviceBase + $icmalDifference
        $knownDuplicate = if ($programType -eq [OzelEgitimHakedis.Core.Models.TransportationProgramType]::BasicOrSecondary) {
            $priceDifference
        }
        else {
            $priceDifference + $guidePriceDifference
        }

        Add-Comparison $Rows $Program $icmalSheet 'C8' 'Icmal.WorkAmount' $serviceBase 'APPROVED_DIFFERENCE' $knownDuplicate
        Add-Comparison $Rows $Program $icmalSheet 'D8' 'Icmal.PriceDifferenceAndBasicDeductions' $icmalDifference
        Add-Comparison $Rows $Program $icmalSheet 'E8' 'Icmal.TotalAmount' $icmalTotal 'APPROVED_DIFFERENCE' $knownDuplicate
    }
    finally {
        foreach ($item in @($icmalSheet, $reportSheet, $calculationSheet, $homeSheet, $worksheets, $workbook)) {
            if ($null -ne $item -and [Runtime.InteropServices.Marshal]::IsComObject($item)) {
                [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($item)
            }
        }
    }
}

$excel = $null
$rows = [System.Collections.Generic.List[object]]::new()
try {
    $excel = New-Object -ComObject Excel.Application
    $excel.Visible = $false
    $excel.DisplayAlerts = $false
    $excel.EnableEvents = $false
    $excel.AutomationSecurity = 3 # msoAutomationSecurityForceDisable
    Compare-Workbook $excel $BasicWorkbookPath 'Temel Eğitim' $rows
    Compare-Workbook $excel $SpecialWorkbookPath 'Özel Eğitim' $rows

    $results = $rows.ToArray()
    $results | Format-Table Program, ExcelCell, ApplicationField, ExcelValue, ApplicationValue, DifferenceApplicationMinusExcel, Status -AutoSize
    $failed = @($results | Where-Object Status -eq 'FAIL')
    $approved = @($results | Where-Object Status -eq 'APPROVED_DIFFERENCE')
    Write-Output "`nPASS=$(@($results | Where-Object Status -eq 'PASS').Count); APPROVED_DIFFERENCE=$($approved.Count); FAIL=$($failed.Count)"
    Write-Output 'Excel formülleri tam yeniden hesaplandı; workbook dosyaları salt okunur açıldı ve VBA makroları kapalıydı.'

    if (-not [string]::IsNullOrWhiteSpace($ReportPath)) {
        $report = [pscustomobject]@{
            CreatedAtUtc = [DateTime]::UtcNow.ToString('O')
            ExcelVersion = $excel.Version
            MacroExecution = 'Disabled; formulas recalculated with Excel CalculateFullRebuild'
            Tolerance = $tolerance
            Workbooks = @(
                [pscustomobject]@{ Program = 'Temel Eğitim'; Path = (Resolve-Path -LiteralPath $BasicWorkbookPath).Path; Sha256 = (Get-FileHash -LiteralPath $BasicWorkbookPath -Algorithm SHA256).Hash },
                [pscustomobject]@{ Program = 'Özel Eğitim'; Path = (Resolve-Path -LiteralPath $SpecialWorkbookPath).Path; Sha256 = (Get-FileHash -LiteralPath $SpecialWorkbookPath -Algorithm SHA256).Hash }
            )
            Results = $results
            Summary = [pscustomobject]@{ Pass = @($results | Where-Object Status -eq 'PASS').Count; ApprovedDifference = $approved.Count; Fail = $failed.Count }
        }
        $report | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $ReportPath -Encoding utf8
        Write-Output "JSON raporu: $ReportPath"
    }

    if ($failed.Count -gt 0) {
        exit 1
    }
}
finally {
    if ($excel) {
        $excel.Quit()
        [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)
    }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}
