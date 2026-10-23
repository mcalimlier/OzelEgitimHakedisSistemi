# OEHS Çekirdek Mimarisi — Değerlendirme ve Aşamalı Plan

**Tarih:** 17.10.2026  
**Durum:** Mimari değerlendirme; henüz kod bölme/refaktör işlemi yapılmadı.  
**Amaç:** Hata bulma, Excel eşitliği, program türlerinin izolasyonu ve dört PDF'nin doğrulanmasını hızlandıracak düşük riskli modül sınırlarını belirlemek.

## 1. Mevcut durum

Şu an çözümün ana yapısı iki projeden oluşuyor:

- `OzelEgitimHakedis.Core` — .NET 8 sınıf kütüphanesi; modeller ve iş servisleri.
- `OzelEgitimHakedis.Wpf` — WPF arayüzü ve belge/PDF üretim bileşenleri; Core'a proje referansı veriyor.

Core içinde `ProgressPaymentCalculator`, `ProgressPaymentReportService`, `ProgressPaymentIcmalService`, arşiv, kimlik, dönem, doğrulama ve veri saklama servisleri ayrı dosyalara bölünmüş durumda. PDF üretiminde `OehsOfficialDocumentFactory` ve `PdfVectorWriter` WPF projesinde. Yani modülerliğin bir kısmı zaten mevcut; öneri sıfırdan mimari kurmak değil, mevcut ayrımı daha açık sözleşmelerle güçlendirmek.

## 2. Önerilen mantıksal çekirdekler

### A. Ortak hesaplama altyapısı
Sorumluluk: decimal hassasiyeti, ortak modeller, ortak oran/sabit tanımları, ortak doğrulama yardımcıları ve hesaplama sonuçlarının veri yapıları.

Kural: Bu katman program türüne özgü iş kuralı kararlarını kendi içinde gizlememeli. Ortaklaştırılan her davranış iki Excel kaynağında da kanıtlanmalı.

### B. Özel Eğitim hesaplama çekirdeği
Sorumluluk: taşıma + rehber personel zinciri; K/T ve L/M/N/O ile U/V/W/X; Özel Eğitim'e özgü rapor/kesinti davranışları.

Kaynak referansı: Özel Eğitim XLSM'sindeki `Hesaplama` sayfası ve ilgili VBA aktarım zinciri.

### C. Temel/Ortaöğretim hesaplama çekirdeği
Sorumluluk: taşıma kalemi ve bu programın kendi KDV, tevkifat, fiyat farkı, ek kesin teminat ve rapor kuralları.

Kaynak referansı: Temel/Ortaöğretim XLSM'sindeki `Hesaplama Bilgileri` sayfası ve ilgili makrolar.

Kritik sınır: Temel/Ortaöğretim hesaplamasında rehber personel kalemi bulunmadığı için Özel Eğitim girdilerinin yanlışlıkla bu programa sızması testlerle engellenmeli. Aynı firma/hat numarası, farklı program türlerinde aynı kayıt kimliği sayılmamalı.

### D. Hakediş raporlama ve icmal çekirdeği
Sorumluluk: dönem sıralaması, önceki hakedişlerin doğru filtrelenmesi, kümülatif toplamlar, önceki ay kesintisi ve rapor hücrelerinin Excel eşleştirmeleri.

Bu katman hesaplama motorundan ayrı test edilebilir; çünkü hesaplama sonucu doğru olsa bile geçmiş hakedişlerin seçimi veya rapor hücrelerine aktarım yanlış olabilir.

### E. Resmî belge ve PDF çekirdeği
Sorumluluk: dört belge modelini oluşturma, A4 sayfa koordinatları, tablo/kenarlık/metin çizimi, Türkçe karakterler, PDF'ye vektörel aktarım ve dosya çıktısı.

Dört belge tek üretim sözleşmesi altında kalmalı; ancak her birinin ayrı şablon oluşturucusu veya açık alt bileşeni olmalı. Bu katman hesaplama yapmamalı, kendisine verilen doğrulanmış rapor verisini belgeye dönüştürmeli.

