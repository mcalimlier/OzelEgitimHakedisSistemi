# OEHS Hakediş Arşiv, Düzeltme ve Excel Uyumlu Veri Akışı Tasarımı

## 1. Amaç

OEHS masaüstü uygulamasında hakediş oluşturma, kontrol, düzeltme, arşivleme ve resmi çıktı üretimini tek bir tutarlı veri akışı altında toplamak.

Bu tasarımın finansal/hesapsal otoritesi yalnızca **30.09.2026 tarihinde son yüklenen**
`Özel Eğitim Hakediş Ödeme Programı (1)(1).xlsm` dosyasıdır. Önceki Excel dosyaları, eski çalışma notları veya önceki uygulama formülleri hesaplama için kaynak kabul edilmez.

Başarı ölçütü:
- XLSM'deki hesap zinciri ile OEHS hesaplama motoru arasında formül düzeyinde uyum.
- İhale/dosya no, VKN/TCKN ve hat ilişkilerinin isimlerden bağımsız, kararlı biçimde tutulması.
- Hakediş oluşturma sonrası arşivlenebilir ve tekrar çağrılabilir tekil kayıt.
- Kaynak kayıt düzeltildiğinde ilişkili aktif kayıtların kontrollü biçimde yeniden hesaplanması.
- Normal yeni arşivlemelerde revision üretimi; ARŞİVLER > DÜZELT akışında ise seçilen mevcut snapshot'ın aynı revision dosyası üzerinde güncellenmesi.
- 4 resmi çıktının tek A4 PDF çıktısında korunması ve dosya adının anlamlı olması.
- Son aşamada derleme, test, veri akışı ve UI için genel kontrol raporu verilmesi.

## 2. Kaynak Excel Hesap Sözleşmesi

### 2.1 İhale Verileri

İhale/dosya no, bağımsız bir ihale kümesinin ana kimliğidir.

Hat verisi şu alanları taşır:
- Hat No
- Hattın Adı
- Yüklenici
- TC.Kimlik No / Vergi No
- Taşıma İş Günü
- Taşıma Günlük Fiyatı
- Taşıma Yıllık = `İş Günü * Günlük Fiyat`
- Rehber İş Günü
- Rehber Personel Günlük Fiyatı
- Rehber Yıllık = `Rehber İş Günü * Rehber Günlük Fiyat`
- Banka Hesap No
- Sözleşme Bedeli = `Taşıma Yıllık + Rehber Yıllık`
- İhale Tarihi
- Sözleşme Tarihi
- İşe Başlama Tarihi
- İşin Bitiş Tarihi

Bir yüklenici birden fazla ihale/dosya no altında farklı hatlar alabilir.

### 2.2 Hesaplama sayfasındaki formüllerin kanonik karşılığı

Aşağıdaki formüller OEHS hesaplama motorunun tek referansı olacaktır.

Taşıma:
- `K = (G * H) + I - J`
- `L = IF(E = "Hayır", K * 10 / 100, 0)`
- `M = L / 2`
- `N = K + M`
- `O = K + L`

Rehber:
- `T = (P * Q) + R - S`
- `U = IF(E = "Hayır", T * 20 / 100, 0)`
- `V = IF(E = "Hayır", U * 9 / 10, 0)`
- `W = T + V`
- `X = T + U`

Kesintiler:
- `AC = (I * 6 / 100) + (R * 6 / 100)`
- `AD = (K * 9.48 / 1000) + (T * 9.48 / 1000)`
- `AE = M + V + Y + Z + AA + AB + AC + AD`

Sonuç:
- `AF = O + X`
- `AG = AF - AE`

Önemli uygulama kuralı:
- Kaynak XLSM formüllerinde gösterilmeyen ara `ROUND(...,2)` işlemleri OEHS motoruna eklenmeyecektir.
- Sonuçların kuruş gösterimi UI/çıktı katmanında yapılacaktır.
- Özellikle `AD`, `AE`, `AF`, `AG` zincirinde Excel'in hesaplama sırasını değiştirecek ara yuvarlama uygulanmayacaktır.
- Negatif fiyat farkı Excel tarafından engellenmiyorsa OEHS de yapay olarak sıfırlamayacaktır.
- KDV muafiyetindeki eşleme açık olmalıdır: Excel'deki `E = "Hayır"` durumu KDV hesaplandığı durumu ifade eder.

### 2.3 Hakediş Raporu kanonik davranışı

