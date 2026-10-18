# Yeni Güncel XLSM Kaynak İncelemesi — 18.10.2026

## Kaynak önceliği

Bu iki dosya, kullanıcının açık talimatıyla OEHS hesaplama ve belge davranışı için yeni öncelikli Excel kaynaklarıdır. Önceki çalışma kitapları bunlarla çelişirse eski davranış otomatik olarak korunmaz. Orijinal yüklemeler salt okunur incelenmiştir; hiçbir hücreye yazılmamıştır.

## Dosya bütünlüğü

| Dosya | Bayt | SHA-256 |
|---|---:|---|
| Özel Eğitim Hakediş Ödeme Programı En Güncel.xlsm | 120328 | `80679ad9773bdf4ac746bfbd80c122948546d0d5e1ab4f018ae7eb79b27bbbe2` |
| Temel Eğitim Taşıma Ödemes Programı En Güncel.xlsm | 676539 | `cb8256408b6fff5c7e0ba09ec369c36e45c350844106f371001392f06409168c` |

## Sayfa ve formül envanteri

### Özel Eğitim — 8 sayfa, 293 formül hücresi

| Sayfa | Kullanılan aralık | Dolu hücre | Formül hücresi |
|---|---|---:|---:|
| Anasayfa | A1:AE23 | 80 | 34 |
| İhale Verileri | A1:P4 | 58 | 9 |
| Hesaplama | A1:AI11 | 153 | 95 |
| Hakediş Raporu | A1:V42 | 61 | 19 |
| Hizmetişleri Hakediş Raporu | A1:J45 | 106 | 13 |
| Hizmet İşleri K. Tutanağı | A1:Z35 | 31 | 11 |
| Hakediş İcmali | A1:M46 | 94 | 54 |
| İcmal Verileri | A1:AY208 | 113 | 58 |

Elle giriş açıklaması: `Hesaplama!G11` hücresinde taşıma iş günü ve rehber personel iş günü değerlerinin Anasayfa `E3` ve `E4` alanlarına elle girildiği belirtiliyor.

### Temel Eğitim — 11 sayfa, 237 formül hücresi

| Sayfa | Kullanılan aralık | Dolu hücre | Formül hücresi |
|---|---|---:|---:|
| Anasayfa | A1:AD22 | 78 | 34 |
| İhale Verileri | A1:N8 | 46 | 3 |
| Hesaplama Bilgileri | A1:Y17 | 98 | 42 |
| Hakediş Raporu | A1:U42 | 61 | 21 |
| Hizmet İşleri Hakediş Raporu | A1:J47 | 107 | 21 |
| Hizmet İşleri K. Tutanağı | A1:AB35 | 31 | 19 |
| Hakediş İcmali | A1:E46 | 94 | 51 |
| İcmal Verileri | A1:AM6 | 88 | 45 |
| Ayarlar | A1:A21 | 14 | 0 |
| Data | A1:B14 | 26 | 0 |
| Users | A1:K37 | 14 | 1 |

Elle giriş açıklaması: `Hesaplama Bilgileri!H17` hücresinde taşıma iş günü bilgisinin Anasayfa `E3` alanına elle girildiği belirtiliyor. Temel Eğitim dosyasında `Data` sayfası veryHidden; ayrıca `Ayarlar` ve `Users` sayfaları mevcut. Yetki/login yapısı ve gömülü kullanıcı verileri güvenlik incelemesine alınmalı. Kimlik bilgileri bu belgeye kopyalanmamıştır.

## İlk kritik formül bulguları

### 1. Ay başlıkları ve önceki dönem toplamı

Her iki dosyada da `İcmal Verileri!D3:M3` Eylül-Haziran aylarını bağlıyor, `N3` Temmuz metnini içeriyor. `O4:O...` önceki dönem toplamı formülünde:

- Özel Eğitim: `MATCH(Anasayfa!$E$5,Anasayfa!$I$5:$I$14,0)` ve `SUM($D4:INDEX($D4:$L4,1,MATCH(Anasayfa!$E$5,Anasayfa!$I$6:$I$15,0)-1))`
- Temel Eğitim: aynı mantık, fakat dönem seçimi `Anasayfa!$E$4` üzerinden.

Formülün başlık eşleşmesi `I5:I14` iken toplama indeksinin `I6:I15` kullanması incelenmesi gereken bir ofset farkı. Temmuzun N sütununda olması ve toplama aralığının D:L ile sınırlı kalması nedeniyle Temmuz seçiminin davranışı örneklerle doğrulanmalı. Kodda Temmuz hakkında değişiklik yapmadan önce bu formülün Excel'deki gerçek sonucu belirlenmeli.

### 2. Kesinti/fiyat farkı toplamı programlar arasında farklı

- Özel Eğitim `İcmal Verileri!AA4:AA...`: `IF(Anasayfa!$E$4="Eylül",0,SUM(P4:Z4))`
- Özel Eğitim `İcmal Verileri!AY4:AY...`: `IF(Anasayfa!$E$4="Eylül",0,SUM(AN4:AX4))`
- Temel Eğitim `İcmal Verileri!AA4:AA...`: `SUM(P4:Z4)`
- Temel Eğitim `İcmal Verileri!AM4:AM...`: `SUM(AB4:AL4)`

