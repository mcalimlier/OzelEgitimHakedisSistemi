# OEHS / TEHYS — VBA kaynak ve Excel formül doğrulaması
**Tarih:** 20.10.2026

## VBA sayfa referansları
- Temel Eğitim kaynak arşivinde 11 VBA modül/form/sayfa sınıfı, çalışma kitabında 11 sayfa bulundu.
- Özel Eğitim kaynak arşivinde 3 VBA modül/sayfa sınıfı, çalışma kitabında 8 sayfa bulundu.
- Kaynak kodundaki sabit Sheets(...) / Worksheets(...) referanslarıyla çalışma kitabı sayfa adları karşılaştırıldı. İki programda da eksik sabit sayfa referansı: **0**.
- Bu kontrol dinamik adları, buton atamalarını ve çalışma zamanı erişimlerini garanti etmez.

## Kritik formül ve hücre sonuçları (salt okunur XLSM kontrolü)

### Temel Eğitim
| Hücre | Formül / cached sonuç |
|---|---|
| Hesaplama Bilgileri!T4 | `=I4*6/100` → 441.921 |
| Hesaplama Bilgileri!U4 | 3033.523428144 |
| Hesaplama Bilgileri!W4 | 332516.074511856 |
| Hakediş Raporu!D15 | 3033.523428144 |
| Hakediş Raporu!D28 | 332516.08 |

### Özel Eğitim — AP3 düzeltilmiş kopya
| Hücre | Formül / cached sonuç |
|---|---|
| Hesaplama!AC4 | 661.8 |
| Hesaplama!AD4 | 736.2857196 |
| Hesaplama!AE4 | 27328.7843196 |
| Hesaplama!AF4 | 88837.724 |
| Hesaplama!AG4 | 61508.9396804 |
| Hakediş Raporu!D15 | 736.2857196 |
| Hakediş Raporu!D28 | 61508.94 |
| İcmal Verileri!AP3 | `=Anasayfa!I8` → Kasım 2026 |
| İcmal Verileri!AQ3 | `=Anasayfa!I9` → Aralık 2026 |

## Yama ve statik testler
- Güvenli ay bazlı silme, başlık doğrulama, Temel Eğitim yazdırma hata yolunda ScreenUpdating geri yükleme ve iki programın Module2 dışa aktarma hata raporlaması için aday `.bas` kaynakları pakete eklendi.
- Statik regresyon kontrolü: **27/27 PASS**.
- Bunlar VBA derlemesi veya gerçek Excel çalışma zamanı testleri değildir. Kaynak XLSM'ler bu aday modüllerle değiştirilmedi.
- Temel Eğitim Module2 içindeki kaynakta gömülü kullanıcı adı/şifre korunmuştur ve güvenlik riski olarak kaydedilmiştir.

## Açık kabul kapıları
1. Microsoft Excel VBA IDE `Debug > Compile`.
2. Kopya XLSM üzerinde boş/geçersiz ay, bozuk başlık, seçili ay silme ve önceki/sonraki ayların korunması testleri.
3. Temel Eğitim `T4/U4/W4`, rapor `D15/D28`; Özel Eğitim `AC4:AG4`, rapor `D15/D28` değerlerinin gerçek yeniden hesaplamadan sonra kaydı.
4. Her iki programın dört PDF'sinin, toplam **8 PDF**, gerçek Excel çıktılarıyla tutar, dönem, yüklenici, hat, imza ve sayfa düzeni açısından karşılaştırılması.
5. PR #3 açık/taslak kalmalı; açık onay olmadan merge/release yok.

## Son yama genişletmesi
- Özel Eğitim `Module1` adayına `Option Explicit` eklendi; değişkenler ve parametreler mevcut modül içinde bildirilmiş olarak kontrol edildi.
- Dışa aktarma penceresindeki `.ButtonName` ve `.AllowMultiSelect` satırlarının yanlışlıkla birleşmesini yakalayan bir statik regresyon testi eklendi. Bu test ilk denemede hatayı yakaladı; satır ayrımı düzeltildi.
- Son statik regresyon sonucu: **29/29 PASS**. Bu sonuç VBA IDE derlemesi değildir.


## Veri silme işlemi için geri alma koruması
- Her iki `AyVerileriniSil` aday yordamı, ilk `ClearContents` öncesinde tüm hedef sütunların `.Formula` yedeklerini alır. Hedef başlıkları eksikse veya sayfa korumalıysa işlem başlamaz. Silme sırasında hata oluşursa yedekleri geri yüklemeyi dener.
- Bu eklemelerden sonra statik regresyon kontrolü **33/33 PASS** oldu. Excel çalışma zamanı testleri hâlâ gerekli.

