# Pratik Ana Ekran ve Güvenli Hakediş İşlemleri Tasarımı

## İstek ve hedef

Kullanıcı altı geliştirmeyi istedi: gerçek sözleşme numarası, tam yedekleme/geri yükleme, PDF öncesi kontrol, düzeltmelerde önce/sonra karşılaştırması, aylık takip, geçmiş/arşiv filtreleri. PDF öncesi kontrol Ana Ekran düğmesinin açtığı panelde bulunacak. Ana Ekran gereksiz ve yinelenen bilgilerden arındırılarak pratik ve anlaşılır olacak.

Başarı: kullanıcı seçilen ayda hangi hattın eksik olduğunu görebilmeli, ilgili kayda geçebilmeli, seçili hakedişi kontrol ederek doğru dört PDF'yi üretebilmeli. Sözleşme kimliği ve geçmiş ilişkileri korunmalı; yedek başka bir bilgisayarda geri yüklenebilmeli.

Bu belge uygulama öncesi tasarım önerisidir. Kod değişikliği veya uygulanmış özellik raporu değildir.

## Yaklaşım seçimi

1. Mevcut ekranların üzerine yeni kartlar eklemek daha az değişiklik gerektirir ancak kullanıcının şişkinlik sorununu büyütür.
2. **Seçilen yaklaşım:** mevcut WPF uygulamasında Ana Ekran'ı aylık iş merkezi olarak yeniden düzenlemek; hesaplama/düzenleme ekranlarını koruyup ortak kontrol ve takip servisleri eklemek.
3. Bütün uygulamayı yeni bir mimariye taşımak bu işin kapsamını ve regresyon riskini gereksiz artırır. Yalnız bu özelliklerin sorumlulukları ayrı sınıflara çıkarılır.

## Ana Ekran: üç bölüm

```text
ANA EKRAN                  Program | İhale: Tümü/seçili | Ay/Yıl

Bekleyen: 4       Kontrole hazır: 8       Tamamlanan: 12

AYLIK İŞ LİSTESİ
Ara: firma, hat, İKN…       Durum: Tümü / Bekleyen / Hazır / Tamamlanan
Firma | Hat | Dönem | Net ödeme | Durum | Sonraki işlem
...

SEÇİLİ HAKEDİŞ — BELGE KONTROLÜ
Firma / Hat / Ay / Hakediş no / İKN
Taşıma: gün × günlük fiyat | Rehber: gün × günlük fiyat
Fiyat farkı | Toplam kesinti | Belgede gösterilecek net ödeme
Eksikler: açıklama + ilgili alanı aç
[Hakedişi aç] [Belgeleri önizle] [Dört PDF'yi kaydet]
```

- Ana Ekran aynı menü düğmesinden açılır. Aylık takip ve PDF kontrolü burada birlikte bulunur; yeni bir üst düzey menü eklenmez.
- Üstte yalnız program/ihale/dönem bağlamı ve üç adet tıklanabilir durum sayacı bulunur. İlk açılışta geçerli ay seçilir; program seçilmemişse seçim istenir. Programlar fark edilmeden karıştırılmaz.
- İKN bir kez gösterilir. Sistem/hazır kartı, motor göstergesi, altı adımlı sürekli yol haritası, yinelenen vergi/taşıma/rehber kartları ve genel toplam kutuları bu panelden kaldırılır.
- Ayrıntılı hesap dökümü mevcut Hakediş/Geçici Hesap Tutucu ekranında, oranlar Ayarlar'da, imzacılar belge ayarlarında kalır. Seçili kayıtta gerekli ayrıntılar açılır bölümle sunulur.
- Kayıt yoksa kısa bir açıklama ve tek sonraki adım gösterilir: sözleşme yoksa Sözleşme oluştur; hat yoksa Hat ekle; seçilen ayda ödeme yoksa Hakediş oluştur.
- Satır seçmeden belge kontrol alanı boş yönerge gösterir. Görünen bir önceki satırın tutarı yeni seçim adına gösterilemez.
- 1024×600'de dikey kaydırma ve daralan/sarılan işlem grupları kullanılır. İş listesi gerektiğinde yatay kaydırılır. 1366×768 ve 1920×1080'de düğme/metin örtüşmesi olmaz. Mevcut lacivert/altın görsel dil korunur; durumlar renk yanında metinle belirtilir.

