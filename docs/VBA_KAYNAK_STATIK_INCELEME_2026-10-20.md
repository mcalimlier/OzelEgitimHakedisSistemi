# VBA kaynak arşivleri — statik ön inceleme

Tarih: 20.10.2026  
Kapsam: Kullanıcının sağladığı Temel Eğitim ve Özel Eğitim VBA RAR arşivlerinden kaynak metinlerin çıkarılması ve statik incelemesi.  
Önemli sınır: Bu inceleme Microsoft Excel VBA editöründe Compile/Debug çalıştırması değildir. Makrolar gerçek workbook üzerinde çalıştırılmadı; bulgular çalışma zamanı testinin yerine geçmez.

## Kaynak envanteri

### Temel Eğitim arşivi
- `All_Pages.bas`
- `BuÇalışmaKitabı.cls`
- `Form_Fail.frm` + `.frx`
- `Form_Success.frm` + `.frx`
- `LoginForm.frm` + `.frx`
- `Module1.bas`
- `Module2.bas`
- `Oto_close.bas`
- `Roller.bas`
- `Sayfa1.cls`
- `Sayfa12.cls`

### Özel Eğitim arşivi
- `Module1.bas`
- `Module2.bas`
- `Sayfa1.cls`

Bu arşivlerdeki kaynaklar uygulama kodundan bağımsızdır. Temel ve Özel Eğitim makroları birbirine taşınmamalı; hücre/sayfa eşlemeleri ayrı tutulmalıdır.

## Yüksek öncelikli bulgular

### VBA-01 — AyVerileriniSil seçili ayla sınırlı değil
**Kaynak:** Temel Eğitim `Module1.bas`, `AyVerileriniSil`; Özel Eğitim `Module1.bas`, `AyVerileriniSil`.

Her iki prosedür `Anasayfa!E4/E5` içindeki seçili ayı `seciliAy` değişkenine alıyor, ancak bu değişken temizleme aralığını belirlemek için kullanılmıyor. Bunun yerine tüm aylık aktarım bloklarının tamamı temizleniyor:
- Temel: `D4:N103`, `P4:Y103`, `AB4:AK103`
- Özel: `D4:N103`, `P4:Z103`, `AB4:AL103`, `AN4:AX103`

Ardından yalnızca seçili ayın verilerinin silindiği mesajı gösteriliyor. Bu davranış, çağrı adından ve mesajdan anlaşılan kapsamla uyumsuz ve önceki ayların aktarılmış verilerini de silebilir. **Yüksek risk / veri kaybı.** Özgün XLSM değiştirilmedi; düzeltme, hedef sütun eşlemesi ve yedekli bir kopyada doğrulama yapılmadan uygulanmamalıdır.

### VBA-02 — Özel Eğitim ay aktarım makrolarında geçersiz dönem için sütun koruması yok
**Kaynak:** Özel Eğitim `Module1.bas`: `HakedişToplamlariniAktar`, `FiyatFarkıVerileriniAktar`, `RehberPersonelFiyatFarkıVerileriniAktar`, `KesintiBilgileriniAktarAktar`.

`Select Case ay` bloklarında `Case Else` bulunmuyor. Boş dönem ayrı kontrol edilse de dolu fakat tanınmayan bir değer `hedefSutun=0` bırakabilir; sonraki `Cells(i, hedefSutun)` çağrısı çalışma zamanı hatasına yol açabilir. Bu makrolarda yerel hata yakalama/raporlama da yok. Güvenli düzeltme, ayın hedef aralıkta tek bir başlıkla eşleştiğini yazma döngüsünden önce doğrulamak ve eşleşme yoksa hiçbir hücreyi değiştirmeden çıkmaktır.

### VBA-03 — Özel Eğitim makro işlemleri kısmi aktarımda kalabilir
**Kaynak:** Özel Eğitim `Module1.bas`, dört aktarım makrosu.

