Attribute VB_Name = "Module1"

Sub HakediþToplamlariniAktar()

    Dim ay As String
    Dim i As Long
    Dim toplam As Double
    Dim hedefSutun As Long
    If MsgBox("Hakediþ Toplamlarý Aktarýlsýn mý?", vbQuestion + vbYesNo, "Aktarým Onayý") <> vbYes Then Exit Sub
    
    ' Anasayfa E5 hücresindeki ayý al
    ay = Trim$(CStr(Sheets("Anasayfa").Range("E5").Value)): ay = Left$(ay & " ", InStr(1, ay & " ", " ") - 1): If ay = "" Then MsgBox "Ödeme Yapýlan Dönem'i Anasayfa sayfasýnda seçin.", vbExclamation: Exit Sub
    
    ' Ay ismine göre Ýcmal Verileri sayfasýnda doðru sütunu bul
    hedefSutun = GetIcmalMonthTargetColumn(ay, 4)
    If hedefSutun = 0 Then
        MsgBox "'" & ay & "' geçerli bir ödeme dönemi deðil. Anasayfa!E5 hücresini kontrol edin.", vbExclamation, "Aktarým"
        Exit Sub
    End If
    
    ' Satýr bazlý döngü: 4’ten 103’e kadar
    For i = 4 To 103
        toplam = Sheets("Hesaplama").Cells(i, 11).Value + _
                 Sheets("Hesaplama").Cells(i, 20).Value
        
        ' Toplamý Ýcmal Verileri sayfasýnda ilgili satýr ve ay sütununa aktar
        Sheets("Ýcmal Verileri").Cells(i, hedefSutun).Value = toplam
    Next i
    
    ' Ýþlem tamamlandýðýnda bilgi mesajý göster
    MsgBox ay & " Ayýna Ait Hakediþ Toplamlarýnýn Aktarma Ýþlemi Tamamlanmýþtýr.", vbInformation, "Bilgi"
    
    
End Sub



Sub FiyatFarkýVerileriniAktar()

    Dim ay As String
    Dim i As Long
    Dim toplam As Double
    Dim hedefSutun As Long
    
    ' Anasayfa E5 hücresindeki ayý al
    ay = Trim$(CStr(Sheets("Anasayfa").Range("E5").Value)): ay = Left$(ay & " ", InStr(1, ay & " ", " ") - 1): If ay = "" Then MsgBox "Ödeme Yapýlan Dönem'i Anasayfa sayfasýnda seçin.", vbExclamation: Exit Sub
    
    ' Ay ismine göre Ýcmal Verileri sayfasýnda doðru sütunu bul
    hedefSutun = GetIcmalMonthTargetColumn(ay, 16)
    If hedefSutun = 0 Then
        MsgBox "'" & ay & "' geçerli bir ödeme dönemi deðil. Anasayfa!E5 hücresini kontrol edin.", vbExclamation, "Aktarým"
        Exit Sub
    End If
    
    ' Satýr bazlý döngü: 4’ten 103’e kadar
    For i = 4 To 103
        toplam = Sheets("Hesaplama").Cells(i, 9).Value
                         
        ' Toplamý Ýcmal Verileri sayfasýnda ilgili satýr ve ay sütununa aktar
        Sheets("Ýcmal Verileri").Cells(i, hedefSutun).Value = toplam
    Next i
    
    ' Ýþlem tamamlandýðýnda bilgi mesajý göster
    MsgBox ay & " Ayýna Ait Fiyat Farký Verilerinin Aktarma Ýþlemi Tamamlanmýþtýr.", vbInformation, "Bilgi"
    
End Sub


Sub RehberPersonelFiyatFarkýVerileriniAktar()

    Dim ay As String
    Dim i As Long
    Dim toplam As Double
    Dim hedefSutun As Long
    
    ' Anasayfa E5 hücresindeki ayý al
    ay = Trim$(CStr(Sheets("Anasayfa").Range("E5").Value)): ay = Left$(ay & " ", InStr(1, ay & " ", " ") - 1): If ay = "" Then MsgBox "Ödeme Yapýlan Dönem'i Anasayfa sayfasýnda seçin.", vbExclamation: Exit Sub
    
    ' Ay ismine göre Ýcmal Verileri sayfasýnda doðru sütunu bul
    hedefSutun = GetIcmalMonthTargetColumn(ay, 28)
    If hedefSutun = 0 Then
        MsgBox "'" & ay & "' geçerli bir ödeme dönemi deðil. Anasayfa!E5 hücresini kontrol edin.", vbExclamation, "Aktarým"
        Exit Sub
    End If
    
    ' Satýr bazlý döngü: 4’ten 103’e kadar
    For i = 4 To 103
        toplam = Sheets("Hesaplama").Cells(i, 18).Value
                         
        ' Toplamý Ýcmal Verileri sayfasýnda ilgili satýr ve ay sütununa aktar
        Sheets("Ýcmal Verileri").Cells(i, hedefSutun).Value = toplam
    Next i
    
    ' Ýþlem tamamlandýðýnda bilgi mesajý göster
    MsgBox ay & " Ayýna Ait Rehber Personel Fiyat Farký Verilerinin Aktarma Ýþlemi Tamamlanmýþtýr.", vbInformation, "Bilgi"
    
