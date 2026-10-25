[CmdletBinding()]
param(
 [string]$OutputRoot='.superpowers/pdf-acceptance',
 [string]$BasicWorkbookPath=(Join-Path $env:USERPROFILE 'Downloads/03-Temel Eğitim Taşıma Ödeme SON.xlsm'),
 [string]$SpecialWorkbookPath=(Join-Path $env:USERPROFILE 'Downloads/02-Özel Eğitim Hakediş Ödeme SON.xlsm')
)
# Same-input acceptance copies only. Source workbooks are never saved or unblocked.
# Known source fixes: November header; duplicate icmal difference; service delivery
# start date; stale Special report period (frozen signature date is an explicit exception). All financial expected values are
# recalculated by Excel, never copied from OEHS outputs.
$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$qaRoot=if([IO.Path]::IsPathRooted($OutputRoot)){[IO.Path]::GetFullPath($OutputRoot)}else{[IO.Path]::GetFullPath((Join-Path $repoRoot $OutputRoot))}
New-Item -ItemType Directory -Path $qaRoot -Force|Out-Null
$excel=$null;$cases=[Collections.Generic.List[object]]::new()
function CellValue($ws,$a){$r=$ws.Range($a);try{return $r.Value2}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($r)}}
function Update-QaDrawingDates($shape){
 if($shape.Type -eq 6){foreach($child in @($shape.GroupItems)){Update-QaDrawingDates $child}}
 try{
  $text=[string]$shape.TextFrame2.TextRange.Text
  $corrected=$text.Replace('24.06.2026','25.10.2026').Replace('30.04.2026','30.11.2026')
  if($text -cne $corrected){$shape.TextFrame2.TextRange.Text=$corrected}
 }catch{if($shape.Type -eq 17){throw}} # Text boxes must not silently lose a requested correction.
}
try{
 $excel=New-Object -ComObject Excel.Application
 $excel.EnableEvents=$false;$excel.DisplayAlerts=$false;$excel.Visible=$false;$excel.AutomationSecurity=3
 foreach($program in 'SpecialEducation','BasicOrSecondary'){
  $sp=$program -eq 'SpecialEducation';$name=if($sp){'02-Özel Eğitim Hakediş Ödeme SON.xlsm'}else{'03-Temel Eğitim Taşıma Ödeme SON.xlsm'}
  $source=if($sp){[IO.Path]::GetFullPath($SpecialWorkbookPath)}else{[IO.Path]::GetFullPath($BasicWorkbookPath)}
  $qaOriginalHash=(Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
  $qaExpectedHash=if($sp){'C31CB024AC1EB59F3B88D2EE12234BE98D1F07056884374D8DAD108F674D7D1E'}else{'37A7818881AAE52E8B2FCF95F30A75E1F302DFCE1E91E2345F3473668A4D5296'}
  if($qaOriginalHash -ne $qaExpectedHash){throw 'Workbook differs from the reviewed 2026 acceptance reference.'}
  $copy=Join-Path $qaRoot ('PDF_SOURCE_'+$program+'.xlsm');Copy-Item -LiteralPath $source -Destination $copy
  $book=$excel.Workbooks.Open($copy,0,$false)
  try{
   $hs=$book.Worksheets.Item('Anasayfa');$ts=$book.Worksheets.Item('İhale Verileri');$ds=$book.Worksheets.Item('İcmal Verileri');$cs=$book.Worksheets.Item($(if($sp){'Hesaplama'}else{'Hesaplama Bilgileri'}));$ics=$book.Worksheets.Item('Hakediş İcmali')
   $rr=if($sp){2}else{3};$ts.Range("B$rr").Value2='Kaynak Test Güzergâhı';$ts.Range("C$rr").Value2='Örnek Yüklenici'
   $hs.Range($(if($sp){'E6'}else{'E5'})).Value2=46320 # Home date is not the Special report drawing date; corrected separately below
   if($sp){$ds.Range('AP3').Formula='=Anasayfa!I8'}
   for($row=6;$row -le 16;$row++){
    $old=[string]$ics.Range("C$row").Formula
    $index='MATCH(SUBSTITUTE($B'+$row+',"Hizmet Alımı İşi ",""),''İcmal Verileri''!$D$3:$N$3,0)'
    $raw='IFERROR(INDEX(''İcmal Verileri''!$P$4:$Z$4,1,'+$index+'),0)'
    if($sp){$raw+=' + IFERROR(INDEX(''İcmal Verileri''!$AB$4:$AL$4,1,'+$index+'),0)'}
    $ics.Range("C$row").Formula2='=IF($B'+$row+'="","",('+ $old.Substring(1)+')-('+ $raw+'))'
   }
   $qaReferenceDate=[datetime]::new(2026,10,25).ToOADate()
   foreach($qaSheet in @($book.Worksheets)){
    if($qaSheet.Name -match 'Hizmet.*Hakediş Raporu'){
     $qaSheet.Range('F15').Formula=$(if($sp){"='İhale Verileri'!O2"}else{"='İhale Verileri'!J3"})
     $qaSheet.Range('F3').Value2=$qaReferenceDate
     $qaSheet.Range($(if($sp){'A41'}else{'A42'})).Value2=$qaReferenceDate
    }
    if($qaSheet.Name -match 'K. Tutanağı'){$qaSheet.Range('A31').Value2=$qaReferenceDate}
    if($qaSheet.Name -eq 'Hakediş İcmali'){$qaSheet.Range('A36').Value2=$qaReferenceDate}
   }
   $qaReport=$book.Worksheets.Item('Hakediş Raporu')
   if($sp){
    $qaReport.Range('A4').Value2='30.11.2026 TARİHİNE KADAR YAPILAN İŞİN'
    foreach($qaShape in @($qaReport.Shapes)){Update-QaDrawingDates $qaShape}
   }
   $excel.CalculateFullRebuild()
   $sheets=@($book.Worksheets)|Where-Object {$_.Name -match 'Hakediş Raporu|Hakediş İcmali|K. Tutanağı'}
   $allSheets=[ordered]@{}
   foreach($qaSheet in @($qaReport,$ics)){
    $qaFields=[ordered]@{}
    $qaAddresses=if($qaSheet.Name -eq 'Hakediş Raporu'){@('D6','D7','D8','D10','D11','D12','D13','D14','D15','D16','D17','D18','D19','D20','D21','D22','D23','D24','D25','D26','D28')}else{@('C6','D6','E6','C7','D7','E7','C8','D8','E8','C17','D17','E17','E21','E22','E23','E25')}
    foreach($qaAddress in $qaAddresses){$qaCell=$qaSheet.Range($qaAddress);$qaFields[$qaAddress]=[ordered]@{Value=$qaCell.Value2;Text=[string]$qaCell.Text;Formula=[string]$qaCell.Formula};[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($qaCell)}
    $allSheets[$qaSheet.Name]=$qaFields
   }
   if($qaOriginalHash -ne (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash){throw 'Original workbook changed.'}
   foreach($s in $sheets){$pdf=Join-Path $qaRoot ($program+'_Excel_'+$s.Name.Replace(' ','_')+'.pdf');$s.ExportAsFixedFormat(0,$pdf)}
   $cases.Add([ordered]@{Program=$program;SourceSha256=(Get-FileHash $source).Hash;ReportPeriodHeader=(CellValue $qaReport 'A4');Sheets=$allSheets})
   $cases|ConvertTo-Json -Depth 12|Set-Content (Join-Path $qaRoot 'corrected-pdf-source-export.json') -Encoding utf8
  }finally{$book.Close($false)}
 }
 'Source PDF export complete.'
}finally{if($excel){$excel.Quit();[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)}}
