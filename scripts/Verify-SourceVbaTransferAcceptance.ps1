[CmdletBinding()]
param(
 [string]$OutputRoot='.superpowers/vba-acceptance',
 [string]$BasicWorkbookPath=(Join-Path $env:USERPROFILE 'Downloads/03-Temel Eğitim Taşıma Ödeme SON.xlsm'),
 [string]$SpecialWorkbookPath=(Join-Path $env:USERPROFILE 'Downloads/02-Özel Eğitim Hakediş Ödeme SON.xlsm'),
 [switch]$KeepOriginalJulyHeaders
)
# Runs only reviewed transfer procedures in disposable copies. Honors current
# Excel macro policy; never lowers security, enables events, or changes originals.
# The extra VBA module records known dialog messages; original Module1 is checked
# byte-for-byte in its live CodeModule representation before and after execution.
function Write-QaFile {
 param([Parameter(Position=0)]$Path,[Parameter(ValueFromPipeline)]$Value,[string]$Encoding)
 begin {$qaLines=[Collections.Generic.List[string]]::new()}
 process {$qaLines.Add([string]$Value)}
 end {for($qaAttempt=0;$qaAttempt -lt 20;$qaAttempt++){try{[IO.File]::WriteAllLines($Path,$qaLines,[Text.UTF8Encoding]::new($false));return}catch [IO.IOException]{if($qaAttempt -eq 19){throw};Start-Sleep -Milliseconds 100}}}
}

