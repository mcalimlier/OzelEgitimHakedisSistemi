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


## Devam döngüsü — CI doğrulaması ve KDV istisnası davranışı

- CI sonuçları tekrar kontrol edildi: UI commit'i `032febbc31bd78e5c7e055ce2c489112e65de2f2` için [39661935908](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39661935908) ve [39661930041](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39661930041) tamamlandı ve SUCCESS. Her iki çalışmada tam çözüm/WPF derleme, UI sözleşme, kompakt yerleşim, test derleme ve test çözümü adımları başarılı; ikinci çalışmada PDF smoke üretim/doğrulama da başarılı.
- Kaynak kod denetimi KDV istisnası kontrolünün artık Yüklenici/Firma düzenleme kartında olduğunu doğruladı. Ancak gerçek veri modeli `ContractDraftData.VatExempt` alanını sözleşme kaydında saklıyor; `SaveContractorButton_Click` firma verisini kaydediyor ve KDV alanını sözleşmeye yazmıyor. Bu yüzden checkbox'ın firma kaydetme ile kalıcılaşacağı garanti edilmez. Hesaplama semantiğini değiştirmeden, kontrolün sözleşme düzeyindeki niteliğini kullanıcıya açıkça göstermek gerekiyor; firmaya ait kalıcı bir alan eklemek ise veri modeli ve geçmiş kayıtlar için ayrı bir tasarım kararı gerektirir.
- Henüz gerçek Windows UI/DPI testi yapılmadı. Bulut CI, WPF derleme ve statik UI sözleşmesini doğruladı; ekran görüntüsüyle Tasarım 1 karşılaştırması değildir.
- Başlangıç noktası: `8f999024384b7d004f335f672602ec154647abe7`. Branch: `feature/official-payment-document-templates`. Ana dala merge/release yok.


## Kullanıcı isteği — KDV istisnası davranışı ve Excel-hibrit hakediş alanı (21.10.2026)

- KDV istisnasının veri semantiği açıkça belirtildi: `VatExempt` `ContractDraftData` üzerinde tutulur; `ContractorDraft` alanı değildir. Firma kaydetme işlemi bu değeri kalıcılaştırmaz.
- Firmalar panelindeki yardım metni açıkça firma/sözleşme ayrımını, ilgili sözleşmenin düzenlenip kaydedilmesi gerektiğini anlatıyor. Checkbox değişince `ContractorStatusTextBlock` henüz kaydedilmedi uyarısı gösteriyor. Firma kaydetme işlemi tamamlandığında, seçili sözleşmedeki kayıtlı değer ile checkbox farklıysa başarı mesajına rağmen KDV'nin kaydedilmediğini belirten açık uyarı veriliyor. Hesaplama semantiği değiştirilmedi.
- Hakediş oluştur panelindeki çoklu hat girişi Excel-hibrit çalışma alanı olarak güçlendirildi: satır numarası başlıkları, ilk sütunu sabitleme, hücre seçimi, Excel başlıklarıyla panoya kopyalama, daha belirgin seçili hücre, satır/sütun sanallaştırma ve açık hücre düzenleme yönergesi. Günlük fiyat alanları kaynak hat kaydından geldiği için salt okunur tutuldu; kullanıcı girdileri mevcut iki yönlü bağlamalarla hesaplamaya aktarılır. Resmî hesaplama zinciri değiştirilmedi.
- Kod commit'leri: XAML `403778db95146641e63dda0369560cae80dd2dec`; checkbox durum bildirimi `7e6d066a38c95e0a7f2175fc10fcb8bcaaa213fb`; firma kaydı sonrası kalıcılık uyarısı `fd73de77219c4df01ed71bb042599f63c4354af2`.
- Son doğrulama: eski UI commit'leri için CI PASS; bu döngüdeki güncel commit CI sonuçları henüz bekleniyor. Gerçek Windows masaüstü, %100/%125/%150 DPI ve ekran görüntüsü karşılaştırması yapılmadı.
- Başlangıç/son branch: `feature/official-payment-document-templates`. Son kaynak commit `fd73de77219c4df01ed71bb042599f63c4354af2`. Ana dala merge/release yok.


