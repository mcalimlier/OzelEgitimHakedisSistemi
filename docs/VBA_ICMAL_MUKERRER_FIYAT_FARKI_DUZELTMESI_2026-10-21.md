# OEHS — Kaynak VBA İcmal Mükerrer Fiyat Farkı Düzeltme Kaydı

**Tarih:** 21.10.2026  
**Dal:** `feature/official-payment-document-templates`  
**Önceki doğrulanmış HEAD:** `f533de53345a2294f877b0c085a73584c850587a`

## 1. Kaynak VBA'da doğrulanan hata

Dışa aktarılmış Özel Eğitim `Module1` içinde `HakedişToplamlariniAktar` yordamı:

```vb
toplam = Sheets("Hesaplama").Cells(i, 11).Value + _
         Sheets("Hesaplama").Cells(i, 20).Value
```

yani `K + T` değerini `İcmal Verileri` aylık hizmet tutarı alanına yazıyor. Kaynak formül zincirinde K taşıma fiyat farkı I'yi, T rehber fiyat farkı R'yi zaten içeriyor. Aynı modüldeki `FiyatFarkıVerileriniAktar` ve `RehberPersonelFiyatFarkıVerileriniAktar` ise I ve R'yi ayrı fiyat farkı bloklarına aktarıyor. Dolayısıyla hizmet tutarı alanı K+T olarak kalırsa I/R ikinci kez toplanır.

Dışa aktarılmış Temel Eğitim `Module1` içindeki `HakedisTutarlariniAySutununaAktar` yordamı K değerini hizmet tutarı alanına yazar. Kaynak K formülü I fiyat farkını içerdiği, I ise ayrı fiyat farkı alanına aktarıldığı için burada da I mükerrerleşir.

Bu tespit, kayıtlı dışa aktarılmış VBA metni ve kaynak hücre ilişkilerine dayanır; masaüstü Excel'de VBA IDE derlemesi/çalıştırması değildir.

## 2. Değiştirilecek hesap ifadeleri

Yalnızca hizmet tutarı aktarım ifadesi değiştirilmeli; ayrı fiyat farkı aktarım yordamları değiştirilmemelidir.

### Temel Eğitim — `HakedisTutarlariniAySutununaAktar`

Mevcut ifade:

```vb
wsIcmal.Cells(bulunanHat.Row, ayBasligi.Column).Value = _
    wsHesap.Cells(r, "K").Value
```

Aday düzeltme:

```vb
' K fiyat farkı I'yi içerir; I ayrı icmal bloğuna aktarılır.
' Hizmet tutarı sütununa fiyat farkı hariç K-I yaz.
wsIcmal.Cells(bulunanHat.Row, ayBasligi.Column).Value = _
    CDec(wsHesap.Cells(r, "K").Value2) - _
    CDec(wsHesap.Cells(r, "I").Value2)
```

### Özel Eğitim — `HakedişToplamlariniAktar`

Mevcut ifade:

```vb
toplam = Sheets("Hesaplama").Cells(i, 11).Value + _
         Sheets("Hesaplama").Cells(i, 20).Value
```

Aday düzeltme:

```vb
' K ve T fiyat farkları I ve R'yi zaten içerir.
' I ve R ayrı bloklarda gösterildiği için hizmet tabanından çıkar.
toplam = CDec(Sheets("Hesaplama").Cells(i, 11).Value2) - _
         CDec(Sheets("Hesaplama").Cells(i, 9).Value2) + _
         CDec(Sheets("Hesaplama").Cells(i, 20).Value2) - _
         CDec(Sheets("Hesaplama").Cells(i, 18).Value2)
```

> Bu değişiklikler **aday VBA yamasıdır**; özgün XLSM dosyalarına aktarılmamıştır. Orijinalleri bu ortamdan doğrudan değiştirmek veya Excel/VBA IDE çalıştırılmış gibi göstermek güvenli/doğru değildir. Yama, çalışma kopyasına uygulanıp Excel'de doğrulandıktan sonra kabul edilebilir.

## 3. Kaynak vektörleriyle aritmetik kabul

### Temel Eğitim

- Kaynak `K4 = 319.991,9228`
- Kaynak `I4 = 7.365,35`
- Hizmet tabanı: `K4-I4 = 312.626,5728`
- Ayrı fark/kesinti bloğu: `R4+AD4 = 26.840,390568144`
- Düzeltilmiş satır toplamı: `339.466,963368144`
- Eski mükerrer toplam: `346.832,313368144`

### Özel Eğitim

- Kaynak `K4+T4 = 77.667,27`
- Kaynak `I4+R4 = 11.030,00`
- Hizmet tabanı: `K4+T4-I4-R4 = 66.637,27`
- Ayrı fark: `11.030,00`
- Düzeltilmiş satır toplamı: `77.667,27`
- Eski mükerrer toplam: `88.697,27`

Para hesapları Excel kaynak zincirinde ara adımlarda yuvarlanmadan korunmalıdır. `CDec` yalnız VBA'da sayısal tür dönüşümüdür; yeni `ROUND` kuralı değildir. Kaynak hücre boş/hatali metin içeriyorsa `CDec` hata verebileceğinden, çalışma kopyasında hata senaryosu ayrıca test edilmelidir.

## 4. Uygulama tarafındaki mevcut düzeltme ve kanıt

OEHS WPF `BuildHakedisIcmali` hizmet tabanını `CalculateWorkbookMonthlyPaymentAmount` ile, ayrı fiyat farkını `CalculateWorkbookPriceDifference` ile hesaplıyor. Temel Eğitim için kaynak kesinti bloğu ayrıca korunuyor. Kaynak vektör regresyon testleri `IcmalDuplicatePriceDifferenceRegressionTests.cs` içinde bulunuyor; WPF çıktı testleri ayrıca Özel ve Temel Eğitim icmal satırlarının farkı yalnız bir kez göstermesini doğruluyor.

## 5. Kabul kapıları

- Kaynak XLSM kopyasında VBA IDE `Debug > Compile`: **AÇIK**
- Makroların Excel'de çalıştırılması ve kritik hücrelerin kaydı: **AÇIK**
- Önceki/sonraki dönemlerin etkilenmediği regresyon testi: **AÇIK**
- 8 resmi PDF'nin Excel referanslarıyla alan bazlı karşılaştırması: **AÇIK**
- Özgün XLSM dosyalarının değişmeden kaldığının hash doğrulaması: bu patch kaydıyla yeniden çalıştırılmadı; **AÇIK**
