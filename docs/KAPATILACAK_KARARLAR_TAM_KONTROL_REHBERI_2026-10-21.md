# OEHS — Kapatılmamış Kararlar ve Kapanış Rehberi
**Tarih:** 21.10.2026  
**Dal:** `feature/official-payment-document-templates`  
**Amaç:** Her açık konunun ne olduğunu, neden önemli olduğunu, tam olarak hangi dosya/hücre/kod/PDF alanına bakılacağını, hangi testin yapılacağını ve hangi kanıtla kapatılacağını tek yerde açıklamak.

> **Bağlayıcı ilke:** Temel Eğitim ve Özel Eğitim birbirleriyle kıyaslanmaz. Her program yalnızca kendi güncel XLSM dosyası, kendi VBA zinciri ve kendi örnek çıktılarıyla doğrulanır. En son gönderilen XLSM kaynak dosyadır. Şüphe veya matematiksel sezgi tek başına resmî hesap davranışını değiştirme gerekçesi değildir. Kaynak Excel davranışı ile kod farklıysa fark belgelenir; kanıt ve kabul kararı olmadan hesap kuralı sessizce değiştirilmez.

## 1. Kararların özeti

| ID | Konu | Şu anki durum | Risk | Kapanış için gereken kanıt |
|---|---|---|---|---|
| K-01 | Temel Eğitim Hakediş İcmali fiyat farkının ikinci kez eklenmesi | Açık; formül zinciri şüphe doğuruyor | İcmal toplamı fazla çıkabilir | Kaynak Excel'in formül ve hücre zinciri + gerçek Excel yeniden hesaplaması + aynı örneğin program çıktısı; resmî davranış kararı |
| K-02 | Özel Eğitim Hakediş İcmali fiyat farkı bloklarının ikinci kez eklenmesi | Açık; Temel'den bağımsız incelenmeli | İcmal toplamı fazla çıkabilir | Özel Eğitim'in kendi formül zinciri ve gerçek örnek çıktısı; ayrı karar |
| K-03 | Temel Eğitim W4 ile Hakediş Raporu D28 eşleşmesi | Açık; bire bir eşitlik varsayılmayacak | Net ödeme/rapor tutarı hatalı eşlenebilir | Hücrelerin formülleri, bağımlılık zinciri, gerçek Excel değerleri ve program alan eşlemesi |
| K-04 | Özel Eğitim AG4 ile Hakediş Raporu D28 eşleşmesi | Açık; Temel'den bağımsız | Net ödeme/rapor tutarı hatalı eşlenebilir | AC4:AG4 zinciri, D28 formülü/değeri, gerçek Excel ve program eşlemesi |
| K-05 | Özel Eğitim AP3/AQ3 ay başlığı çelişkisi | Kaynakta ikisi de Aralık'a bağlı; VBA eşlemesi AP=Kasım/AQ=Aralık | Kasım/Aralık verisi yanlış aya yazılabilir | Ay başlığı formülü, VBA kolon indeksi, tüm 12 ay eşleme testi; kaynak XLSM'nin mi yoksa VBA eşlemesinin mi düzeltileceği kararı |
| K-06 | Hakediş Raporu D15/D28 ve İcmal E8 gibi hedef alanların kaynak eşlemesi | Açık; her program için ayrı harita gerekli | Yanlış hücreden değer okuma | Alan-hücre eşleme tablosu ve örnek üzerinde alan bazlı karşılaştırma |
| K-07 | Sekiz nihai PDF'nin şablon/alan/sayfa kabulü | Gerçek Excel PDF'leriyle tam kabul bekliyor | Resmî evrakta eksik/yanlış alan veya sayfa taşması | Her programın dört PDF'si, Excel kaynak çıktısı ile alan bazlı kontrol ve bir sayfa A4 görsel kontrol |
| K-08 | VBA gerçek çalışma zamanı ve masaüstü Excel yeniden hesaplama | Statik kontroller mevcut; masaüstü çalışma kabulü tamamlanmış sayılmaz | Derlenme/olay/hesaplama sorunu gözden kaçabilir | Excel masaüstünde makro çalıştırma, kaydedilmiş gerçek sonuçlar, hata/log çıktısı ve tekrar üretilebilir test kaydı |
| K-09 | Güncel dal/commit/CI kanıtı | Her yeni commit için ayrıca kontrol edilmeli | Eski commit'in CI sonucu yeni değişiklik sanılabilir | Test edilen commit SHA'sı ile CI run SHA'sının eşleşmesi |

