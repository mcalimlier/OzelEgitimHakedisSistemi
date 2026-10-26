# Gerçek program penceresinde bordro kabulü — 26.10.2026

## Sonuç

İki bordrodaki toplam 42 sayısal örnek, uygulamanın gerçek App/MainWindow başlangıcı üzerinden açılan ayrı deneme penceresinde hücrelere girildi. Gerçek Hesapla/Kaydet butonunun olayı çalıştırıldı. Programın JSON veri deposu tekrar okunarak 42 kaydın girişleri ve hesapları doğrulandı. Her kayıttan dört resmî belge oluşturuldu: toplam 168 tek sayfalık PDF.

Kaydedilmiş net ödemelerin tamamı Excel ile uyuşuyor. Hakediş Raporu PDF netinde 34 örnek birebir aynı; 8 örnekte yalnızca ±0,01 TL yuvarlama farkı var. Kullanıcı bu farkları açıkça kabul etti ve programın mevcut yuvarlama yönteminin korunmasını istedi. 0,01 TL'den büyük PDF net farkı bulunmadı.

## Önceki kontrolden farkı

Bu çalışma yalnız çekirdek fonksiyon çağrısı veya başlatılmamış pencere nesnesi değildir. Gerçek uygulama kaynakları, pencere kurucusu, yüklü kontroller, seçiciler, DataGrid hücre düzenlemesi, bağlama doğrulaması ve Hesapla/Kaydet butonu çalıştırıldı. Girdiler görünür hücrelerin düzenleyicilerine yazıldı; hücre ve satır düzenlemesi onaylandı. Kaydedilmiş hakediş için uygulamanın kendi belge bağlamı, belge fabrikası ve PdfVectorWriter kullanıldı.

Kabul çalıştırıcısı yalnız kendi çalıştırılabilir dizinine `OEHS_DENEME_PAKETI.flag` koyar. Veri kökünün bu dizin altında olduğu kontrol edilmeden uygulama açılmaz. Asıl kullanıcı deposundaki 8 dosyanın başlangıç ve bitiş SHA256 değerleri eşittir.

## Karşılaştırma kapsamı

- Her kaynak dosyada 18 taşıma ve 3 özel eğitim satırı vardır; iki dosya boyunca 42 senaryo. Tekrarlanan örnekler 42 farklı gerçek hat olarak sunulmaz.
- Kaynaktaki bütün sayısal girdiler ve dört basamaklı fiyat hassasiyeti korundu. Kaydedilen girdiler kaynak `ProgressPaymentInput` ile tam eşitlikte karşılaştırıldı.
- 422 karşılaştırılabilir hesap alanı yeniden, bu kez gerçek pencerede kaydedilmiş sonuçlar üzerinden `0,0000001 TL` ham değer toleransıyla doğrulandı.
- Her iki dosyada Taşıma!M21 boş bir fatura referansıdır. Taşıma!M23 kaynakta matrahı gösterir; çekirdeğin matrah + tevkifat alanıyla aynı tanım değildir. Bu dört alan doğruymuş gibi eşleştirilmedi veya anlamlı farkları gizlemek için genel tolerans uygulanmadı. Önceki ayrıntılı açıklama `BORDRO_DOGRULAMA_VE_HESAP_AYRINTILARI_2026-10-26.md` dosyasındadır.
- Hakediş Raporu PDF'lerinde matrah, KDV, tahakkuk/brüt, damga vergisi, tevkifat, ek kesin teminat, kesinti toplamı ve net ödeme doğrudan PDF'deki hücre konumlarından geri okundu: 42 × 8 = 336 kritik para hücresi.
- Net dışındaki bu para hücreleri Excel karşılıklarının iki basamaklı gösterimiyle eşleşti. PDF neti ayrıca kullanıcının kabul ettiği 0,01 TL sınırında doğrulandı.
- 168 PDF'nin tamamında tek sayfa kontrolü, sayfa sınırı içinde metin kontrolü ve çizilen metinlerin PDF'den kayıpsız geri okunması kontrolü geçti.
- Taşıma ve özel eğitim için dört belge türünün birer temsilcisi, toplam sekiz PDF renderı görsel olarak incelendi. Görsel inceleme 168 sayfanın tamamına tek tek bakıldığı iddiası değildir.
- İcmalin toplamı KDV dahil yüklenici netiyle aynı kavram değildir; farklı belge kavramları doğrudan birbirine eşitlenmedi.

## Kabul edilen yuvarlama farkları

Aşağıdaki dört örnek iki dosyada tekrarlandığından toplam sekiz fark vardır:

| Sayfa / satır | Excel net | Hakediş Raporu PDF net | Fark |
|---|---:|---:|---:|
| Taşıma / 6 | 29.529,25 TL | 29.529,26 TL | +0,01 TL |
| Taşıma / 10 | 28.735,02 TL | 28.735,01 TL | −0,01 TL |
| Özel Eğitim / 2 | 54.122,89 TL | 54.122,88 TL | −0,01 TL |
| Özel Eğitim / 3 | 52.295,09 TL | 52.295,08 TL | −0,01 TL |