XLSM'deki güncel rapor mantığı korunacaktır:
- A: sözleşme fiyatları ile yapılan işlerin kümülatif toplamı.
- B: fiyat farkı tutarı.
- C: A+B.
- D: bir önceki hakedişlerin toplam tutarı.
- E: mevcut hakedişte ödenecek hizmet tutarı; kaynak formülündeki mevcut dönem hizmet toplamı ile mevcut dönem fiyat farkı davranışı korunacaktır.
- F: taşıma KDV + rehber KDV.
- G: tahakkuk = E+F.
- Kesintiler kaynak sütunlardan alınır.
- I: yükleniciye ödenecek = `ROUND(G,2) - ROUND(H,2)`.

Geçmiş verinin sıralanması ödeme dönemi/tarih mantığıyla yapılacaktır; oluşturulma/düzenleme zaman damgası finansal sıra yerine geçmez.

### 2.4 Hakediş İcmali

İcmal:
- geçmiş dönemlerin iş tutarları,
- geçmiş dönemlerin fiyat farkı,
- toplam iş tutarı,
- bir önceki hakediş kesintisi,
- bu hakediş kesintisi,
- toplam kesinti,
- toplam hakediş tutarı

alanlarını dönem sırasını bozmadan üretir.

## 3. Veri kimlikleri ve ilişkiler

İsimler ilişki anahtarı olarak kullanılmayacaktır.

Kanıtlanmış anahtarlar:
- Yüklenici: normalize edilmiş **VKN/TCKN**.
- İhale: **İhale/Dosya No**.
- Hat ilişkisi: **İhale/Dosya No + Hat No + Yüklenici VKN/TCKN**.
- Sözleşme: **İhale/Dosya No + Sözleşme No**.
- Hakediş benzersizliği: **İhale/Dosya No + Hat No + Yüklenici VKN/TCKN + Hakediş No + Dönem Tarihi**.

İsim değişiklikleri (firma adı veya hat adı) bu anahtarları değiştirmemelidir.

## 4. Hakediş giriş akışı

Hakediş oluşturma ekranı kullanıcıyı aşağıdaki sırayla ilerletir:

1. İhale/dosya seçimi.
2. Seçili ihaleye bağlı hat seçimi.
3. Yüklenici bilgisi salt-okunur olarak seçili hattan getirilir.
4. Sistem, son geçerli hakediş numarasını otomatik önerir; kullanıcı değiştirebilir ancak yinelenen kombinasyon kabul edilmez.
5. Dönem tarihi seçilir.
6. Dönem, sözleşme tarihleri ve önceki hakedişlerle kronolojik olarak doğrulanır.
7. Kullanıcı yalnızca Excel'de kullanıcı girdisi olan alanları girer:
   - Taşıma İskonto
   - Taşıma Fiyat Farkı
   - Rehber İskonto
   - Rehber Fiyat Farkı
   - Vergi Borcu
   - SGK Borcu
   - Asgari Ücret Destek Tutarı
   - Denetim Cezası
8. Hat/ihale/yüklenici kaynaklı sabit değerler otomatik doldurulur.
9. Hesaplama öncesi özet/kontrol ekranı gösterilir.
10. Hesaplama başarılı ise sonuç özeti ve sonraki eylemler gösterilir.

UI'daki otomatik getirilen alanlar kullanıcı tarafından yanlışlıkla değiştirilememelidir.

## 5. Hesaplama öncesi kontroller

`HAKEDİŞİ HESAPLA` öncesi sistem:
- ihale, hat, yüklenici kimliği ve hakediş numarasının dolu olduğunu,
- hat ile ihalenin gerçek ilişkisini,
- VKN/TCKN'nin normalize edilebilir olduğunu,
- ücret/gün değerlerinin negatif olup olmadığını,
- borç/kesinti alanlarının negatif olup olmadığını,
- aynı anahtar kombinasyonunda kayıt bulunmadığını,
- dönem tarihinin önceki hakedişlerden önce olmadığını,
- sözleşme başlangıç/bitiş aralığıyla çelişmediğini,
- gereken statik kaynak kayıtlarının mevcut olduğunu

kontrol eder.

Hata varsa hesaplama yapılmadan kullanıcıya hangi alanın neden düzeltileceği gösterilir.

## 6. Hesaplama sonucu eylemleri

Hesaplama başarılı olduğunda sonuç alanında en az:
- `ARŞİVLE`
- `DÜZELT`

görünür.

`DÜZELT` seçeneği aşağıdaki seçenekleri sunar:
- `İHALE BİLGİLERİ`
- `YÜKLENİCİ FİRMA`
- `HAT BİLGİLERİ`
- `HAKEDİŞ GİRDİLERİ`
- `VAZGEÇ`