## Devam notu — Tasarım 1 hibrit hakediş paneli ve firma KDV tercihi (21.10.2026)

- Hakediş Oluştur üst akışı Tasarım 1'e yaklaştırıldı: dört aşamalı altın/lacivert şerit korunarak İKN/Sözleşme, Firma ve Hakediş Dönemi kartları ayrıştırıldı; dönem seçicisi ve çalışma günü özeti dönem kartına taşındı; firma hatları seçimi ve seçili hat durumu iki ayrı kartta sunuluyor. Mevcut `ProgressRouteComboBox`, `ProgressRouteRelationTextBlock` ve `ProgressPeriodMonthComboBox` kontrolleri yeniden kullanıldı; yeni etkileşimler için ayrı, pasif kontrol üretilmedi. Çoklu hat Excel-hibrit tablosunun hücre düzenleme, sabit ilk sütun ve başlıkla panoya kopyalama özellikleri korundu.
- KDV istisnası artık `Contractor` / `ContractorDraft` firma kaydında saklanıyor. Firmalar panelindeki kutu firma kaydıyla birlikte kalıcılaşıyor; kayıtlı firmalar kartında Türkçe VAR/YOK durumu gösteriliyor. Yeni tekli ve çoklu hakediş hesap girdileri seçili firmanın KDV durumunu kullanıyor.
- Eski JSON kayıtları için `Contractor.VatExempt` nullable bırakıldı: özellik bulunmuyorsa eski sözleşme KDV bitinden firma seviyesine tek seferlik geçiş yapılır; yeni kaydedilen açık `false` değeri sonradan eski değere ezilmez. Sözleşme değişikliği ve arşiv düzeltmeleri daha önce düzenlenmiş hakedişlerin kayıtlı `IsVatExempt` değerini sessizce değiştirmeyecek şekilde ayarlandı.
- Regresyon testleri eklendi: firma KDV alanının JSON true/false kalıcılığı, eski JSON'da alanın yokluğunun ayırt edilmesi ve sözleşme değişikliğinin eski hakediş KDV kararını değiştirmemesi.
- Doğrulama: commit `5d5e4d58a7b15148b2def397f999b093da64e544` derlemesi XAML'deki taşınmış ListBox kapanış etiketi nedeniyle FAIL verdi. Kapanış etiketi `147c36856733b48f0f7809d223fb60f0e61d3b00` commit'inde düzeltildi; bu commit'in Windows CI sonucu henüz bekleniyor. Önceki başarısız CI sonuçları yeni commit'in sonucu değildir.
- Açık kabul: gerçek Windows masaüstünde ekran görüntüsüyle birebir piksel karşılaştırması ve %100/%125/%150 DPI testi henüz yapılmadı. Dört resmi PDF'nin alan bazlı tam karşılaştırması da bu UI döngüsünde yapılmadı.
- Branch `feature/official-payment-document-templates`; son doğrulanmış kaynak SHA, not kaydedilirken `147c36856733b48f0f7809d223fb60f0e61d3b00`. Ana dala merge/release yapılmadı.


### Son doğrulama güncellemesi — 21.10.2026 19:57 UTC

- Kod ve test commit'i: `226eb60d8c9cbb2fb27ddcd80d1d0c9a8dd65438`.
- Windows CI bu SHA için PASS: [run 39665892859](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39665892859). Tam çözüm ve WPF derlemesi, UI sözleşme/statik kontrolleri, test derlemeleri, 246 birim testi ve PDF smoke üretimi geçti. (Toplam test sayısı koşu günlüğünde 246; yeni testler dâhil.)
- Gerçek masaüstü ekran görüntüsü/DPI karşılaştırması CI kapsamı dışında; bu nedenle Tasarım 1'in piksel düzeyinde klon kabulü henüz PASS sayılmıyor. CI'nin ürettiği EXE yayın/release değildir; ana dala merge veya release yapılmadı.
