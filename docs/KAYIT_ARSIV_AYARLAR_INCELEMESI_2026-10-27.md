# Kayıtlar, arşiv ve ayarlar incelemesi

27 Ekim 2026 — `feature/official-payment-document-templates`

## Sadeleştirme

Kullanıcının gösterdiği beş menü öğesi iki ana girişe indirildi:

| Önce | Yeni düzen |
| --- | --- |
| Hakediş Geçmişi | Kayıtlar ve Arşiv → Hakediş Kayıtları |
| Arşivler | Kayıtlar ve Arşiv → Arşiv Revizyonları |
| İstatistikler | İşlevi olmayan yer tutucu ekran kaldırıldı |
| Geçici Hesap Tutucu | Hakediş Oluştur → Canlı Hesap Dökümü; Hakedişe Dön düğmesi var |
| Ayarlar | Yedekleme, pencere görünümü ve hesaplama sabitleri |

Canlı hakediş kaydı ile arşiv revizyonu aynı şey sayılmadı; farklı işlemleri iki sekmede korunuyor. İstatistikler ekranında hesaplanan veri veya kayıt bulunmuyordu. Kaldırılan menüler kullanıcı verisi silmiyor.

Kayıt alanında eğitim türü açıkça seçilir. Sık kullanılan arama/dönem alanları görünür; İKN, firma, durum ve önceki revizyon seçimi ayrıntılı filtrelerde. Genişleyen filtreler kendi alanında kayar. Tablo kalan yüksekliği kullanır; işlem düğmeleri altında görünür kalır. Uzun metinler hücre içinde alt satıra geçer.

Ayarlar'daki değiştirilemeyen tema/para birimi kutuları ve genel altyapı etiketleri kaldırıldı. Yedekleme işlemleri, otomatik kaydedilen pencere tercihi ve gerekçeli oran değişikliği ekranına giriş korundu.

## Kod düzeltmeleri

- Kayıt filtreleri başka ekrandaki program seçimine bağlıydı. Yerel eğitim türü seçimi iki sekmenin filtre ve dönem seçeneklerini birlikte yönetiyor.
- Filtre sonucu boşken geçmişin boş durum mesajı genel kayıt sayısına bakıyordu. Artık görünen sonuçlar esas alınıyor; filtrelenmiş net toplam da aynı kapsama bağlı.
- Liste yenilendiğinde satır seçimi kayboluyordu. Geçmiş seçimi kimlik ve programla, arşiv seçimi tam dosya yoluyla korunuyor.
- Daraltılan gerçek pencerede para/revizyon sütunları asgari genişlik tanımlı olmadığı için aşırı küçülebiliyordu. Güvenli asgari genişlikler eklendi; genişten dara geçiş test edildi. UTC olarak saklanan kayıt saatleri yerel saate çevrilerek gösteriliyor; eski yerel/belirsiz tarih değerleri yeniden kaydırılmıyor.
- Arşiv kartlarındaki satır düğmeleri tablo altına alındı. Seçim yokken işlemler kapalı; okunamayan arşivlerde çağırma/düzenleme kapalı, klasör erişimi korunuyor.
- Klasör açma hataları yakalanmıyordu ve var olmayan klasörler oluşturuluyordu. Eksik klasör için açıklama gösteriliyor; erişim/işletim sistemi hataları yakalanıyor.
- Canlı döküm için ayrı menü yerine Hakediş Oluştur düğmesi ve geri dönüş eklendi. Üst modül adı ve etkin menü düzeltildi. Arşiv hakedişini düzeltirken salt okunur döküme bakılabiliyor; düzenleme bağlamı korunuyor.
- Arşiv çağırmada, girdiler yüklendikten sonra genel seçim listelerinin yenilenmesi hat/firma/dönem seçimini temizliyordu. Liste yenileme öne alındı ve mükerrer yükleme kodu, mevcut ortak kayıt düzenleme akışıyla değiştirildi. Canlı formül açıklamasındaki günlük fiyat/fark/iskonto girdileri dört basamakla gösteriliyor; hesaplama sonuçları iki basamakta kalıyor.
- Yedek geri yükleme kontrolü bazı kaydedilmemiş finansal alanları atlıyordu. Günlük fiyatlar, fiyat farkları, iş günleri ve kesinti girdileri; geçersiz veya negatif girilen değerler dahil kontrol ediliyor. Boş/sıfır alanlar tek başına açık çalışma sayılmıyor.
- Pencere tercihi yazılamazsa istisna dışarı çıkıyordu. Önceki seçim korunup hata Ayarlar içinde açıklanıyor.
- Deneme paketinin arşiv yolu tam yedeğin topladığı çıktı ağacının dışındaydı. Deneme arşivleri artık yedek kapsamındaki `DenemeVerisi/OzelEgitimHakedisSistemi/Arsiv/Hakedisler` içinde. Eski konumdaki dosyalar silinmeden kopyalanıyor; farklı içerikli aynı dosya adı ezilmiyor. Başarılı aktarım kaydı eski konumda tutulduğu için, daha sonra geri yüklenen bir yedeğe eski revizyonlar kendiliğinden yeniden eklenmiyor. Normal kurulumun veri ve arşiv yolları değişmedi.