Satır 4–103 aralığında hücreler sırayla yazılıyor; işlem ortasında hata oluşursa daha önce yazılan satırlar geri alınmıyor ve kullanıcıya güvenilir bir işlem sonucu verilmiyor. Başlamadan önce tüm gerekli sayfa/sütun eşlemeleri ve kaynak hücrelerin doğrulanması, ardından hata durumunda yazılan hedef aralığın önceki değerlerini geri yükleme ya da işlem kaydını açıkça raporlama gerekir. Resmî hesap değerleri üzerinde varsayımla değişiklik yapılmamalıdır.

### VBA-04 — Yazdırma makrosu hata durumunda ScreenUpdating değerini geri yüklemeyebilir
**Kaynak:** Her iki `Module1.bas`, `SayfaYazdir`.

`Application.ScreenUpdating=False` sonrası dört sayfadan biri yazdırılırken hata oluşursa, mevcut akışta hata işleyicisi yoktur ve `ScreenUpdating=True` satırına ulaşılmayabilir. Güvenli çıkış bölümü ile önceki Application durumunun saklanıp her durumda geri yüklenmesi önerilir. Yazıcı/çıktı doğrulaması gerçek Excel masaüstünde yapılmalıdır.

### VBA-05 — Temel Eğitim rol modülünde Option Explicit ve değişken bildirimi eksik
**Kaynak:** Temel Eğitim `Roller.bas`.

`User_Role_Pages` içinde `i` döngü değişkeni bildirilmiyor ve modülde `Option Explicit` yok. Bu tek başına mevcut ayarda kesin bir compile error kanıtı değildir; ancak yazım hatalarını derleme aşamasında yakalamayı engeller. Sayfa adları `data` sayfasından alındığı için geçersiz/boş ad veya eksik sayfa durumunda rol görünürlüğü kısmen değiştirilmiş halde kalabilir. Önce kaynak kopyada değişken/sayfa envanteriyle doğrulanmalı.

### VBA-06 — VBA dışa aktarma araçları başarısızlığı gizleyebilir
**Kaynak:** Her iki arşivdeki `Module2.bas`; ayrıca Temel Eğitim `LoginForm.frm` sosyal bağlantı olayları.

Kod dışa aktarma yordamı `On Error Resume Next` ile döngü boyunca oluşan hataları bastırıp işlem sonunda yine de başarı mesajı gösterebilir. Böylece bazı modüller dışa aktarılmamışken tamamlanmış gibi raporlanabilir. Sosyal bağlantı olaylarında da `On Error Resume Next` bağlantı hatalarını kullanıcıya bildirmiyor. Dışa aktarma için hata sayacı ve dosya başına sonuç; sosyal bağlantı için sınırlı ve açıklayıcı hata mesajı kullanılmalı.

## Orta öncelikli kontroller

- Temel Eğitim `All_Pages.bas`, `A2.End(xlDown)` ile veri temizliğinin bitişini belirliyor. A2 boşsa veya listede boşluk varsa temizleme aralığı beklenenden farklı olabilir.
- Temel Eğitim `LoginForm.frm`, kullanıcı adı/parolayı `Users` sayfasındaki düz metin değerlerle karşılaştırıyor. Bu, kod derleme sorunu değil; hassas veri/erişim tasarımı riskidir. Bu denetim, uygulama tarafındaki kimlik doğrulamanın aynı mekanizmayı kullanması gerektiği anlamına gelmez.
- Her iki `Sayfa1.cls` değişiklik olayında `Application.EnableEvents` kapatılıyor ve hata yolunda geri açılıyor. Bu iyi bir koruma; yine de gerçek Excel'de toplu yapıştırma, `Undo`, boş/yanlış dönem ve eksik tutanak sayfası senaryolarıyla denenmeli.
- Temel Eğitim `Module1.bas` aktarım prosedürleri dönem başlığını `Find` ile doğruluyor ve hata yolunda `ScreenUpdating` değerini geri açıyor; bu koruma Özel Eğitim aktarım yordamlarında aynı ölçüde bulunmuyor.
- Temel Eğitim `HesapKontrol` ile Özel Eğitim `HesapKontrol`, D8−D10 ile D11'i tam eşitlikle karşılaştırıyor. Kaynak Excel'in saklanan hassasiyeti/yuvarlama zinciriyle gerçek örneklerde test edilmeli; tolerans eklemek kaynak davranışı teyit edilmeden yapılmamalı.

