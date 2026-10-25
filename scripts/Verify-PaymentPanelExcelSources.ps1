param(
 [Parameter(Mandatory)][string]$OutputRoot,
 [string]$BasicWorkbookPath=(Join-Path $HOME 'Downloads/03-Temel Eğitim Taşıma Ödeme SON.xlsm'),
 [string]$SpecialWorkbookPath=(Join-Path $HOME 'Downloads/02-Özel Eğitim Hakediş Ödeme SON.xlsm'),
 [ValidateSet('Debug','Release')][string]$Configuration='Release'
)
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$assembly=Join-Path $repoRoot "src/OzelEgitimHakedis.Core/bin/$Configuration/net8.0/OzelEgitimHakedis.Core.dll"
if(-not(Test-Path -LiteralPath $assembly)){throw 'Build the selected configuration before running this audit.'}
foreach($source in @($BasicWorkbookPath,$SpecialWorkbookPath)){if(-not(Test-Path -LiteralPath $source)){throw "Workbook not found: $source"}}
New-Item -ItemType Directory -Force -Path $OutputRoot|Out-Null
Add-Type -Path $assembly
$inv=[Globalization.CultureInfo]::InvariantCulture
function Number($sheet,[string]$address){
 $cell=$sheet.Range($address)
 try {if($null -eq $cell.Value2){return [decimal]0}; return [decimal]::Parse([Convert]::ToString($cell.Value2,$inv),[Globalization.NumberStyles]::Float,$inv)}
 finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($cell)}
}
function Put($sheet,[string]$address,$value){$cell=$sheet.Range($address);try{if($value -is [string]){$cell.Formula=$value}else{$cell.Value2=[double]$value}}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($cell)}}
function InputNumber($sheet,[string]$address){
 # Excel inputs have 15 significant decimal digits. Do not introduce a
 # binary Value2 tail (7365.349999999999 instead of 7365.35) as new user input.
 # Output cells retain their raw Value2 for the independent tolerance check.
 $number=Number $sheet $address
 return [decimal]::Parse(([double]$number).ToString('G15',$inv),[Globalization.NumberStyles]::Float,$inv)
}
$excel=$null;$cases=[Collections.Generic.List[object]]::new();$comparisons=[Collections.Generic.List[object]]::new();$sourceTotals=[Collections.Generic.List[object]]::new()
try{
 $excel=New-Object -ComObject Excel.Application
 $excel.Visible=$false;$excel.DisplayAlerts=$false;$excel.EnableEvents=$false;$excel.AutomationSecurity=3
 foreach($program in @('BasicOrSecondary','SpecialEducation')){
  $special=$program -eq 'SpecialEducation'
  $path=if($special){$SpecialWorkbookPath}else{$BasicWorkbookPath}
  $sourceHash=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
  $book=$excel.Workbooks.Open($path,0,$true)
  try{
   $sheet=$book.Worksheets.Item($(if($special){'Hesaplama'}else{'Hesaplama Bilgileri'}))
   $excel.CalculateFullRebuild()
   $checks=if($special){@{K='TransportNetAmount';L='TransportVat';M='TransportWithholding';T='GuideNetAmount';U='GuideVat';V='GuideWithholding';X='GuideGrossAmount';AC='AdditionalGuarantee';AD='StampTax';AE='TotalDeductions';AF='GrossAmount';AG='NetPayable'}}else{@{K='TransportNetAmount';L='TransportVat';M='TransportWithholding';O='GrossAmount';T='AdditionalGuarantee';U='StampTax';V='TotalDeductions';W='NetPayable'}}
   $last=if($special){6}else{8}
   $scenarioNames=@('source','vat-exempt','fractional-days','negative-difference','discounts-and-debts','zero-input','precision-price','maximum-month-days','difference-only')
   foreach($scenario in $scenarioNames){
    if($scenario -ne 'source'){
     # Change only this read-only workbook's in-memory cells. Never save the original.
     foreach($col in @('G','H','I','J')+$(if($special){@('P','Q','R','S','Y','Z','AA','AB')}else{@('P','Q','R','S')})){Put $sheet ($col+'4') ([double]0)}
     Put $sheet E4 'Hayır';Put $sheet G4 ([double]16);Put $sheet H4 ([double]19539.1608)
     if($special){Put $sheet P4 ([double]23);Put $sheet Q4 ([double]1362.49)}
     switch($scenario){
      'vat-exempt' {Put $sheet E4 'Evet';Put $sheet I4 ([double]7530);if($special){Put $sheet R4 ([double]3500)}}
      'fractional-days' {Put $sheet G4 ([double]18.5);if($special){Put $sheet P4 ([double]21.5)}}
      'negative-difference' {Put $sheet I4 ([double]-1500);if($special){Put $sheet R4 ([double]-350)}}
      'discounts-and-debts' {Put $sheet J4 ([double]1000);Put $sheet I4 ([double]2300);if($special){Put $sheet S4 ([double]200);Put $sheet Y4 ([double]125);Put $sheet Z4 ([double]50);Put $sheet AA4 ([double]375);Put $sheet AB4 ([double]250)}else{Put $sheet P4 ([double]125);Put $sheet Q4 ([double]50);Put $sheet R4 ([double]375);Put $sheet S4 ([double]250)}}
      'zero-input' {Put $sheet G4 ([double]0);if($special){Put $sheet P4 ([double]0)}}
      'precision-price' {Put $sheet H4 ([double]1234.56789012);Put $sheet I4 ([double]987.654321);if($special){Put $sheet Q4 ([double]321.987654)}}
      'maximum-month-days' {Put $sheet G4 ([double]30);if($special){Put $sheet P4 ([double]30)}}
      'difference-only' {Put $sheet G4 ([double]0);Put $sheet I4 ([double]750);if($special){Put $sheet P4 ([double]0);Put $sheet R4 ([double]250)}}
     }
     $excel.CalculateFullRebuild()
    }
    foreach($r in $(if($scenario -eq 'source'){@(4..$last)}else{@(4)})){
     $vatCell=$sheet.Range('E'+$r);try{$exempt=$vatCell.Value2 -eq 'Evet'}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($vatCell)}
     $i=[OzelEgitimHakedis.Core.Models.ProgressPaymentInput]::new((InputNumber $sheet G$r),(InputNumber $sheet H$r),(InputNumber $sheet J$r),(InputNumber $sheet I$r),100000,$exempt,(InputNumber $sheet $(if($special){'Y'+$r}else{'P'+$r})),(InputNumber $sheet $(if($special){'AA'+$r}else{'R'+$r})),(InputNumber $sheet $(if($special){'AB'+$r}else{'S'+$r})),(InputNumber $sheet $(if($special){'Z'+$r}else{'Q'+$r})),$(if($special){InputNumber $sheet P$r}else{[decimal]0}),$(if($special){InputNumber $sheet Q$r}else{[decimal]0}),$(if($special){InputNumber $sheet S$r}else{[decimal]0}),$(if($special){InputNumber $sheet R$r}else{[decimal]0}),[Enum]::Parse([OzelEgitimHakedis.Core.Models.TransportationProgramType],$program))
     $calc=[OzelEgitimHakedis.Core.Services.ProgressPaymentCalculator]::Calculate($i)
     $expected=[ordered]@{};$rounded=[ordered]@{};$display=[ordered]@{}
     foreach($col in ($checks.Keys | Sort-Object)){
      $field=$checks[$col];$value=Number $sheet ($col+$r);$expected[$field]=$value
      $rounded[$field]=[decimal]::Parse([Convert]::ToString($excel.WorksheetFunction.Round([double]$value,2),$inv),[Globalization.NumberStyles]::Float,$inv)
      $view=$sheet.Range($col+$r);try{$display[$field]=[string]$view.Text}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($view)}
      $delta=$calc.$field-$value
      $applicationMoney=[decimal]::Parse($calc.$field.ToString('F2',$inv),$inv)
      $comparisons.Add([pscustomobject]@{Program=$program;Scenario=$scenario;Row=$r;Cell=$sheet.Name+'!'+$col+$r;Field=$field;Excel=$value;Application=$calc.$field;Delta=$delta;ExcelMoney=$rounded[$field];ApplicationMoney=$applicationMoney;ExcelDisplay=$display[$field];Pass=([decimal]::Abs($delta) -le [decimal]0.000001 -and $applicationMoney -eq $rounded[$field])})
     }
     $cases.Add([pscustomobject]@{Program=$program;Scenario=$scenario;Row=$r;Input=$i;Expected=$expected;ExpectedRounded=$rounded;ExcelDisplay=$display;Sha256=$sourceHash})
    }
    if($scenario -eq 'source'){
     foreach($col in ($checks.Keys|Sort-Object)){
      $range=$sheet.Range($col+'4:'+$col+$last)
      try{$sum=$excel.WorksheetFunction.Sum($range)}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($range)}
      $money=$excel.WorksheetFunction.Round($sum,2)
      $sourceTotals.Add([pscustomobject]@{Program=$program;Field=$checks[$col];ExcelRange=$sheet.Name+'!'+$col+'4:'+$col+$last;ExcelSum=[decimal]::Parse([Convert]::ToString($sum,$inv),[Globalization.NumberStyles]::Float,$inv);ExcelMoney=[decimal]::Parse([Convert]::ToString($money,$inv),[Globalization.NumberStyles]::Float,$inv)})
     }
    }
   }
  }finally{$book.Close($false);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($sheet);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($book)}
  if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $sourceHash){throw "Source workbook changed: $path"}
 }
 $cases | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $OutputRoot 'excel-recalculated-cases.json') -Encoding utf8
 $comparisons | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $OutputRoot 'excel-recalculated-comparisons.json') -Encoding utf8
 $sourceTotals | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $OutputRoot 'excel-source-totals.json') -Encoding utf8
 $fail=@($comparisons|Where-Object {-not $_.Pass});Write-Output "Cases=$($cases.Count); Fields=$($comparisons.Count); Fail=$($fail.Count); Excel=$($excel.Version); Macros=disabled; Originals=read-only"
 if($fail.Count){$fail|Format-Table;exit 1}
}finally{if($excel){$excel.Quit();[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)};[GC]::Collect();[GC]::WaitForPendingFinalizers()}
