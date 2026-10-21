# OEHS — Excel → Uygulama → PDF Tam Alan Eşleme Matrisi
**Tarih:** 21.10.2026  
**Dal:** `feature/official-payment-document-templates`  
**Amaç:** İki programın her önemli hücresini uygulama hesap alanı ve PDF alanına bağlamak; bilinmeyen eşlemeleri açıkça OPEN tutmak.

> Temel Eğitim ve Özel Eğitim ayrı matrislerdir. Kaynak XLSM önbellek değeri masaüstü Excel'de bu oturumda yeniden hesaplanmış değer değildir. Programdaki icmal mükerrer fiyat farkı düzeltmesi kaynak XLSM'nin mevcut icmal D8/E8 sonucundan bilinçli olarak ayrılabilir; bu fark saklanmamalıdır.

## 1. Temel Eğitim

**Kaynak dosya:** `03-Temel Eğitim Taşıma Ödeme SON(1).xlsm`  
**SHA-256:** `37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296`

### 1.1 Hesaplama hücreleri

| Excel sayfa/hücre | Kaynak formül | Kayıtlı değer | Uygulama alanı | PDF hedefi | Durum |
|---|---|---:|---|---|---|
| `Hesaplama Bilgileri!I4` | Girdi/değer | 7.365,35 TL | `ProgressPaymentInput.PriceDifference` | Hakediş Raporu, fiyat farkı satırı / İcmal aylık tutarı | Kaynak hücre biliniyor; masaüstü yeniden hesaplama OPEN |
| `Hesaplama Bilgileri!K4` | `=(G4*H4)+I4-J4` | 319.991,9228 TL | `CalculateWorkbookMonthlyPaymentAmount` + raw I bileşeni; toplam alanı `CalculateWorkbookMonthlyPaymentTotal` | Hakediş Raporu A/B/C toplam zinciri; Hakediş İcmali İş Tutarı | Kaynak formül ve kayıtlı değer biliniyor |
| `Hesaplama Bilgileri!T4` | `=I4*6/100` | 441,921 TL | `ProgressPaymentCalculation.AdditionalGuarantee` | Hakediş Raporu “Fiyat Farkı Teminat Kesintisi” ve kesinti toplamı | Formül eşlemesi biliniyor; Excel masaüstü OPEN |
| `Hesaplama Bilgileri!U4` | `=K4*9.48/1000` | 3.033,523428144 TL | `ProgressPaymentCalculation.StampTax` | Hakediş Raporu damga vergisi/kesintiler | Formül eşlemesi biliniyor; Excel masaüstü OPEN |
| `Hesaplama Bilgileri!V4` | `=P4+Q4+R4+S4+T4+U4+M4` | 19.475,040568144 TL | `ProgressPaymentCalculation.TotalDeductions` | Hakediş Raporu H ve İcmal kesinti bölümü | Formül eşlemesi biliniyor |
| `Hesaplama Bilgileri!W4` | `=O4-V4` | 332.516,074511856 TL | `ProgressPaymentCalculation.NetPayable` temel hesap adayı | Hakediş Raporu net ödenecek alanı | D28 ile bire bir eşit değil; aşağıdaki notu uygula |

### 1.2 Rapor neti: W4 ile D28

| Excel hedefi | Kaynak/kayıt | Uygulama testi | PDF alanı | Karar |
|---|---|---|---|---|
| `Hesaplama Bilgileri!W4` | 332.516,074511856 TL | Hesaplama neti | — | Kaynak kayıtlı değer |
| `Hakediş Raporu!D28` | Test vektöründe 332.516,08 TL | `ProgressPaymentReportService.CalculateReportNetPayable` | Hakediş Raporu “Yükleniciye Ödenecek Tutar (G-H)” | D28=W4 bire bir eşitliği kanıtlanmadı; doğrudan kopyalama yapılmaz |