## 1. Kullanıcı düzeltmesi: yalnız İKN

Kullanıcı 1. maddeyi kapsamdan çıkardı: ayrı resmî sözleşme numarası alanı eklenmeyecek ve belge kontrolünde zorunlu tutulmayacak. İKN kullanılacak. Mevcut ContractNumber iç ilişki anahtarı korunur; INTERNAL- değerleri belgelerde resmî numara gibi gösterilmez. Başlangıçtaki numara/snapshot önerileri bu kararla geçersizdir.

## 2. Tam yedekleme ve geri yükleme

- Ayarlar içinde `Yedekleme` bölümü: `Tam yedek al`, `Yedeği incele ve geri yükle`, son başarılı yedek zamanı ve dosya konumu. Ana Ekran'a ayrı bir büyük yedek kartı eklenmez.
- Kullanıcı bir `.oehs-backup.zip` dosyası seçer. Paket programlara ayrılmış canlı verileri, hesaplama sabitleri/değişiklik geçmişini, imzacı/düzen ayarlarını, bütün arşiv revizyonlarını, yönetilen PDF çıktıları ve çıktı kayıtlarını kapsar.
- Paket bildirimi sürüm, tarih, uygulama sürümü, kayıt/dosya sayıları ve dosya SHA-256 özetlerini içerir. Mutlak bilgisayar yolları taşınabilir göreli yollara çevrilir.
- Yedek güncel kaydedilmiş durumdan alınır; kaydedilmemiş form girdileri yedeklenmiş gibi gösterilmez. Yarım `.tmp`, `.pending`, kilit, eski reset yedeği ve yeni yedeğin kendisi pakete alınmaz.
- Geri yüklemeden önce paket sürümü, dosya özetleri, JSON içerikleri ve ilişkileri doğrulanır. Bozuk, yinelenen, mutlak yol içeren veya `..` ile dışarı çıkan ZIP girdileri reddedilir. Çıkarılan toplam veri boyutu ve dosya sayısı sınırlanır.
- İnceleme ekranı tarih, programlar, sözleşme/hat/hakediş/arşiv/PDF sayıları ve işlemin mevcut verilerin yerine geçeceğini gösterir. İlk sürümde birleştirme yapılmaz; kullanıcı onayıyla tam geri yükleme yapılır.
- Değiştirmeden önce mevcut kurulumun otomatik kurtarma yedeği başarıyla oluşturulur; bu adım başarısızsa işlem ilerlemez. Veri ve çıktı kökleri önce geçici alanda hazırlanır, sonra geri alınabilir değiştirme uygulanır. Hata hâlinde eski dosyalar geri getirilir; kurtarma dosyaları korunur.
- Başka bilgisayarda eski mutlak çıktı yolları yeni yönetilen köke çözülür. İlk açılış/reset işaretinin geri yüklenmiş veriyi tekrar sıfırlamaması doğrulanır. Deneme paketi ile gerçek veri kökü ayrımı korunur.
- Kaydedilmemiş düzenleme veya arşiv düzeltmesi varken geri yükleme başlatılamaz; kullanıcı önce mevcut işi kaydeder/iptal eder.

## 3. Ana Ekran'da PDF öncesi kontrol

