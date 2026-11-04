# Hakediş Düzenleyicisi ve Arşiv Düzeltme Tasarımı

## Amaç

Hakediş oluşturma ekranı yeni hesaplama, çoklu hat hesaplama ve arşivden
düzeltme akışlarında birbirinden bağımsız, denetlenebilir oturum durumlarıyla
çalışmalıdır. Başarılı yeni hesaplamadan sonra yeni işlem girişleri temizlenir;
son hesap kaydı ise arşivleme ve düzeltme için erişilebilir kalır.

## Sorunun kaynağı

Arşiv düzeltme ekranı, arşiv anlık görüntüsündeki sözleşme ve hattı göstermek
yerine canlı kayıt koleksiyonlarından eşini bulmaya çalışır. Canlı kayıt
silindiyse ya da kimlik alanları değiştiyse İKN, ay veya hat seçimi boş kalır.
Çoklu hat taslak önbelleği de arşiv düzeltme kontrolleriyle aynı görsel
kontrolleri paylaştığından eski seçimlerin sızma riski vardır.

Mevcut yeni hesaplama akışı formu dolu bırakarak arşivleme ve düzeltme
düğmelerini kullanılabilir tutar. Bu, tamamlanmış kaydın bağlamı ile yeni
taslağın bağlamını birbirine karıştırır ve tekrar hesaplama riskini artırır.

## Oturum durumları

| Durum | Bağlam kaynağı | Hat seçimi | Başarı sonrası davranış |
| --- | --- | --- | --- |
| Yeni tek/çoklu hesaplama | Canlı sözleşme, firma ve hatlar | Kullanıcı seçer | Yeni taslak temizlenir; son kaydın işlem düğmeleri kalır |
| Canlı geçmiş düzeltme | Canlı kayıt | Tek ilgili hat | Düzenleme biter veya iptal edilir |
| Arşiv düzeltme | Arşiv snapshot'ı | Tek snapshot hat, kilitli | Yeni arşiv revizyonu kaydedilir veya iptal edilir |

Arşiv düzeltme için ekrana geçici bir sözleşme/firma/hat bağlamı sağlanır.
Bu nesneler canlı koleksiyonlara eklenmez ve kaydedilmiş ana verileri
değiştirmez. Ay seçenekleri snapshot sözleşme tarihinden üretilir.

## Geçiş kuralları

1. Arşivden `Hakediş` düzeltmesi seçilince geçici arşiv bağlamı kurulur,
   çoklu-hat taslak önbelleği temizlenir, seçicide yalnızca snapshot hat
   bulunur ve bu hat seçilir.
2. Arşiv düzeltmede hesaplama, yalnızca arşiv snapshot verileriyle yeni
   revizyon üretir. Canlı sözleşme, hat veya yüklenici bulunmaması işlemi
   engellemez.
3. Başarılı yeni tek veya çoklu hesaplamada tüm yeni-taslak seçimleri ve
   girdileri temizlenir. Son hesaplanan kayıt kimliği saklanır; `Arşivle` ve
   `Düzelt` düğmeleri bu kimlik üzerinden çalışır.
4. İptal, arşiv revizyonunun kaydedilmesi ve yeni kayıt akışına dönüş;
   geçici arşiv bağlamını, seçilmiş hatları ve taslak önbelleğini temizler.

## Panel yerleşimi

Panel dört dikey bölüme ayrılır:

1. İhale, firma ve ay için bağlam şeridi.
2. Seçilebilir hat tablosu.
3. Girdi kartları ve hesap özeti.
4. İşlem çubuğu: hesapla, arşivle ve düzelt.

Arşiv düzeltme modunda ilk bölüm amber renkte ve kilitli gösterilir; böylece
kullanıcı canlı kayıt yerine hangi arşiv snapshot'ını düzenlediğini açıkça
görür.

## Hata yönetimi

- Snapshot içinde zorunlu kimlik ya da dönem bilgisi yoksa kullanıcıya hangi
  arşiv alanının bozuk olduğu bildirilir; canlı veriyle tahmin yapılmaz.
- Arşiv düzeltme sırasında çoklu-hat hesaplaması başlatılamaz.
- Kalıcı kaydetme başarısız olursa bellek değişikliği geri alınır ve taslak
  otomatik temizlenmez.

## Doğrulama

- Canlı sözleşme/hat kaldırılmış bir arşiv snapshot'ı yine de hakediş
  düzeltme ekranını tek kilitli hat ve doğru ayla açar.
- Arşiv düzeltme yeni revizyonu snapshot kimliğiyle kaydeder.
- Tek ve çoklu yeni hesaplama sonrasında İKN, firma, ay, hat seçimleri ve
  taslak girdi koleksiyonları temizlenir; son kayıt için arşivleme çalışır.
- Mevcut tam çözüm test takımı ve WPF UI sözleşme testleri çalışır.