### F. Veri, arşiv ve program izolasyonu çekirdeği
Sorumluluk: kayıt kimlikleri, program kapsamı, arşivleme/geri yükleme, yedekleme, dosya yolları ve veri geçişleri.

Bu katmanın kabul testleri aynı firma/hat numarası için iki program arasında veri karışmamasını; düzenleme ve geri yüklemenin yanlış hakedişi etkilememesini kanıtlamalı.

### G. WPF arayüz katmanı
Sorumluluk: kullanıcı girişleri, ekran durumu, onay istemleri, temizleme/düzenleme akışı, önizleme ve kullanıcıya hata bildirimi.

Arayüzün içinde resmi hesaplama kuralları veya PDF koordinatları birikmemeli; UI mümkün olduğunca Core'daki servisleri çağırmalı.

## 3. Ayrı proje mi, ayrı dosya/namespace mi?

İlk aşamada her çekirdeği ayrı EXE veya ayrı çalışan servis yapmak önermiyorum. Bunun yerine:

1. Önce mevcut iki proje yapısını koru.
2. Core içinde sorumlulukları namespace ve servis sözleşmeleriyle ayır.
3. Program türlerine özgü kuralların testlerle ayrı doğrulanmasını sağla.
4. PDF üreticisini, hesaplama sonuçlarını alan bir belge modeli üzerinden çalıştır.
5. Bağımlılık ve test ihtiyacı gerçek bir sınır oluşturduğunda çekirdeği ayrı `.csproj` sınıf kütüphanesine taşı.

Bu sıra, bir seferde çok sayıda dosya ve proje değiştirme riskini azaltır. Ayrı proje oluşturmak tek başına kalite sağlamaz; yanlış sınırlar varsa yalnızca proje referanslarını ve hata ayıklama adımlarını artırır.

## 4. Önerilen test kapıları

Her çekirdek için değişiklikler küçük ve bağımsız olmalı:

- **Excel golden-master:** Her iki XLSM için ayrı örnek satırlar, ara hücreler ve nihai değerler; muaf/muaf değil, iskonto, rehber, pozitif/negatif fiyat farkı ve kuruş yuvarlaması.
- **Program izolasyonu:** Aynı firma/hat numarası, farklı program; farklı ihale/yüklenici/sözleşme; arşiv/düzenleme/geri yükleme.
- **Rapor eşleştirmesi:** D6, D7, D8, D10, D11, D13, D26, D28 ve İcmal hücreleri.
- **PDF yapısal testleri:** Her program için dört PDF, tek sayfa A4, metin alanları, dönem/tutar/yüklenici/hat/imza kontrolü.
- **PDF görsel regresyonu:** Dört çıktıyı kaynak PDF'lerle aynı DPI'da render edip sayfa, kenarlık, imza ve metin yerleşimi farklarını kontrol etme.
- **Arşiv ve dayanıklılık:** bozuk JSON, erişim reddi, kilitli dosya, yarım yazım, yedek geri yükleme ve yinelenen hakediş senaryoları.

CI'da test başarısı ile görsel kabulü ayrı raporlanmalı. PDF'nin oluşturulması ve A4 olması, kaynak şablona görsel olarak eşit olduğu anlamına gelmez.

## 5. Geçiş sırası

1. Mevcut dal/commit/CI'ı doğrula.
2. Önce hesaplama ve rapor hücre testlerini güçlendir; mevcut hesaplama davranışını değiştirme.
3. Özel Eğitim ve Temel/Ortaöğretim davranışlarını açık ayrı servisler/stratejiler olarak tanımla; yalnızca iki Excel'de ortaklığı kanıtlanan kuralları paylaş.
4. PDF veri modelini ve dört belge üreticisini hesaplama katmanından ayır.
5. Arşiv/program kapsamı testlerini tamamla.
6. Her küçük değişiklikten sonra CI; dört PDF için görsel karşılaştırma ve test artefaktı incelemesi.
7. Bütün aşamalarda ana dala birleştirme/yayın için ayrıca açık onay bekle.

## 6. Bilinçli olarak bu aşamada yapılmayacaklar

