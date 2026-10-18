# OEHS Yol Haritası

## Güncel kalite kapısı — 2026-10-18

Bu dalda yeni değişiklikler için aşağıdaki doğrulamalar birbirinden ayrı değerlendirilir:

- **Derleme:** Son commit için doğrulanmış sonuç bulunmadan başarılı kabul edilmez.
- **Otomatik test:** Kaynak kodda regresyon testi bulunması, testin çalıştırıldığı anlamına gelmez.
- **PDF görsel kabulü:** Dört belgenin A4 yerleşimi, tutarları, dönem/yüklenici/hat bilgileri ve imza blokları aynı örnek veri üzerinden birlikte kontrol edilmelidir.
- **Kesintiler ve Mahsuplar başlığı:** Dikey yön ve yazı biçimi korunur; metin, tüm kesinti satırlarının yüksekliğini kapsayan sol hücre içinde hem yatay hem düşey eksende ortalanır. `PdfVectorWriter` vektör çıktısında döndürülmüş metin için tanımlı layout ölçülerini kullanır; `VerticalAlignment.Center` ile çizgi hücrenin kısa ekseninde ortalanır.
- **Açık doğrulama:** Son başlık hizalama değişikliği ve buna ait regresyon testi Windows/.NET 8 ortamında çalıştırılmalı, üretilen PDF görsel olarak incelenmelidir.

Bu kapı, derleme/test sonucu veya PDF ekran görüntüsü görülmeden “tamamlandı” işareti verilmesini önlemek için eklenmiştir.

## Çekirdek ilerleme panosu — 2026-10-18

Aşağıdaki %100 değerleri yalnızca bu dar kapsamlı alt modüllerin **uygulama kapsamının** tamamlandığını belirtir; CI/Windows çalıştırması ve kullanıcı kabulü ayrı kapılardır.

| Çekirdek alt modülü | Uygulama kapsamı | Doğrulama durumu | Kapanış ölçütü |
|---|---:|---|---|
| Dosya adı güvenliği ve normalizasyon | **%100** | Kaynak ve regresyon testleri gözden geçirildi; test çalıştırma sonucu bekliyor | Geçersiz karakter, boş/göreli yol, uzun parça, Windows aygıt adı ve uzantı varyasyonları test edilir |
| PDF dosya adı üretimi ve program/yüklenici ayrımı | **%100** | Program enum üyeleri kaynakta doğrulandı; otomatik test çalıştırma sonucu bekliyor | Özel Eğitim / Temel-Ortaöğretim ayrımı, aylık tarih, çoklu hat, sözleşme ve yüklenici kimlik özeti test edilir |
| Resmî PDF kesinti hücresi başlık yerleşimi | **%100** | Kaynak geometrisi ve merkezleme regresyon kontrolleri mevcut; Windows görsel kabulü bekliyor | Dikey yön, yazı biçimi, döndürülmüş kutu ve birleşik hücre merkezi korunur |
| Dört resmî PDF'nin çıktı sözleşmesi | **%100** | Dört belge türü, tek sayfa portre ve imza içerikleri için testler mevcut/genişletildi; çalıştırma sonucu bekliyor | Her hakedişte dört farklı belge türü üretilir; sayfa yapısı ve imza blokları korunur |

### Yeni kapanan uygulama kapsamları — 2026-10-18

- **Resmî PDF kesinti hücresi başlık yerleşimi (%100 uygulama kapsamı):** mevcut `AddRotatedText` yerleşimi ve `PdfVectorWriter` döndürülmüş metin çizimi incelendi. Regresyon testi, başlığın döndürme açısını, hizalama değerlerini ve birleşik hücre merkeziyle koordinat uyumunu denetliyor. Bu oran görsel kullanıcı kabulü değildir.
- **Dört resmî PDF'nin çıktı sözleşmesi (%100 uygulama kapsamı):** dört belge türünün tam kümesini, tek sayfalı portre çıktılarını ve mevcut imza bloklarını kontrol eden testler bulunuyor. Bu turda her çıktının tek sayfa portre yapısını ve dört belge türünün eksiksiz kümesini ayrıca doğrulayan assertion'lar eklendi. Otomatik test sonucu bekleniyor.

