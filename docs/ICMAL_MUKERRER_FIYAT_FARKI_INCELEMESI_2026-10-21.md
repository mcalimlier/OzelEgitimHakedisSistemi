# OEHS — İcmal Mükerrer Fiyat Farkı İncelemesi ve Düzeltme Kapısı

**Tarih:** 21.10.2026  
**Dal:** `feature/official-payment-document-templates`  
**Başlangıç referansı:** `ae0d2a3c4c220ee1f2c0c0b9c0ec993aa8f8f5e1`  
**Kapsam:** Aşama 5 — Temel Eğitim ve Özel Eğitim icmal tutar zinciri  
**Kaynak önceliği:** Son XLSM hash'leri ve kayıtlı hücre/formülleri; bu belge gerçek Excel/VBA masaüstü kabulü değildir.

## 1. Sürüm ve kaynak güvenliği

- GitHub dalı bulundu: `feature/official-payment-document-templates`.
- Başlangıç commit'i bulundu: `ae0d2a3c4c220ee1f2c0c0b9c0ec993aa8f8f5e1`.
- PR #3 açık, taslak ve birleştirilmemiş durumda; base dalı `feature/wpf-contract-entry-v0.5`.
- Başlangıç commit'i için mevcut combined-status sorgusu boş durum döndürdü; bu commit için yeni bir CI sonucu PASS olarak raporlanamaz.
- Temel Eğitim XLSM SHA-256: `37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296`.
- Özel Eğitim XLSM SHA-256: `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e`.
- Kaynak XLSM dosyalarına dokunulmadı.

## 2. Formül zinciri ve kanıt

### Temel Eğitim

Kaynak formül özeti:
- `Hesaplama Bilgileri!K4 = (G4*H4)+I4-J4`; K4, I4 fiyat farkı bileşenini zaten içerir.
- `İcmal Verileri!R4 = 7.365,35 TL` ve `AD4 = 19.475,040568144 TL`.
- `Hakediş İcmali!C8 = 319.991,9228 TL`.
- `Hakediş İcmali!D8 = R4 + AD4 = 26.840,390568144 TL`.
- `Hakediş İcmali!E8 = C8 + D8 = 346.832,313368144 TL`.

D8'in R4 bileşeni, C8'deki K4 üzerinden zaten dahil edilmiş I4 fiyat farkını tekrar ekliyor. Kaynakta ayrıca AD4 kesintisi de aynı D8 sütununa ekleniyor; bu da sütun başlığı olan “Fiyat Farkı Tutarı” ile içerik arasında anlam uyuşmazlığı oluşturuyor.

Karşılaştırma hesabı — **iş kuralı kararı değil**:
- Fiyat farkı ikinci kez eklenmez ve kesinti ayrı bir bileşen olarak korunursa: C8 + AD4 = **339.466,963368144 TL**.
- Kaynak E8 ile fark: **7.365,35 TL**, R4 ile tam aynı.

### Özel Eğitim

Kaynak formül özeti:
- `Hesaplama!K4 = 43.630,00 TL`.
- `Hesaplama!T4 = 34.037,27 TL`.
- Aktarım tutarı K4+T4 = **77.667,27 TL**; taşıma ve rehber fiyat farkı bileşenleri bu tutarlara zaten dahildir.
- `Hakediş İcmali!C8 = 77.667,27 TL`.
- `İcmal Verileri!I4 = 7.530,00 TL` ve `R4 = 3.500,00 TL`.
- `Hakediş İcmali!D8 = I4 + R4 = 11.030,00 TL`.
- `Hakediş İcmali!E8 = C8 + D8 = 88.697,27 TL`.

Burada da D8'in 11.030 TL fiyat farkı, C8'de K4+T4 üzerinden bulunan hizmet toplamına yeniden ekleniyor.

Karşılaştırma hesabı — **iş kuralı kararı değil**:
- Fiyat farkı ikinci kez eklenmezse C8 = **77.667,27 TL**.
- Kaynak E8 ile fark: **11.030,00 TL**, I4+R4 ile tam aynı.

## 3. Aritmetik doğrulama

| Test | Temel Eğitim | Özel Eğitim |
|---|---:|---:|
| C8 kayıtlı değer + D8 kayıtlı değer = E8 | PASS | PASS |
| D8'in iki kaynak bileşeniyle eşleşmesi | PASS | PASS |
| C8'in aylık hizmet toplamında fiyat farkı bulunması | PASS | PASS |
| D8'de fiyat farkının tekrar eklenmesi riski | FAIL — risk doğrulandı | FAIL — risk doğrulandı |
| Gerçek Excel/VBA masaüstü yeniden hesaplama | AÇIK | AÇIK |
| Resmî kabul edilmiş alternatif tutar | AÇIK | AÇIK |

“Risk doğrulandı”, aritmetik bağımlılıkta aynı fiyat farkının iki yerde bulunduğu anlamına gelir; kaynak Excel'in iş kuralı/çıktısı değiştirilmiş veya resmî olarak geçersiz ilan edilmiş değildir.

## 4. Kod yolu incelemesi ve uygulanan düzeltme

