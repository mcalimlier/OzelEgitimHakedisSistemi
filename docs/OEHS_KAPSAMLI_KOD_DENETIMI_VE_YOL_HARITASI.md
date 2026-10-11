# OEHS — Kapsamlı Kod Denetimi ve Yayına Hazırlık Yol Haritası

**Depo:** mcalimlier/OzelEgitimHakedisSistemi  
**Çalışma dalı:** feature/wpf-contract-entry-v0.5  
**Denetim tarihi:** 11.10.2026

> Bu belge kaynak kod incelemesi ve CI bulgularına dayanır. Sıfır hata garantisi değildir. Gerçek PDF görüntü karşılaştırması, fiziksel yazıcı ve temiz Windows kurulum/kaldırma testleri tamamlanmadan üretime hazır denmemelidir.

## 1. Uygulanan değişiklikler

- DoğruTemin'deki DocumentViewer / FixedDocument önizleme yaklaşımı OEHS'ye taşındı. Yazdırma aynı dört sayfalı FixedDocument nesnesini PrintDialog + DocumentPaginator üzerinden kullanıyor.
- DoğruTemin'deki vektör tabanlı PdfVectorWriter OEHS'ye aktarıldı. Dört resmi belge tek OehsOfficialDocumentFactory tarafından sabit A4 FixedPage şablonları olarak üretiliyor. Önizleme/yazdırma dört sayfalı birleşik belge, PDF kaydetme dört ayrı tek sayfalı PDF alıyor.
- Eski FlowDocument rapor üreticileri ve eski raster PDF dışa aktarma yolu kaldırıldı.
- PDF dosyası varsa üzerine yazmadan önce onay isteniyor. Dört yeni PDF önce geçici klasörde üretiliyor; hedef dosyalara ancak hepsi hazırsa aktarılıyor. Aktarım hatasında rollback deneniyor. PDF kaydı sonrası meta veri kaydı başarısız olursa ayrı uyarı veriliyor.
- Hesaplama sayfasındaki AG ile Hakediş Raporu D28 ayrı hesaplanıyor. D28 formülü Excel'deki gibi brüt ve kesintinin iki haneli yuvarlanmış değerlerini ayrı çıkarıyor.
- Excel'deki Hakediş Raporu D11 = IF(D7<0,H2-ABS(D7),H2+D7) davranışı korundu; satır etiketindeki “C-D” ile çelişki sessizce değiştirilmedi.
- İcmal Eylül–Temmuz 11 satır üretir. XLSM lookup aralığının Eylül–Haziran olması ve İcmal Verileri AN3:AX3 içinde Kasım/Aralık başlık sorunu belgelenip testlere alındı.
- Önceki mali yıldaki veya tarihsiz hakedişlerin seçili dönem toplamına karışması önlendi. Sözleşme iş günü verisi yokken takvim günü farkı “iş günü” diye tahmin edilmiyor.
- İlk çalıştırma temiz başlangıcı artık JSON/yedek/çıktıları kalıcı silmek yerine yedek konumlarına taşımak üzere değiştirildi; aktif veri yine boş başlar.
- Arşiv geri yükleme aynı GUID yanında, alanlar doluysa ihale/hat/yüklenici/hakediş no/dönem bileşik anahtarını da denetleyerek farklı GUID'li yinelenen hakedişi eklemiyor.
- Windows WPF smoke-test projesi çözüm ve CI'ya eklendi. Dört PDF için tek sayfa A4, dosya oluşması ve PDFsharp ile tekrar açılabilirlik test ediliyor.
- Smoke-test PDF'lerini CI alanında saklama ve kısa süreli artifact olarak yüklemeyi deneme özelliği eklendi. Artifact kota hatası build/test başarısını düşürmemeli.

## 2. CI durumu