Eylül koşulu yalnızca Özel Eğitim'de görülüyor. Bu fark ortak metoda zorla birleştirilmemeli; makroların hangi ay sütunlarına yazdığı ve Eylül başlangıcındaki icmal davranışıyla beraber test edilmeli.

### 3. Hakediş raporundaki D11 / “C - D” ilişkisi

Her iki raporda da D11 formülü `=IF(D7<0,H2-ABS(D7),H2+D7)`; etiket ise “Bu Hakedişin Tutarı (C - D)” anlamında. D8 `=D6+D7`, D10 ise önceki hakediş toplamına bağlanıyor.

- Özel Eğitim D7, taşıma ve rehber fiyat farkını iki ayrı VLOOKUP ile topluyor.
- Temel Eğitim D7 tek taşıma fiyat farkı alanını okuyor.
- Özel Eğitim D12 iki KDV alanını topluyor; Temel Eğitim D12 tek taşıma KDV'sini okuyor.

Bu nedenle D11'in anlamı yalnızca etiketinden türetilmemeli. D6 dizi formülü, H2, D10 ve gerçek Excel sonuçları aynı seçili dönem için birlikte incelenmeli. İki programın hesaplama zincirleri ayrı tutulmalı.

### 4. Önbellek hatası

İki dosyada da `Anasayfa!AA2` hücresinin saklanan sonucu `#VALUE!` olarak okundu. Formül/nesne bağlantısı incelenmeden bu hata zararsız kabul edilmemeli. Çalışma kitapları tam yeniden hesaplama istiyor (`fullCalcOnLoad` ve `forceFullCalc` etkin). Python/openpyxl formülleri çalıştırmaz; LibreOffice sonucu da Excel masaüstüyle aynı sayılmaz. Excel içinde yeniden hesaplama ve kaynak bağlantısı kontrolü açık doğrulama adımıdır.

## VBA ikili dosyaları ve çözümleme durumu

| Dosya | VBA ikili dosyası | Bayt | SHA-256 |
|---|---|---:|---|
| Özel Eğitim | `xl/vbaProject.bin` | 78336 | `077f23997466126fb6ffd11be55a26b137c0f698ea584b2875a325191932d516` |
| Temel Eğitim | `xl/vbaProject.bin` | 256512 | `faffb20c4501f87f1cdf28c6c855975ef264bb79c7c3859f50a1fc9946eb8c0d` |

Ham byte/strings taraması şu yordam adlarını doğruladı; bu liste **tam makro dökümü değildir**:

- Özel Eğitim: `HakedişToplamlariniAktar`, `FiyatFarkıVerileriniAktar`, `RehberPersonelFiyatFarkıVerileriniAktar`, `KesintiBilgileriniAktarAktar`, `TumFiyatFarkiAktarimlari`, `AyVerileriniSil`, `Worksheet_Change`, `HesapKontrol` dizgeleri/prosedür adları taramada görüldü.
- Temel Eğitim: `HakedisToplamlariniAktar`, `FiyatFarkiVerileriniAktar`, `NetOdenenVeIsGunuAktar`, `HakedisTutarlariniAySutununaAktar`, `TasimaFiyatFarkiAktar`, `KesintiToplamlariniAktar`, `AyVerileriniSil`, `Workbook_Open`, `Worksheet_Change`, `LoginForm` ve login akışına ilişkin adlar taramada görüldü.

VBA projesi binary olarak mevcut; bu inceleme turunda VBA kaynak modüllerinin tüm gövdeleri eksiksiz çözümlenmiş değildir. “Hiçbir makro atlanmadı” kabulü için her modül/yordamın tam kaynak dökümü, satır sayısı/hash karşılaştırması ve çağrı grafiği gereklidir. Kaynak çözümlenmeden makro davranışı hakkında kesin hüküm verilmemelidir.

## Devam planı / kabul kapıları

1. VBA projesini tam çözümleyip modül, prosedür, olay yordamı ve çağrı ilişkilerinin eksiksiz envanterini çıkar.
2. Formül dökümünü her sayfa için hücre adresi, formül, saklı değer, biçim ve bağlı ad/nesne ile karşılaştır.
3. İki dosyada E3/E4 elle girişini, seçilen ayı, aylık hakediş aktarımını, fiyat farkı ve kesinti aktarımını gerçek Excel'de yürüt.
4. Eylül, Kasım, Aralık, Haziran, Temmuz ve boş ay senaryoları için her programın beklenen hücre sonuçlarını ayrı kaydet.
5. OEHS hesap motoru ve dört PDF'deki değerleri bu doğrulanmış sonuçlarla karşılaştır; yalnızca kanıtlanan farklara kod değişikliği yap.
6. Her kod değişikliğini test et, CI sonucunu doğrula; dört PDF'yi aynı örnek hakediş üzerinden görsel kabulden geçir.

## Bu turun doğrulama durumu

- Dosyaların açılıp okunması, SHA-256 alınması, sayfa/formül envanteri: **tamamlandı**.
- Önemli formül farklarının ilk tespiti: **tamamlandı; Excel'de yürütme ile doğrulanmadı**.
- VBA tüm kaynak gövdelerinin eksiksiz dökümü: **bekliyor**.
- Gerçek Excel makro çalıştırması ve hücre hücre OEHS karşılaştırması: **bekliyor**.
- Kod değişikliği: **bu turda yapılmadı**; yeni kaynaklardan kesin sonuç çıkarmadan hesap davranışı değiştirilmedi.
