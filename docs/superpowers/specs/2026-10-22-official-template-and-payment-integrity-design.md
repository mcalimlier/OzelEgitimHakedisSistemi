# Resmî Şablon Uyumu ve Hakediş Bütünlüğü Tasarımı

## Amaç

Hakediş hesaplarının kuruş hassasiyetini koruyan, resmî Temel/Özel Eğitim Excel şablon terminolojisini doğru kullanan, PDF çıktısında toplamları eksiksiz gösteren ve kullanıcıya dört bölümlü güvenli bir hakediş çalışma alanı sunan sistem oluşturmak.

## Başarı Ölçütleri

- Düzenleme/seçme akışı günlük fiyatın hesap hassasiyetini azaltmaz.
- Hizmet bedeli sıfır olsa da pozitif fiyat farkı ilgili ayın icmal/PDF toplamına dahil edilir.
- PDF bağlamı İKN ile sözleşme numarasını görünür resmî alanlarda taşır; uzun metin sessizce kaybolmaz.
- Program türüne göre resmî şablon adları ve alan etiketleri kullanılır.
- `İşin Adı`, serbest zorunlu ihale girdisi değildir; `Hattın Adı + " Hattı Öğrenci Taşıma İşi"` kuralıyla üretilir.
- Boş sözleşme başlangıç/bitiş tarihi kalıcı kayda dönüştürülemez.
- İcmal ve Kabul Tutanağı ekranlarına erişilebilir; 1024×600 ile 1920×1080 arasında kontrol erişilebilirliği korunur.

## Hesaplama ve Belge Bütünlüğü

### Para hassasiyeti

`decimal` değerleri hesap ve kalıcı kayıtta tam hassasiyetle kalır. Metin kutuları kullanıcıya yerel biçimde gösterilir fakat seçim, düzenleme veya yeniden hesaplama öncesinde iki ondalık haneye zorlanmaz. Ekran gösterimi, hesap girişinden ayrıdır.

### İcmal satırı

Resmî belge fabrikası bir ayı icmale, `service > 0` yerine raporlanabilir parasal bileşen varlığına göre ekler. Taşıma/referans hizmet, rehber hizmet, fiyat farkı veya düzeltme toplamından en az biri sıfır olmayan her geçerli ay satırı görünür. Kesinti toplamı tek başına yeni ay yaratmaz.

### PDF doğrulaması

Belge bağlamı İKN ve sözleşme numarasını ilgili resmî satırda gösterir. PDF vektör yazıcısı sığmayan metni ya güvenli biçimde satıra böler ya da alanın taşma durumunu açık hataya dönüştürür; kalan metni sessizce atmaz. Testler yalnız `TextBlock` içeriğini değil, üretilen PDF metnini ve sayfa düzenini doğrular.

## Resmî Şablon Profilleri

İki sabit şablon profili bulunur:

| Kavram | Temel Eğitim | Özel Eğitim |
| --- | --- | --- |
| Hesap sayfası | `Hesaplama Bilgileri` | `Hesaplama` |
| Hizmet raporu | `Hizmet İşleri Hakediş Raporu` | `Hizmetişleri Hakediş Raporu` |
| Hat alanı | `Hattın Adı` | `Hattın Adı` |
| Taşıma fiyat farkı | `Taşıma Fiyat Farkı KDV Hariç` | `Taşıma Fiyat Farkı KDV Hariç` |
| Rehber fiyat farkı | yok | `Rehber Personel Fiyat Farkı KDV Hariç` |
| Net tutar | `Ele Geçen` | `Ele Geçen` |

Ekran etiketleri program türüne göre bu adları kullanır. İKN, iletişim bilgileri ve benzeri uygulamaya özgü kayıt alanları resmî Excel sütunu gibi sunulmaz; gerektiğinde “Ek uygulama bilgisi” olarak ayrılır.

## Hakediş Paneli

Panel, genişlikten bağımsız olarak aşağıdaki dört görsel bölümü sıralı tutar:

1. **Bağlam şeridi:** yeni/arşiv düzeltme durumu, arşivde amber-kilitli görünüm.
2. **İhale ve hat seçimi:** ihale, yüklenici, dönem ve hat listesi.
3. **Hakediş bileşenleri ve sonuç:** çalışma günleri, günlük fiyatlar, fiyat farkı, kesintiler ve net ödeme özeti.
4. **İşlemler:** Hesaplama Yap, Arşivle, Düzelt ve Temizle; her zaman kartların altında.

1024 px genişlik ve 600 px yükseklikte sol menü kaydırılabilir, geçmiş tablosu yatay kaydırılabilir, form işlem düğmeleri sarılabilir olmalıdır. 1366×768 ve 1920×1080’de gereksiz taşma veya örtüşme olmayacaktır.

## Erişilebilirlik ve Doğrulama

- `İcmal` ve `Kabul Tutanağı` panellerinin ayrı gezinme hedefleri bulunur.
- İstatistikler yer tutucu ise kullanıcıya açıkça belirtilir; çalışma işlevi değilmiş gibi menüden kaldırılmaz.
- Başlangıç/bitiş tarihleri zorunludur ve başlangıç bitişten sonra olamaz.
- Her XAML gezinme hedefi bir görünür panele eşlenir.

## Test Stratejisi

- Kuruş hassasiyeti: `19539.1608` günlük fiyatla düzenleme ve PDF neti değişmez.
- Sıfır hizmet/pozitif fiyat farkı: icmal satırı ve toplamı görünür.
- Uzun unvan ve güzergâh: PDF metni tam veya görünür hata üretir.
- İKN/sözleşme no: PDF metin çıkarımında bulunur.
- Her program profili: sayfa/başlık eşleme tablosu kaynak şablonlarla eşleşir.
- Gezinme, tarih zorunluluğu ve dört hedef çözünürlük: WPF sözleşme/uygulama testleriyle doğrulanır.

## Kapsam Dışı

- Kullanıcının sağladığı XLSM dosyalarını değiştirmek veya makroları çalıştırmak.
- Mevzuat oranlarını dış kaynaklardan yeniden yorumlamak.
- Kullanılmayan eski `ExcelDocumentExporter` sınıfını resmî çıktı yolu yapmak.