## Otonom VBA statik denetim genişletmesi — 20.10.2026

### Kaynak envanteri ve açık hata yolları
- Çıkarılmış orijinal kaynakta Temel Eğitim için 11, Özel Eğitim için 3 adet `.bas/.cls/.frm` metin kaynağı incelendi; UserForm `.frx` dosyaları da mevcut. Basit prosedür sınır sayımı 49 Sub/Function/Property başlangıç-bitiş eşleşmesi verdi; bu bir VBA derleyicisi değildir.
- Sabit `Sheets(...)` / `Worksheets(...)` referanslarında daha önceki envanterde eksik sayfa adı bulunmadı. Bu, dinamik referansların veya buton/event bağlantılarının çalıştığını kanıtlamaz.
- Orijinal kaynakta birçok kritik aktarım yordamında hata yakalama yoktu. Yerel güvenli yama adayının v2 paketinde Temel Eğitim `HesapKontrol` ile Özel Eğitim'in dört ay aktarım yordamı ve `HesapKontrol` yordamına açık `On Error GoTo Hata` yolu eklendi.
- Yama v2 dosyası: `OEHS_VBA_Guvenli_Ay_Aktarim_Yamasi_v2_2026-10-20.zip`; SHA-256 `a4438fa9e6d79335f576defdba27a89573fa7a68243bf25aa0649da36984f38a`. Statik kontrol **39/39 PASS**.
- Yama adayı orijinal XLSM'lere içe aktarılmadı. Hata mesajları sağlar; aktarım sırasında yazma hatası yaşanırsa tüm önceki yazıları geri alma garantisi vermez. Bu nedenle “tam hata güvenliği” kabulü değildir.
- Bu statik incelemede VBA IDE compile, gerçek Excel çalışma zamanı, UserForm `.frx` kontrol olayları, buton atamaları ve DPI/ekran testi yürütülmedi. Bunlar açık kabul kapıları olarak kalır.

### Son CI doğrulaması
- HEAD `626feba9c4b879e542fc89fded0511d564c2a973` için PR run [39465630481](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39465630481) başarılı: tam çözüm/WPF derleme, UI statik kontrolleri, test çözümü, PDF smoke ve Windows EXE publish adımları başarılı.
- Smoke PDF/EXE artefaktlarının yüklenmesi ortam sınırlaması nedeniyle atlandı. Bu CI, Excel/VBA masaüstü kabulü ve 8/8 gerçek PDF karşılaştırması değildir.
- PR #3 açık/taslak kalır; merge/release yapılmadı.
## Otonom VBA hata günlüğü adayı — v3

- Yerel yama paketi: `OEHS_VBA_Guvenli_Ay_Aktarim_Yamasi_v3_2026-10-20.zip`; SHA-256 `130fec24eab0eaae5017907e26f553ab3b9e11e8ebf8eaa675a0179be9df8688`.
- Yeni `OEHS_ErrorLog.bas` adayı, hata zamanı, çalışma kitabı, yordam adı, hata numarası/açıklamasını `OEHS_VBA_Error.log` dosyasına en iyi çabayla yazar; çalışma kitabı klasörüne yazılamazsa TEMP klasörünü dener.
- Temel Eğitim ve Özel Eğitim aday Module1/Module2 hata etiketlerine log çağrıları eklendi. Temel Eğitim LoginForm'da çıkış ve sosyal bağlantı yordamları açık hata yoluna geçirildi; Temel/Özel Eğitim `Worksheet_Change` ve Temel Eğitim `Workbook_Open` hata yollarına log çağrısı eklendi.
- Statik regresyon: **45/45 PASS**. Kontroller kaynak metni, yordam sınır sayımı, log çağrısı ve önceki 39 kontrolü kapsar. Bu bir VBA derlemesi/çalıştırması değildir.
- Aday `.frm/.cls` kodu orijinal VBA projesine aktarılmadı; UserForm `.frx` kontrol tasarımı ve gerçek click olay bağlantıları doğrulanmadı. Kopya XLSM'de Excel VBA IDE `Debug > Compile`, hata senaryoları ve log dosyası yazımı doğrulanmadan üretim kabulü verilmemelidir.
- Son doğrulanmış HEAD `c70cbe6b94592b501e6a7885b579f2a270e98f3f` için PR CI [39466840508](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39466840508) ve push CI [39466831941](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39466831941) başarılıdır. Bunlar Windows uygulama build/test/smoke kapılarıdır; VBA masaüstü kabulü ve 8/8 gerçek Excel PDF kabulü açık kalır.
- PR #3 açık/taslak; merge/release yapılmadı.
