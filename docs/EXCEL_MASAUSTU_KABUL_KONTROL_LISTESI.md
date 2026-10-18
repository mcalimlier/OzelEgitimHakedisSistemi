# Excel Masaüstü Kabul Kontrol Listesi — 18.10.2026

Bu kontrol, güncel iki XLSM'nin gerçek Excel hesap motoru ve VBA makrolarıyla doğrulanması içindir. **Orijinal dosyaları değiştirmeyin.** Her program için ayrı bir deneme kopyası oluşturun; sonuçları kopyada kaydedin. Bu dosya OEHS kod değişikliği veya kullanıcı kararı yerine geçmez.

## Güvenli hazırlık

- [ ] Güncel Temel Eğitim XLSM'sinin ayrı bir deneme kopyasını oluştur.
- [ ] Güncel Özel Eğitim XLSM'sinin ayrı bir deneme kopyasını oluştur.
- [ ] Dosyaları yalnızca güvenilir kaynaktan geldiklerinden eminseniz ve makroların ne yaptığını bilerek Excel'de açın.
- [ ] Her iki dosyada `Ctrl+Alt+F9` ile tam yeniden hesaplama yap; hücre değerlerini ve formül çubuğunu ayrı ayrı kaydet.
- [ ] **`AyVerileriniSil` makrosunu çalıştırma.** Statik VBA incelemesinde seçili ay yerine birden fazla ay sütununu topluca temizlediği görüldü.
- [ ] Kaynak XLSM'leri GitHub'a veya herkese açık bir yere yükleme; deneme dosyaları yerelde kalsın.

## A. Temel/Ortaöğretim

### A1. Kasım 2026 — önceki ay kesintisi ve aylık aktarım

- [ ] `Anasayfa!E4` alanından Kasım 2026'yı seç.
- [ ] `Anasayfa!K16` seçili hat numarasını ve `Hesaplama Bilgileri!A4` satırındaki hat numarasını doğrula.
- [ ] `İcmal Verileri!AB3:AL3` ay başlıklarını ve `AB4:AL4` aylık kesinti hücrelerini kaydet.
- [ ] Ekim kesinti hücresi `İcmal Verileri!AC4` değerini kaydet. Mevcut dosya önbelleğinde bu değer **24.104,02472 TL**.
- [ ] `Hakediş İcmali!E21` değerini kaydet. Mevcut dosya önbelleğinde E21 **0**; OEHS regresyonu ise aylık makro düzenine göre önceki ay AC4 değerinin taşınmasını bekliyor. Bu iki sonucu birbirine karıştırma.
- [ ] `HakedisTutarlariniAySutununaAktar` ve `KesintiToplamlariniAktar` işlemlerini yalnızca kopyada, her biri öncesi/sonrası hücreleri kaydederek çalıştır.
- [ ] Kasım seçiliyken E21'in ve aylık hücrelerin değişip değişmediğini, tam hangi hücrelerin değiştiğini yaz.

### A2. Ek kesin teminat ve damga vergisi

- [ ] `Hesaplama Bilgileri!T4:T6` hücrelerinin formül çubuğunu kontrol et; mevcut kaynakta bu hücreler formül değil, sabit **1500** değerleri.
- [ ] `Hesaplama Bilgileri!U4` formülünü kaydet; mevcut formül `=(K4*9.48/1000)+T4*9.48/1000`.
- [ ] `Hesaplama Bilgileri!K4,T4,U4,V4,W4` hücrelerinin tam yeniden hesaplama sonrası değerlerini kaydet.
- [ ] T4'ün neden sabit olduğunu ve resmi ödeme hesabında nasıl yorumlanması gerektiğini belirlemeden formülü değiştirme.

### A3. Hakediş Raporu net tutar zinciri