End Sub


Sub KesintiBilgileriniAktarAktar()

    Dim ay As String
    Dim i As Long
    Dim toplam As Double
    Dim hedefSutun As Long
    If MsgBox("Kesinti bilgileri aktarýlsýn mý?", vbQuestion + vbYesNo, "Aktarým Onayý") <> vbYes Then Exit Sub
    ' Anasayfa E5 hücresindeki ayý al
    ay = Trim$(CStr(Sheets("Anasayfa").Range("E5").Value)): ay = Left$(ay & " ", InStr(1, ay & " ", " ") - 1): If ay = "" Then MsgBox "Ödeme Yapýlan Dönem'i Anasayfa sayfasýnda seçin.", vbExclamation: Exit Sub
    
    ' Ay ismine göre Ýcmal Verileri sayfasýnda doðru sütunu bul
    hedefSutun = GetIcmalMonthTargetColumn(ay, 40)
    If hedefSutun = 0 Then
        MsgBox "'" & ay & "' geçerli bir ödeme dönemi deðil. Anasayfa!E5 hücresini kontrol edin.", vbExclamation, "Aktarým"
        Exit Sub
    End If
    
    ' Satýr bazlý döngü: 4’ten 103’e kadar
    For i = 4 To 103
        toplam = Sheets("Hesaplama").Cells(i, 31).Value
                         
        ' Toplamý Ýcmal Verileri sayfasýnda ilgili satýr ve ay sütununa aktar
        Sheets("Ýcmal Verileri").Cells(i, hedefSutun).Value = toplam
    Next i
    
    ' Ýþlem tamamlandýðýnda bilgi mesajý göster
    MsgBox ay & " Ayýna Ait Kesinti Tutarlarýnýn Aktarma Ýþlemi Tamamlanmýþtýr.", vbInformation, "Bilgi"
    
End Sub

Sub SayfaYazdir()
    Application.ScreenUpdating = False

    MsgBox "Yazdýrma iþlemi baþlatýlýyor. Lütfen bekleyin...", vbInformation, "Bilgi"

    ThisWorkbook.Sheets("Hakediþ Raporu").PrintOut
    ThisWorkbook.Sheets("Hizmetiþleri Hakediþ Raporu").PrintOut
    ThisWorkbook.Sheets("Hizmet Ýþleri K. Tutanaðý").PrintOut
    ThisWorkbook.Sheets("Hakediþ Ýcmali").PrintOut

    ThisWorkbook.Sheets("Anasayfa").Activate
    Application.ScreenUpdating = True

    MsgBox "Seçilen sayfalar baþarýyla yazdýrýldý!", vbInformation, "Bilgi"
End Sub

Sub TumFiyatFarkiAktarimlari()
If MsgBox("Taþýma ve rehber personel fiyat farký verileri aktarýlsýn mý?", vbQuestion + vbYesNo, "Aktarým Onayý") <> vbYes Then Exit Sub
    ' Önce fiyat farký verilerini aktar
    Call FiyatFarkýVerileriniAktar
    
    ' Sonra rehber personel fiyat farký verilerini aktar
    Call RehberPersonelFiyatFarkýVerileriniAktar
    
    ' Ýþlem tamamlandýðýnda bilgi mesajý
    MsgBox "Fiyat farký ve rehber personel verileri baþarýyla aktarýldý!", vbInformation, "Bilgi"

End Sub