## Otomatik statik envanter sonucu

- Temel Eğitim: 11 metin/kod bileşeni ve 3 formun kaynak dosyaları; form ikili kaynakları da arşivde mevcut.
- Özel Eğitim: 3 metin/kod bileşeni.
- Kaynak modüllerde `Option Explicit` kullanımı tutarlı değil. Temel Eğitim `Module1.bas` açıkça kullanıyor; Temel Eğitim `Roller.bas` ve Özel Eğitim standart modüllerinde görünmüyor.
- Bu turda VBA derlemesi, Excel içi olay testi veya gerçek makro çalıştırması yapılmadı. Dolayısıyla "satır satır compile/runtime kabulü tamamlandı" sonucu çıkarılamaz.

## Önerilen kapanış sırası

1. Her iki workbook'un kopyasında `AyVerileriniSil` kapsamını ve tüm ay sütun başlıklarını doğrula; önceki ay verilerinin yanlışlıkla silinmesini engelle.
2. Özel Eğitim aktarım prosedürlerine yazma öncesi ay/sütun doğrulaması ve güvenli hata akışı ekle; önce kaynak formül/başlık eşlemesini test et.
3. `SayfaYazdir` ve `Roller.bas` için güvenli çıkış, durum geri yükleme ve eksik sayfa senaryolarını doğrula.
4. Excel VBA IDE'de Debug > Compile çalıştır; her workbook'ta boş/yanlış dönem, son ay, eksik sayfa, hatalı hücre ve aktarım ortası hata senaryolarını gerçek kopyada test et.
5. Makro sonrası Temel `T4/U4/W4`, rapor `D15/D28`; Özel `AC4:AG4`, rapor `D28` ve icmal aktarım hücrelerini kaydet.
6. Aynı girdilerle 4×2 PDF matrisi karşılaştırmasını tamamla.

**Resmî ödeme uyarısı:** Bu rapor statik kaynak incelemesidir. Kaynak XLSM dosyaları değiştirilmedi; hesaplama kuralları değiştirilmedi. İcmal fiyat farkı tekrar sayımı riski, gerçek Excel/VBA aktarımı ve kaynak çıktılarıyla doğrulanmadan kodda sessizce düzeltilmemelidir.


## Ek kontrol — aylık silme kapsamı ve başlık eşleşmesi (20.10.2026)

Kaynak RAR arşivleri tekrar çıkarılarak XLSM satır 3 başlıklarıyla birlikte incelendi.

### VBA-07 — Temel Eğitim silme aralıkları Temmuz sütunlarını da dışarıda bırakıyor
- `AyVerileriniSil` tüm ay bloklarını temizliyor; bu zaten VBA-01'deki veri kaybı riskidir.
- Temel Eğitim aktarım yordamlarının başlık aralıkları `D3:N3`, `P3:Z3` ve `AB3:AL3`; Temmuz sütunları sırasıyla N, Z ve AL.
- Silme yordamı ise `D4:N103`, `P4:Y103`, `AB4:AK103` aralıklarını temizliyor; ikinci ve üçüncü aralıkta Z ve AL sütunları eksik. Dolayısıyla tüm aralıkları silmeye devam etse bile Temmuz fiyat farkı ve kesinti verilerini bırakıyor.
- Kaynak XLSM hücre başlıkları, cached values ile teyit edildi: Temel `N3=Temmuz`, `Z3=Temmuz`, `AL3=Temmuz`. Doğru düzeltme, tüm yılı silmek değil, E4'te seçili dönemin her ilgili blokta eşleşen sütununu bulup yalnızca o sütunun 4:103 satırlarını temizlemektir. Tüm hedef sütunlar önceden bulunmadan hiçbir hücre temizlenmemelidir.

