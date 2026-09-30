# OEHS - 1 Ekim 2026 Devam Notu

## Durduğumuz yer

Branch: feature/wpf-contract-entry-v0.5

Bu oturumda onaylanmış spec ve implementation plan uygulanmaya devam edildi.

## İşlenen ana parçalar

- Final XLSM hesaplama zinciri esas alındı.
- Taşıma tevkifatı: KDV'nin 1/2'si.
- Rehber tevkifatı: rehber KDV'nin 9/10'u.
- Ara hesaplamalara kaynakta olmayan ROUND(2) eklenmemesi.
- VKN/TCKN normalize edilmiş stable identity.
- İhale/Dosya No + Hat No + VKN/TCKN stable route ilişkisi.
- Hakediş duplicate anahtarı.
- Dönem sıralaması ve sözleşme tarih aralığı kontrolü.
- Hakediş numarası için aynı ihale/hat/yüklenicide max+1 önerisi.
- ARŞİVLE, DÜZELT, ARŞİVLER, ÇAĞIR, KLASÖRÜ AÇ.
- Arşiv snapshot'ı ve revision: v01/v02/v03; eski revision'lar üzerine yazılmıyor.
- Arşiv snapshot restore: canlı kaynaklar eksik olsa da snapshot'tan ihale/yüklenici/hat/hakediş geri getirilebiliyor.
- Kaynak ihale/yüklenici/hat değişikliklerinin ilişkili hakedişlere propagation'ı.
- DÜZELT hedefleri: İHALE BİLGİLERİ, YÜKLENİCİ FİRMA, HAT BİLGİLERİ, HAKEDİŞ GİRDİLERİ, VAZGEÇ.
- Hakediş Raporu kümülatif hesapları Core servisine taşındı.
- PDF filename servisi eklendi ve desen: OEHS_[İhale]_Hat-01_Hakediş-03_2026-11-30.pdf
- PDF yazdırma akışında A4 portrait ve Microsoft Print to PDF korunuyor.
- 4 belge sırası korunuyor: Hakediş Raporu, Hizmet İşleri Hakediş Raporu, Hizmet İşleri Kabul Tutanağı, Hakediş İcmali.

## Son kullanıcı F5 durumu

Visual Studio'da .NET SDK 8.0.425 kurulu ve Fetch/Pull yapıldı.

Önceki hata listesinde 4 gerçek CS8604 hatası vardı; bunlar düzeltildi. Sonraki görüntüde kırmızı hatalar görünmüyor; ancak tam Rebuild Solution / F5 başarı kanıtı henüz alınmadı.

GitHub Actions run'ları çok kısa sürede failure durumuna düşüyor ve connector job steps/logs vermiyor; bu nedenle CI sonucu güvenilir derleme kanıtı kabul edilmedi.

## PDF ile ilgili KESİN SON BULGU

Kullanıcının yüklediği PDF ve yeni ekran görüntüleri incelendi.

PDF toplam 4 sayfa ve dört resmi belge içeriyor. Referans PDF'de:
- 1. sayfa: HAK EDİŞ RAPORU
- 2. sayfa: HİZMET İŞLERİ HAK EDİŞ RAPORU
- 3. sayfa: HİZMET İŞLERİ KABUL TUTANAĞI
- 4. sayfa: HAKEDİŞ İCMALİ

Yeni ekran görüntülerinde PDF içeriğinin yatayda A4'ün sağ tarafına taştığı, içerik sağ kenarda kırpıldığı ve kullanılabilir dikey alan varken yukarı/aşağı dengesinin bozulup sayfanın yukarı doğru sıkıştırıldığı açıkça görüldü.

Bu bulgu daha önce yanlışlıkla PDF'de taşma yok diye değerlendirilmişti. BU DEĞERLENDİRME GEÇERSİZDİR. PDF layout problemi final düzeltme konusu olarak kabul edilecek.

## PDF düzeltme hedefi

Mevcut içerik ve belge tasarımı korunacak; yalnızca sayfa geometrisi düzeltilecek:
- bütün tablo/document genişliği A4 printable width içine sığacak,
- sağ kenardan taşma olmayacak,
- içerik dikey olarak gereksiz sıkıştırılmayacak,
- A4 portrait 210x297mm ölçüsünde kalacak,
- dört belgenin her biri tek sayfada kalmalı,
- sağ/sol kenar boşlukları tutarlı olmalı,
- BuildCombinedRouteDocuments ve TryShowDocument birlikte incelenecek,
- FlowDocument PageWidth/PageHeight/Margin/column-width sabitleri kontrol edilecek,
- özellikle tabloların sabit piksel genişlikleri A4 printable width'e göre yeniden hesaplanacak,
- sonrasında gerçek PDF alınarak render ile doğrulanacak.

## PDF veri tutarlılığı için açık konular

Kullanıcının yüklediği örnek PDF'de:
- 1. sayfa Hakediş Raporu D/E alanlarında sayısal tutarsızlık gözlendi.
- 4. sayfa İcmal'de önceki kesinti ile cari kesinti ayrı gösteriliyor.
- 3. sayfa Kabul Tutanağı'ndaki hizmet dönemi, hakediş tarihini takip eden ay olarak oluşturulmuş.

Kodda Hakediş Raporu artık E = C - D ve G = E + F mantığına bağlandı.

Ancak bu üç belgenin aynı geçmiş/dönem kaynağını kullandığının gerçek PDF üzerinde yeniden doğrulanması gerekiyor.

## Yarın ilk yapılacaklar

1. Kullanıcı tarafında: GitHub Desktop -> Fetch -> Pull; Visual Studio -> Rebuild Solution -> F5; Hata Listesi ekranı gönderilecek.
2. Derleme yeşil ise kontrollü bir hakediş oluştur; Raporlar -> 4 Belge Tek Yazdırma; PDF dialogunda önerilen adı kontrol et; gerçek PDF'yi yükle.
3. PDF üzerinde A4 sağ/sol taşma, dikey sıkıştırma, dört sayfanın tek sayfa/4 sayfa düzeni, tablo genişlikleri ve belge içi değerlerin birbirleriyle tutarlılığı tek tek kontrol edilecek.
4. Arşiv v01, arşiv v02, ARŞİVLER -> ÇAĞIR, DÜZELT, kaynak kayıt değişikliği propagation ve duplicate collision senaryoları F5 üzerinde doğrulanacak.
5. Son olarak dotnet restore, dotnet build ve dotnet test ile tam doğrulama ve Genel Kontrol raporu tamamlanacak.

## Önemli not

PDF'de taşma yok şeklindeki önceki değerlendirme tekrar kullanılmayacak. Yeni kullanıcı görüntüleri taşmayı net biçimde gösteriyor ve PDF layout düzeltmesi final kabul kriterine dahil edilmiştir.

Bu dosya, bir sonraki oturumda doğrudan kaldığımız yerden devam etmek için yazılmıştır.