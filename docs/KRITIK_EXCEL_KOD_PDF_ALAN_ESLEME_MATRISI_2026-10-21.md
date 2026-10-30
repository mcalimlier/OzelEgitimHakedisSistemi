# OEHS — Kritik Alan Eşleme Matrisi (21.10.2026)
**Dal:** `feature/official-payment-document-templates`  
**Amaç:** Önce ödeme tutarı ve dönem doğrulaması; ardından kimlik ve tüm belge alanlarına genişletme. Temel ve Özel Eğitim satırları birbirinden bağımsızdır.

## Durum anahtarı
- **Kaynak biliniyor:** Son XLSM'de formül/değer daha önce incelendi.
- **Kod eşlendi:** Uygulama kodunda ilgili kaynak mantığına göre eşleme mevcut.
- **Test ekli:** İlgili otomatik regresyon testi kaynak koduna eklendi.
- **OPEN:** Gerçek Microsoft Excel tam yeniden hesaplaması ve/veya Excel'in gerçek PDF çıktısıyla alan bazlı karşılaştırma bekleniyor.

> Kayıtlı hücre önbelleği gerçek Excel masaüstü yeniden hesaplaması değildir. D15/D28 gibi rapor alanları, hücre formülü ve etiketleri doğrudan kaynak dosyadan tekrar okunmadan kesin kabul edilmez.

## 1. Temel Eğitim / Ortaöğretim

Kaynak: `03-Temel Eğitim Taşıma Ödeme SON(1).xlsm`  
SHA-256: `37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296`

| Öncelik | Excel sayfa/hücre | Formül / kayıtlı değer | Uygulamadaki karşılık | PDF alanı | Durum / kontrol |
|---|---|---|---|---|---|
| Kritik | `Hesaplama Bilgileri!I4` | 7.365,35 TL | `ProgressPaymentInput.PriceDifference` | Hakediş Raporu fiyat farkı satırı; icmal fiyat farkı | Kaynak biliniyor; gerçek Excel yeniden hesaplama OPEN |
| Kritik | `Hesaplama Bilgileri!K4` | `=(G4*H4)+I4-J4` | `CalculateWorkbookMonthlyPaymentTotal` bileşenleri | Hakediş Raporu yapılan iş / bu hakediş | Kaynak formülü içerir; aynı I tutarı ikinci kez eklenmemeli |
| Kritik | `Hesaplama Bilgileri!T4` | `=I4*6/100` | `ProgressPaymentCalculator.Calculate` ek kesin teminat | Hakediş Raporu ek kesin teminat kesintisi | Test vektörü mevcut; gerçek Excel/VBA OPEN |
| Kritik | `Hesaplama Bilgileri!U4` | `=K4*9.48/1000` | Damga vergisi hesap zinciri | Hakediş Raporu damga vergisi | Test vektörü mevcut; D15 rapor zinciri ayrıca kontrol edilir |
| Kritik | `Hesaplama Bilgileri!V4` | `=P4+Q4+R4+S4+T4+U4+M4` | Toplam kesinti bileşenleri | Hakediş Raporu kesinti toplamı | Kaynak formülü biliniyor; tüm alt alanlarla karşılaştırma OPEN |
| Kritik | `Hesaplama Bilgileri!W4` | `=O4-V4` | `Calculation.NetPayable` | Hakediş Raporu D28 adayı | **Karar uyuşmazlığı:** son kaynak örneğinde ham W4 332.516,074511856 TL; rapor D28 332.516,08 TL. D28 rapor zincirinin ayrı yuvarlaması korunuyor; gerçek Excel yeniden hesaplamasıyla karar kapatılmalı |
| Kritik | `Hakediş Raporu!D15` | Formül/değer kaynak XLSM'den yeniden kaydedilmeli | `CalculateReportStampTax` | Hakediş Raporu damga vergisi | Kodda Temel Eğitim negatif matrahı sıfırlama davranışı var; gerçek hücre formülüyle kabul OPEN |
| Kritik | `Hakediş Raporu!D28` | Kaynak rapor zinciri, ayrı yuvarlanmış rapor toplamlarına dayanabilir | `CalculateReportNetPayable` (Basic dalı) | Yükleniciye ödenecek tutar | Kaynak örneği 332.516,08 TL; doğrudan W4 eşitliği kanıtlanmış değil |
| Kritik | `İcmal Verileri!R4` | 7.365,35 TL | Aylık ham fiyat farkı | Hakediş İcmali fiyat farkı bileşeni | Aynı I fiyat farkının tekrar eklenmesi önlenmeli |
| Kritik | `İcmal Verileri!AD4` | 19.475,040568144 TL | Ayrı icmal kesinti bloğu | Hakediş İcmali ek blok | Bu bileşen korunur; yalnızca tekrarlanan I çıkarılır |
| Kritik | `Hakediş İcmali!C8` | Kayıtlı 319.991,9228 TL | Aylık hizmet tabanı | İcmal iş tutarı sütunu | Fiyat farkı ayrıştırılmış gösterimle test edilir |
| Kritik | `Hakediş İcmali!D8` | Kayıtlı 26.840,390568144 TL | Fiyat farkı + kaynak icmalinin ayrı kesinti bileşeni | İcmal fiyat farkı/ek tutar sütunu | R4 çift sayılmamalı; AD4 korunmalı |
| Kritik | `Hakediş İcmali!E8` | Kayıtlı 346.832,313368144 TL | İcmal satır/toplam zinciri | İcmal toplamı | Düzeltme sonrası tanısal toplam 339.466,963368144 TL; bu bir kabul tutarı değil, mükerrer I bileşeninin çıkarılmasıyla oluşan kontrol sonucudur |

