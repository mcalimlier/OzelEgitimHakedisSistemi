# OEHS Genel Kontrol — 01.10.2026

## Kapsam

Bu kontrol kaydı, OEHS WPF uygulamasının Excel uyumlu hesaplama, iki kaynaklı ilişki modeli, dört belge çıktısı, PDF üretimi ve arşiv düzeltme akışının release öncesi durumunu kaydeder.

## Otomatik Kontrol

GitHub Actions Windows Build üzerinde son doğrulanan kod commit'i:

- `dd3e4be90197ce5cab21ed1c8f81e4f17dc3e86e`
- WPF restore: başarılı
- Test restore: başarılı
- WPF build: başarılı
- Test build: başarılı
- Test Solution: başarılı
- Windows EXE publish: başarılı
- EXE artifact upload: başarılı

Önceki UI/PDF denemelerinde oluşan derleme hataları düzeltilmiştir. Son başarılı workflow bu commit'in WPF ve test derlemelerini tamamlamıştır.

## Hesaplama

Kontrollü senaryoda kullanılan temel değerler ve hesap zinciri mevcut testlerde korunmaktadır. Hesaplama motoru; taşıma, rehber, fiyat farkı, KDV, tevkifat, damga, ek teminat ve kullanıcı girişli diğer kesintileri birlikte hesaplar.

## İlişki Anahtarı

Hat kimliği:

`İhale/Dosya No + Hat No + normalize edilmiş Yüklenici VKN/TCKN`

Bu sayede aynı firma farklı ihale/dosya numaralarında aynı hat numarasını kullanabilir.

## PDF

PDF üretimi artık yazıcı bağımlı değildir.

Hedef klasör:

`Desktop\\OzelEgitimHakedisSistemi\\Ciktilar\\Hakedisler\\<İhale-Dosya-No>\\Hat-<Hat-No>\\`

Dosya adı:

`OEHS_<İhale>_Hat-<Hat-No>_Hakediş-<NN>_<yyyy-MM-dd>.pdf`

Önizleme aynı A4 sayfa rasterı üzerinden yapılır; sayfa geçişi ve zoom kontrolleri vardır.

## Arşiv

Normal ilk arşivleme:

- v01 oluşturur.

Arşiv düzeltme:

- `ARŞİVLER > DÜZELT`
- Seçilen bitmiş işin snapshot'ı açılır.
- İlgili bölüm seçilir.
- Değişiklik aynı arşiv JSON dosyasına ve aynı revision numarasına doğrudan yazılır.
- Yeni v02/v03 oluşturulmaz.
- Aktif canlı kayıtta yeni bir hat oluşturulmaz.

HAT BİLGİLERİ düzeltmesinde Hat No dahil aşağıdaki alanlar düzenlenebilir:

- Hat No
- Hat Adı
- Taşıma İş Günü
- Rehber İş Günü
- Rehber Günlük Fiyat

## Kullanıcı Kabul Testleri — Açık

Gerçek Windows makinesinde son kabul için:

1. GitHub Desktop → Fetch origin → Pull origin.
2. Visual Studio → Rebuild → F5.
3. ÇIKTILAR → BELGE ÖNİZLEME açılarak A4 sayfanın okunabilirliği ve sağ kenar taşması kontrol edilir.
4. PDF OLARAK KAYDET ile dört sayfalı gerçek PDF üretilir ve dosya açılarak görsel kontrol yapılır.
5. ARŞİVLER → DÜZELT → HAT BİLGİLERİ.
6. Hat No'yu değiştirip kaydet.
7. Popup'ta mevcut revision'ın doğrudan güncellendiği ve yeni arşiv oluşturulmadığı görülür.
8. ARŞİVLER listesinde aynı arşiv kaydının yeni hat bilgisiyle kaldığı ve yeni revision oluşmadığı doğrulanır.
9. İki farklı ihale altında aynı firma ve aynı hat numarası ile ilişki izolasyonu test edilir.
10. Hat 1 için Hakediş 1 + Hakediş 2, Hat 2 için Hakediş 1 oluşturularak rapor/İcmal ayrımı kontrol edilir.

## Release Öncesi Durum

- Hesaplama motoru: hazır
- Veri saklama: hazır
- WPF ana akış: hazır
- Arşiv: hazır
- Arşiv düzeltme: uygulanmış ve otomatik testle Replace davranışı doğrulanmış
- PDF üretimi: yazıcısız doğrudan üretim uygulanmış
- PDF önizleme: A4 sayfa görüntüleyici uygulanmış
- Otomatik Windows build/test: son kod commit'inde başarılı
- Manuel gerçek PDF ve iki-hatlı senaryo: kullanıcı kabulü bekliyor
- Release/kurulum paketi: manuel kabulden sonra son aşama

## Not

Gerçek PDF görselinin kurum referansına birebir eşleştirilmesi ve iki-hatlı manuel senaryo, uygulamanın son kabul kanıtıdır. Otomatik CI bu fiziksel/görsel Windows çıktısını tek başına doğrulayamaz.