Kullanıcının 21.10.2026 tarihli açık kararı: **kanıtlanan mükerrer fiyat farkı bileşenini programda düzelt**. Kaynak XLSM dosyaları değiştirilmedi.

`src/OzelEgitimHakedis.Wpf/OehsOfficialDocumentFactory.cs` içindeki `BuildHakedisIcmali` artık icmal tablosunun sütun anlamlarını koruyacak şekilde ayrıştırır:
- `İş Tutarı (a)` için `CalculateWorkbookMonthlyPaymentAmount` kullanılır; ham I/R fiyat farkı bu baz tutara dahil edilmez.
- `Fiyat Farkı Tutarı (b)` için `CalculateWorkbookPriceDifference` kullanılır; I veya I+R bir kez gösterilir.
- Temel Eğitim kaynak aktarımında D bloğunda bulunan kesinti bileşeni `Calculation.TotalDeductions` olarak ayrıca korunur.
- Satır toplamı `a+b` ile oluşur. Böylece hizmet tutarı + fiyat farkı, K (Temel) veya K+T (Özel) toplamına denk gelir; aynı I/R farkı ikinci kez eklenmez.
- İcmal alt bölümündeki kesinti düşümü ayrı tutulur.

### Düzeltilmiş uygulama örnekleri

**Temel Eğitim**
- Kaynak K/I: K = 319.991,9228 TL, I = 7.365,35 TL.
- Uygulama sütun A: K-I = **312.626,5728 TL**.
- Uygulama sütun B: I + AD kesinti = 7.365,35 + 19.475,040568144 = **26.840,390568144 TL**.
- Uygulama A+B = **339.466,963368144 TL**.
- Kaynak XLSM E8 = **346.832,313368144 TL**; fark **7.365,35 TL**, yani I fiyat farkı kaynak C8'de ve D8'de tekrar sayılıyordu.

**Özel Eğitim**
- Kaynak K+T = **77.667,27 TL**; I+R = 7.530 + 3.500 = **11.030,00 TL**.
- Uygulama sütun A: (K+T)-(I+R) = **66.637,27 TL**.
- Uygulama sütun B: I+R = **11.030,00 TL**.
- Uygulama A+B = **77.667,27 TL**.
- Kaynak XLSM E8 = **88.697,27 TL**; fark **11.030,00 TL**, yani I/R farkı kaynak C8'de ve D8'de tekrar sayılıyordu.

Bu tutarlar kaynak hücre önbelleklerinden yapılan aritmetik doğrulamalardır; masaüstü Excel'in bu oturumda yeniden hesaplandığı anlamına gelmez.

## 5. Düzeltme sonrası test ve güvenli kabul planı

1. Temel Eğitim ve Özel Eğitim için ayrı regresyon vektörleri çalıştırılır.
2. Her satırda `a = aylık K veya K+T - ham I/R`, `b = ham I/R + (yalnız Temel Eğitim için kaynakta bulunan kesinti bloğu)`, `a+b` ve alt kesinti çıkarımı doğrulanır.
3. PDF metin testleri yalnızca beklenen tutarları değil, mükerrer toplamların artık görünmediğini de kontrol eder.
4. Kaynak XLSM'nin D8/E8 değeri ile uygulama sonucu ayrı sütunlarda gösterilir. Kaynak XLSM değişmediği için bu fark “kullanıcı kararıyla uygulanan mükerrer fiyat farkı düzeltmesi” olarak kaydedilir.
5. Kaynak Excel PDF'siyle tam eşitlik iddia edilmez; kaynak formül davranışı ve düzeltilmiş uygulama sonucu yan yana raporlanır.
6. Gerçek Excel/VBA yeniden hesaplaması, 12 aylık ay eşlemesi ve sekiz PDF'nin alan bazlı/görsel kabulü tamamlanana kadar ilgili kapılar `OPEN` kalır.
7. Windows CI yalnızca test ettiği commit SHA için PASS sayılır; masaüstü Excel kabulünün yerine geçmez.

## 6. Çalışma kapsamı dışında kalan kabul kapıları

- Temel Eğitim ve Özel Eğitim hesaplama çekirdekleri, ayrı tutulmaya devam eder.
- 20+ sentetik hesaplama regresyonunun tümü bu döngüde yeniden çalıştırılmadı.
- VBA Project Compile ve gerçek makro/UserForm runtime testi yapılmadı.
- Sekiz nihai PDF'nin gerçek Excel çıktılarıyla alan-alan ve görsel karşılaştırması tamamlanmadı.
- %100 kabul veya tahmini ilerleme yüzdesi verilmedi.
- PR #3 açık/taslak kalmalı; merge ve yayınlama yapılmamalıdır.

## Sonraki başlangıç noktası

Önce kaynak Excel kopyasında VBA aktarımından önce/sonra icmal hücrelerini karşılaştır; ardından kaynak hücreleriyle `BuildHakedisIcmali` hesaplamasını bire bir eşleştir. İki programın formül zincirlerini birleştirme. Üretim formülü değişikliği ancak kaynak davranışı ve resmî tutar kabulü için izlenebilir kanıt oluştuğunda yapılmalıdır.