## 2. Özel Eğitim

Kaynak: `02-Özel Eğitim Hakediş Ödeme SON(1).xlsm`  
Özgün SHA-256: `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e`  
AP3 düzeltmeli test kopyası SHA-256: `2afc7ae6b183237e5c6367396a652bac6d67cb4fd552e986a53ed280a6024189`

| Öncelik | Excel sayfa/hücre | Formül / kayıtlı değer | Uygulamadaki karşılık | PDF alanı | Durum / kontrol |
|---|---|---|---|---|---|
| Kritik | `Hesaplama!I4` | 7.530 TL | `ProgressPaymentInput.PriceDifference` | Hakediş Raporu fiyat farkı | Kaynak biliniyor; yeniden hesaplama OPEN |
| Kritik | `Hesaplama!K4` | `=(G4*H4)+I4-J4`; kayıtlı 43.630 TL | Taşıma hizmeti + fiyat farkı | Hakediş Raporu / icmal | Kaynak zinciri biliniyor |
| Kritik | `Hesaplama!T4` | `=(P4*Q4)+R4-S4`; kayıtlı 34.037,27 TL | Rehber hizmeti + fiyat farkı | Hakediş Raporu / icmal | Temel Eğitim'den ayrı zincir |
| Kritik | `Hesaplama!AC4` | `=(I4*6/100)+(R4*6/100)` | Ek kesin teminat | Hakediş Raporu kesinti satırı | Test vektörü var; gerçek Excel OPEN |
| Kritik | `Hesaplama!AD4` | `=(K4*9.48/1000)+(T4*9.48/1000)` | Damga vergisi | Hakediş Raporu damga vergisi | Test vektörü var; gerçek Excel OPEN |
| Kritik | `Hesaplama!AE4` | `=M4+V4+Y4+Z4+AA4+AB4+AC4+AD4` | Toplam kesintiler | Hakediş Raporu kesinti toplamı | Kaynak formülü biliniyor |
| Kritik | `Hesaplama!AF4` | `=O4+X4` | Brüt tutar | Hakediş Raporu tahakkuk/ödenecek tutar zinciri | Kaynak formülü biliniyor |
| Kritik | `Hesaplama!AG4` | `=AF4-AE4` | `Calculation.NetPayable` | Hakediş Raporu D28 | D28, AG4 netinin iki ondalıklı para hassasiyetinde kullanıldığı kod/test eşlemesiyle doğrulanır; masaüstü Excel OPEN |
| Kritik | `Hakediş Raporu!D15` | Formül/değer tekrar kaynak dosyadan kaydedilmeli | Damga vergisi rapor formülü | D15 damga vergisi | Gerçek hücre formülü ile otomatik kabul OPEN |
| Kritik | `Hakediş Raporu!D28` | Kaynak rapor net alanı | `CalculateReportNetPayable` Special dalı | Yükleniciye ödenecek tutar | AG4'ün iki ondalıklı değeriyle eşleme |
| Kritik | `İcmal Verileri!R4` | 7.530 TL | Taşıma fiyat farkı | İcmal fiyat farkı | C8 hizmet tabanı içinde fiyat farkı varsa ikinci kez toplanmamalı |
| Kritik | `İcmal Verileri!AD4` | 3.500 TL | Rehber fiyat farkı bloğu | İcmal fiyat farkı | R4 ile birlikte K+T içinde tekrar sayılmamalı |
| Kritik | `Hakediş İcmali!C8` | Kayıtlı 77.667,27 TL | Aylık K+T toplamı | İcmal iş tutarı/toplamı | Kaynak K+T ham I+R fiyat farklarını içeriyor |
| Kritik | `Hakediş İcmali!D8` | Kayıtlı 11.030 TL | Ham I+R fiyat farkı | İcmal fiyat farkı | Aynı I+R ikinci kez eklenmemeli |
| Kritik | `Hakediş İcmali!E8` | Kayıtlı 88.697,27 TL | İcmal toplamı | İcmal toplamı | Tanısal düzeltilmiş toplam 77.667,27 TL; masaüstü Excel kabulü olmadan resmî tutar sayılmaz |

