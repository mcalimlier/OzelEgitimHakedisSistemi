# Rehber yıllık günün kaldırılması ve onaylı tablo kaydı

Tarih: 23.10.2026
Depo: `mcalimlier/OzelEgitimHakedisSistemi`
Dal: `feature/official-payment-document-templates`; PR #3 açık/taslak.
Başlangıç: `fc8ac2e192653776e70aa1f394daca966a664852`, Windows CI `39930372023` başarılı.

## Uygulanan davranış

1. **Rehber Yıllık İş Günü kaldırıldı.** `Route`, `RouteData` ve WPF `RouteDraft` kayıtlarından yıllık gün özelliği; Hatlar panelindeki kontrol/etiket; okuma, yazma, doğrulama ve özet bağlantıları kaldırıldı. Eski JSON'daki yıllık alan okunurken yok sayılır, yeni kaynak kaydına yazılmaz. Değişmez eski arşiv dosyaları topluca yeniden yazılmadı. Hakedişin dönemsel `ProgressPaymentInput.GuideWorkDays` alanı korunur. Hat/arşiv düzeltmeleri artık yıllık değeri dönem gününün üzerine yazamaz.
2. **Kayıtlı hat → seçili hat → düzenlenebilir tablo → hesap girdisi/kayıt zinciri korunur.** Taşıma günü, taşıma günlük fiyatı ve rehber günlük fiyatı yeni hat satırına kaynaktan alınır. Kaydedilmiş aylık tablo taslağı varsa doğru hat/program/firma/sözleşme/ay eşleşmesiyle geri yüklenir. Aynı satırı yeniden seçmek kullanıcı fiyatını ezmez. İlk ayın hat seçildikten sonra seçilmesi de kayıtlı aylık taslağı yükler.
3. **Tablonun hemen altında `TABLO DEĞİŞİKLİKLERİNİ KAYDET` butonu vardır.** Başlangıçta pasiftir. Seçili bir satırda gerçek fark olduğunda etkinleşir. Kullanıcı tüm farkları geri alırsa tekrar pasifleşir. Tıklamada Evet/Hayır onayı çıkar. Hayır: kaynaklar ve disk değişmez, hücreler korunur ve buton etkin kalır. Evet: değişen seçili hatların kaynak bilgileri ve aylık hücre girdileri atomik program veri kaydıyla yazılır; listeler, seçiciler, açık hat formu ve özetler yenilenir. Başarılı kayıt yeni temel değer olur ve buton pasifleşir.
4. **Hata davranışı:** geçersiz hücre/gün/fiyat/iskonto, boş hat adı, sözleşme dönemi veya sahiplik eşleşmesi hatasında kayıt yapılmaz. Disk/izin hatasında bellek kaynakları geri alınır; taslak ve yeniden deneme olanağı korunur. Aynı hat numarasının farklı program/firma/ay verileri ayrı saklanır.

## Alan eşlemesi

| Kaynak veya anlam | Tablo başlığı | Düzenleme ve kayıt |
| --- | --- | --- |
| `RouteDraft.Name` | Hattın Adı | Düzenlenir; onayla kaynak hat ve ilgili görünümler güncellenir |
| `RouteDraft.TransportWorkDays` — Döneme Esas THGS | Taşıma Gün | Düzenlenir; onayla kaynak hat ve ay taslağına yazılır |
| `RouteDraft.DailyPrice` | Taşıma Günlük Fiyat | Düzenlenir; hat bazlı fiyat hassasiyeti korunur |
| **Hatlar / Rehber Personel Günlük Fiyat** — `GuideDailyPrice` | **Rehber Günlük Fiyat** | **Doğrudan aynı veri; artık düzenlenebilir ve onayla kaynak hatta yazılır** |
| Dönemsel rehber hizmet günü — `ProgressPaymentInput.GuideWorkDays` | Rehber Gün | Aylık girdidir; yıllık hat alanı değildir |
| Fiyat farkı, iskonto, borç, destek, ceza | İlgili parasal sütunlar | Aylık taslakta saklanır; hakediş oluşturma hesabında kullanılır |
| Hat numarası / program / İKN / sözleşme / firma | Hat ve üst seçiciler | Kimlik alanlarıdır; sayısal düzenleme tablosundan değiştirilmez |

İki günlük fiyat da iki yönlü hücre bağlamasıyla düzenlenir. Excel'den sayısal fiyat yapıştırma ve geri alma da desteklenir; negatif fiyatlar reddedilir. Hesaplanan net salt okunur türetilmiş sonuçtur.

## Kaynak tutarlılığı ve kapsam kararları

- Bir hattın günlük fiyatını kaydetmek başka hatların veya sözleşmenin fiyatını topluca değiştirmez. İlgisiz sözleşme değişikliği hat bazlı fiyatı ezmez. Sözleşmenin günlük fiyatı açıkça değiştirilirse mevcut sözleşme yayılım davranışı kullanılır.
- Kaynak hat başka panelden değişirse, temiz kaynak hücreleri güncellenir; kullanıcının gerçekten değiştirdiği hücreler korunur. Kaydedilmiş aylık taslaklar kaynak anlık görüntüsüyle uzlaştırılır; eski fiyatın tekrar kaynağa yazılması önlenir. Bağımsız aylık rehber günü/kesintiler korunur.
- Hat/firma/sözleşme kimliği kaynak panelde değişirse aylık taslak anahtarları da taşınır. Kaynak kayıt başarısızsa anahtarlar ve taslaklar geri alınır.
- Bu buton **kaynak hat kayıtlarını ve aylık taslakları** günceller. Daha önce oluşturulmuş resmî hakedişleri veya değişmez arşivleri topluca yeniden hesaplamaz. Resmî ödeme kaydı mevcut hakediş oluşturma ve hedefli düzeltme akışındadır. Onay kutusunda bu kapsam açıkça yazılıdır.
- Ay değişimi önceki ayın kaydedilmemiş hücrelerini yeni aya taşımaz; varsa yeni aya ait doğrulanmış taslak yüklenir.
- Otonom yürütme tercihi nedeniyle ek geliştirme onayı sorulmadı; istenen ürün içi onay zorunluluğu uygulandı.