Seçenek seçildiğinde ilgili ekrana gidilir ve mümkün olduğu kadar düzenlenen kayıt önceden doldurulur.

Hakediş girdilerinin düzenlenmesi aynı hakediş anahtarını koruyarak ilgili kaydı günceller ve yeniden hesaplar.

## 7. Kaynak kayıt düzeltmeleri ve yayılım

Kaynak kayıtlardan biri değiştirildiğinde:
- ilişki anahtarları yeniden doğrulanır;
- etkilenen hakedişler bulunur;
- her hakedişin kullanıcı girdi alanları korunur;
- güncel ihale/yüklenici/hat sabit değerleri ile yeniden hesaplama yapılır;
- kümülatif raporlar ve icmaller ilgili dönem sırasına göre yeniden oluşturulur;
- geçersiz hale gelen yinelenen hakediş kombinasyonları engellenir;
- değişiklik kullanıcının görebileceği bir sonuç mesajında özetlenir.

Normal canlı kaynak değişiklikleri geçmiş arşiv snapshot'larını değiştirmez. Yalnızca kullanıcı `ARŞİVLER > DÜZELT` ile belirli bir snapshot'ı seçtiğinde o snapshot aynı revision dosyası üzerinde güncellenir.

## 8. Arşiv sistemi

### 8.1 Dizin

Kullanıcıya görünür çıktı/arşiv alanı:

`%USERPROFILE%\\Desktop\\OzelEgitimHakedisSistemi\\Arsiv\\Hakedisler`

Mevcut PDF/Belge klasörleri korunacaktır.

### 8.2 Arşiv dosyasının içeriği

Arşiv dosyası kendi başına yeniden yüklenebilir olmalıdır. Sadece bir kayıt ID'sine referans veren ince bir işaretçi dosyası olmayacaktır.

Dosya şunları içerir:
- schema/version bilgisi,
- arşiv revision no,
- oluşturulma zamanı,
- ihale kimliği ve sözleşme bilgileri,
- yüklenici kimliği ve iletişim/finans alanları,
- hat bilgileri,
- hakediş no ve dönem,
- tüm kullanıcı girişleri,
- hesaplanan sonuçlar,
- kesinti kırılımı,
- önceki hakediş referanslarının özetlenmiş tutarları,
- resmi çıktı kimlik bilgileri,
- değişikliğin kaynağı (aktif kayıt / arşiv düzenlemesi).

### 8.3 Dosya adı

İlk sürüm:

`OEHS_[İhale]_Hat-01_Hakediş-03_2026-11-30_v01.json`

Yeni düzenleme:

`OEHS_[İhale]_Hat-01_Hakediş-03_2026-11-30_v02.json`

Dosya sistemi açısından geçersiz karakterler güvenli biçimde normalize edilir.

Normal yeni arşiv revision'ları silinmez veya üzerine yazılmaz. ARŞİVLER > DÜZELT ise açıkça seçilmiş mevcut snapshot'ı aynı revision dosyasında günceller; bu akışta yeni revision üretilmez.

### 8.4 Arşiv paneli

Yeni `ARŞİVLER` paneli:
- arşiv listesini gösterir,
- ihale/hat/hakediş/dönem bilgilerini gösterir,
- revision bilgisini gösterir,
- `ÇAĞIR` ile arşivi çalışma ekranına yükler,
- `DÜZELT` ile düzeltme akışını açar,
- `KLASÖRÜ AÇ` ile klasörü açar.

Arşiv çağrıldıktan sonra `ÇAĞIR` normal canlı veri yükleme akışıdır. `DÜZELT` seçildiğinde archive snapshot doğrudan düzenlenir ve kaydetme aynı dosya/same revision üzerinde yapılır.

## 9. Canlı veri ile arşiv arasındaki ayrım

Canlı uygulama verisinin güvenilir işlem deposu mevcut `LocalApplicationData\\OEHS\\oehs-data.json` yaklaşımında kalabilir.

Kullanıcıya görünür Desktop OEHS klasörü:
- arşiv,
- PDF,
- belge metadata'sı,
- gerektiğinde yedek/snapshot

için kullanılacaktır.

Bu aşamada canlı veri deposu sessizce Desktop'a taşınmayacaktır; veri bütünlüğünü bozabilecek gereksiz depolama migrasyonu yapılmayacaktır.

## 10. Resmi çıktılar ve PDF

