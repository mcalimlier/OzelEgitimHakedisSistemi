# OEHS Yol Haritası

## Ana dönüşüm sırası

1. Excel tersine mühendislik — **TAMAMLANDI**
2. İş kurallarının çıkarılması — **TAMAMLANDI / doğrulama sürüyor**
3. VBA/makro kontrolü — **TAMAMLANDI** (aktif VBA projesi tespit edilmedi)
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

### Excel v2 ikinci tarama — tamamlandı / uygulanıyor
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
