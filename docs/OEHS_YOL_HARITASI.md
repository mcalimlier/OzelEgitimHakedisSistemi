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