- Kontrol nesnesi seçili ödeme kimliğine, programına ve dönemine bağlıdır. Ekranda başka satır seçilmesi eski kontrolü geçersiz kılar.
- Firma, hat, dönem, hakediş numarası, İKN, sözleşme tarihleri, taşıma/rehber günleri ve günlük fiyatları gösterilir. Temel programda rehber bilgileri gizlenir.
- Özet: fiyat farkı, kesinti, PDF'nin kullandığı rapor neti. Hesap motoru neti ile rapor neti farklı kavramlarsa etiketler açık olur; iki değer birbirinin yerine kullanılmaz.
- Zorunlu kimlik/tarih/ilişki eksikleri belge üretimini engeller; bir alana git düğmesi doğru sözleşme/firma/hat/hakedişi seçerek açar. Sıfır çalışma günü otomatik hata sayılmaz; fiyat farkı veya düzeltme içeren geçerli ay korunur.
- İmzacı bilgileri ve kullanılan hesaplama oranları açılır ayrıntıda incelenebilir; uygulama mevzuata uygunluğu kendiliğinden onaylamaz.
- `Belgeleri önizle` ve `Dört PDF'yi kaydet` aynı seçili ödeme bağlamını kullanır. Çıktılar ekranındaki mevcut kaydetme yolu da aynı kontrolü uygular; kontrol başka ekrandan atlanamaz.
- Var olan dört dosyanın geçici alanda üretilmesi, topluca yayımlanması ve hata durumunda geri alınması korunur. Uzun metin düzen hatası açıkça gösterilir; başarısız işlem tamamlandı durumuna geçmez.

## 4. Düzeltmelerde önce/sonra

- Arşiv ve canlı geçmiş düzeltmelerinde kaydetmeden önce ortak karşılaştırma penceresi açılır: `Alan | Önce | Sonra`.
- Sadece değişen alanlar gösterilir: sözleşme/firma/hat kimliği ve metinleri, tarihler, çalışma günleri, fiyatlar, fiyat farkları, kesintiler, KDV istisnası ve etkilenen hesap sonucu. Değişmeyen alanlar listeyi şişirmez.
- Günlük fiyat/gün girdileri tam ondalık hassasiyetle; sonuç tutarları para biçiminde gösterilir. Fark, yuvarlanmış gösterime göre değil gerçek değerlerle belirlenir.
- Altta önceki net, yeni net ve işaretli fark; resmî rapor neti değişiyorsa ayrı açık satır bulunur. Kimlik/iletişim değişikliklerinde sıfır parasal fark değişikliği gizlemez.
- Onay/iptal mevcut işleme uygulanır. Arşivde eski revizyon korunur ve yeni revizyon numarası belirtilir. Canlı geçmişte mevcut kayıt değiştirilir; işlem kapsamı açık yazılır. İptal hiçbir dosya veya geçmiş kaydını değiştirmez.
- Hesaplamada kullanılan oranlar değişmişse yeni sonucun güncel oranlarla hesaplandığı karşılaştırmada gösterilir.

## 5. Aylık takip ve çıktı güncelliği

- Beklenen hatlar, seçili program/ihalede sözleşme tarihleri seçili ayla kesişen hatlardır. Sözleşme bağlantısı kopuk kayıtlar sessizce kaybolmaz, eksik ilişki olarak gösterilir.
- Takip birimi program + iç sözleşme kimliği + firma kimliği + hat + yıl/aydır. Çoklu hat işlemi her hattı ayrı satır sayar; başka programın aynı numaralı hattı karışmaz.
- Birden fazla ödeme/revizyon varsa mevcut kimlik servislerinin belirlediği güncel kayıt esas alınır; arşiv revizyonları tamamlanan iş sayısını artırmaz. Belirsiz mükerrer ödeme kontrol sorunu olarak gösterilir.
- Satır durumu: `Hakediş yok`, `Bilgi eksik`, `Kontrole hazır`, `Arşivlendi · PDF bekliyor`, `Tamamlandı`, `PDF eksik/güncel değil`. Sonraki işlem satırdaki somut eksikliğe göre seçilir.
- Üç üst sayaç bu durumları gruplar: Bekleyen (ödeme/bilgi eksik), Kontrole hazır (hazır veya PDF bekleyen/güncel olmayan), Tamamlanan (güncel arşiv ve doğrulanmış dört PDF). Dönem dışı hatlar bu sayaçlara girmez.
- PDF durumuna yalnız arşivdeki belge adından karar verilmez. Başarılı üretimde ödeme/revizyon, rapor bağlamı, imzacı/şablon bilgisi ve dört dosyanın göreli yolu/özeti kaydedilir. Güncel bağlamla eşleşmeyen veya silinmiş/değiştirilmiş dosya tamamlandı sayılmaz.
- Eski doğrulanmamış çıktılar için `PDF doğrulanmadı` gösterilir; yeniden üretimle doğrulanmış duruma geçer. Seçili ayın net toplamları yalnız mevcut ödemeleri kapsar; eksik hatlar sıfır ödenmiş gibi yorumlanmaz.
- Toplu PDF üretimi veya otomatik arşivleme bu sürümün kapsamı değildir; satır bazında açık işlem yapılır.