W4'ün iki ondalık basamağa normal yuvarlanması 332.516,07 TL iken kaynak D28 test beklentisi 332.516,08 TL'dir. Bu fark nedeniyle D28, W4'ün yalnızca biçimlendirilmiş hali gibi ele alınmamalı. Raporun D13/D26 gibi ara hücrelerinde ayrı ayrı yuvarlama yapılıyorsa, uygulama D28'in kaynak formülünü taklit etmelidir. Gerçek Excel hücre formülleri/masaüstü sonucu ile son kabul yapılana kadar eşleme “formül bazlı; masaüstü OPEN” durumundadır.

### 1.3 İcmal hücreleri ve mükerrer bileşen düzeltmesi

| Excel sayfa/hücre | Kaynak formül / kayıt | Kayıtlı değer | Yeni uygulama alanı | Durum |
|---|---|---:|---|---|
| `İcmal Verileri!R4` | Kaynak aylık fiyat farkı bloğu | 7.365,35 TL | İcmal fark sütununa ikinci kez eklenmeyecek | Mükerrer bileşen kanıtlandı |
| `İcmal Verileri!AD4` | Kesinti toplamıyla eşleşen blok | 19.475,040568144 TL | `Calculation.TotalDeductions` bileşeni olarak korunur | Bileşen eşleşmesi kaynak önbelleğiyle biliniyor |
| `Hakediş İcmali!C8` | Kaynakta K toplamı | 319.991,9228 TL | `CalculateWorkbookMonthlyPaymentAmount` (K'den ham I çıkarılmış baz) | Uygulama “İş Tutarı (a)” sütununda baz tutarı gösterir |
| `Hakediş İcmali!D8` | `VLOOKUP(...,15+MATCH)+VLOOKUP(...,27+MATCH)` | 26.840,390568144 TL | I fiyat farkı + AD kesinti = 26.840,390568144 TL | Fiyat farkı bir kez; kesinti bileşeni korunur |
| `Hakediş İcmali!E8` | `=C8+D8` | Kaynakta 346.832,313368144 TL | Yeni uygulama: 339.466,963368144 TL | Kaynak E8'den 7.365,35 TL düşük; C'den I ayrılıp D'ye bir kez taşındı |

Temel Eğitimde yeni uygulama C sütununda K'den ham I fiyat farkını ayırır; D sütununda I'yi bir kez ve kesinti bileşenini korur. Böylece A+B, K+kesinti olur; alttaki kesinti çıkarıldığında hizmet tutarı K ile tutarlı kalır. Kaynak XLSM C8 zaten K'yi içerirken D8'e I+AD eklediği için kaynak E8'den 7.365,35 TL fazla çıkar. Bu fark bilinçli ve raporlanmıştır.

## 2. Özel Eğitim

**Kaynak dosya:** `02-Özel Eğitim Hakediş Ödeme SON(1).xlsm`  
**SHA-256:** `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e`

### 2.1 Hesaplama hücreleri

| Excel sayfa/hücre | Kaynak formül | Kayıtlı değer | Uygulama alanı | PDF hedefi | Durum |
|---|---|---:|---|---|---|
| `Hesaplama!I4` | Girdi/değer | 7.530,00 TL | `ProgressPaymentInput.PriceDifference` | Hakediş Raporu ve aylık hizmet toplamı | Kaynak hücre biliniyor |
| `Hesaplama!K4` | `=(G4*H4)+I4-J4` | 43.630,00 TL | Taşıma aylık tutarı + raw I; `CalculateWorkbookMonthlyPaymentTotal` | Hakediş Raporu / İcmal | Kaynak kayıtlı değer |
| `Hesaplama!T4` | `=(P4*Q4)+R4-S4` | 34.037,27 TL | Rehber aylık tutarı + raw R; `CalculateWorkbookMonthlyPaymentTotal` | Hakediş Raporu / İcmal | Kaynak kayıtlı değer |
| `Hesaplama!AC4` | `=(I4*6/100)+(R4*6/100)` | 661,80 TL | `ProgressPaymentCalculation.AdditionalGuarantee` | Hakediş Raporu teminat kesintisi | Kaynak formül; uygulama eşlemesi |
| `Hesaplama!AD4` | `=(K4*9.48/1000)+(T4*9.48/1000)` | 736,2857196 TL | `ProgressPaymentCalculation.StampTax` | Hakediş Raporu damga vergisi | Kaynak formül; uygulama eşlemesi |
| `Hesaplama!AE4` | `=M4+V4+Y4+Z4+AA4+AB4+AC4+AD4` | 27.328,7843196 TL | `ProgressPaymentCalculation.TotalDeductions` | Hakediş Raporu H / İcmal kesinti | Kaynak kayıtlı değer |
| `Hesaplama!AF4` | `=O4+X4` | 88.837,724 TL | Brüt/ödenek toplamı bileşeni | Hakediş Raporu ara toplamı | Kaynak kayıtlı değer |
| `Hesaplama!AG4` | `=AF4-AE4` | 61.508,9396804 TL | `ProgressPaymentCalculation.NetPayable` | Hakediş Raporu net alanı | D28 iki ondalık yuvarlama nedeniyle bire bir eşit değil |

### 2.2 Rapor neti: AG4 ile D28

| Excel hedefi | Kaynak/kayıt | Uygulama testi | PDF alanı | Karar |
|---|---|---|---|---|
| `Hesaplama!AG4` | 61.508,9396804 TL | Hesaplama neti | — | Kaynak kayıtlı değer |
| `Hakediş Raporu!D28` | Test vektöründe 61.508,94 TL | `ProgressPaymentReportService.CalculateReportNetPayable` | Hakediş Raporu “Yükleniciye Ödenecek Tutar (G-H)” | Rapor değeri kaynak formül/yuvarlama yoluyla üretilir; AG4'e doğrudan bağlandığı varsayılmaz |

D28'in iki ondalık görünümü AG4'ün yuvarlanmış karşılığıyla bu örnekte eşleşir; fakat kaynak testlerde D28'in D13/D26 gibi alanları ayrı yuvarlayarak hesaplandığı belgelenmiştir. Bu nedenle aynı değer görünse bile formül eşdeğerliği kanıtlanmadan doğrudan hücre eşlemesi yapılmaz.

### 2.3 İcmal hücreleri ve mükerrer bileşen düzeltmesi

| Excel sayfa/hücre | Kaynak formül / kayıt | Kayıtlı değer | Yeni uygulama alanı | Durum |
|---|---|---:|---|---|
| `İcmal Verileri!R4` | Rehber fiyat farkı bloğu | 3.500,00 TL | İkinci kez eklenmeyecek | Mükerrer bileşen kanıtlandı |
| `İcmal Verileri!I4` | Taşıma fiyat farkı bloğu | 7.530,00 TL | İkinci kez eklenmeyecek | Mükerrer bileşen kanıtlandı |
| `Hakediş İcmali!C8` | Kaynakta K+T toplamı | 77.667,27 TL | `CalculateWorkbookMonthlyPaymentAmount` (K+T'den ham I/R çıkarılmış baz) | Uygulama “İş Tutarı (a)” sütununda baz tutarı gösterir |
| `Hakediş İcmali!D8` | İki aylık fiyat farkı bloğu toplamı | 11.030,00 TL | Fiyat farkı I+R = 11.030,00 TL | I/R baz hizmet tutarından ayrılıp ayrı sütunda bir kez gösterilir |
| `Hakediş İcmali!E8` | `=C8+D8` | Kaynakta 88.697,27 TL | Yeni uygulama: 77.667,27 TL | Kaynak E8'den 11.030 TL düşük; çift I/R eklemesi kaldırıldı |

Özel Eğitimde yeni uygulama K+T'den ham I/R fiyat farklarını ayırır; fark sütununda I/R'yi bir kez gösterir. Böylece a+b toplamı K+T ile eşleşir. Kesintiler icmalin alt kesinti bölümünde ayrı hesaplanmaya devam eder.

## 3. Dönem/kimlik alanları — tüm sekiz PDF için ortak kontrol şeması

Aşağıdaki alanlar her programın kendi bağlamından doldurulmalı. Bu tabloda henüz kaynak örneğinden kesin hücre/alan değeri alınmamış alanlar OPEN bırakılmıştır.

| Alan | Kaynak Excel adresi/formülü | Uygulama kaynağı | PDF'ler | Beklenen doğrulama |
|---|---|---|---|---|
| Program türü | Kaynak dosyanın kendisi | `ProgressPaymentInput.ProgramType` | 4 PDF | Temel/Özel belge ailesi karışmamalı |
| Dönem | `Anasayfa!I9` ve ilgili dönem hücreleri; tam hücre eşlemesi OPEN | `PeriodDate` | 4 PDF | Gün/ay/yıl aynı |
| İKN | Kaynak hücre eşlemesi OPEN | `TenderNumber` | 4 PDF | Tam metin eşitliği |
| Sözleşme no/tarihi | Kaynak hücre eşlemesi OPEN | `ContractNumber`, `ContractDate` | Hizmet İşleri raporu ve diğer ilgili PDF'ler | Tam metin/tarih eşitliği |
| Yüklenici | Kaynak hücre eşlemesi OPEN | `ContractorName` | 4 PDF | Tam unvan eşitliği |
| Hat numarası/adı | Kaynak hücre eşlemesi OPEN | `RouteNumber`, `RouteName` | 4 PDF | Yalnız seçilen hat(lar) |
| Hakediş neti | Temel `W4/D28`; Özel `AG4/D28` | `ReportNetPayable` ve kaynak rapor formülü | Hakediş Raporu | D28 formülüne göre eşleşme |
| İcmal baz hizmeti | `Hakediş İcmali!C8` | `CalculateWorkbookMonthlyPaymentAmount` | Hakediş İcmali | Kaynak toplamdan fiyat farkı ayrılmış baz tutar; program bazlı kıyas |
| İcmal fark sütunu | `Hakediş İcmali!D8` | `BuildHakedisIcmali` içindeki `difference` | Hakediş İcmali | Kullanıcı kararıyla ikinci fiyat farkı kaldırılmış; kaynak farkı ayrıca gösterilmeli |
| İcmal toplamı | `Hakediş İcmali!E8` | İcmal A/B/C toplamı | Hakediş İcmali | Yeni program sonucu + kaynak Excel farkı birlikte raporlanmalı |
| İmza/unvan/tarih | Kaynak şablon ve onaylı ayar; hücre adresleri OPEN | Belge bağlamı ve imza ayarları | 4 PDF | Görsel yerleşim ve metin doğrulaması |
| A4/tek sayfa | Kaynak PDF şablonu | `FixedDocument` / PDF dışa aktarımı | 8 PDF | A4, her PDF tek sayfa |

## 4. Otomatik karşılaştırma çıktısı

Her satır için aşağıdaki alanlar test raporuna yazılmalıdır:

`Program | XLSM SHA256 | Dönem | Hat | Excel sayfa!hücre | Excel formülü | Excel yeniden hesaplanan değer | uygulama alanı | uygulama değeri | PDF adı/sayfa/alan | PDF değeri | fark | PASS/FAIL/OPEN`

- Tutarlar sayısal kıyaslanırken yuvarlama politikası hücre/alan bazında belirtilmelidir; genel tolerans kullanılarak formül farkı gizlenmemelidir.
- Kimlik alanları (İKN, sözleşme, yüklenici, hat, dönem) tam eşitlik ile kontrol edilmelidir.
- PDF metin kıyası sonrası görsel kabul yapılmalıdır.
- Herhangi bir kaynak formülünün gerçek masaüstü sonucu alınmadıysa hücre satırı OPEN kalmalıdır.

## 5. Açık kabul kapıları

- Temel Eğitim ve Özel Eğitim gerçek Microsoft Excel tam hesaplama: OPEN.
- Gerçek VBA compile/makro/UserForm çalışma testi: OPEN.
- D28 ara hücrelerinin formül zinciri ve gerçek Excel değerleri: OPEN.
- Özel Eğitim AP3/AQ3 için Eylül-Ağustos 12 aylık kaynak başlık/aktarımı: OPEN.
- Sekiz PDF'nin gerçek Excel PDF'siyle alan bazlı karşılaştırması: OPEN.
- Program icmal düzeltmesi sonrası kaynak XLSM D8/E8 ile bilerek ayrışma kaydı: belgelendi; resmî Excel kaynak dosyası değiştirilmedi.
- CI: kod/test commit'i için run sonucu ayrıca aynı SHA ile kaydedilmeli.