## Çekirdek ilerleme panosu — 7 alt modül hedefi (2026-10-18)

Bu yedi satır **dar kapsamlı alt modüller** için uygulama kapsamı tahminidir; ana hesaplama motorunun veya tüm ürünün %100 hazır olduğu anlamına gelmez. Çalıştırılmamış testler ve kullanıcı görsel kabulü ayrı doğrulama kapıları olarak kalır.

| # | Alt çekirdek modülü | Uygulama kapsamı | Doğrulama kapısı |
|---:|---|---:|---|
| 1 | Dosya adı güvenliği ve normalizasyon | **%100** | Windows/.NET test çalıştırması bekliyor |
| 2 | PDF dosya adı üretimi ve program/yüklenici ayrımı | **%100** | Dosya adı regresyon testleri çalıştırılmalı |
| 3 | Kesinti/mahsup dikey başlığı ve hücre geometrisi | **%100** | Windows PDF görsel kabulü bekliyor |
| 4 | Dört resmî PDF'nin çıktı sözleşmesi | **%100** | Dört belgenin PDF smoke testi ve görsel kontrolü bekliyor |
| 5 | İmza ayarları varsayılanları, boş değer ve bozuk JSON davranışı | **%100** | Var olan üç test Windows'ta çalıştırılmalı |
| 6 | Hakediş raporu Excel eşleştirmesi: A–I tutar hücreleri | **%100** | Örnek test zinciri kaynakta var; .NET testi ve gerçek Excel örneğiyle karşılaştırma bekliyor |
| 7 | Özel Eğitim / Temel-Ortaöğretim belge davranışının ayrılması | **%100** | Program türü başlık/KDV/tevkifat senaryoları çalıştırılmalı |

### 5–7 alt modülleri için bu turda yapılanlar

- **İmza ayarları:** mevcut testlerin eksik dosyada düzenlenebilir varsayılanlar oluşturmayı, özel değerleri korumayı, boş alanlarda varsayılanlara dönmeyi ve bozuk JSON için anlamlı hata vermeyi kapsadığı doğrulandı.
- **Excel A–I eşleştirmesi:** mevcut regresyon testindeki A=220, B=10, C=230, D=120, E=110, F=0, G=110, H=5, I=105 TL kontrol zinciri korundu. Hakediş raporu testine yüklenici, ihale numarası, sözleşme numarası ve dönem tarihi için açık metin kontrolleri eklendi.
- **Program türü ayrımı:** mevcut testte Özel Eğitim ve Temel/Ortaöğretim için ayrı belge çıktıları ve KDV/tevkifat etiketleri denetleniyor; bu davranış test kabul kapsamına alındı.
- Bu üç alt modülün yüzdeleri yalnızca kaynak ve test kapsamı düzeyindedir. Test çalıştırılmadığı için çalışma zamanı doğrulaması başarılı sayılmaz.

### Bu turdaki kapsam kapanışları

- Windows için ayrılmış aygıt adlarında uzantı ve araya boşluk/nokta girme durumları ele alındı; COM¹–COM³ ve LPT¹–LPT³ biçimleri eklendi.
- Dosya adı testleri; fallback, program türü, aylık dönem biçimi, çoklu hat, yüklenici kimlik özeti ve geçersiz hakediş numarası senaryolarını kapsayacak şekilde genişletildi.
- Bu testler GitHub kaynak dosyasına eklendi, ancak burada .NET test çalıştırıcısı çalıştırılamadığı için başarılı test sonucu iddia edilmez.
- Sonraki hedefler daha geniş ve yüksek riskli çekirdeklerdir: dört resmî PDF'nin görsel kabulü, Excel ile hesap eşleştirmesi ve iki taşıma programının uçtan uca senaryoları.

## Ana dönüşüm sırası