- [ ] `Hakediş Raporu!D6,D7,D8,D10,D11,D12,D13,D26,D28` hücrelerinin değerlerini kaydet.
- [ ] `Hesaplama Bilgileri!W4` ile `Hakediş Raporu!D28` değerlerini karşılaştır.
- [ ] D6−D10, D8−D10, H2+D7, D11 ve D28−W4 aritmetiğini yaz; fark varsa kaynağını belirt.

Mevcut önbellek örneği: D6=836.742; D7=1.500; D8=838.242; D10=616.252; D11=221.990; D28=223.410,03; W4=221.910,0348.

## B. Özel Eğitim

### B1. Kasım 2026 — önceki ay kesintisi ve ay başlıkları

- [ ] `Anasayfa!E5` alanından Kasım 2026'yı seç.
- [ ] `Anasayfa!K17` seçili hat numarasını doğrula.
- [ ] `İcmal Verileri!AN3:AX3` başlıklarını ve `AN4:AX4` değerlerini kaydet.
- [ ] `Hakediş İcmali!E21` değerini kaydet; mevcut dosya önbelleğinde **10.580,582128 TL**, `İcmal Verileri!AO4` değeriyle eşleşiyor.
- [ ] Kasım başlığının sütun dizisinde atlanması ve Aralık başlığının iki kez görünmesi durumunu, ay ay başlık/değer tablosuyla kaydet.
- [ ] `HakedişToplamlariniAktar`, `FiyatFarkıVerileriniAktar`, `RehberPersonelFiyatFarkıVerileriniAktar` ve `KesintiBilgileriniAktarAktar` makrolarını yalnızca kopyada, her biri öncesi/sonrası hücreleri kaydederek çalıştır.

### B2. Hesaplama satırları 4–6

- [ ] `Hesaplama!K4,T4,AC4,AD4,AE4,AF4,AG4` hücrelerini kaydet.
- [ ] Aynı hücreleri satır 5 ve satır 6 için de kaydet.
- [ ] OEHS'nin güncel kaynak vektörleriyle karşılaştır. Bu vektörler otomatik testte mevcut; gerçek Excel/VBA sonucu ayrıca gereklidir.

Mevcut satır 4 önbellek örneği: K4=51.256,94; T4=41.954,70; AC4=202,80; AD4=883,6463472; AE4=11.201,1393472; AF4=106.728,274; AG4=95.527,1346528.

### B3. Hakediş Raporu net tutar zinciri

- [ ] `Hakediş Raporu!D6,D7,D8,D10,D11,D12,D13,D26,D28` hücrelerini kaydet.
- [ ] `Hesaplama!AG4` ile `Hakediş Raporu!D28` değerlerini karşılaştır.
- [ ] D6−D10, D8−D10, H2+D7, D11 ve D28−AG4 aritmetiğini yaz; fark varsa kaynağını belirt.

Mevcut önbellek örneği: D6=277.026,66; D7=3.380; D8=280.406,66; D10=183.815,02; D11=96.591,64; D28=98.907,13; AG4=95.527,1346528.

## C. Dört resmî PDF

Her program için aynı hat ve aynı hakediş bağlamında aşağıdaki dört belgeyi ayrı ayrı üret:

- [ ] Hakediş Raporu
- [ ] Hizmet İşleri Hakediş Raporu
- [ ] Hizmet İşleri Kabul Tutanağı
- [ ] Hakediş İcmali

Her belge için dönem, yüklenici, hat, hakediş no, tutarlar, imza blokları, A4 sayfa yönü, taşma ve sayfa sayısını kaydet. Kaynak Excel çıktılarıyla OEHS çıktıları farklıysa, farkı belge adı ve hücre/alan bazında yaz.

## Sonuç kaydı

Her program için ayrı olarak şu bilgileri kaydet: Excel sürümü, seçilen dönem, seçilen hat, makro adı, çalıştırma öncesi hücre değerleri, çalıştırma sonrası hücre değerleri, dört PDF dosya adı ve gözlenen farklar. Hiçbir formül veya kaynak dosya, farkın anlamı netleşmeden değiştirilmemelidir.