Sub HesapKontrol()
    Dim ws As Worksheet
    Dim d8Value As Variant
    Dim d10Value As Variant
    Dim d11Value As Variant
    Dim oncekiTutar As Variant
    Dim fark As Variant

    Set ws = ThisWorkbook.Worksheets("Hakediþ Raporu")
    ws.Range("D11").Interior.Color = vbWhite

    d8Value = ws.Range("D8").Value2
    d10Value = ws.Range("D10").Value2
    d11Value = ws.Range("D11").Value2

    If IsError(d8Value) Then
        MsgBox "D8 hücresinde hesaplama hatasý var.", vbExclamation, "Kontrol Sonucu"
        Exit Sub
    End If
    If Not IsNumeric(d8Value) Then
        MsgBox "D8 hücresinde sayýsal bir toplam bulunamadý.", vbExclamation, "Kontrol Sonucu"
        Exit Sub
    End If

    If IsError(d10Value) Then
        MsgBox "D10 hücresinde önceki hakediþler için hesaplama hatasý var.", vbExclamation, "Kontrol Sonucu"
        Exit Sub
    End If
    If Len(Trim$(CStr(d10Value))) = 0 Then
        oncekiTutar = 0
    ElseIf IsNumeric(d10Value) Then
        oncekiTutar = CDec(d10Value)
    Else
        MsgBox "D10 hücresinde sayýsal bir önceki hakediþ tutarý bulunamadý.", vbExclamation, "Kontrol Sonucu"
        Exit Sub
    End If

    If IsError(d11Value) Then
        MsgBox "D11 hücresinde hesaplama hatasý var.", vbExclamation, "Kontrol Sonucu"
        Exit Sub
    End If
    If Not IsNumeric(d11Value) Then
        MsgBox "D11 hücresinde sayýsal bir hakediþ tutarý bulunamadý.", vbExclamation, "Kontrol Sonucu"
        Exit Sub
    End If

    fark = CDec(d8Value) - CDec(oncekiTutar)
    If CDec(fark) = CDec(d11Value) Then
        MsgBox "Hesaplama doðru!", vbInformation, "Kontrol Sonucu"
        ws.Range("D11").Interior.Color = vbWhite
    Else
        MsgBox "Hesaplama hatalý!", vbCritical, "Kontrol Sonucu"
        ws.Range("D11").Interior.Color = vbRed
    End If
End Sub


Private Function DonemSiraNo(ByVal wsAna As Worksheet, ByRef donem As String) As Long
    Dim bulunanDonem As Range

    donem = Trim$(CStr(wsAna.Range("E4").Value2))

    If Len(donem) = 0 Then Exit Function

    Set bulunanDonem = wsAna.Range("I5:I14").Find( _
        What:=donem, _
        After:=wsAna.Range("I14"), _
        LookIn:=xlValues, _
        LookAt:=xlWhole, _
        SearchOrder:=xlByRows, _
        SearchDirection:=xlNext, _
        MatchCase:=False)

    If Not bulunanDonem Is Nothing Then
        DonemSiraNo = bulunanDonem.Row - wsAna.Range("I5").Row + 1
    End If
End Function

Sub AyVerileriniSil()

    Dim seciliAy As String
    Dim wsAnasayfa As Worksheet
    Dim wsIcmal As Worksheet
    
    ' Sayfalarý tanýmla
    Set wsAnasayfa = Worksheets("Anasayfa")
    Set wsIcmal = Worksheets("Ýcmal Verileri")
    
    ' Anasayfa E4 hücresindeki ayý al
    seciliAy = wsAnasayfa.Range("E5").Value
    
    ' Ýlgili aralýklarý temizle
    wsIcmal.Range("D4:N103").ClearContents
    wsIcmal.Range("P4:Z103").ClearContents
    wsIcmal.Range("AB4:AL103").ClearContents
    wsIcmal.Range("AN4:AX103").ClearContents
    
    ' Bilgilendirme mesajý
    MsgBox seciliAy & " ayýna ait veriler silindi.", vbInformation

End Sub

Public Function GetIcmalMonthTargetColumn(ByVal ay As String, ByVal ilkSutun As Long) As Long
    Dim monthOffset As Long
    Select Case Trim$(ay)
        Case "Eylül": monthOffset = 0
        Case "Ekim": monthOffset = 1
        Case "Kasým": monthOffset = 2
        Case "Aralýk": monthOffset = 3
        Case "Ocak": monthOffset = 4
        Case "Þubat": monthOffset = 5
        Case "Mart": monthOffset = 6
        Case "Nisan": monthOffset = 7
        Case "Mayýs": monthOffset = 8
        Case "Haziran": monthOffset = 9
        Case "Temmuz": monthOffset = 10
        Case Else
            GetIcmalMonthTargetColumn = 0
            Exit Function
    End Select
    Select Case ilkSutun
        Case 4, 16, 28, 40: GetIcmalMonthTargetColumn = ilkSutun + monthOffset
        Case Else: GetIcmalMonthTargetColumn = 0
    End Select
End Function