1. Excel tersine mühendislik — **YAPISAL TARAMA TAMAMLANDI / SEMANTİK DOĞRULAMA SÜRÜYOR**
2. İş kurallarının çıkarılması — **TAMAMLANDI / doğrulama sürüyor**
3. VBA/makro kontrolü — **KAYNAK KODU ÇIKARILDI / UÇTAN UCA DOĞRULAMA SÜRÜYOR** (iki XLSM'de VBA projesi bulundu; makro aktarım davranışı hesap ve aylık geçmiş kayıtlarıyla çapraz doğrulanıyor)
4. Domain model — **TAMAMLANDI / genişletme sürüyor**
5. Hakediş hesaplama motoru — **TAMAMLANDI**
6. Veri saklama — **TAMAMLANDI / geliştirme sürüyor**
7. WPF arayüzü — **TAMAMLANDI / kullanılabilirlik iyileştirmeleri sürüyor**
8. İcmal ve raporlama — **TAMAMLANDI / gerçek belge eşleştirmesi sürüyor**
9. Excel karşılaştırma testleri — **AKTİF**
10. Gerçek senaryo testleri — **AKTİF**
11. PDF/çıktı son karşılaştırması — **AKTİF**
12. Release ve kurulum paketi — **SON AŞAMA**

## Mevcut çalışma sırası

### 1. Hesaplama doğrulama
- Excel'den doğrulanmış 30.06.2026 / Hakediş No 10 senaryosu otomatik testte korunuyor.
- Hizmet tutarı, fiyat farkı, toplam, KDV, tevkifat, damga, ek teminat, denetim cezası, kesinti toplamı ve net ödeme birlikte doğrulanıyor.
- İskonto, fiyat farkı, vergi borcu, SGK borcu, asgari ücret desteği ve denetim cezası **kullanıcı tarafından dönem/hakediş bazında girilen değerlerdir**.
- Denetim cezası artık sözleşme bedelinden sabit binde oranla türetilmiyor; kullanıcı tarafından girilen tutar doğrudan kesintiye ekleniyor.
- KDV için kullanıcı seçimi esas alınır: **KDV HESAPLA / DAHİL ET** işaretliyse KDV ve buna bağlı tevkifat hesaplanır; işaretli değilse KDV ve tevkifat uygulanmaz.
- Yeni mevzuat/iş kuralları kullanıcı girdisi olarak tanımlanan alanlar dışında sabitlenmeyecek.

### 2. Hat / hakediş veri bütünlüğü
- Raporlar ekranı seçilen hatta göre filtrelenir.
- Seçilen hat değiştiğinde liste, toplamlar ve son hakediş özeti yenilenir.
- Uygulama açılışında kayıtlı ilk hat otomatik seçilir.
- Çıktı üretimi yalnızca seçilen hatta ait hakediş kaydı varsa devam eder.

### 3. Dört belge çıktısı
Her hat için:
1. Hakediş Raporu
2. Hizmet İşleri Hakediş Raporu
3. Hizmet İşleri Kabul Tutanağı
4. Hakediş İcmali

Önizleme ve yazdırma akışı korunur.

### 4. PDF / sayfa düzeni
- A4 portre
- Taşma kontrolü
- Kolon genişlikleri
- Başlıklar
- İmza alanları
- Kabul tutanağındaki resmî metin
- İcmal A/B/C yapısı
- Kurum örneklerine yakın yerleşim

16.10.2026 CI smoke artefaktındaki dört örnek PDF görsel olarak açıldı: her biri tek sayfa A4 portre, Türkçe karakterler okunur ve örnek çıktıda belirgin metin taşması görülmedi. Bu, sentetik test verili smoke çıktısının kontrolüdür; orijinal Excel'in yazdırma alanlarıyla birebir karşılaştırma, gerçek kullanıcı verisiyle kabul testi ve yazıcı kontrolünün yerine geçmez.

Kaynak XLSM dosyalarının yazdırma alanları ayrıca LibreOffice ile PDF'e aktarılarak dört resmi formun yerleşimiyle karşılaştırıldı. Bu karşılaştırma Excel masaüstü/VBA çalıştırmalı birebir kabul testi değildir; kaynak dosyanın mevcut önbellekli değerleri ve LibreOffice yazdırma motoruyla sınırlıdır. Görsel karşılaştırmada Kabul Tutanağı için iki somut uyumsuzluk düzeltildi: Temel/Ortaöğretim formunda süre değeri birim olmadan gösterilirken Özel Eğitim'de "İş Günü" eki korunuyor; anlatım paragrafları programın kaynak form yerleşimine göre farklı düşey konum kullanıyor. Kod değişikliği commit'i: [2818ec6](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/commit/2818ec6cf271114a21d645ee98a4665e151b2b55). Bu değişiklik için yeni CI sonucu henüz doğrulanmadı.

Bu aşamanın son doğrulaması gerçek üretilen PDF'lerin referanslarla karşılaştırılmasıdır.

### 5. Gerçek senaryo testi
Otomatik hesap regresyon testleri genişletildi. Şimdi UI üzerinden gerçek iki hatlı senaryo ile doğrulanacak.

Test sırası:
- Hat 1 oluştur
- Hat 2 oluştur
- Hat 1 için Hakediş 1 ve Hakediş 2 oluştur
- Hat 2 için Hakediş 1 oluştur
- Her hat için dört belgeyi ayrı ayrı önizle / PDF'e çıkar
- Hatların birbirine karışmadığını kontrol et
- İcmal aylarının ve önceki hakediş toplamlarının doğru taşındığını kontrol et

### 6. PDF ve çıktı son kontrolü
- Dört belge ayrı ayrı önizlenecek.
- A4 portre ve taşma kontrolü yapılacak.
- Kabul tutanağı metni ve imza yerleşimi referansla karşılaştırılacak.
- İcmal A/B/C toplamları kontrol edilecek.

### 7. Release
- Error = 0
- Core testleri başarılı
- WPF Rebuild başarılı
- Gerçek senaryo testi tamam
- Dört belge son karşılaştırmadan geçti
- Test paketi oluştur
- Windows kurulum / dağıtım paketi hazırlan

## 29.09.2026 Orijinal Excel doğrulaması

Kullanıcının yeniden yüklediği orijinal `Özel Eğitim Hakediş Ödeme Programı.xlsm` doğrudan incelendi.

- Çalışma sayfaları: Anasayfa, İhale Verileri, Hesaplama, Hakediş Raporu, Hizmetişleri Hakediş Raporu, Hizmet İşleri K. Tutanağı, Hakediş İcmali, İcmal Verileri.
- Dosyada `xl/vbaProject.bin` bulundu; yani bu workbook sürümünde VBA projesi gerçekten mevcut.
- VBA içindeki metinlerde hakediş kontrolü, fiyat farkı aktarımı, rehber personel fiyat farkı aktarımı, kesinti aktarımı, icmal/hakediş toplam aktarımı ve kabul tutanağı kontrolü gibi işlemlere ait makro izleri bulundu.
- Bu nedenle önceki “aktif VBA tespit edilmedi” notu bu yüklenen orijinal dosya için geçerli değildir ve düzeltilmiştir.

### Kullanıcı tarafından girilecek hakediş değerleri

Hakediş ekranında dönem bazında kullanıcı tarafından girilecek alanlar:

- İskonto
- Fiyat Farkı
- Vergi Borcu
- SGK Borcu
- Asgari Ücret Desteği
- Denetim Cezası

Bu alanlar hesap motoruna doğrudan aktarılır; sabit bir mevzuat tutarı olarak üretilmez.

### KDV seçimi

Kullanıcı ekranında **KDV HESAPLA / DAHİL ET** seçeneği bulunur.

- İşaretli → KDV hesaplanır ve toplam/brüt/net zincirine dahil edilir; buna bağlı tevkifat da hesaplanır.
- İşaretsiz → KDV ve buna bağlı tevkifat uygulanmaz.

Excel'in orijinal Hesaplama sayfasındaki “KDV'den Muafmı?” alanı ile mevcut formüller ayrıca karşılaştırılmıştır; OEHS kullanıcı arayüzünde seçim davranışı açık ve anlaşılır olacak şekilde sunulmuştur.

### Excel v2 ikinci tarama — formül ve VBA envanteri tamamlandı / semantik kabul doğrulaması sürüyor
- `vbaProject.bin` içindeki Module1 VBA kaynak kodu çıkarıldı ve makro aktarım akışları doğrulandı.
- Rehber personel hakediş hesabı eklendi.
- Taşıma ve rehber personel fiyat farkları ayrı alanlara ayrıldı.
- Taşıma %10 / rehber %20 KDV ve 5/10 + 9/10 tevkifat ayrımı eklendi.
- Ek kesin teminat, iki fiyat farkının toplamı üzerinden %6 olarak bağlandı.
- İhale tarihi ve sözleşme tarihi veri akışına eklendi.
- Panel kartlarının sabit/placeholder metinleri yerine kayıtlı verileri göstermesi için bağlantılar genişletildi.
- PDF raporlarında kesinti başlığının düşey yerleşimi ve referans tablo düzenleri iyileştirildi.
- Orijinal Excel'de Hakediş Raporu D28 ile Hesaplama AG arasında fiyat farkı kaynaklı formül tutarsızlığı ayrıca belgelendi; PDF birebir eşleştirme aşamasında karar verilecek.


## 1.10.2026 KULLANICI GERİ BİLDİRİMİYLE KESİNLEŞEN ARŞİV/PDF DAVRANIŞI

- ARŞİVLER > DÜZELT, seçilen bitmiş işin snapshot'ını düzenleme moduna alır.
- HAT BİLGİLERİ seçildiğinde Hat No dahil tüm ilgili hat alanları değiştirilebilir.
- Onaylanan arşiv düzeltmesi `SaveRevision` ile yeni bir arşiv revizyonu oluşturur; önceki vXX dosyası korunur.
- PDF OLARAK KAYDET yazıcı sürücüsüne bağlı değildir; OEHS yerleşik PDF üreticisiyle doğrudan PDF dosyası üretir.
- PDF dosyası Masaüstü\OzelEgitimHakedisSistemi\Ciktilar\Hakedisler\<İhale-Dosya-No>\Hat-<Hat-No> altında descriptive adla tutulur.
- Önizleme gerçek A4 sayfa rasterı üzerinden, sayfa geçişi ve zoom kontrolleri ile gösterilir.


## 15.10.2026 — İzole deneme paketi ve arşiv çakışması kontrolü

- Arkadaş incelemesi için Windows x64 self-contained paket üretimi CI'a bağlandı. Paket yalnızca `OEHS.exe`, `OEHS_DENEME_PAKETI.flag` ve `KULLANIM.txt` içerir; kaynak kullanıcı verisi veya mevcut arşiv pakete kopyalanmaz.
- İşaret dosyası bulunan pakette ana veri, .bak yedeği, arşivler, imza ayarları ve PDF/çıktılar EXE yanındaki `DenemeVerisi` klasörüne yönlendirilir. İşaret dosyası olmayan normal çalıştırmada standart kullanıcı profili ve Masaüstü dizinleri korunur.
- Arşiv geri yüklemede aynı GUID veya aynı iş anahtarındaki kayıt mevcutsa ikinci hakediş satırı eklenmez. Aktif kayıt ile arşiv girdileri, brüt/net/kesinti tutarları ya da hesapta kullanılan sabitler farklıysa aktif ve arşiv net tutarlarını gösteren uyarı üretilir; aktif kayıt otomatik değiştirilmez.
- İzole yol seçimi ve normal yol geri uyumluluğu için testler; farklı GUID'li iş anahtarı çakışması ve aynı GUID'li ama farklı girdili arşiv için regresyon testleri eklendi. CI sonuçları son committe doğrulanacaktır.

### Güncel iş akışı

```mermaid
flowchart LR
    A["İhale, yüklenici, hat ve dönem girdileri"] --> B["Hesaplama sabitleri paneli"]
    B --> C["Taşıma + rehber ayrı hesap"]
    C --> D["Kontrol ve hesap sabiti snapshot'ı"]
    D --> E["Geçmiş / revizyonlu arşiv"]
    E --> F{"Aynı GUID veya iş anahtarı var mı?"}
    F -->|Yok| G["Arşive ekle"]
    F -->|Var, fark yok| H["Yeni kopya ekleme"]
    F -->|Var, tutar/girdi farkı| I["Net fark uyarısı; otomatik ezme yok"]
    D --> J["4 resmi A4 PDF + önizleme/yazdırma"]
    K{"Çalışma modu"} -->|Normal| L["%LOCALAPPDATA% + Masaüstü"]
    K -->|Deneme işaret dosyası| M["EXE yanındaki DenemeVerisi"]
    B --> N["Eski/yeni oran, kullanıcı, gerekçe ve onay kaydı"]
```

### Kalan kritik kabul işleri

- Kaynak Excel'in D11 etiketi/formül çelişkisi, AG/D28 farkı ve icmal Temmuz/Kasım/Aralık davranışı için iş sahibi/Excel tabanlı karar alınmalı; kaynak farkları varsayımla değiştirilmemeli.
- Dört gerçek PDF'nin kaynak örnekleriyle görsel fark karşılaştırması, fiziksel yazıcı ve temiz Windows profili kabul testleri tamamlanmadan uygulama üretime hazır sayılmamalıdır.


## Genel çekirdek modül tamamlama tablosu — 2026-10-18

**Yüzdeler, mevcut özellikler ve açık kabul işleri üzerinden yapılan mühendislik tahminidir; test/CI başarısı veya üretime hazır olma oranı değildir.** Alt modüllerin dar kapsamlı %100 değerleri aşağıdaki daha geniş modül yüzdelerine doğrudan eşitlenmemelidir.

| # | Çekirdek modül | Tahmini tamamlanma | Dayanak / açık kalan işler |
|---:|---|---:|---|
| 1 | WPF arayüzü ve kullanıcı akışları | **%80** | İhale/yüklenici/hat/hakediş ve arşiv akışları mevcut; ekranların iki program türünde tam izolasyonu, açılış/temizleme davranışlarının uçtan uca kullanıcı kabulü ve okunabilirlik kontrolü sürüyor. |
| 2 | Ortak veri modeli, kayıt ve arşiv güvenliği | **%85** | Revizyon koruması, çakışma uyarıları, yedek/geri yükleme ve deneme verisi izolasyonu tanımlı; temiz Windows profili ve gerçek kullanıcı verisiyle kabul testi gerekiyor. |
| 3 | Özel Eğitim taşıma ve rehber hesaplama | **%75** | Taşıma/rehber bileşenleri, ayrı KDV/tevkifat davranışları ve rapor bağlantıları mevcut; özgün XLSM/VBA ile uçtan uca örneklerin tam karşılaştırması ve sınır senaryoları sürüyor. |
| 4 | Temel/Ortaöğretim taşıma hesaplama | **%75** | Ayrı program türü davranışı ve belge etiketleri mevcut; ihale-firma-hat izolasyonu ile aylık/çoklu hat senaryolarının gerçek Excel örnekleriyle doğrulanması gerekiyor. |
| 5 | Hakediş hesaplama motoru ve Excel eşleştirmesi | **%80** | A–I tutar zinciri ve regresyon senaryoları tanımlı; XLSM'deki D11 etiket/formül çelişkisi, AG/D28 farkı ve icmal ay davranışları karar/kanıt gerektiriyor. |
| 6 | Dört resmî PDF, icmal ve önizleme | **%75** | Dört çıktı türü, A4 üretimi ve PDF smoke kontrolleri mevcut; son kesinti başlığı hizalaması, kaynak Excel ile birebir görsel karşılaştırma ve yazıcı/temiz profil kabulü açık. |
| 7 | Testler, CI, kalite ve dağıtım | **%70** | 18.10.2026 tarihli son dal commit'inde Windows CI başarılı: 180 Core + 4 WPF testi, tam çözüm/WPF derlemesi, PDF smoke, Windows x64 EXE publish ve izole deneme paketi adımları geçti. Gerçek senaryo kabulü ve kurulum paketi son kontrolü hâlâ gerekli. |

**Basit ortalama: %77,1.** Bu yalnızca yukarıdaki yedi tahminin eşit ağırlıklı ortalamasıdır; proje başarı olasılığı veya resmî kabul sonucu değildir. Hesaplama/PDF/Excel belirsizlikleri yüksek riskli olduğundan üretime hazır olma kararı bu ortalamadan çıkarılamaz.

### Yüzdeyi yükseltmek için zorunlu kanıt kapıları

1. Etkin dalın son commit'inde Windows/.NET 8 tam çözüm derlemesi ve tüm testler başarılı olmalı.
2. Her iki taşıma türünde aynı firma/hat numaralarının karışmadığı, birden fazla hat ve birden fazla aylık hakediş içeren uçtan uca senaryo çalıştırılmalı.
3. Aynı örnek veriden üretilen dört PDF, kaynak Excel çıktılarıyla tutar, dönem, yüklenici, hat, imza ve sayfa yerleşimi açısından karşılaştırılmalı.
4. Kaynak Excel çelişkileri belgelenmeli; karar gerektiren noktalarda varsayımla değişiklik yapılmamalı.
5. Gerçek Windows kullanıcı profili, arşivden düzeltme/geri yükleme ve izole deneme paketi kabulü tamamlanmalı.


## 18.10.2026 — Son Windows CI ve PDF smoke kabul kontrolü

- Son doğrulanmış commit: `2016dd7a1e190410e242aa93f54a7bb668abdf15`.
- Windows CI: [39118599426](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39118599426) başarılı.
- Son CI sonucu: **177/177 Core testi**, **4/4 WPF PDF testi**, tam çözüm/WPF derlemesi, dört resmî PDF smoke üretimi, Windows x64 EXE publish ve izole deneme paketi adımları başarılı.
- CI, `OEHS-PDF-Smoke-Review` artefaktını üretti. Artefakt içinde iki program türü için dört belge olmak üzere 8 PDF bulundu. Hepsi tek sayfa A4 portre (595.276 × 841.89 pt) ve PDF okuyucusuyla açılabilir.
- 90 DPI görsel önizlemede belirgin metin taşması veya boş sayfa görülmedi; kesinti/mahsup başlığı dikey durumda. Bu inceleme **kaynak Excel PDF'leriyle piksel karşılaştırması değildir** ve gerçek kullanıcı verisi kabulü yerine geçmez.
- Smoke testinde başarısız olan eski `TEST-IHALE` ve `TEST-SOZ` metin beklentileri kaldırıldı; resmî Hakediş Raporu şablonunda bulunmayan alanları PDF'ye ekleyerek şablonu değiştirmek yerine testler kaynak form alanlarına hizalandı.
- Dosya adı testlerinde derlemeyi durduran CLS attribute/array test verisi kullanımı, döngü içinde çalışan `Fact` testlerine dönüştürüldü. Son Windows CI bu değişikliklerle başarılı oldu.

### Bu oturumun devam notu

**Yapılan iş:** dosya adı testlerindeki derleme hatası giderildi; resmî PDF test beklentileri şablonla tutarlı hale getirildi; testlerdeki metin karşılaştırmaları açık hale getirildi; 8 smoke PDF görsel ve sayfa ölçüsü bakımından incelendi; genel çekirdek yüzdeleri ve kabul kapıları belgede tutuldu.

**Doğrulama:** Windows CI başarılı; 177 Core + 4 WPF testi geçti; PDF smoke üretimi, EXE publish ve izole deneme paketi adımları geçti.

**Açık riskler:** XLSM D11 etiketi/formül çelişkisi, AG/D28 farkı ve icmal ay davranışları için karar gereksinimi sürüyor. Kaynak Excel ile dört PDF'nin gerçek görsel fark karşılaştırması, fiziksel yazıcı ve temiz Windows profili kabulü tamamlanmadı. Analyzer uyarıları da var.

**Sıradaki adım:** XLSM'deki her açık hesap farkını gerçek hücre/makro örneğiyle izole et; karar gerektirenleri ayrı listede tut. Sonra iki program için çoklu hat/ay uçtan uca testini çalıştır ve dört PDF'yi kaynak örneklerle karşılaştır. Bu kanıtlar olmadan çekirdek modüller %90+ olarak işaretlenmemeli.


### Ek regresyon — program verisi çift dosya geri alma

- `OehsProgramScopedApplicationDataStore` için, ikinci program dosyasına commit başarısız olduğunda ilk dosyanın eski içeriğinin geri yüklendiğini sınayan regresyon testi eklendi.
- Commit: [`c514b9b`](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/commit/c514b9b6fcfd4a98d199562dc13fc31858b1f280).
- Son Windows CI [39118853975](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39118853975) başarılı: **178/178 Core**, **4/4 WPF PDF testi**; çözüm/WPF derlemesi, PDF smoke, Windows x64 publish ve deneme paketi yükleme adımları geçti.
- Bu sonuç program dosyalarının tüm olası disk arızalarında kusursuz olduğunu kanıtlamaz; kontrollü kısmi-commit hata senaryosunun geri alma davranışını doğrular.


## 18.10.2026 — Program türleri arasında hat aktarımı regresyonu

- Etkin dal: `feature/official-payment-document-templates`.
- Doğrulanmış HEAD: `0248e0ac59be055085dc6904fe9eab1dcb694308` (`test: isolate route propagation by transportation program`).
- Windows CI: [39120219227](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39120219227) başarılı; çalıştırılan HEAD commit ile birebir eşleşiyor.
- Son test sonuçları: **180/180 Core**, **4/4 WPF PDF testi** başarılı. Tam çözüm ve WPF derlemesi, statik UI kontrolleri, PDF smoke üretimi, Windows x64 EXE publish ve izole deneme paketi adımları başarılı.
- Yeni regresyon testi, aynı hat/ihale/yüklenici kimlik alanları kullanıldığında Özel Eğitim hat fiyatı değişikliğinin Temel/Ortaöğretim hakediş girdisi ve hesap sonucuna taşınmadığını denetliyor. Bu commit test kapsamı ekler; hesaplama formüllerini değiştirmez.
- CI'da CA1822, CA1861 ve CA1014 analiz uyarıları ile GitHub Actions Node.js deprecation uyarıları sürüyor. Bu uyarılar mevcut çalıştırmayı başarısız yapmadı.

### Devam notu

**Yapılan iş:** son dal/commit ve CI doğrulandı; program türleri arasında hat aktarımı için yeni regresyon testinin başarılı çalıştığı teyit edildi; yol haritasındaki güncel CI durumu düzeltildi.

**Doğrulama:** Windows CI başarılı; 180 Core + 4 WPF testi geçti. Bu, yerel kullanıcı arayüzünde gerçek veriyle kabul testi veya dört PDF'nin orijinal Excel çıktılarıyla birebir eşleştiği anlamına gelmez.

**Açık riskler:** D11 etiket/formül çelişkisi, AG/D28 farkı ve icmal ay davranışları karara/kanıta bağlı. Dört PDF'nin kaynak Excel örnekleriyle görsel karşılaştırması, fiziksel yazıcı ve temiz Windows profili kabulü açık. Analiz uyarıları temizlenmedi.

**Sıradaki adım:** arşivden geri yükleme ve program bazlı kayıt ayrımı testlerini inceleyip gerekiyorsa hedefli regresyon ekle; ardından iki programda çoklu hat/ay senaryosunu ve dört PDF'nin birlikte karşılaştırmasını tamamla. PR taslak ve birleştirilmemiş kalmalı; ana dala geçiş yapılmamalı.
