# Hakediş Oluştur — ürün ve doğruluk sözleşmesi (20.10.2026)

## Amaç

`Hakediş Oluştur`, TEHYS'in merkezî iş akışıdır. Kullanıcıyı Excel alışkanlıklarından koparmadan, aynı ay ve aynı ihale altında yalnızca istediği yüklenici/hatları hesaplatabilmeli; her hat için ayrı veri ve düzenleme tutabilmeli; yalnızca seçili hatları hakediş ve resmî çıktılara dahil edebilmelidir.

Bu belge bir kabul/uygulama sözleşmesidir. Henüz uygulanmamış gereksinimleri uygulanmış gibi göstermemelidir.

## Temel kullanıcı akışı

1. Kullanıcı program türünü seçer: **Temel Eğitim/Ortaöğretim** veya **Özel Eğitim**. Boş seçim hiçbir zaman sessizce varsayılan programa dönüşmez.
2. İKN (İhale Kayıt Numarası) ve ihale/sözleşme seçimi program türüne göre filtrelenebilir; ayrıca ortak tabloda tüm program türleri görüntülenebilir. Ortak tabloda program türü açık bir sütun olmalı, aynı İKN farklı program türlerinde tek kayıt gibi birleştirilmemelidir.
3. Seçilen ihale/sözleşmeye ait yüklenici seçilir. Bir sözleşme birden fazla yüklenici/hat ilişkisini destekliyorsa ilişki veri modelinden doğrulanmalıdır; UI'da varsayım yapılmaz.
4. Kullanıcı hakediş ayını/dönemini seçer.
5. Sözleşmeye bağlı hatlar görünür. Kullanıcı tüm hatları seçmek zorunda değildir: örneğin beş hattan yalnızca 1, 3 ve 5'i seçebilir.
6. Her seçili hat için bağımsız düzenlenebilir taslak alanları bulunur. Bir hatta yapılan fiyat farkı, indirim, çalışma günü, rehber/personel veya kesinti değişikliği başka hattın taslağını değiştirmemelidir.
7. Seçimden çıkarılan hatların geçici taslağı aynı dönem oturumu içinde kaybolmamalı; yeniden seçilince geri gelmelidir. Ancak seçili olmayan hat hiçbir hesaplamaya, toplama, arşiv kaydına veya PDF'ye girmemelidir.
8. Dönem değişince eski dönemin geçici girişleri temizlenmeli veya açık onayla yeni döneme kopyalanmalıdır. Sessiz veri taşıma yapılmamalıdır.
9. Hesaplama öncesi özet: program, İKN, ihale/sözleşme, yüklenici, dönem, seçili hat sayısı ve her hat için kritik girişler gösterilir.
10. Hesaplama sonucu önce hat bazında, sonra seçili hatların toplamı olarak gösterilir. Genel toplamın kaynağı hat sonuçları olmalı; tek tek hat sonuçları bağımsız kontrol edilebilir kalmalıdır.
11. Dört resmî çıktı birlikte üretilir: Hakediş Raporu, Hizmet İşleri Hakediş Raporu, Hizmet İşleri Kabul Tutanağı, Hakediş İcmali. Her çıktının hangi seçili hatları içerdiği doğrulanabilir olmalıdır.

## Kaynak adlandırma ve Excel uyumluluğu

- Kaynak XLSM'de bir değişken/girdi hangi tam adla geçiyorsa, o programın çekirdeğinde ve alan eşlemesinde tam yazımıyla korunur. Eş anlamlı diye yeniden adlandırılmaz.
- Temel Eğitim ve Özel Eğitim kaynaklarında aynı kavram farklı adlandırılıyorsa iki ad da kendi program kapsamındaki model/DTO/etiket eşlemesinde korunur. Ortak iç hesaplama modeli ancak açık bir adaptör/haritalama ile kullanılabilir; kaynak adı kaybolmamalıdır.
- Kaynak Excel sayfa adları değiştirilmez.
- Sol menüdeki adlar, kaynak çalışma kitaplarındaki sayfa adlarını esas alır. Aynı işi yapan sayfalar ortak modülde sunulursa menü adı iki kaynak adını da açıkça içeren birleşik bir etiket olur; ör. `Hakediş Raporu` ve `Hizmet İşleri Hakediş Raporu` çıktıları tek bir ekranda gruplanacaksa adları silinmez.
- Nihai menü etiketleri yazılmadan önce iki son XLSM'deki gerçek sayfa listesi ve VBA'nın kullandığı sayfa adları doğrulanmalıdır. Şu anki geçici envanter kaynak referanslarına dayanır; tam workbook envanteri kabul edilmemiştir.
- İKN, sözleşme numarası ve uygulama içi kayıt kimliği ayrı alanlardır. İKN tek başına kayıt kimliği olarak kullanılmamalıdır.

## Veri bütünlüğü ve hesaplama sınırları