## Test kanıtı ve durum

- `fb789cb`: kaldırma/gün koruma testleri eklendi. CI `39931903346`: eski yıllık alanın yeniden yazılması ve 18 günlük ödemenin 180 güne dönmesi testlerle gösterildi.
- `b14135b` / `90728ac`: yıllık alan kaldırıldı; kalan eski konumsal test kurucuları düzeltildi. CI `39932717853`: Core **264/264** başarılı; yalnız iki beklenen WPF yeni-özellik testi başarısız (buton henüz yok; 1234,5678 düzenlemesi yeniden seçimde 1000'e dönüyor).
- `6391f22`: onaylı kaynak/aylık taslak kaydı uygulandı; nullable akış derleme hatası `fb40782` içinde düzeltildi.
- Bağımsız inceleme dört önemli risk bildirdi: ilk ay yükleme, ilgisiz sözleşme düzenlemesinde fiyat ezilmesi, eski kaynak alanlarının geri gelmesi, bazı disk hatalarında geri alma. Ek testler yazıldı.
- CI `39933974155`: Core **264/264**; WPF **153 başarılı / 7 başarısız**. Onay/iptal ve temel kaynak yazma testi geçti; ek risk testleri başarısızlıkları gösterdi. Başarısızlıklardan biri eski salt-okunur fiyat beklentisiydi, biri test düzeneğinin ItemsSource kurulumu düzeltmesini gerektiriyordu.
- `209a828`: kaynak uzlaştırma, ilk ay yükleme, atomik hata geri alma, ilgisiz sözleşme düzenlemesinde hat fiyatını koruma ve düzenlenebilir fiyat yapıştırma düzeltmeleri.

- `209a828` için [CI 39934707882](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39934707882) **başarılı**: Core 264/264, WPF 162/162. İncelemenin dört önemli bulgusu ve negatif iskonto doğrulaması kapandı.
- `33ad8e5` için [CI 39935159074](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39935159074) **başarılı**: Core **265/265**, WPF **163/163**, toplam **428/428**. Program/firma/ay ayrımı ve kimlik değişiminde taslak taşıma ek regresyonları da geçti. PDF smoke **24 A4 PDF** üretim/doğrulaması başarılı.
- Mevcut `xml:lang="tr-TR"` sayısal hücre dilini doğrulamak için dört gerçek WPF Binding testi eklendi (`18,5` gün; `1.234,5678` günlük fiyat). Test editörü gerçek arayüzdeki gibi XAML dilini üst öğeden devralır. Bu kontrol için üretim hesaplama veya kültür kodu değiştirilmedi.

**Son doğrulama:** `5567b5380f0e179f52d3fe8edd3b63539afa8e31` için [Windows CI 39935756432](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39935756432) **success**.

- **Core 265/265 + WPF 167/167 = 432/432**; başarısız/atlanan test 0.
- Gerçek WPF hücre bağlamalarıyla Türkçe gün ve fiyat girişinin dört testi geçti.
- Tam çözüm/WPF derlemesi, UI sözleşmesi ve yerleşim kontrolleri başarılı.
- Onay/iptal, hat bazlı kayıt, disk hatasında geri alma, kaynak panel değişiklikleriyle uzlaştırma, ilk ay yükleme, kimlik anahtarı taşıma, program/firma/ay ayrımı ve yeniden açılış regresyonları geçti.
- PDF smoke adımındaki WPF test tekrarı da **167/167**; **24 A4 PDF** üretim/doğrulaması başarılı.
- Windows EXE derleme/publish adımı başarılı; üretim release/yayın yapılmadı.
- Yerel XAML XML ayrıştırması ve `git diff --check` başarılı.
- Bağımsız incelemenin dört önemli bulgusu testlerle giderildi; ertelenmiş küçük inceleme bulgusu yok.
- Bu kanıt yukarıdaki kod/test commit'ine aittir. Ardından yalnız bu devam notu/plan sonuçları güncellendi ve geçici yürütme kaydı temizlendi.

**Tamamlanan görevler:** yıllık rehber gün kaldırılması; düzenlenebilir hat kaynak aktarımı; onaylı atomik kaynak/aylık taslak kaydı; fiyat başlığı eşlemesi; bağımsız inceleme düzeltmeleri ve tam CI doğrulaması.

## Açık kabul sınırı ve sonraki adım

Gerçek kullanıcı Windows masaüstünde onay kutusunun etkileşimli kullanımı, fiziksel DPI/monitör değişimi ve Microsoft Excel/VBA ile dört PDF'nin alan alan görsel karşılaştırması bu CI kanıtından ayrı kabul kapılarıdır. Kaynak XLSM/VBA ve ödeme formülleri değiştirilmedi. Ana dala birleştirme/release/üretim yayını yapılmadı.