$ErrorActionPreference='Stop'
$repoRoot=Split-Path -Parent $PSScriptRoot
$root=if([IO.Path]::IsPathRooted($OutputRoot)){[IO.Path]::GetFullPath($OutputRoot)}else{[IO.Path]::GetFullPath((Join-Path $repoRoot $OutputRoot))}
New-Item -ItemType Directory -Path $root -Force|Out-Null
$moduleEvidence=[Collections.Generic.List[object]]::new()
$snapshots=[Collections.Generic.List[object]]::new();$results=[Collections.Generic.List[object]]::new();$excel=$null
function Value($sheet,[string]$cell){$r=$sheet.Range($cell);try{return $r.Value2}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($r)}}
function Put($sheet,[string]$cell,$value){$r=$sheet.Range($cell);try{if($value -is [string]){$r.Formula=$value}else{$r.Value2=[double]$value}}finally{[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($r)}}
try{
 $excel=New-Object -ComObject Excel.Application
 $excel.EnableEvents=$false;$excel.DisplayAlerts=$false;$excel.Visible=$true;$excel.AutomationSecurity=2
 foreach($program in @('SpecialEducation','BasicOrSecondary')){
  $special=$program -eq 'SpecialEducation'
  $source=if($special){[IO.Path]::GetFullPath($SpecialWorkbookPath)}else{[IO.Path]::GetFullPath($BasicWorkbookPath)}
  $sourceHash=(Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
  $expectedSourceHash=if($special){'C31CB024AC1EB59F3B88D2EE12234BE98D1F07056884374D8DAD108F674D7D1E'}else{'37A7818881AAE52E8B2FCF95F30A75E1F302DFCE1E91E2345F3473668A4D5296'}
  if($sourceHash -ne $expectedSourceHash){throw 'Workbook differs from the reviewed VBA reference. Review its VBA before executing it.'}
  $copy=Join-Path $root ('OEHS_QA_BATCH_'+$program+'.xlsm');Copy-Item -LiteralPath $source -Destination $copy
  $book=$excel.Workbooks.Open($copy,0,$false)
  try{
# The diagnostic adapter records and answers known dialogs; Module1 remains unchanged.
   $qaOriginalCode=$book.VBProject.VBComponents.Item('Module1').CodeModule
   $qaBefore=$qaOriginalCode.Lines(1,$qaOriginalCode.CountOfLines)
   $qaLog=$book.Worksheets.Add();$qaLog.Name='QA_Messages'
   $qaModule=$book.VBProject.VBComponents.Add(1);$qaModule.Name='QA_DialogAdapter'
   $qaModule.CodeModule.AddFromString(@'
Option Explicit
Public Function MsgBox(Prompt As Variant, Optional Buttons As VbMsgBoxStyle = vbOKOnly, Optional Title As Variant, Optional HelpFile As Variant, Optional Context As Variant) As VbMsgBoxResult
    Dim r As Long
    r = ThisWorkbook.Worksheets("QA_Messages").Cells(Rows.Count, 1).End(xlUp).Row + 1
    ThisWorkbook.Worksheets("QA_Messages").Cells(r, 1).Value = CStr(Prompt)
    ThisWorkbook.Worksheets("QA_Messages").Cells(r, 2).Value = CStr(Title)
    If Buttons = vbQuestion + vbYesNo And (Title = "Aktarım Onayı" Or Title = "Hakediş Tutarı Aktarımı" Or Title = "Fiyat Farkı Aktarımı" Or Title = "Kesinti Aktarımı") Then
        MsgBox = vbYes
    ElseIf Buttons = vbInformation And (Title = "Bilgi" Or LCase$(CStr(Title)) = "aktarım tamamlandı") Then
        MsgBox = vbOK
    Else
        ThisWorkbook.Worksheets("QA_Messages").Cells(r, 3).Value = "UNEXPECTED"
        MsgBox = vbCancel
    End If
End Function
'@)
   if($qaBefore -cne $qaOriginalCode.Lines(1,$qaOriginalCode.CountOfLines)){throw 'Original transfer Module1 changed.'}

   $book.Activate();$qaHomeSheet=$book.Worksheets.Item('Anasayfa');$calc=$book.Worksheets.Item($(if($special){'Hesaplama'}else{'Hesaplama Bilgileri'}));$data=$book.Worksheets.Item('İcmal Verileri');$icmal=$book.Worksheets.Item('Hakediş İcmali')
   # Reset numeric test destinations only in this throwaway copy, never the original.
   foreach($range in @('D4:N103','P4:Z103')+$(if($special){@('AB4:AL103','AN4:AX103')}else{@('AB4:AL103')})){Put $data $range 0}
   if($special){
    $data.Range('AP3').Formula='=Anasayfa!I8' # Previously accepted November-header correction.
    # The first MATCH in O4 accidentally stops at May; June is in the second MATCH.
    # Extend only that source-copy lookup to June; leave the July policy unchanged.
    $qaPreviousFormula=[string]$data.Range('O4').Formula
    $data.Range('O4').Formula=$qaPreviousFormula.Replace('$I$6:$I$14','$I$6:$I$15')
   }
   # Original Basic July headers omit the year: reviewed source-copy correction.
   if(-not $special -and -not $KeepOriginalJulyHeaders){foreach($qaHeader in @("N3","Z3","AL3")){$data.Range($qaHeader).Value2="Temmuz 2027"}}
   $specs=if($special){@(@{Macro='HakedişToplamlariniAktar';Base=4;Sources=@('K4','T4')},@{Macro='FiyatFarkıVerileriniAktar';Base=16;Sources=@('I4')},@{Macro='RehberPersonelFiyatFarkıVerileriniAktar';Base=28;Sources=@('R4')},@{Macro='KesintiBilgileriniAktarAktar';Base=40;Sources=@('AE4')})}else{@(@{Macro='HakedisTutarlariniAySutununaAktar';Base=4;Sources=@('K4')},@{Macro='TasimaFiyatFarkiAktar';Base=16;Sources=@('I4')},@{Macro='KesintiToplamlariniAktar';Base=28;Sources=@('V4')})}
   foreach($period in @(@{Text='Ekim 2026';Offset=1},@{Text='Kasım 2026';Offset=2},@{Text='Aralık 2026';Offset=3},@{Text='Ocak 2027';Offset=4},@{Text='Mayıs 2027';Offset=8},@{Text='Haziran 2027';Offset=9},@{Text='Temmuz 2027';Offset=10})){
    Put $qaHomeSheet $(if($special){'E5'}else{'E4'}) $period.Text
    Put $calc G4 (10+$period.Offset);Put $calc I4 (125.25*($period.Offset+1))
    $excel.CalculateFullRebuild()
    foreach($spec in $specs){
     $col=$spec.Base+$period.Offset;$cell=$data.Cells.Item(4,$col);$target=$cell.Address($false,$false);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($cell)
     $expected=[decimal]0;foreach($src in $spec.Sources){$expected+=[decimal](Value $calc $src)}
     Put $data $target -999991
     [pscustomobject]@{Program=$program;Period=$period.Text;Macro=$spec.Macro;Target=$target;Expected=$expected;Status='Awaiting reviewed transfer macro'}|ConvertTo-Json|Write-QaFile (Join-Path $root 'batch-current-step.json') -Encoding utf8
     $null=$excel.Run("'$($book.Name)'!Module1.$($spec.Macro)")
     if($excel.WorksheetFunction.CountIf($qaLog.Range('C:C'),'UNEXPECTED') -gt 0){
      $qaLast=$qaLog.Cells.Item($qaLog.Rows.Count,1).End(-4162).Row
      $qaMessage=Value $qaLog ("A"+$qaLast)
      [pscustomobject]@{Program=$program;Period=$period.Text;Macro=$spec.Macro;Message=$qaMessage;Target=$target;Actual=(Value $data $target)}|ConvertTo-Json|Write-QaFile (Join-Path $root 'unexpected-vba-message.json') -Encoding utf8
      throw "Source VBA rejected transfer: $qaMessage"
     }
     $actual=[decimal](Value $data $target)
     $results.Add([pscustomobject]@{Program=$program;Period=$period.Text;Macro=$spec.Macro;Target=$target;Sentinel=-999991;Expected=$expected;Actual=$actual;Pass=([decimal]::Abs($actual-$expected) -lt [decimal]0.000001)})
     $results|ConvertTo-Json -Depth 6|Write-QaFile (Join-Path $root 'batch-transfer-results.json') -Encoding utf8
     if([decimal]::Abs($actual-$expected) -ge [decimal]0.000001){throw "Macro transfer did not match at $program/$($period.Text)/$target"}
    }
    $excel.CalculateFullRebuild()
    $qaReport=$book.Worksheets.Item('Hakediş Raporu');$qaReportValues=[ordered]@{}
    foreach($qaAddress in @('D6','D7','D8','D10','D11','D12','D13','D15','D26','D28')){$qaReportValues[$qaAddress]=Value $qaReport $qaAddress}
    $qaCalcValues=[ordered]@{};foreach($qaAddress in @('K4','L4','M4','T4','U4','V4','W4','X4','AC4','AD4','AE4','AF4','AG4')){$qaCalcValues[$qaAddress]=Value $calc $qaAddress}
    $snapshots.Add([pscustomobject]@{Program=$program;Offset=$period.Offset;Period=$period.Text;Calculation=$qaCalcValues;Report=$qaReportValues;E21=(Value $icmal E21)})
    $snapshots|ConvertTo-Json -Depth 8|Write-QaFile (Join-Path $root 'batch-period-snapshots.json') -Encoding utf8

    [pscustomobject]@{Program=$program;Period=$period.Text;E21=(Value $icmal E21)}|ConvertTo-Json -Compress|Add-Content (Join-Path $root 'batch-e21-results.jsonl') -Encoding utf8
   }
   $qaAfter=$qaOriginalCode.Lines(1,$qaOriginalCode.CountOfLines)
   if($qaBefore -cne $qaAfter){throw 'Transfer Module1 changed during execution.'}
   if($sourceHash -ne (Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash){throw 'Original workbook changed.'}
   $codeHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($qaBefore)))
   $moduleEvidence.Add([pscustomobject]@{Program=$program;SourceSha256=$sourceHash;Module1BeforeSha256=$codeHash;Module1AfterSha256=$codeHash;OriginalUnchanged=$true;DialogMode='Known-dialog diagnostic adapter; original transfer Module1 unchanged';CopyCorrections=$(if($special){@('AP3 November header','O4 first MATCH includes June')}else{@('N3/Z3/AL3 full July period header')})})
   $moduleEvidence|ConvertTo-Json -Depth 6|Write-QaFile (Join-Path $root 'vba-module-evidence.json') -Encoding utf8
   $book.Save()
  }finally{$book.Close($false);[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($book)}
 }
 Write-Output "Transfer checks=$($results.Count); Fail=$(@($results|Where-Object {-not $_.Pass}).Count)"
}finally{if($excel){$excel.Quit();[void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($excel)}}