Son başarılı doğrulanmış çalışma [OEHS Windows Build #1171](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/38157474563): 91/91 Core testi, 1/1 WPF PDF smoke testi, üç proje derlemesi ve Windows x64 dotnet publish başarılı, 0 derleme hatası.

Ancak #1171, sonradan eklenen ilk açılış yedeği, arşiv bileşik anahtar kontrolü ve smoke PDF artifact saklama değişikliklerini kapsamıyor. Bu değişiklikleri içeren yeni çalışma [OEHS Windows Build #1182](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/38157792475) kontrol ediliyor. Bu çalışma tamamlanmadan son commit'ler için CI başarısı ileri sürülmemelidir.

PR tetiklerinde ana uygulama artifact'i atlanabilir; bu nedenle başarılı publish, indirilebilir EXE bulunduğunun kanıtı değildir. Smoke PDF artifact'inin Actions sayfasında gerçekten oluştuğu ayrıca kontrol edilmeli. Önceki bir koşuda storage kotası hatası görülmüştü.

## 3. Referans Excel ve PDF gözlemleri

Dört örnek PDF — 1(3).pdf Hakediş Raporu, 2(3).pdf Hizmet İşleri Hakediş Raporu, 3(3).pdf Hizmet İşleri Kabul Tutanağı ve 4(3).pdf Hakediş İcmali — tek sayfalı A4 dikey ve yaklaşık 595 × 842 pt ölçüsünde.

Formül ayrıntıları docs/OEHS_EXCEL_REFERANS_FORMULLERI_VE_DORT_PDF_SABLONU.md dosyasındadır. Ürün koduna kaynak Excel'deki değerler kopyalanmamalı; yalnızca doğrulanmış iş kuralı ve belge yerleşimi uygulanmalı.

### Kaynak formüllerdeki açık kararlar

| Konu | Kaynakta görülen durum | Açık karar |
|---|---|---|
| Hakediş Raporu D11 | Formül H2 ± ABS(D7); satır etiketi “C-D” der | İş sahibi resmi doğru davranışı yazılı onaylamalı |
| Hesaplama AG ve Hakediş Raporu D28 | Farklı formüller ve farklı sonuçlar mümkün | PDF neti için D28 kullanımı golden-master senaryolarıyla doğrulanmalı |
| İcmal dönemleri | PDF Eylül–Temmuz 11 satır, canlı lookup Eylül–Haziran 10 ay | Temmuzun sıfır/boş/gerçek tutar davranışı teyit edilmeli |
| İcmal Verileri AN3:AX3 | Kasım eksik ve Aralık başlığı tekrarlı görünüyor | XLSM düzeltilmeli veya kaynak fallback davranışını taklit etme kararı belgelenmeli |

### PDF kabul kriterleri

- **Hakediş Raporu:** dikey “KESİNTİLER VE MAHSUPLAR” başlığı; kırmızı E satırı; gri G/H/I toplamları; h maddesinin satır sarması; imza ve örnek no.
- **Hizmet İşleri Hakediş Raporu:** dış çerçeve; uzun hizmet adının sarılması; sözleşme bedeli ve süre uzatım tabloları; imzaların çerçevenin altında kalması.
- **Kabul Tutanağı:** iki sütunlu uzun iş adı tablosu; iki ayrı kabul paragrafı; yüklenici imzası ve imza/unvan alanları.
- **Hakediş İcmali:** beş sütun; Eylül–Temmuz 11 satır; gri toplam/A-TOPLAM; kesinti bölümü; imza ve örnek no.

Yeni smoke testi PDF dosyalarının tek sayfa A4 ve açılabilir olduğunu doğrular; henüz piksel düzeyi görsel eşitliği kanıtlamaz. CI'dan PDF'ler indirildikten sonra referanslarla aynı DPI'da render ve görsel fark karşılaştırması yapılmalıdır.

## 4. Önizleme / yazdırma / PDF akışı

    Seçili hat + son hakediş
              |
              v
    BuildSelectedOfficialDocumentContext()
              |
              v
    OehsOfficialDocumentFactory
       |                    |
       v                    v
    BuildCombined()       BuildSeparate()
       |                    |
       +-> DocumentViewer   +-> PdfVectorWriter -> 4 ayrı PDF
       +-> PrintDialog

Açık doğrulamalar: PDF metninin seçilebilir/aranabilir olması; döndürülmüş yazının doğru konumu; gerçek yazıcıda A4 kenar boşlukları; aynı ada sahip PDF'yi reddederken hiçbir dosyanın değişmemesi; onay verildiğinde dört PDF'nin tamamının doğru aktarılması ve metadata hatasının doğru bildirilmesi.

## 5. Veri ve arşiv güvenliği

### İlk açılış
Eski reset işlemi aktif JSON, yedek ve masaüstü çıktı klasörünü kalıcı silebiliyordu. Yeni kod bunları yedek konumuna taşır. CI'da kontrol edilecek: ilk çalıştırmada aktif dosyalar temizleniyor ve yedekte korunuyor mu; ikinci çalıştırma yeni veriyi koruyor mu; dosya taşıma veya işaretçi yazma hatası yarım durum oluşturuyor mu.

### Arşiv restore
Aynı iş anahtarına sahip farklı GUID'li kayıtların çift olarak eklenmesini engelleyen kontrol ve test eklendi. Son CI bunu henüz doğrulamadı. Aynı iş anahtarında farklı tutarlar bulunursa daha ayrıntılı çakışma raporu eklenmesi değerlendirilmeli.

### JSON store
Bozuk ana JSON için .bak yedeğine dönme ve temp dosyadan atomik taşıma yaklaşımı var. Temp temizliği asıl hatayı maskeleyebilir; dosya kilidi, disk dolu, erişim reddi ve bozuk yedek senaryoları için ek test gerekli.

## 6. Arayüz denetimi

Kaynak XAML'den çıkarılan yapısal şema:

    ┌──────────────────────────────────────────────────────────────────┐
    │ OEHS başlık | Kayıt durumu | Aktif modül | pencere araçları      │
    ├────────────────────┬─────────────────────────────────────────────┤
    │ Ana Ekran          │ Özet / durum                                │
    │ İhale Bilgileri    │ Sözleşme alanları + doğrulama                │
    │ Yüklenici Firmalar │ Firma / kimlik / banka                      │
    │ Hatlar             │ İhale–yüklenici–hat ilişkisi                 │
    │ Hakediş Oluştur    │ Dönem / gün / fiyat / fark / KDV / kesinti   │
    │ Çıktılar           │ Seçim → önizle → yazdır → dört PDF           │
    │ Hakediş Geçmişi    │ Dönem sıralı kayıt                           │
    │ Arşivler           │ Revizyon / restore / klasör                  │
    │ Ayarlar            │ Kurum ve tercih                              │
    └────────────────────┴─────────────────────────────────────────────┘

Açık öneriler:
- İki sabit “KAYIT DURUMU / Hazır” alanı yerine gerçek kayıt/hata durumunu gösteren tek dinamik bileşen kullanılmalı.
- İmza isim/unvanları ve sabit kurumsal metinler kurum yetkilisince onaylanmalı; mümkünse Ayarlar'dan yapılandırılmalı.
- MainWindow.xaml.cs halen yaklaşık 4.945 satır; arşiv, kayıt, sözleşme ve ekran durumunu testlerle koruyarak servis/bölümlere ayırma sürmeli.
- 1366×768 ve 1920×1080 ekranlarda %100/%125/%150/%200 DPI; uzun ad/tutar; klavye odağı ve erişilebilirlik gerçek Windows'ta denenmeli.
- Silme/sıfırlama işlemlerinden önce etkilenen kayıt/çıktı özeti ve açık onay sağlanmalı.

## 7. Öncelikli kalan işler

### P0 — Excel doğruluğu
1. D11 formül/etiket çelişkisi için iş sahibi kararı.
2. AG/D28 farkı için Excel hücre referanslı beklenen değer testleri.
3. Temmuz satırı ve Kasım/Aralık lookup hatası için XLSM düzeltme/uyumluluk kararı.
4. Pozitif/negatif fiyat farkı, iskonto, KDV muafiyeti, rehber, kesinti ve kuruş yuvarlama kombinasyonlarına golden-master testleri.

### P1 — Görsel ve yazdırma
1. #1182 PDF smoke artifact'ini ve CI sonucunu kontrol et.
2. Dört PDF'yi orijinal örneklerle aynı DPI'da render edip farkları düzelt.
3. DocumentViewer, Microsoft Print to PDF ve fiziksel yazıcıda sayfa/kenar/sarma kontrolü.
4. Onaylı üzerine yazma, aktarım hatası ve metadata başarısızlığı senaryolarını Windows'ta dene.

### P1 — Veri/arşiv
1. İlk açılış yedeği ve arşiv bileşik anahtar düzeltmelerinin CI sonucunu doğrula.
2. Kısmi taşıma, bozuk JSON/yedek, disk dolu, erişim reddi, temp dosyası kilitli senaryolarını test et.
3. Farklı tutarlı aynı ticari anahtar çakışmalarında anlaşılır rapor üret.

### P2 — Paket
1. ExcelDocumentExporter için ZIP/XML bütünlük smoke testi ekle.
2. PDF'ye ek olarak çok uzun iş adı, uzun tutar, boş/opsiyonel alan ve on bir aylık tarih senaryolarını test et.
3. Artifact storage kotasını kontrol et; gerçek indirilebilir win-x64 paketi doğrula.
4. Temiz Windows profilinde ilk açılış, kayıt, yeniden açılış, arşiv, PDF, yazdırma, kurulum, güncelleme ve kaldırma kabul testi yap.

## 8. Yayın kabul kontrol listesi

- [ ] CI #1182 ve daha sonraki son commit kontrolleri başarılı.
- [ ] Dört PDF örneğiyle görüntü fark karşılaştırması tamam; taşma/çakışma yok.
- [ ] PDF metni aranabilir/seçilebilir; gerçek yazıcı sonucu kontrol edildi.
- [ ] D11/“C-D”, AG/D28, Temmuz ve Kasım/Aralık davranışı yazılı onaylandı.
- [ ] İlk açılış aktif veriyi temiz başlatıyor fakat eski dosyaları yedekte koruyor.
- [ ] Arşiv restore çift hakediş oluşturmuyor.
- [ ] Var olan PDF, kullanıcı onayı olmadan değiştirilmiyor.
- [ ] JSON/arşiv hata ve yedek geri dönüş testleri tamam.
- [ ] Resmi isimler/unvanlar ve sabit metinler kurumca onaylandı.
- [ ] Gerçek indirilebilir win-x64 paketi ve temiz Windows profil testi tamam.
- [ ] Installer/update/uninstall veri koruma testleri başarılı.

**Mevcut karar:** PDF mimarisi ve ilk smoke test çalışır durumda; #1171 temel build/test/publish'i doğruladı. Son yedekleme, arşiv yinelenen kayıt ve PDF artifact saklama değişiklikleri #1182 ile doğrulanmalı. Gerçek PDF görüntü farkı ve Windows yazıcı/kurulum testleri tamamlanmadan “birebir aynı” veya “hatasız yayın” kabulü yapılmamalı.