Hesaplama formülleri, vergi oranları ve dört resmî PDF şablonu bu değişikliklerde değiştirilmedi. Yedek geri yükleme onayı, paket doğrulaması, otomatik kurtarma yedeği ve arşiv revizyonlarını koruma davranışı devam ediyor.

## Doğrulama

Kaynak senaryoları düzeltme öncesinde yeniden üretildi: kayıt/arayüz/geri yükleme için 13 başarısız test ve deneme arşivinin yedek kökü dışında kalması için bir başarısız test kaydı var. Düzenleme sonrasında Core 276 ve WPF 316 test geçti. WPF yerleşim kontrolleri 1024×600, 1024×768, 1366×600, 1366×768 ve 1920×1080 ölçülerini; ayrıntılı filtrelerin açık/kapalı durumunu kapsıyor.

| Gerçek uygulama kontrolü | Sonuç |
| --- | --- |
| Bağımsız eğitim türü seçimi | İki programın kayıt ve arşiv sonuçları ayrılıyor; Ana Ekran seçimi değişmiyor |
| Sekmeler ve satır işlemleri | Geçmiş/arşiv erişimi, seçimin yenilemede korunması ve boş sonuç mesajları doğrulandı |
| Arşiv revizyonları | Son revizyonlar ile tüm revizyonlar arasında geçiş doğrulandı |
| Arşiv çağırma | İki programda firma, hat, dönem ve dört ondalıklı fiyat editöre doğru yüklendi |
| Canlı döküm | İki programda doğru bağlam, dört basamaklı formül girdileri, etkin menü ve geri dönüş doğrulandı |
| Küçük/büyük pencere | Gerçek 1024×600 ve 1366×768 pencerelerde tablo ve işlem alanları görüntülendi; tutar/revizyon sütunları daralmıyor ve kullanılabilir genişlik dolduruluyor |
| Kayıt saati | UTC saklanan kayıt zamanının yerel gösterimi doğrulandı |
| Pencere tercihi | Başarılı kayıt ve yazma hatasında önceki seçimin korunması doğrulandı |
| Tam yedek | Ayrı deneme alanındaki dört hakediş ve beş arşiv revizyonu pakete dahil edildi ve paket incelendi |
| Visual Studio | Temiz yeniden derleme ve son değişikliklerden sonraki derleme başarılı; son hata listesi 0 hata, 0 uyarı |
| Yerel CI kontrolleri | Dört statik kontrol başarılı |

Son kanıtlar `.superpowers/records-core-final.log`, `.superpowers/records-final-wpf-316.log`, `.superpowers/records-vs-final-build.log`, `.superpowers/records-vs-state.txt` ve `.superpowers/RecordsNativeSmoke/bin/Debug/net8.0-windows/records-result.txt` dosyalarındadır. Native deneme 96 DPI'da, kendi işaretli deneme veri klasöründe çalıştırıldı. Kayıt, arşiv, ayarlar ve canlı döküm görüntüleri aynı deneme aracının çıktı klasöründedir.

Kullanıcı kayıtlarında silme veya geri yükleme denemesi yapılmadı. Deneme araçları ve üretilen görüntüler `.superpowers` altında tutulur; Git'e eklenmez. Gerçek 144 DPI testi yapıldığı veya uygulamanın bütün olası koşullarda hatasız olduğu iddia edilmiyor.