## 3. Ay eşleme testi — Özel Eğitim

Kaynak ay zinciri:
- `Anasayfa!I8` = Kasım + seçilen yıl
- `Anasayfa!I9` = Aralık + seçilen yıl
- Özgün XLSM: `İcmal Verileri!AP3 = Anasayfa!I9`, `AQ3 = Anasayfa!I9` (hatalı çift Aralık)
- Düzeltilmiş kopya: `AP3 = Anasayfa!I8` (Kasım), `AQ3 = Anasayfa!I9` (Aralık)

Otomatik test `ProgressPaymentIcmalServiceTests` içinde Eylül–Ağustos 12 aylık geçişi ve özellikle Kasım→Aralık→Ocak önceki dönem eşlemesini kontrol eder. Bu bir uygulama regresyonudur; gerçek XLSM'de formülün masaüstü Excel'de yeniden hesaplandığını tek başına kanıtlamaz.

## 4. Kimlik ve dönem alanları — sekiz PDF

Her programın dört PDF'si için ortak kimlik matrisi:
- Dönem/ay/yıl ← seçili hakediş `PeriodDate` ve kaynak Anasayfa dönem seçimi
- İKN ← `TenderNumber`
- Sözleşme no ← `ContractNumber`
- Yüklenici unvanı ← `ContractorName`
- Hat numarası/adı ← `RouteNumber` / `RouteName`
- Hakediş sıra numarası ← `ProgressPaymentNumber`
- Sözleşme bedeli/tarihleri ← sözleşme kaydı
- İmza adları/unvanları ← belge ayarları
- Tutarlar ← yukarıdaki programa özel hesaplama ve rapor servisleri

Her PDF için otomatik karşılaştırma: metin alanı/para tutarı/dönem/İKN/yüklenici/hat; ardından A4 tek sayfa ve görsel kontrol. Mevcut WPF testleri A4/tek sayfa ve belirli alanları kontrol eder; gerçek Excel kaynak PDF'leriyle 8/8 alan bazlı kabul hâlâ **OPEN**.

## 5. Şu anki kabul durumu

| Alan | Temel Eğitim | Özel Eğitim |
|---|---|---|
| İcmal fiyat farkı kodu | Kod değişikliği yapıldı; CI/regresyon sonucu bekleniyor | Kod değişikliği yapıldı; CI/regresyon sonucu bekleniyor |
| D28 net alanı | Kaynak vektörüyle 1 kuruş fark var; mevcut rapor formülü korundu; karar OPEN | AG4'ün iki ondalık değeriyle eşleme kodlandı; masaüstü Excel OPEN |
| Ay eşlemesi | Uygulanamaz | AP3 Kasım düzeltmesi ayrı XLSM kopyasında; 12 aylık regresyon eklendi |
| Sekiz PDF otomatik alan karşılaştırması | OPEN | OPEN |
| Gerçek Excel/VBA çalıştırma | OPEN | OPEN |
| CI | Son commit için sonucu doğrulanmalı | Son commit için sonucu doğrulanmalı |