---

## 2. K-01 — Temel Eğitim: Hakediş İcmali fiyat farkı ikinci kez mi ekleniyor?

### Bu soru ne demek?
İcmal, aylık taşıma/hizmet tutarını bir hücreden alıyor; ayrıca fiyat farkı veya benzer bir tutar bloğunu ikinci bir sütun aralığından ekliyor olabilir. Ancak aylık hizmet tutarının ilk hücresinin formülü zaten fiyat farkını içeriyorsa, aynı tutar icmal toplamına ikinci kez girmiş olabilir. Bu, **kanıtlanmış hata** olarak değil, kaynak zinciri incelenmesi gereken **açık risk** olarak ele alınır.

### Tam olarak nerelere bakılacak?
Kaynak dosya: `03-Temel Eğitim Taşıma Ödeme SON(1).xlsm`

1. `Hesaplama Bilgileri!I4`: fiyat farkı girdisi/değeri.
2. `Hesaplama Bilgileri!K4`: mevcut formül `(G4*H4)+I4-J4`; I4'ün K4 hesabına girdiği görülüyor.
3. `İcmal Verileri!R4`: kaynakta 7.365,35 TL olarak görülen fiyat farkı değeri.
4. `İcmal Verileri!AD4`: 19.475,040568144 TL olarak görülen ikinci ilgili tutar bloğu.
5. `Hakediş İcmali!C8`: ay seçimine göre `İcmal Verileri!A2:N58` aralığından aylık hizmet toplamını getiriyor.
6. `Hakediş İcmali!D8`: `İcmal Verileri!A4:AY58` aralığında `15+MATCH(...)` ve `27+MATCH(...)` ile seçilen iki sütunu topluyor.
7. `Hakediş İcmali!E8`: `=C8+D8`.