Her hakediş için 4 resmi belge korunacaktır:
1. Hakediş Raporu
2. Hizmet İşleri Hakediş Raporu
3. Hizmet İşleri Kabul Tutanağı
4. Hakediş İcmali

Çıktı tek bir A4 PDF dosyasında, bu dört belgenin mevcut düzen mantığı bozulmadan tutulur. PDF OLARAK KAYDET OEHS'nin doğrudan PDF üreticisini kullanır; Microsoft Print to PDF veya başka bir yazıcı kuyruğuna bağımlı değildir.dan üretilecektir.

PDF dosya adı:

`OEHS_[İhale]_Hat-01_Hakediş-03_2026-11-30.pdf`

Resmi çıktılarda:
- ihale/dosya no,
- hat no,
- hat adı,
- yüklenici,
- sözleşme tarihi/no,
- sözleşme bedeli,
- hakediş no,
- dönem/tarih

tümünün aynı kaynak kayıttan gelmesi sağlanacaktır.

Word çıktısı bu akışa geri getirilmeyecektir.

## 11. Veri saklama ve yazma güvenliği

Mevcut ana veri deposundaki atomik yazma yaklaşımı korunacaktır.

Arşiv yazımında da:
- önce geçici dosya,
- başarılı serileştirme,
- sonra atomik/çakışma güvenli taşıma

uygulanacaktır.

Arşiv schema sürümü ile ileride veri modeli değişikliklerinin yönetilmesi sağlanacaktır.

## 12. Test stratejisi

TDD uygulanacaktır. Her üretim değişikliğinden önce ilgili davranışı gösteren başarısız test yazılacak ve kırmızı sonuç gözlemlenecektir.

Test kapsamı:
- XLSM formüllerinin doğrudan karşılıkları,
- VAT muaf/uygulanır durumları,
- fiyat farkı ve iskonto,
- negatif fiyat farkı,
- ek kesin teminat,
- damga vergisi,
- tüm kesinti alanları,
- hakediş benzersizlik anahtarı,
- dönem sıralama/validasyon,
- sonraki hakediş numarası önerisi,
- ihale-hat-yüklenici ilişki bütünlüğü,
- kaynak kayıt düzeltmesinin ilişkili geçmiş hakedişlere yayılması,
- arşiv serialization/deserialization,
- revision v01 -> v02,
- arşivden çağırma/düzenleme,
- PDF dosya adı üretimi,
- rapor kümülatif değerlerinin dönem sırasına göre hesaplanması.

## 13. Genel kontrol / teslim kabulü

Kod tamamlandıktan sonra tek bir son kontrol yapılacaktır:
- `dotnet restore`
- `dotnet build .\\OzelEgitimHakedisSistemi.sln --configuration Debug`
- `dotnet test .\\OzelEgitimHakedisSistemi.sln --configuration Debug --no-build`
- mümkün olan UI/çıktı senaryolarının manuel kontrolü,
- kaynak XLSM ile seçili referans senaryoların karşılaştırılması,
- ilişkisel veri kontrolleri,
- arşiv oluşturma ve tekrar çağırma,
- PDF dört belge birleşimi,
- okunabilirlik/taşma/clipping kontrolü,
- mevcut DogruTemin akışının bozulmadığına ilişkin regresyon kontrolü.

Genel kontrol sonunda kullanıcıya:
- doğrulananlar,
- bulunan hatalar,
- düzeltilenler,
- kalan küçük riskler/iyileştirmeler

açıkça raporlanacaktır.

## 14. Ek koruma önerileri

Uygulama kapsamına dahil edilecek küçük ama değerli korumalar:
- arşivlerde schema sürümü,
- revision numarası,
- insan okunur değişiklik özeti,
- aynı revision dosyasının üzerine yazılmasının engellenmesi,
- arşiv yüklemede bütünlük/doğrulama kontrolü,
- ilişki anahtarlarının normalize edilmesi.

Bunlar iş akışını büyütmeden veri kaybı ve yanlış kayda bağlanma riskini azaltmak için tasarımın parçasıdır.

## 15. Kapsam dışı

Bu iterasyonda:
- yeni veritabanı motoru eklemek,
- Excel'i çalışma zamanında hesaplama motoru olarak kullanmak,
- gereksiz MVVM/altyapı yeniden yazımı,
- ilgisiz modüllerde büyük refaktör,
- yeni belge formatları

yapılmayacaktır.

Amaç mevcut WPF uygulamasını bozmadan, son XLSM'nin kanıtlanmış davranışını ve güvenli hakediş yaşam döngüsünü uygulamaktır.
