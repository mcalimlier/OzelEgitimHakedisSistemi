# OEHS Devam Notu — 21.10.2026

## Dal ve doğrulama
- Dal: `feature/official-payment-document-templates`
- Başlangıç referansı: `ae0d2a3c4c220ee1f2c0c0b9c0ec993aa8f8f5e1`
- Bu not yazılmadan önce doğrulanan HEAD: `43c2d9db77301c602e95ed69262cf1f198b377e4`
- CI run [39610415905](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39610415905): PASS; çözüm/WPF/test derlemeleri, test çözümü, UI statik sözleşme kontrolleri, kompakt yerleşim kontrolü ve PDF smoke adımları geçti.
- CI run [39610407205](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39610407205): PASS; aynı kontroller geçti, izole test paketi ve Windows EXE artefakt yüklemeleri başarılı.
- İki başarılı run da `43c2d9db77301c602e95ed69262cf1f198b377e4` SHA'sını test ediyor.
- Ana dala birleştirme veya yayınlama yapılmadı.

## Yapılan müdahale
- `IcmalDuplicatePriceDifferenceRegressionTests.cs` içindeki Özel Eğitim regresyon testinin mükerrer tutar kontrolü düzeltildi: 88.697,27 TL hatalı toplam ayrı değişkende açıkça doğrulanıyor ve doğru aylık toplamın bu hatalı toplam olmadığı ayrıca sınanıyor.
- Bu test düzeltmesi üretim hesaplama kodunu değiştirmedi.
- Mevcut çıktı testlerinde Temel Eğitim ve Özel Eğitim icmal satır toplamlarının fiyat farkını yalnızca bir kez saydığına yönelik kontroller bulundu; CI bu testleri geçti.

## Kanıt sınırları ve açık riskler
- CI'daki .NET derleme/test ve PDF smoke PASS sonuçları, gerçek masaüstü Excel/VBA derleme veya makro çalışma zamanı kabulü değildir.
- Temel ve Özel Eğitim kaynak XLSM dosyaları bu çalışma bağlamında doğrudan erişilebilir ek olarak mevcut değildi; kaynak formül/hücre kanıtı önceki inceleme belgesindeki kayıtlı hücre değerlerine dayanır.
- Sekiz nihai PDF'nin kaynak Excel çıktılarıyla alan-alan ve görsel karşılaştırması tamamlanmadı.
- %100, %125 ve %150 gerçek Windows DPI/UserForm testleri tamamlanmadı.
- İcmal regresyon vektörleri aritmetik ve çıktı testi kapsamındadır; kaynak XLSM'yi masaüstü Excel'de yeniden hesaplayıp doğrulama yapılmadı.
- Kaynak Excel'deki Temel Eğitim D8 hücresinin fiyat farkı ve kesinti bileşenlerini aynı sütunda birleştirmesi başlık/anlam uyuşmazlığı doğurur. Uygulama tarafında kesinti bileşeni korunmuş olsa da bu raporlama semantiği resmî kabul belgesiyle teyit edilmelidir.

## Sonraki başlangıç noktası
1. Başlangıç SHA'sı `ae0d2a3c4c220ee1f2c0c0b9c0ec993aa8f8f5e1` ile güncel HEAD arasındaki değişiklik zincirini ve PR durumunu tekrar doğrula.
2. Kaynak XLSM kopyalarını çalışma ortamına alabilir hâle gelince dosya hash'lerini doğrula; orijinalleri değiştirmeden Excel formül ağacını, VBA modüllerini ve kaynak PDF'leri çıkar.
3. İcmal için hem Temel hem Özel Eğitim'de kaynak hücre -> model -> PDF hücresi alan bazlı testlerini genişlet.
4. Gerçek Windows Excel/VBA compile/runtime, UserForm DPI ve sekiz PDF kabul matrisleri tamamlanana kadar ilgili çekirdekleri PASS/100% kabul etme.


## Devam — kaynak VBA icmal mükerrerliği (21.10.2026)

- Önceki HEAD `f533de53345a2294f877b0c085a73584c850587a` için CI run [39611192750](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39611192750) ve [39611185377](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39611185377) PASS.
- Kaydedilmiş dışa aktarılmış VBA metinlerinde kaynak hata doğrulandı: Özel Eğitim `HakedişToplamlariniAktar` K+T'yi hizmet alanına yazarken I/R ayrı bloklara aktarılıyor; Temel Eğitim `HakedisTutarlariniAySutununaAktar` K'yı hizmet alanına yazarken I ayrı blokta gösteriliyor.
- Düzeltme adımları ve kaynak vektörleri `docs/VBA_ICMAL_MUKERRER_FIYAT_FARKI_DUZELTMESI_2026-10-21.md` içinde kaydedildi. Temel hizmet tabanı K-I; Özel hizmet tabanı K+T-I-R olmalı, I/R ayrı fiyat farkı bloklarında korunmalı.
- VBA düzeltmeleri yalnızca aday yama olarak belgelendi; orijinal XLSM'lere aktarılmadı. Excel VBA IDE derlemesi/çalışma zamanı kabulü bu ortamda yapılmış gibi gösterilmedi.
- Yeni belge commit'i: `35758f54b3137ad336b2c15cebb77b610286630f`. CI run [39651142308](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39651142308) son kontrolde çalışıyordu; sonucu yeniden doğrula.
- PR #3 [açık/taslak](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/pull/3), merge edilmedi. Dal HEAD'i doğrulanmalı; merge/release yok.


## UI referans eşleştirmesi — Tasarım 1 (21.10.2026)

- Kullanıcı tarafından sağlanan Tasarım 1 referansına göre `src/OzelEgitimHakedis.Wpf/MainWindow.xaml` içinde hakediş ekranının dört aşamalı adım şeridi lacivert-altın prestij kartlarına dönüştürüldü.
- İKN/sözleşme, yüklenici firma ve seçili hatlar alanları üç ayrı koyu lacivert, altın çerçeveli karta ayrıldı. Mevcut `x:Name`, event handler ve seçim kontrolleri korundu.
- Çoklu hat Excel-hibrit DataGrid'inde mevcut `ModernDataGridHeader` kullanılarak lacivert başlık görünümü etkinleştirildi. Net ödeme/hesap özeti altın vurgulu karta dönüştürüldü.
- `VatExemptCheckBox` görsel olarak Yüklenici Firmalar paneline taşındı. Mevcut veri modeli KDV istisnasını sözleşme düzeyinde tuttuğu için arayüz notu, değişikliğin İhale Bilgileri ekranında sözleşme kaydıyla saklanacağını belirtiyor; hesaplama semantiği değiştirilmedi.
- UI commit'i: `06b201643664e8d6afc89dcdaadac35ba67587b6`; stil ve hibrit grid commit'i: `032febbc31bd78e5c7e055ce2c489112e65de2f2`.
- CI run [39661935908](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39661935908) ve [39661930041](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39661930041) son kontrolde çalışıyordu; sonuçları yeniden doğrula. Gerçek Windows ekran görüntüsü/DPI testi henüz yok.
- Sonraki adım: CI sonuçlarını kontrol et; başarısız statik UI sözleşmesi veya XAML derlemesi varsa düzelt. Ardından gerçek Windows ekranında Tasarım 1 ile görsel karşılaştırma yap. Ana dala merge/release yok.