### Şu an bilinen örnek
- `C8` kayıtlı önbellek değeri: **319.991,9228 TL**
- `D8` kayıtlı önbellek değeri: **26.840,390568144 TL**
- `E8` kayıtlı önbellek değeri: **346.832,313368144 TL**
- D8 bileşenlerinden biri `R4 = 7.365,35 TL), diğeri `AD4 = 19.475,040568144 TL` ile eşleşiyor.
- K4 formülü I4 fiyat farkını içeriyor. Bu nedenle C8 içindeki aylık toplamda fiyat farkı zaten bulunuyor mu sorusu cevaplanmalı.

**Tanısal karşılaştırma:** D8'in tamamı yerine yalnız AD4'ün eklenmesi durumunda `C8+AD4 = 339.466,963368144 TL` olur; kaynak E8'den tam **7.365,35 TL** düşüktür. Bu yalnızca şüpheli ikinci eklemeyi görünür kılan bir teşhistir; **onaylanmış ödeme tutarı değildir**.

### Yapılacak test
- Excel masaüstünde dosyayı aç, hesaplamayı tam yeniden çalıştır (full recalculation), kaydet.
- Yukarıdaki hücrelerin formülünü ve hesaplanmış değerini ayrı ayrı kaydet.
- C8'in hangi kaynak hücrelerden oluştuğunu izleyerek, fiyat farkının C8'e girip girmediğini göster.
- D8'in her iki MATCH kolunun gerçek sütun indeksini ve karşılık gelen başlığı/aylık alanı kaydet.
- Aynı girdilerle programın Temel Eğitim icmalini üret; C8/D8/E8 karşılıklarını tek tek kıyasla.
- İkinci eklemenin Excel'in mevcut formülü tarafından kasıtlı biçimde yapıldığı doğrulanırsa program Excel'le aynı davranışı korur; bu durumda matematiksel/iş kuralı riski ayrıca belgelenir. Excel formül zinciri ve doğrulanmış örnek açıkça hatalı olduğunu kanıtlarsa, düzeltme ancak bu kanıtla yapılır.

### Kapanış ölçütü
Aşağıdaki dört öğe aynı kayıt içinde bulunmalı: (1) formül ekranı veya formül metni, (2) yeniden hesaplanan değerler, (3) C8/D8/E8 alan eşleme tablosu, (4) test sonucu ve karar. Bunlar olmadan “mükerrer ekleme düzeltildi” ya da “sorun yok” denmez.

---

## 3. K-02 — Özel Eğitim: Hakediş İcmali fiyat farkı ikinci kez mi ekleniyor?

Bu konu Temel Eğitim'den ayrı incelenir. Temel Eğitimdeki sonucu veya düzeltmeyi Özel Eğitim'e kopyalamak yasaktır.

### Tam olarak nerelere bakılacak?
Kaynak dosya: `02-Özel Eğitim Hakediş Ödeme SON(1).xlsm`

1. `Hesaplama!I4`: 7.530 TL.
2. `Hesaplama!K4`: `(G4*H4)+I4-J4`; I4 fiyat farkı bu zincire dahil.
3. `Hesaplama!T4`: `(P4*Q4)+R4-S4`; ikinci taşıma/rehber veya ilgili hesap bloğu kendi formülüyle kontrol edilmeli.
4. `Hesaplama!AC4`: `(I4*6/100)+(R4*6/100)`.
5. `Hesaplama!AD4`: `(K4*9.48/1000)+(T4*9.48/1000)`.
6. `Hesaplama!AE4`: `M4+V4+Y4+Z4+AA4+AB4+AC4+AD4`.
7. `Hesaplama!AF4`: `O4+X4`.
8. `Hesaplama!AG4`: `AF4-AE4`.
9. `İcmal Verileri!R4`: 7.530 TL; `İcmal Verileri!AD4`: 3.500 TL; aynı sayfadaki AP/AQ ay eşlemesi ayrıca K-05'te incelenir.
10. `Hakediş İcmali!C8`, `D8`, `E8`: C8 aylık hizmet toplamı, D8 iki MATCH sütununun toplamı, E8=C8+D8.

### Şu an bilinen örnek
- K4: **43.630 TL**
- T4: **34.037,27 TL**
- K4+T4 / C8 kayıtlı değeri: **77.667,27 TL**
- R4: **7.530 TL**
- AD4: **3.500 TL**
- D8 kayıtlı değeri: **11.030 TL**
- E8 kayıtlı değeri: **88.697,27 TL**

C8'in K4+T4 ile aynı olması, bu örnekte fiyat farkı içeren tutarların C8'e girmiş olabileceğini gösterir. D8'in 7.530 + 3.500 TL toplaması ise ek bir blok daha eklenmesi riskini doğurur. **Bu, tek başına hatalı hesap olduğunun kanıtı değildir.**

**Tanısal karşılaştırma:** D8 hiç eklenmezse E8=C8=77.667,27 TL olur; kaynak E8'den **11.030 TL** düşüktür. Bu tutar yalnızca farkın büyüklüğünü gösterir; resmî olarak kabul edilmiş yeni tutar değildir.

### Yapılacak test
- Excel masaüstünde Özel Eğitim dosyasını tam yeniden hesapla ve kaydet.
- K4, T4, AC4:AG4, C8:D8:E8 formül ve değerlerini dışa aktar/kayıt altına al.
- D8'deki iki MATCH kolunun ay başlıklarını ve gerçek sütunlarını yaz.
- C8'in K4/T4 zincirini zaten içerip içermediğini bağımlılık izleme ile göster.
- Özel Eğitim programının ürettiği icmalde aynı alanlarla bire bir kıyasla.
- Sonucu Temel Eğitim testinden ayrı kaydet.

### Kapanış ölçütü
Özel Eğitim için bağımsız bir test kaydı ve karar gerekir. Temel Eğitim dosyasının sonucu bu kararı kapatamaz.

---

## 4. K-03 — Temel Eğitim: `Hesaplama Bilgileri!W4` ile `Hakediş Raporu!D28` nasıl eşleşiyor?

### Sorun
W4, hesaplama sayfasında net/sonuç değer zincirinin parçasıdır. D28 ise resmî Hakediş Raporu üzerindeki bir çıktı alanıdır. İsimleri veya ekrandaki konumları benziyor diye bunların aynı tutar olması varsayılmamalıdır.

### Bakılacak yerler
- `Hesaplama Bilgileri!T4` — güncel formül `=I4*6/100`.
- `Hesaplama Bilgileri!U4` — `=K4*9.48/1000`.
- `Hesaplama Bilgileri!V4` — `=P4+Q4+R4+S4+T4+U4+M4`.
- `Hesaplama Bilgileri!W4` — `=O4-V4`.
- `Hakediş Raporu!D15` ve `Hakediş Raporu!D28` — formül, değer, biçim ve komşu etiketler.
- Program kodunda bu Excel hücrelerini PDF/arayüz alanlarına bağlayan eşleme.

### Yapılacak test
1. Her hücrenin hem formülünü hem yeniden hesaplanan değerini kaydet.
2. D28'in formülünü izleyerek kaynağını bul; W4'e doğrudan mı, başka ara hücreye mi, yoksa ayrı bir toplam zincirine mi bağlı olduğunu belirle.
3. Programda aynı örneğin Hakediş Raporu PDF'sindeki ilgili alanını bul.
4. Formül eşleşmesi varsa sayısal eşitlik kontrolü yap; formül farklıysa farkın açıklamasını belgeleyin.
5. Yuvarlama ve sayı biçimini ayrıca kontrol et: aynı sayısal değer farklı para biçiminde görünmesi, hesap farkı ile karıştırılmamalı.

### Kapanış ölçütü
`Excel hücresi → formül/değer → program alanı → PDF koordinatı` zinciri tek satırda gösterilmelidir. D28 ile W4 farklıysa otomatik olarak birini diğerine eşitlemeyin; önce kaynak formül ve rapor düzeni açıklanmalıdır.

---

## 5. K-04 — Özel Eğitim: `Hesaplama!AG4` ile `Hakediş Raporu!D28` nasıl eşleşiyor?

### Bakılacak yerler
- `Hesaplama!AC4`: ek kesin teminat benzeri yüzde hesabı; formülü `(I4*6/100)+(R4*6/100)`.
- `Hesaplama!AD4`: `(K4*9.48/1000)+(T4*9.48/1000)`.
- `Hesaplama!AE4`: `M4+V4+Y4+Z4+AA4+AB4+AC4+AD4`.
- `Hesaplama!AF4`: `O4+X4`.
- `Hesaplama!AG4`: `=AF4-AE4`.
- `Hakediş Raporu!D15` ve `D28`.
- Özel Eğitim kaynak hücrelerini rapor alanlarına bağlayan program kodu.

### Yapılacak test ve kapanış
K-03'teki beş test adımı aynen uygulanır ama yalnızca Özel Eğitim dosyası/kodu/çıktısı kullanılır. `AG4 → D28` eşitliği formül zinciriyle kanıtlanmadan “aynı alan” kabul edilmez. AC4:AG4 aralığının tamamı kaydedilmelidir; yalnız AG4 değerine bakmak ara kesintileri ve toplam zincirini doğrulamaz.

---

## 6. K-05 — Özel Eğitim `İcmal Verileri!AP3` ve `AQ3`: Kasım/Aralık başlık çelişkisi

### Şu an görülen çelişki
Kaynak XLSM'de AP3 ve AQ3 hücreleri de `=Anasayfa!I9` formülüne bağlı ve ikisi de Aralık ayını gösteriyor. VBA eşlemesi ise AP'yi Kasım, AQ'yu Aralık olarak kullanıyor. Dolayısıyla kaynak başlığı ile kodun ay indeksi aynı şeyi söylemiyor.

### Neden önemli?
Ay kolonunun bir ay kayması, Kasım tutarının Aralık'a veya Aralık tutarının Kasım'a yazılmasına; rapor ve icmal toplamlarının yanlış döneme ait görünmesine yol açabilir. Bu, toplam para doğru olsa bile dönem doğruluğunu bozar.

### Bakılacak yerler
1. `02-Özel Eğitim Hakediş Ödeme SON(1).xlsm` → `İcmal Verileri!AP3`, `AQ3` formül ve hesaplanan değerleri.
2. Aynı sayfadaki O3:AY3 ay başlıklarının tümü; sadece AP/AQ değil, önceki/sonraki kolonlar da kontrol edilmeli.
3. Özel Eğitim VBA modülünde ay adından sütun numarasına dönüşüm ve AP/AQ kolon indeksleri.
4. Programın icmal veri modelinde ay anahtarı (Kasım/Aralık) ile Excel sütunu eşlemesi.
5. Test verisinde Kasım ve Aralık için birbirinden belirgin farklı tutarlar kullanılarak çıktı ayları doğrulanmalı.

### Karar yöntemi
- Önce kullanıcının en son verdiği özgün XLSM'nin gerçekten kaynak olduğu doğrulanır; düzeltilmiş test kopyası özgün dosyanın yerine geçirilmiş sayılmaz.
- Kaynak Excel AP3'ün Kasım olması gerektiği şeklinde açıkça düzeltilmişse, bu değişiklik ayrı bir kaynak revizyonu olarak belgelenir ve formül/başlık tüm aylar için yeniden kontrol edilir.
- Kaynak Excel değişmeden kalıyorsa, VBA'yı kaynak başlığına göre düzeltmek ile Excel'in mevcut davranışını korumak arasında çelişki vardır. Hangi tarafın resmî davranış sayılacağı kanıtsız seçilmez; uyuşmazlık açık kalır.
- Test kopyasında yapılan AP3 düzeltmesi, tek başına özgün XLSM'nin onaylandığını göstermez.

### Kapanış testi
12 ayın her biri için `Ay adı → kaynak başlık hücresi → VBA sütun indeksi → hesaplama/çıktı alanı` tablosu hazırlanır. Kasım ve Aralık testleri ayrı ayrı yürütülür; bir ayı test etmek diğerini doğrulamaz.

---

## 7. K-06 — Alan eşleme matrisi: Excel hücresinden resmî PDF alanına

Her program için ayrı matris hazırlanmalıdır. Aşağıdaki kolonlar zorunludur:

| Alan | Excel sayfa/hücre | Excel formülü | Yeniden hesaplanan değer | Program değişkeni/alanı | PDF adı/sayfa/bölüm | PDF'den okunan değer | Fark | Sonuç |
|---|---|---|---:|---|---|---:|---:|---|
| Hakediş raporu ara toplamı | programa göre | tam formül | değer | kod eşlemesi | Hakediş Raporu | değer | hesaplanır | PASS/FAIL/OPEN |
| Net ödenecek | programa göre | tam formül | değer | kod eşlemesi | ilgili çıktı | değer | hesaplanır | PASS/FAIL/OPEN |
| İcmal aylık tutarı | C8 karşılığı | tam formül | değer | kod eşlemesi | Hakediş İcmali | değer | hesaplanır | PASS/FAIL/OPEN |
| İcmal ek/fiyat farkı | D8 karşılığı | tam formül | değer | kod eşlemesi | Hakediş İcmali | değer | hesaplanır | PASS/FAIL/OPEN |
| İcmal toplamı | E8 karşılığı | `=C8+D8` veya gerçek formül | değer | kod eşlemesi | Hakediş İcmali | değer | hesaplanır | PASS/FAIL/OPEN |

Her satırda kaynak hücrenin adı aynen korunur. Temel Eğitim değişken adları Özel Eğitim'e, Özel Eğitim adları Temel Eğitim'e taşınmaz.

---

## 8. K-07 — Sekiz PDF için kabul kontrolü

Üretilmesi gereken belgeler:

| Program | PDF 4 | PDF 5 | PDF 6 | PDF 7 |
|---|---|---|---|---|
| Temel Eğitim | Hakediş Raporu | Hizmet İşleri Hakediş Raporu | Hizmet İşleri Kabul Tutanağı | Hakediş İcmali |
| Özel Eğitim | Hakediş Raporu | Hizmet İşleri Hakediş Raporu | Hizmet İşleri Kabul Tutanağı | Hakediş İcmali |

### Her PDF'de zorunlu kontrol
- Dosya doğru programa mı ait?
- Dönem/ay/yıl doğru mu?
- İKN ve sözleşme/ihale bilgisi doğru mu?
- Yüklenici/unvan doğru mu?
- Hat/hatlar doğru mu; seçilmeyen hat yanlışlıkla dahil olmuş mu?
- Hesaplanan tutarlar, kesintiler, KDV/tevkifat ve net tutar ilgili programın Excel çıktısıyla aynı mı?
- İmza/unvan/tarih alanları şablondaki yerde ve okunur mu?
- Sayfa boyutu A4 mü ve belge tek sayfa mı?
- Başlık, çizgi, tablo, hizalama, kenar boşluğu, yazı kesilmesi ve sayfa dışına taşma var mı?
- PDF metin katmanı mevcut mu; tutarlar ve kimlik bilgileri metin olarak seçilebiliyor mu?
- PDF'ler arasında aynı hakedişin dönem, yüklenici, hat ve toplamları tutarlı mı?

### Karşılaştırma yöntemi
1. Aynı örnek ve aynı dönem için Excel'in kendi çıktısı/PDF'si alınır.
2. Programdan sekiz nihai PDF üretilir.
3. Metin alanları otomatik karşılaştırılır; ardından sayfa görüntüleri görsel olarak karşılaştırılır.
4. Her fark, PDF adı + sayfa + alan + Excel kaynağı + beklenen/gerçek değer şeklinde kaydedilir.
5. Sekiz PDF'nin hepsi PASS olmadan “PDF kabulü tamamlandı” denmez.

Bir sayfalık smoke-test PDF'sinin geçmesi, gerçek Excel örneğine göre alanların doğru olduğunu tek başına kanıtlamaz.

---

## 9. K-08 — VBA ve masaüstü Excel gerçek kabulü

### Statik kontrol ile gerçek çalışma arasındaki fark
Statik inceleme kod metninde eksik `Option Explicit`, dengesiz prosedür başlangıç/bitişleri veya belirli hata işleme kalıplarını yakalayabilir. Ancak Excel'in makroyu derlediğini, butonların doğru prosedüre bağlı olduğunu, UserForm kontrollerinin doğru çalıştığını veya formüllerin gerçek Excel motorunda yeniden hesaplandığını kanıtlamaz.

### Kontrol edilecekler
- Özgün iki XLSM masaüstü Excel'de açılıyor mu?
- Makrolar beklenen güvenlik koşullarında etkinleştirilebiliyor mu?
- VBA derlemesi hatasız mı?
- Ana sayfadaki butonlar ve UserForm olayları doğru makroya bağlı mı?
- Temel Eğitim ve Özel Eğitim hesaplama akışları ayrı ayrı çalışıyor mu?
- Kaydedilen hücre değerleri (Temel: T4/U4/W4/D15/D28; Özel: AC4:AG4/D15/D28) kayda alındı mı?
- Hata/log dosyası üretildiyse hata var mı?
- Aynı girdiyle ikinci çalıştırma aynı sonucu üretiyor mu?
- Kullanıcı adı/şifre gibi gömülü kimlik bilgileri var mı? Bulunan bir güvenlik riski hesap mantığından ayrı kaydedilmeli; işlev davranışı incelenmeden rastgele değiştirilmemeli.

### Kapanış ölçütü
Test tarihi, Excel sürümü, dosya SHA256, makro/derleme sonucu, girdi örneği, hücre değerleri, log ve ekran görüntüsü/çıktı yolu kayıt altına alınır. Yalnızca statik tarama başarılıysa durum “Statik kontrol PASS; masaüstü kabulü OPEN” olarak yazılır.

---

## 10. K-09 — Dal, commit ve CI doğrulaması

Her değişiklikten sonra:
1. Etkin dalın `feature/official-payment-document-templates` olduğu doğrulanır.
2. Son commit SHA kaydedilir.
3. CI çalışmasının aynı commit SHA'sına ait olduğu doğrulanır.
4. CI job'larının tamamlanma/başarı durumu kontrol edilir; atlanan adımlar ayrıca not edilir.
5. Eski commit'teki başarılı CI, yeni commit için başarı sayılmaz.
6. PR açık kalır; ana dala birleştirme veya yayınlama yapılmaz.

**Durum dili:**  
- **PASS:** Aynı commit'te ilgili test çalıştı ve geçti.  
- **FAIL:** Test çalıştı ve başarısız oldu.  
- **OPEN:** Test henüz çalışmadı veya kanıt yetersiz.  
- **BLOCKED:** Test için erişim/ortam/örnek veri gibi bir engel var; engel açıkça yazılır.

---

## 11. Eski kural ve testlerin güncellenmesi

- Temel Eğitim `Hesaplama Bilgileri!T4` artık güncel XLSM'de `=I4*6/100` formülüyle hesaplanıyor. “T4'e sabit elle 1.500 TL girilir” kuralı güncel kaynakla çelişir ve geçerli kabul kuralı olamaz.
- Eski örnek veya test, son XLSM ile çelişiyorsa silinmeden önce yeni kaynakla neden çeliştiği açıklanmalı; gerekiyorsa “eski/geçersiz” olarak işaretlenmeli. Güncel kabul testlerinin içine karışmamalı.
- Temel ve Özel Eğitim test verileri, formül beklentileri ve PDF çıktıları ayrı tutulmalı.
- Fiyat farkının icmale ikinci kez eklenmesiyle ilgili tanısal tutarlar, resmî kabul tutarı gibi kullanılmamalı.

---

## 12. Kapanış sırası ve teslim edilecek kanıt paketi

Önerilen sıra:
1. İki özgün XLSM'nin SHA256 ve son kaynak oldukları kaydedilir.
2. Her program için gerçek Excel yeniden hesaplaması alınır.
3. K-01 ve K-02 icmal formül zincirleri ayrı ayrı çözümlenir.
4. K-03 ve K-04 rapor alan eşlemeleri tamamlanır.
5. K-05 ay eşleme tablosu 12 ayın tümüyle doğrulanır.
6. Program kodundaki eşlemeler kaynak formüllerle karşılaştırılır; yalnızca kanıtlanan uyumsuzluklar düzeltilir.
7. Temel ve Özel regresyon testleri ayrı çalıştırılır.
8. Sekiz PDF alan bazlı ve görsel kabulden geçirilir.
9. Aynı commit'e ait CI kontrol edilir.
10. Devam notu güncellenir: yapılan iş, doğrulama durumu, açık risk, sıradaki adım.

### Nihai teslim paketi
- İki XLSM için SHA256 ve Excel sürümü/yeniden hesaplama bilgisi.
- Temel Eğitim alan-hücre eşleme matrisi ve test sonuçları.
- Özel Eğitim alan-hücre eşleme matrisi ve test sonuçları.
- K-01…K-09 karar kayıtları; her biri PASS/FAIL/OPEN/BLOCKED.
- Sekiz PDF'nin dosya adları, sayfa sayısı, metin/alan karşılaştırması ve görsel kontrol sonucu.
- CI run bağlantısı ve test edilen commit SHA.
- Açık kalan her karar için somut engel ve sonraki doğrulama adımı.

## 13. Şu anki gerçek durum

Bu rehberdeki mevcut formül/değer bilgileri kaynak dosyaların formül incelemesinden ve kayıtlı önbellek değerlerinden gelir. Kayıtlı önbellek değerleri, masaüstü Excel'de bu oturumda yeniden hesaplandığı anlamına gelmez. Bu nedenle:
- K-01–K-06: **OPEN** — kaynak zinciri ayrıntılandırılmış; gerçek masaüstü yeniden hesaplama ve nihai karar kanıtı gerekli.
- K-07: **OPEN** — smoke-test PDF'leri nihai sekiz Excel eşleşmeli PDF'nin yerine geçmez.
- K-08: **OPEN** — statik VBA kontrolü, gerçek masaüstü Excel kabulü değildir.
- K-09: her yeni commit için yeniden kontrol edilmeli; eski CI sonucu yeni commit'e taşınmamalıdır.

Bu belge kararları peşinen çözülmüş gibi göstermemek için hazırlanmıştır. Amaç, hangi dosyaya/hücreye/koda bakılacağını ve bir konunun hangi kanıtla kapanacağını tartışmasız hale getirmektir.