Excel nihai neti sonunda yuvarlar. Mevcut Hakediş Raporu zinciri brüt ve kesintiyi ayrı yuvarlayıp çıkarır. Bu kural değiştirilmedi. Her dosyada satır bazında iki basamaklı Excel netleri toplamı 607.521,71 TL, PDF netleri toplamı 607.521,69 TL'dir; −0,02 TL toplam fark kabul edilen satır yuvarlamalarının birikimidir. Bu toplam, ham değerler toplamının bir kez yuvarlanmasıyla karıştırılmaz.

## Hakediş kayıtları yerleşimi

- Düzenle, Sil, Hesaplamayı Görüntüle ve Klasör butonları tablonun altındaki tek eylem çubuğuna taşındı.
- Seçim yoksa butonlar pasif; tek satır seçilince dört buton o kayda bağlanarak etkinleşir. Seçim kaldırılınca hedef ve etkinlik temizlenir.
- Tablo metinleri hücre içinde sarılır; kesme/üç nokta kullanılmaz. Satır yüksekliği metne göre büyür.
- Uzun güzergâh ve yüklenici metni, seçimin kaldırılması ve 1024/1366/1920 genişliklerinde buton erişimi denetlendi.
- WPF regresyon paketi: 213 başarılı, 0 başarısız. Kanıt: `tests/OzelEgitimHakedis.Wpf.Tests/TestResults/history-native-2026-10-26.trx`.

### Visual Studio derleme doğrulaması

Normal Visual Studio Debug derlemesinde, yeni testteki `Assert.Empty(collection.Where(...))` ifadesi xUnit2029 kalite kuralı nedeniyle derlemeyi durdurdu. Önceki hızlı test çalıştırmalarında analizörler kapalı olduğundan bu derleme kuralı yakalanmamıştı. İfade aynı kontrolü yapan `Assert.DoesNotContain(collection, predicate)` ile düzeltildi; test kayıt numarası biçimlemesi de kültürden bağımsız yapıldı. Analizörler veya uyarı kuralları kapatılmadı.

Visual Studio üzerinden tüm çözüm yeniden derlendi: dört proje başarılı. NuGet paket bilgileri yenilendi ve çözüm kaydedilerek yeniden yüklendi. Son doğrulama doğrudan açık Visual Studio oturumundan yapıldı: `BuildState=3`, `LastBuildInfo=0`, Hata Listesi'nde 0 hata; derleme çıktısında 4 başarılı / 0 başarısız. Kanıtlar `.superpowers/vs-open-ide-final-build.log`, `vs-open-ide-final-state.txt` ve `vs-open-ide-final-nuget.log` dosyalarındadır. İlgili iki test Debug derlemesinde tekrar çalıştırıldı ve geçti (`vs-build-fix-2026-10-26.trx`).

## Kanıtlar ve yeniden çalıştırma

Çalışan kabul aracı: `tests/OzelEgitimHakedis.NativeAcceptance/`. Yeniden çalıştırma: `scripts/Run-PayrollNativeAcceptance.ps1`. Betik önce kaynak Excel dosyalarının hashlerini doğrular, sonra kendi yeni deneme klasöründe çalışır. Normal veri farklı çıkarsa kendi kendine geri yükleme yapmaz; kontrolü başarısız sayar.

Bu çalışmanın kanıt klasörü: `.superpowers/payroll-native-2026-10-26/evidence/`.

- `results.json`: kaydedilmiş girişler, hesap sonuçları ve her satırın Excel/PDF farkı.
- `comparison.csv`: 42 satırlık okunabilir karşılaştırma.
- `summary.json`: 42 kayıt / 168 PDF / 8 yuvarlama farkı.
- `normal-data-protection.json`: 8 başlangıç, 8 bitiş, 0 değişiklik.
- `case-01` … `case-42`: gerçek programdan üretilen dört PDF.
- `renders/`: görsel olarak incelenen sekiz çıktı.
- `.superpowers/history-native-tests.log`: son WPF test sonucu.

## Sınırlar

Hat/firma/ihale kimlikleri ve Ekim 2026 dönemi test kurulumu için oluşturuldu; kaynakta bulunmayan sözleşme bedeli 0 kabul edildi. Aynı hattın farklı fiyat parçaları ayrı hesap senaryoları olarak girildi; bu çalışma tek bir gerçek hat-ay kaydına çoklu fiyat dönemi ekleme özelliği getirildiği anlamına gelmez. Kaynak dosyalar yeniden yazılmadı veya Excel'de yeniden hesaplanmış gibi sunulmadı; dosyada saklanan Excel sonuçları referanstır.

Sonuç sağlanan senaryolar için geçerlidir. Önceki gerçek %150 / 144 DPI kabul maddesi kapanmış sayılmadı. `dist` ve GitHub sürüm varlıkları bu çalışmada yeniden yayımlanmadı.
