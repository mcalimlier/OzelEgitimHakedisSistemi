# OEHS Genel Kontrol — 01.10.2026

## Kapsam

Bu kontrol kaydı, OEHS WPF uygulamasının Excel uyumlu hesaplama, iki kaynaklı ilişki modeli, dört belge çıktısı, PDF üretimi ve arşiv düzeltme akışının release öncesi durumunu kaydeder.

## Otomatik Kontrol

GitHub Actions Windows Build üzerinde son doğrulanan kod commit'i:

- `d8c470c33a3443f10fedc04e3c8fa4fb273832bf`
- Push workflow #1033: başarılı
- Windows WPF restore/build/test/publish: başarılı
- WPF restore: başarılı
- Test restore: başarılı
- WPF build: başarılı
- Test build: başarılı
- Test Solution: başarılı
- Windows EXE publish: başarılı
- EXE artifact upload: başarılı

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

## Not

Gerçek PDF görselinin kurum referansına birebir eşleştirilmesi ve iki-hatlı manuel senaryo, uygulamanın son kabul kanıtıdır. Otomatik CI bu fiziksel/görsel Windows çıktısını tek başına doğrulayamaz.


## Kod/Arayüz Statik Denetimi — 01.10.2026

- MainWindow.xaml: 180 named element, duplicate name: 0.
- XAML event handler mapping: missing handler 0.
- StaticResource mapping: missing resource 0.
- MainWindow.cs printer dependency scan: 0 sonuç; System.Printing / PrintDialog / Microsoft Print to PDF bağımlılığı yok.
- Ana Viewbox: Uniform + DownOnly; gereksiz büyütme engelleniyor.
- Belge önizlemesi: A4 raster, %110 başlangıç zoomu, sayfa geçişi ve zoom kontrolleri.
- DÜZENLE aksiyonları: 96 px minimum sabit genişlik uygulandı.
- Uzun FieldLabel/CardTitle metinleri Wrap=True.
- PDF tablo geometri testleri: 3/4/5 kolonlu düzenlerin SafeContentWidth sınırı korunuyor.
- Arşiv düzeltmesi: Replace hedef dosyası yoksa yeni dosya oluşturmayı reddediyor.
- Yerel veri: .bak yedeği + bozuk birincil dosyadan geri yükleme + iki dosya bozuksa kontrollü hata.
- Çıkış dosyası: Windows geçersiz/reserved isimleri, slash/backslash ve aşırı uzun path parçaları korunuyor.
- Silme işlemleri: ihale/hat/firma ilişkileri korunuyor; geri döndürülemez silme öncesi onay var.

### Son arayüz / akış regresyon kontrolü — 01.10.2026

- ARŞİVLER → DÜZENLE → HAT BİLGİLERİ akışında panel açıldıktan sonra **DÜZENLE** modu tekrar uygulanıyor.
- **DÜZENLEME İPTAL** düzenleme sırasında görünür; **TEMİZLE** düzenleme sırasında gizlenir.
- Arşiv düzeltmesi aktifken kayıt düğmesi hiçbir koşulda canlı **HAT EKLE** akışına düşemez.
- Arşiv düzeltmesi sırasında yan menüden başka panele geçiş yapmak açık düzenlemeyi sessizce atmaz; kullanıcıdan açıkça **DÜZENLEME İPTAL** veya kaydetme işlemi beklenir.
- Hat listesi işlem sütunu 170 px, arşiv işlem sütunu 350 px olacak şekilde buton taşması için güvenli genişlik bırakıldı.
- Hakediş düzenleme sırasında ARŞİVLE / DÜZELT işlemleri pasif tutulur.
- Statik XAML denetimi: 199 named element, duplicate name 0; eksik event handler 0; eksik StaticResource 0.
- Son otomatik Windows workflow'u restore, build, test ve win-x64 publish aşamalarının tamamında başarılıdır.

## Otomasyon Sınırı

Kod, static analysis, Windows CI, build, test ve self-contained EXE publish otomatik olarak doğrulandı. Gerçek masaüstü pencere çizimi, kullanıcının monitöründeki piksel ölçeği ve gerçek PDF'nin görsel/kurumsal referansla birebir karşılaştırılması bu ortamda otomatik olarak gözlenemez. Bu yüzden son manuel kabul yalnızca gerçek Windows F5 ve üretilen PDF görsel kontrolüdür.