### VBA-08 — Özel Eğitim AP3 ay başlığı düzeltildi
- Kullanıcı talimatı ve VBA aktarım eşlemesiyle uyumlu doğru sıra: `AN:AX` içinde Kasım `AP`, Aralık `AQ` olmalıdır.
- Yetkili kaynak XLSM'nin ilk halinde `İcmal Verileri!AP3` yanlışlıkla `=Anasayfa!I9` (Aralık) idi; `AQ3` de doğru biçimde `=Anasayfa!I9` idi.
- Güvenli bir düzeltme kopyası oluşturuldu: `İcmal Verileri!AP3 = Anasayfa!I8` (Kasım), `AQ3 = Anasayfa!I9` (Aralık). Düzeltme yalnızca `xl/worksheets/sheet8.xml` içindeki AP3 formül ve önbellek değerini değiştirdi. XLSM arşivindeki 47 parçanın adları korundu ve `xl/vbaProject.bin` SHA-256 değeri değişmedi.
- Kaynak orijinal dosyanın üzerine yazılmadı. Düzeltilmiş kopyanın Excel masaüstünde açılıp yeniden hesaplanması ve makro çalıştırması henüz yapılmadığından bu, dosya düzeyinde formül düzeltmesi doğrulamasıdır; VBA compile/runtime kabulü değildir.

### Ek doğrulama sınırı
- Orijinal XLSM değiştirilmedi; düzeltilmiş ayrı kopya dosya düzeyinde doğrulandı.
- Formül ve cached başlık doğrulandı; Microsoft Excel'de VBA derlemesi/çalıştırması ve PDF kabulü henüz yapılmadı.


## VBA-09 — Güvenli ay bazlı temizleme ve aktarım yaması hazırlandı (20.10.2026)

Ayrı bir aday kaynak yaması oluşturuldu; orijinal XLSM dosyalarına içe aktarılmadı.

- Temel Eğitim AyVerileriniSil: E4'ten seçili ayı alır; D3:N3, P3:Z3, AB3:AL3 başlıklarında ilgili ayı önceden bulur. Üç hedef de çözülmeden hiçbir hücre temizlenmez. Yalnızca seçili sütunların 4:103 satırları temizlenir. Böylece tüm ayların silinmesi engellenir ve Temmuz Z/AL sütunlarının eski temizleme aralıklarından dışarıda kalması düzeltilir.
- Özel Eğitim AyVerileriniSil: E5'ten seçili ayı alır; D:N, P:Z, AB:AL ve AN:AX bloklarının dördünde de başlık eşleşmesi gerekir; bir hedef bulunamazsa işlem kapalı-güvenli biçimde iptal edilir.
- Özel Eğitim'in dört statik ay eşlemeli aktarım prosedürüne tanınmayan ay için Case Else eklendi; yazmadan önce hedef sütun başlığı seçili ayla karşılaştırılır.
- İndirilebilir aday yama paketi: OEHS_VBA_Guvenli_Ay_Aktarim_Yamasi_2026-10-20.zip. İçinde iki .bas kaynak adayı, açıklama ve statik test sonuçları bulunur.
- Statik kontrol: 9/9 yapısal kontrol geçti. Bu sonuç VBA derlemesi veya Excel çalışma zamanı testi değildir. Orijinal XLSM'ler değiştirilmedi; yama Excel'de derlenip çalıştırılmadan resmî kabul edilmiş sayılamaz.
- Kalan gerçek kabul kapıları: Excel VBA IDE Compile; boş/geçersiz ay ve bozuk başlık testleri; önceki/sonraki ayların korunması; Temel Temmuz N/Z/AL; Özel Kasım AP/Aralık AQ; aktarım ve PDF çıktıları.