## 6. Geçmiş ve arşiv filtreleri

- Her iki ekranda ortak düzen: arama kutusu, dönem, İKN, firma, durum ve `Filtreleri temizle`. Aktif program kapsamı her zaman görünür.
- Arama hat no/adı, firma adı ve İKN içinde Türkçe büyük/küçük harf duyarlılığı olmadan çalışır. Dönem ödeme dönemidir; arşivlenme tarihiyle karıştırılmaz. Dönemsiz eski kayıtlar ayrı seçilebilir.
- Arşivlerde varsayılan son revizyon gösterilir; `Önceki revizyonları göster` ile geçmiş açılır. Bozuk/okunamayan arşivler görünür uyarıyla listelenir; sessizce atlanmaz.
- Sonuç sayısı gösterilir. Boş sonuç `Kayıt yok` ve `Filtreye uyan kayıt yok` olarak ayrılır. Temizle yalnız filtreleri sıfırlar, veriyi değiştirmez.
- Görünür satırın düzenleme/çıktı/silme işlemi görünür sıra numarasına değil kalıcı kayıt kimliğine bağlıdır. Filtre değişmesi yanlış kaydı işlemeye yol açmaz.

## Yapı ve doğrulama

- Sözleşme numarası çözümleme, yedek paketleri, aylık takip/filtreleme, kontrol ve fark üretimi küçük bağımsız Core servislerinde bulunur. WPF yalnız seçim, gösterim ve kullanıcı onayını yönetir. Yeni özellikler büyük ana pencereye tek bir ek blok olarak yığılmaz.
- Eski JSON dosyaları yeni isteğe bağlı alanlar olmadan yüklenir. Hesap formülleri, tam decimal hassasiyeti, program ayrımı ve değişmez arşiv revizyonları korunur.
- Testler: eski/yeni sözleşme numarası ve PDF metni; tam yedek roundtrip/bozuk paket/yol kaçışı/başka köke geri yükleme/başarısız değiştirmede kurtarma; gerçek alan farkları/iptal; dönem ve program sınırları/mükerrer kayıt; eksik veya eski PDF; Türkçe arama/filtrede doğru kayıt işlemi; Ana Ekran seçim bağlamı ve dört hedef çözünürlük.
- CI statik Ana Ekran yol haritası kontrolleri yeni tasarıma göre davranış kontrolleriyle değiştirilir; yalnız eski metni korumak için kullanılmayan kontroller bırakılmaz.
- Son doğrulama: tüm Release testleri, CI statik kontrolleri ve `git diff --check`. Mümkün olduğunda bağımsız inceleme; bu oturumda mevcut model erişimi sorunu nedeniyle bağımsız ajan bulunabilirliği ayrıca doğrulanmalıdır.
- Tamamlanan değişiklikler mevcut dala commitlenir. Push, PR ve merge için ayrıca kullanıcı yönlendirmesi gerekir.

## Uygulama yetkisi

Kullanıcı tasarımı onayladı, ayrı resmî sözleşme numarası geliştirmesini çıkardı ve kalan beş maddenin uygulanmasını istedi. Bu kapsamla devam edilir. Mevcut aktif dal kullanılır; bağımsız ajan aracı yoksa oturum içi uygulama ve açıkça belirtilen öz inceleme yapılır.