- Temel Eğitim ve Özel Eğitim ayrı hesaplama çekirdekleri olarak kalır. Aynı ekranı paylaşmak, formülleri veya kesinti kurallarını birleştirme gerekçesi değildir.
- Taslak anahtarı en azından program türü + sözleşme/ihale kimliği + dönem + hat kimliği bileşenlerini kapsamalıdır. Gerçek kimlikler mevcut veri modelinden doğrulanarak seçilmelidir.
- Kullanıcı seçimi tam bir kimlik kümesi olarak doğrulanır: boş/tekrarlı/boşluk anahtarlı/eksik hat seçimi fail-closed davranır.
- UI seçimi, hesaplama giriş kümesi ve PDF/arşiv kümesi aynı doğrulanmış seçili hat listesinden türetilir. Her aşamada ayrı, bağımsız filtre yazılıp tutarsızlaşmasına izin verilmez.
- Her hesaplama kaynak formül/parametre sürümünü, program türünü, dönem ve hat kırılımını saklamalıdır. Geçmiş kayıtlar yeni varsayılanlarla geriye dönük sessizce değişmez.
- Bir değişkenin veya resmî tutarın değiştirilmesi için kaynak hücre/formül, gerçek örnek ve regresyon testi gerekir. İcmal formüllerinde belgelenen olası mükerrer aktarım, karar ve kanıt olmadan değiştirilmez.

## Ekran düzeni ilkeleri

- Üst bölüm: program türü, İKN/ihale, yüklenici, dönem.
- Orta bölüm: bağlı hatlar listesi; her hatta seçili/çıkarılmış durumu ve hat bazında düzenleme.
- Alt/yan bölüm: seçili hat sayısı, hat bazında ara sonuçlar, genel toplam ve doğrulama uyarıları.
- Kullanıcı aynı ekranda yalnızca tek hat veya birden fazla hat hesaplayabilir.
- Ortak İKN/ihale tablosu iki programın kayıtlarını birlikte gösterebilir; her satır program türüyle ayırt edilir ve filtrelenebilir.
- Kullanıcı hat seçimini değiştirince önceki seçili hat taslağı korunur; seçili olmayan hatlar sonuç toplamına dahil edilmez.
- Kritik işlemlerde açık ve anlaşılır doğrulama mesajları kullanılır; program türü/İKN/yüklenici/dönem eksikliği sessiz varsayımla giderilmez.

## Zorunlu kabul testleri

1. Temel Eğitim: tek İKN, tek yüklenici, beş hat içinden üç hat seçimi; yalnızca üç hat hesap ve dört PDF'ye dahil olur.
2. Aynı senaryoda seçili bir hatta fiyat farkı/indirim değişikliği; diğer iki hattın giriş ve sonuçları değişmez.
3. Beş hat seçiliyken A/B/C → A/C → A/B seçimi; B taslağı geri gelir ama seçili olmadığı sürece hesap/çıktıya girmez.
4. Aynı senaryo Özel Eğitim için kendi alan ve formül isimleriyle ayrıca çalışır.
5. Aynı tabloda Temel ve Özel Eğitim İKN kayıtları birlikte gösterilir; program türü, sözleşme ve hatlar karışmaz.
6. Dönem değişiminde eski dönem taslakları yeni dönem hesabına otomatik sızmaz.
7. Boş, yinelenen veya mevcut olmayan hat anahtarıyla hesap/çıktı üretimi engellenir.
8. Bir hat hesabı başarısızsa kısmi ve yanıltıcı genel toplam/rapor üretilmez; hataya hangi hat ve alanın neden olduğu bildirilir.
9. Dört PDF'nin dönem, İKN/sözleşme, yüklenici, seçili hatlar, tutarlar, imza alanları ve sayfa sayısı birlikte doğrulanır.
10. Kaynak alan adı/sayfa adı envanteri hem Temel hem Özel XLSM'den çıkarılır; menü ve model eşlemesi bununla karşılaştırılır.

## Mevcut uygulamada gözlenen durum ve açık işler

- Mevcut WPF ekranında ihale/dosya numarası seçimi, yüklenici bilgisi, çoklu hat seçimi ve `MultiRoutePaymentGrid` mevcut.
- `RouteDraftCache` ve `SelectedRouteDrafts.SelectExact` seçili olmayan taslakların korunması ve yalnızca seçili taslakların çıktıya gönderilmesi için temel korumaları sağlar; CI testleri bu servis düzeyindeki senaryoları kapsar.
- Bu durum, kullanıcı akışının tüm uçtan uca WPF etkileşimlerinin kabul edildiği anlamına gelmez. Özellikle 1/3/5 hat senaryoları, her hatta bağımsız değişiklik, dönem değişimi ve üretilen dört PDF'deki gerçek hat kapsamı WPF uçtan uca testleriyle doğrulanmalıdır.
- Kaynak XLSM'lerin tam sayfa/alan envanteri ve tüm sol menü etiketlerinin kaynak sayfa adlarıyla eşlemesi henüz tamamlanmış kabul edilmemiştir.
- Son bilinen Windows CI run [39436841637](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39436841637) başarılıdır. Bu sonuç kaynak Excel/VBA paritesi veya masaüstü görsel kabulü değildir.
- PR #3 açık/taslak kalır; ana dala birleştirme ve yayınlama için açık onay gerekir.