- Excel'de görülen iş kuralı anomalilerini kod içinde sessizce düzeltmek.
- Özel Eğitim ve Temel/Ortaöğretim hesaplamalarını tek, koşullarla dolu metoda zorlamak.
- PDF şablonlarını yeni mimari gerekçesiyle topluca yeniden yazmak.
- Sadece dosya sayısını artırmak için her servis için ayrı proje oluşturmak.
- Test veya görsel karşılaştırma sonucu olmadan “birebir” ya da “başarılı” kabulü vermek.

## Sonuç

OEHS için modüler çekirdek yaklaşımı önerilir. En büyük kazanç; **hesaplama hatası, geçmiş/raporlama hatası, veri izolasyonu hatası ve PDF yerleşim hatasını ayrı test ve sorumluluk alanlarına ayırmak** olacaktır. İlk hedef daha fazla proje üretmek değil, mevcut Core/WPF sınırını koruyarak program türlerini ve dört belge üretimini test edilebilir, açık sözleşmeli bileşenlere dönüştürmektir.


## 7. Kod ve test incelemesinden ek bulgular (17.10.2026)

### CI durumu
- İncelenen dal: `feature/official-payment-document-templates`
- Commit: `9b832e411bcd85b25fe1b0baa10cf5ca89f6abe2`
- Windows Build run: [39054744710](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39054744710)
- Sonuç: **success**. Tam çözüm/WPF/test projeleri derleme, Core testleri, PDF smoke üretimi, Windows EXE ve deneme paketi artefakt adımları tamamlandı.
- Bu CI sonucu, PDF'lerin kaynak Excel/PDF şablonlarıyla piksel bazında eşit olduğunu tek başına kanıtlamaz.

### PDF testlerinin mevcut kapsaması
`OfficialDocumentOutputTests.cs` şu an dört belgeyi her iki program türü için üretip tek sayfa A4, metin, bazı hücre tutarları, imza alanları ve belirli koordinatları denetliyor. Bu değerli bir yapısal regresyon tabanı. Ancak test, kaynak Excel/PDF sayfalarıyla raster karşılaştırma yapmıyor; metin taşması ve çizgi konumu gibi bazı görsel farklar otomatik olarak kaçabilir.

**Güvenli sonraki adım:** mevcut smoke PDF artefaktlarını kaynak kabul etmeyip, önce gerçek kaynak şablon sayfalarının erişilebilirliğini ve lisans/format uygunluğunu doğrulamak; ardından Windows CI üzerinde PDF'leri sabit DPI ile görsele dönüştürüp karşılaştırma raporu üretmek. Onaylı referans görsel olmadan otomatik “pixel-perfect” eşiği koyulmamalı.

### Belge etiketi için açık doğrulama noktası
Kabul Tutanağı testinde `Sözleşmeye göre işin süresi (takvim günü)` etiketiyle `180 İş Günü` değeri birlikte bekleniyor. Bu, etiket ile gösterilen değerin birbiriyle tutarlı olup olmadığının kaynak XLSM şablonunda ve kullanıcının onayladığı resmî örnekte doğrulanması gereken bir farktır. Bu turda etiketi değiştirmedik; testin mevcut beklentisi de sessizce yeniden yazılmadı.

### Sıradaki uygulama kapıları
1. Rapor servisinin D6/D7/D8/D10/D11/D13/D26/D28 hesapları için mevcut testlerin hangi hücre zincirini kanıtladığını haritala; yalnızca gerçek kaynak giriş zinciri tekrar kurulabilen örnekleri golden-master kabul et.
2. Program türüne göre hesaplama stratejisi eklemeden önce mevcut `ProgressPaymentCalculator` dallarını ve çağrı noktalarını listele; mevcut sonuçları karakterizasyon testleriyle sabitle.
3. Dört belge için veri aktarım sözleşmesini tanımla; PDF üreticisinin iş hesabı yapmadığını testlerle kanıtla.
4. Görsel karşılaştırmayı, gerçek kaynak sayfalar ve sabit render ortamı hazır olduğunda ekle.
5. Her aşamayı ayrı commit ve CI ile doğrula; ana dala birleştirme veya yayın yapma.

Bu ek bulgular yalnızca mimari ve test planını netleştirir; resmi ödeme hesaplama kurallarını değiştirmez.
