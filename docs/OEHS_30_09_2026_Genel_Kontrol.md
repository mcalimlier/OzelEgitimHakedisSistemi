# OEHS Genel Kontrol — 01.10.2026

## Kapsam

Bu kontrol kaydı, OEHS WPF uygulamasının Excel uyumlu hesaplama, iki kaynaklı ilişki modeli, dört belge çıktısı, PDF üretimi ve arşiv düzeltme akışının release öncesi durumunu kaydeder.

## Otomatik Kontrol

GitHub Actions Windows Build üzerinde son doğrulanan kod branch'i:

- `feature/wpf-contract-entry-v0.5`
- Arşiv düzeltme/state akışı: güncel
- WPF restore/build/test: CI ile doğrulanıyor
- Windows EXE publish/upload: CI ile doğrulanıyor

Son turda önceki sanitizer derleme hatası da kapatılmıştır. Aynı commit için hem push hem PR Windows workflow'u başarılıdır.

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

## Son düzeltmeler — 01.10.2026

- Arşivden DÜZENLE başlatıldığında düzeltme bölümü artık seçim yapılmadan önce normal canlı düzenleme akışına düşmüyor.
- Hat No düzeltmesi mevcut archive revision üzerinde kalıyor; hedef dosya çakışması korumalı.
- İhale düzeltmesinde güncellenen taşıma günlük fiyatı hakediş girdisine de aktarılıyor ve hesaplama yeniden yapılıyor.
- Hatlar ekranındaki KAYITLI HAT sayacı seçili İhale/Dosya No kapsamındadır.
- Arşivde SON REVİZYON göstergesi listedeki en yüksek revision numarasını kullanıyor.
- Ana WPF shell sabit Viewbox ölçeklemesi yerine gerçek pencere çalışma alanına yayılan responsive Border kullanıyor; minimum pencere 1280×720.
- İlk çalıştırmada eski OEHS veri dosyası, .bak/.tmp dosyaları ve programın Desktop\OzelEgitimHakedisSistemi altındaki eski arşiv/çıktılar bir kez temizleniyor. Temiz başlangıç marker'ı sonraki çalıştırmalarda yeni kullanıcı verilerinin silinmesini engelliyor.
- Fresh-start davranışı için otomatik testler eklendi.
- Statik UI taraması: 199 named element, duplicate name 0, eksik event handler 0, eksik StaticResource 0, yazıcı bağımlılığı 0.

## Not

Gerçek PDF görselinin kurum referansına birebir eşleştirilmesi ve iki-hatlı manuel senaryo, uygulamanın son kabul kanıtıdır. Otomatik CI bu fiziksel/görsel Windows çıktısını tek başına doğrulayamaz.

## Otomasyon Sınırı

Kod, static analysis, Windows CI, build, test ve self-contained EXE publish otomatik olarak doğrulandı. Gerçek masaüstü pencere çizimi, kullanıcının monitöründeki piksel ölçeği ve gerçek PDF'nin görsel/kurumsal referansla birebir karşılaştırılması bu ortamda otomatik olarak gözlenemez. Bu yüzden son manuel kabul yalnızca gerçek Windows F5 ve üretilen PDF görsel kontrolüdür.
