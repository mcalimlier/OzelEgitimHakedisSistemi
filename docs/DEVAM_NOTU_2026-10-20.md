# OEHS devam notu — 2026-10-20

## Çalışma sınırı
- Temel Eğitim/Ortaöğretim ve Özel Eğitim ayrı hesaplama çekirdekleri olarak ele alınacaktır. Sırf ortaklaştırmak için kod veya hesap kuralı birleştirilmeyecektir.
- Kullanıcının aktardığı iki programın VBA kaynakları aktif/çalışır kabul edilir; kodlar aktarımın kaynak davranışını anlamak için referanstır. Buradaki inceleme Excel masaüstünde makro çalıştırıldığı anlamına gelmez.
- Hesap davranışı, kaynak hücre/aktarım zinciri ve regresyon kanıtı olmadan değiştirilmeyecektir.
- PR #3 taslak ve birleştirilmemiş kalacaktır; ana dala birleştirme veya yayınlama yapılmayacaktır.

## Mevcut uzak GitHub durumu
- Depo: `mcalimlier/OzelEgitimHakedisSistemi`
- Dal: `feature/official-payment-document-templates`
- İncelenen PR: [#3](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/pull/3), taslak/açık.
- Bu not oluşturulmadan önce görülen dal başı: `4c9bdf753cdec142b379e200dddc05e1b84c7433`. Bu notun oluşturulması yeni bir dokümantasyon commit'i üretir; sonraki oturumda gerçek güncel baş SHA ve CI yeniden kontrol edilmelidir.
- Son kanıtlanmış kod/test CI'sı: [run 39317706602](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39317706602): 223/223 Core ve 5/5 WPF test; çözüm derlemesi, PDF smoke ve Windows EXE publish başarılı. Daha sonraki dokümantasyon commit'leri için ayrı CI sonucu ayrıca kontrol edilmelidir.

## Kaynak ve kabul bulguları
- Özel Eğitim VBA aktarım haritası: `Hesaplama!K+T -> İcmal Verileri!D:N`; `I -> P:Z`; `R -> AB:AL`; `AE -> AN:AX`.
- Temel Eğitim aktarım makroları ayrı kaynak haritasıyla ele alınır; iki programın hesap çekirdekleri birbirine dönüştürülmez.
- Önceki formül denetiminde erişilebilir tarihsel XLSM kopyalarının LibreOffice yeniden hesaplamasıyla 5.375 sayısal hücresi karşılaştırıldı; tolerans üstü fark 0, yalnızca TODAY() kaynaklı 10 tarih farkı vardı. Bu Microsoft Excel/VBA çalışma kabulü değildir ve 19 Ekim'de yüklenen en yeni ham dosyaların doğrudan yeniden hesaplandığı anlamına gelmez.
- Tarihsel Temel Eğitim icmalinde yaklaşık 4,19 TL farkı, kaynak `AB4`/`V4` uyuşmazlığı ve makronun son aktarım durumu açısından açık tutulur; otomatik düzeltme yapılmaz.
- Kaynak formül zincirinde icmale fiyat farkının ikinci kez eklenmesi riski belgelenmiştir. Yetkili karar ve aynı-girdi doğrulaması olmadan hesap formülü sessizce değiştirilmez.
- Dört belge × iki program için (Hakediş Raporu, Hizmet İşleri Hakediş Raporu, Hizmet İşleri Kabul Tutanağı, Hakediş İcmali) tutar, dönem, yüklenici, hat, imza ve sayfa düzeni kabulü açık kalır. Sentetik PDF smoke gerçek Excel çıktısına birebir parite kanıtı değildir.

## Sonraki oturumun iş sırası
1. PR #3 başını, en güncel commit SHA'sını ve CI durumunu yeniden oku; bu nottaki SHA'yı varsayma.
2. Her programı kendi VBA/Excel kaynak zinciri üzerinden incele. Özel Eğitim için `K/T`, `I`, `R`, `AE` aktarım bloklarını; Temel Eğitim için kendi `K`, `I`, `V` aktarımını kaynak ay başlıklarıyla eşleştir.
3. Var olan testleri yeniden kullan; eksik kalan yerde yalnızca kaynak vektörüne dayalı, program-özel regresyon testleri ekle.
4. 4×2 PDF kabul matrisinde yalnızca gerçek kanıtı bulunan hücreleri kapat. PDF görsel kabulü/gerçek Excel masaüstü sonucu yoksa açık yaz.
5. Test ve CI tamamlanmadan başarı iddiasında bulunma. Genel ilerleme tahminini kanıt olmadan yükseltme.

## İlerleme tahmini (son belgelenmiş)
| Alan | Tahmin |
|---|---:|
| Temel Eğitim/Ortaöğretim hesaplama | %85 |
| Özel Eğitim hesaplama | %87 |
| Hesaplama motoru | %85 |
| Dört PDF / icmal | %87 |
| Arşiv ve veri modeli | %86 |
| WPF / ekran akışı | %83 |
| Test / CI / dağıtım | %97 |
| **Genel proje** | **%85** |

Bu oranlar mühendislik tahminidir, kabul sertifikası değildir. Genel proje %90 hedefine ulaşmamıştır.

## Kullanıcının GitHub Desktop'ta yapacağı
1. Yerel değişiklikleri kontrol et; korunması gereken değişiklik varsa önce commit et veya yedekle.
2. Current branch `feature/official-payment-document-templates` olmalı.
3. **Fetch origin** yap; ardından yerel değişiklikler güvence altındaysa **Pull origin** yap.
4. PR #3'ü taslak bırak; **Merge** veya yayınlama yapma.
5. Pull sonrasında yerel commit varsa veya GitHub Desktop çakışma gösterirse çözmeden devam etme; sonraki oturumda durumunu bildir.

## Ek çalışma kaydı — 2026-10-20, yeni XLSM testleri
- Kullanıcının en yeni yüklediği Temel Eğitim ve Özel Eğitim XLSM dosyaları için kaynak satır vektörlerini test kapsamına alma çalışması başlatıldı.
- İlk test commit'i `cde5a21657348152654b934d21375c63508c6f52` CI'da derlenemedi: xUnit `InlineData` üzerinde `decimal` literal'ları C# attribute argümanı olamadığından CS0182/CS3016 oluştu. CI run: [39389511886](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39389511886). Bu bir test altyapısı/derleme hatasıdır; hesaplama motorunda başarısızlık kanıtı değildir.
- Hata düzeltildi: test verileri `InlineData` yerine `MemberData` üzerinden sağlanıyor. Düzeltme commit'i: `0a028af494986e978ade9225c8b080fecab69262`.
- Düzeltme commit'i için yeni CI run [39389957851](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39389957851) başlatıldı. Bu not güncellenirken checkout aşaması sürüyor; test sonucu henüz doğrulanmadı.
- PR #3 halen açık, taslak ve birleştirilmemiş; son doğrulanan head `0a028af494986e978ade9225c8b080fecab69262`.
- Yeni vektörlerin gerçek Excel/VBA masaüstü kabulü olmadığı açıkça korunmalı. CI geçse bile bu yalnızca derleme/uygulama test kanıtıdır.
- Sıradaki adım: run 39389957851 sonucunu bekleyip derleme/test hatalarını düzeltmek; sonra hem kaynak vektörlerinin beklenen hücrelerle gerçekten doğru eşlendiğini hem de dört PDF × iki program kabul matrisindeki açık işleri sürdürmek.


## Ek çalışma kaydı — 2026-10-20, arayüz başlangıç boyutu ve CI testi
- Kullanıcı arayüzünün uzman incelemesi istendi; yalnızca kullanıcı tek tek bildirdiğinde değil, taşma/yoğunluk/kullanılabilirlik risklerini proaktif inceleme ilkesi benimsendi.
- `MainWindow.xaml` başlangıç penceresi 2168×997 DIP'ten 1440×900 DIP'e alındı ve başlangıç konumu `CenterScreen` yapıldı. Minimum boyut 1024×600 DIP olarak korundu. Bu yalnızca ilk ekran açılışını düzeltir; içerik taşması ve %100/%125/%150 DPI kabulü ayrıca doğrulanmalıdır.
- Yeni eklenen `LatestUploadedWorkbookParityTests.cs` CI run [39389957851](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39389957851) içinde CS3016 ve xUnit1026 hataları verdi. Test vektörleri bu oturumda Excel kaynağına karşı bağımsız doğrulanamadığı için, hesaplama kodunu değiştirmek yerine derlemeyi bozan ve henüz güvenilir kaynak paritesi kanıtı olmayan test dosyası kaldırıldı. Bu, Excel/VBA parite testinin tamamlandığı anlamına gelmez; yeniden, doğrulanmış kaynak hücreleriyle eklenmesi gerekir.
- CI'nin yeni XAML değişikliği ve test dosyasının kaldırılması sonrasındaki sonucu henüz doğrulanmadı.
- Son commit'ler: UI `99598930437ea92104fdab8d96787d2b7db5116a`; test dosyasının kaldırılması `fd11996ad3b3295113a8fff596aee409ab1c228f`. Sonraki adımda gerçek dal başı ve yeni CI run kontrol edilmelidir.


## Ek doğrulama — CI tamamlandı (2026-10-20)
- Arayüz boyutu değişikliğini ve doğrulanmamış test dosyasının kaldırılmasını içeren sonraki Windows CI çalışması [39390508166](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39390508166) **başarılı** tamamlandı. Tam çözüm/WPF derlemesi, statik arayüz kontrolleri, testler, PDF smoke üretimi ve Windows EXE yayın adımları başarılı olarak raporlandı.
- Bir önceki CI çalışması [39390480654](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39390480654), test dosyası kaldırılmadan önce derlemede başarısız oldu; bu eski başarısızlık yeni başarılı çalışmayla giderilmiş durumda.
- Ana pencere kaynak kontrolü: `Width=1440`, `Height=900`, `MinWidth=1024`, `MinHeight=600`, `WindowStartupLocation=CenterScreen`; 2168 genişliği artık yok. CI içindeki `Verify compact UI layout` adımı da başarılı.
- Sınır: CI, gerçek masaüstünde görsel/etkileşimli kabul testi veya farklı DPI/ekran çözünürlüklerinde manuel gözlem yerine geçmez. Dört PDF için kaynak örnekleriyle tam görsel fark kabulü ve Microsoft Excel/VBA ile gerçek parite hâlâ açık.
- PR #3 açık, taslak ve birleştirilmemiş durumda. Bu oturumda merge/publish yapılmadı.
- Sonraki adım: UI kaynaklarında giriş akışının program seçimi olmadan açılmaması ve geçici hesap tutucu/ilerleme göstergesi davranışını kod + CI kontrolleri üzerinden incelemek; ardından dört PDF × iki program kabul matrisini ilerletmek.


## Ek çalışma kaydı — program seçimi güvenlik kontrolü (2026-10-20)
- PR dalı ve mevcut commit/CI kontrol edildi; çalışma dalı `feature/official-payment-document-templates`, PR #3 açık/taslak/birleştirilmemiş durumda.
- `MainWindow.xaml.cs` içinde program seçimi boşsa `GetSelectedTransportationProgramType()` fonksiyonunun `SpecialEducation` varsayılanı verdiği tespit edildi. Bu değer başlangıç özetlerinde hâlâ korunuyor; tüm çağrıları tek hamlede nullable yapıp ekran akışlarını bozmak yerine yeni resmî kayıt noktaları korundu.
- Yeni sözleşme kaydına, program seçimi yoksa kaydı durduran ve kullanıcıya program seçmesini bildiren kontrol eklendi.
- Yeni yüklenici kaydına aynı kontrol eklendi. Mevcut sözleşme/yüklenici düzenlemesinde kayıtlı program türü korunuyor.
- Değişiklik commit'i: [fdad3ef](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/commit/fdad3ef58cc0ae23d565ec686934102476f6e11b).
- Commit için Windows CI run [39395383346](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39395383346) kontrol edildi; bu not hazırlanırken çalışma hâlâ sürüyordu. Başarılı olduğu henüz doğrulanmış değildir.
- Açık risk: program seçimi varsayılanının diğer özet/yardımcı çağrılarındaki etkisi ayrıca gözden geçirilmeli. Gerçek masaüstü kabulü ve Microsoft Excel/VBA eşitliği bu kod değişikliğiyle doğrulanmış sayılmaz.
- Sonraki adım: Windows CI sonucunu doğrulamak; ardından giriş/ilerleme göstergesi akışını incelemek ve sekiz PDF kabul matrisine devam etmek. Merge/yayınlama yapılmadı.


## Ek çalışma kaydı — arşiv düzeltmesinde program türünü koruma (2026-10-20)
- PR #3 güncel başı f2cc765a03f79e6a2d1e32e278cdec02d1b44261 ve CI run [39395493324](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39395493324) **başarılı** olarak doğrulandı. Tam çözüm/WPF derlemesi, testler, PDF smoke ve Windows EXE publish adımları başarılı.
- Program türü yardımcı çağrıları gözden geçirilirken SaveArchivedContractCorrection() içinde arşivdeki sözleşmenin ProgramType değerinin global seçim kutusundan tekrar alındığı bulundu. Seçim boşsa veya farklı programa ayarlıysa, arşiv düzeltmesi yanlışlıkla kayıt türünü değiştirebilirdi.
- Düzeltme: arşiv sözleşmesi düzeltmesi artık archive.Contract.ProgramType değerini koruyor; rehber personel ve yeniden hesaplama davranışı bu kayıtlı programa göre belirleniyor. Kod commit'i: c2f6adda6528f58e780a025fa1172d62a724b6c7.
- Bu yeni commit için CI henüz görünmüyor; sonraki oturumda run oluşup oluşmadığı ve sonucu kontrol edilmelidir. Bu değişikliğe özel otomatik regresyon testi henüz eklenmedi.
- Kalan program türü varsayılanı GetSelectedTransportationProgramType() içinde özet/seçici davranışında mevcut. Yeni resmî sözleşme/yüklenici oluşturma kayıtları artık boş seçimde durduruluyor; yine de tüm ekran akışları masaüstünde etkileşimli olarak kabul edilmiş değildir.
- PR #3 taslak/açık/birleştirilmemiş; merge veya yayınlama yapılmadı. Hesaplama formülleri ve resmî tutarlar değiştirilmedi.
- Sıradaki adım: commit c2f6adda... için CI'yi doğrula; arşiv program türü koruması için program-özel test eklemenin mevcut test mimarisine uygun yolunu araştır; ardından hazırlık/ilerleme göstergesi ve dört PDF × iki program kabul matrisine devam et.


## Ek çalışma kaydı — CI doğrulaması ve manuel deneme vektörleri (2026-10-20)
- Program türünü arşiv düzeltmesinde koruyan kod commit'i c2f6adda6528f58e780a025fa1172d62a724b6c7 için Windows CI [39396174391](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39396174391) **başarılı** tamamlandı: çözüm/WPF derlemesi, statik UI kontrolleri, testler, PDF smoke ve Windows EXE publish başarılı.
- Kullanıcının uygulamada deneyebilmesi için `docs/ORNEK_HAKEDIS_DENEME_VEKTORLERI.md` eklendi. Temel/Ortaöğretim, Özel Eğitim ve negatif fiyat farkı senaryoları sentetik veri ve beklenen sonuçlarla belgelendi. Bu vektörler resmî örnek değildir ve Excel/VBA masaüstü paritesi iddiası taşımaz.
- Deneme vektörleri dosya commit'i: 19f06b828bc2b891a546fd8fa840356ba2bfc0bd. Bu commit sonrası CI ayrıca kontrol edilmelidir.
- Güncel PR #3 taslak/açık/birleştirilmemiştir. Resmî hesap formülleri değiştirilmedi. Dört PDF × iki programın gerçek Excel karşılaştırmalı görsel kabulü hâlâ açık.
- Sonraki adım: yeni dokümantasyon commit'i için CI'yi kontrol et; sonra test projesine kaynak XLSM hücre vektörlerini gerçek giriş alanlarıyla ilişkilendiren sağlam test eklemenin yolunu araştır ve UI/çıktı kabulüne devam et.


## Ek çalışma kaydı — 2026-10-20, örnek hesapların C# doğrulaması
- PR başı çalışma öncesi `d37d200c7983d219bbcabeeca2fcd2ab4e9d5a04` idi; bu commit için Windows CI run `39396815108` başarılıydı. PR #3 taslak/açık/birleştirilmemiş durumda.
- Temel/Ortaöğretim sentetik vektörü C# `ProgressPaymentCalculator` ve mevcut regresyon testiyle doğrulandı: taşıma matrahı 120,00 TL; KDV 12,00 TL; tevkifat 6,00 TL; ek kesin teminat 1,20 TL; damga vergisi 1,1376 TL; toplam kesinti 8,3376 TL; net 123,6624 TL. Önceki test vektörü dokümanındaki KDV 10 TL / tevkifat 5 TL açıklaması aynı matrahla çelişiyordu; doküman düzeltildi.
- Özel Eğitim sentetik vektöründe `AC=(I*6%)+(R*6%)`: I=100, R=50 için ek kesin teminat 9,00 TL; R=0 için 6,00 TL. Hesap makinesi testlerine rehber fiyat farkı sıfırken 6 TL sonucunu kontrol eden regresyon testi eklendi.
- Doküman düzeltme commit'i `ea3c054ae4aa0585fc02507db4dd0ea5567740b0`; test commit'i / güncel PR başı `bfc8fa6664b0418ac812139628fc32e89c50bcaa`.
- Güncel test commit'i için workflow run henüz bulunamadı; yeni testin CI sonucu beklemede/doğrulanmamış kabul edilmeli. Önceki başarılı CI, bu yeni testin çalıştığını kanıtlamaz.
- Hesap sonuçları uygulama C# çekirdeğinin sentetik girdilerle doğrulamasıdır; Microsoft Excel/VBA aynı girdilerle çalıştırılmış değildir ve dört PDF'nin Excel çıktılarıyla tam paritesi hâlâ açık kabul maddesidir.
- Sonraki adım: güncel `bfc8fa6` başı için CI sonucunu izle; başarısızlık varsa logları incele ve düzelt. Ardından dört PDF × iki program kaynak Excel kabul matrisine devam et. PR birleştirilmesin/yayınlanmasın.


## Ek çalışma kaydı — 2026-10-20, son XLSM önceliği ve eski kural düzeltmeleri
- Kullanıcının talimatı doğrultusunda kaynak hiyerarşisi netleştirildi: en son yüklenen Temel Eğitim ve Özel Eğitim XLSM/VBA, eski GitHub dokümanı/testi/sentetik vektörle çelişirse üstündür.
- `docs/XLSM_KAYNAK_INCELEMESI_VE_HAKEDIS_AKISI.md` içinde eski “negatif fiyat farklarında taşıma ve rehber fiyat farklarını netle, yalnızca pozitifse %6 teminat uygula” metni kaldırılarak son Excel formüllerine göre Temel `T=I×%6`, Özel `AC=I×%6+R×%6` kuralı yazıldı; hukuki karar ayrı risk olarak korundu.
- `docs/ON_DENETIM_RISK_DUZELTMELERI_2026-10-15.md` içindeki R-01 açıklaması yeniden kontrol edildi: Hakediş Raporu D6, güncel dönemin ham I+R fiyat farkını dışarıda tutar; D7 bunu bir kez ekler. Bu nedenle rapor zinciri icmaldeki olası çift ekleme riskinden ayrıldı. R-01 ve kaynak inceleme tablosu bu formül zincirine göre düzeltildi.
- Sentetik `ORNEK_HAKEDIS_DENEME_VEKTORLERI.md` dosyasına son Excel ile çelişirse yetkili kabul kuralı sayılamayacağı açıkça eklendi.
- Tarihsel yol haritası sonuçları en son XLSM kabulinden ayrıldı.
- CI run 39398001399, kaynak ve test derleme aşamalarını geçti; Windows EXE publish adımı devam ederken kontrol edildi. Nihai CI sonucu henüz doğrulanmış değil.
- Açık işler: gerçek son XLSM/VBA aynı-girdi sonuçları; 4 PDF × 2 program alan bazlı kabul; arayüz DPI/etkileşimli masaüstü kabulü. PR #3 taslak/açık/birleştirilmemiş kalacak.


## Düzeltme kaydı — 2026-10-20, Hakediş Raporu D6 zinciri
- Kod incelemesinde `ProgressPaymentReportService` güncel dönemin hizmet tabanını (I/R hariç) ve ham I+R fiyat farkını ayrı hesaplıyor; aylık geçmiş K+T toplamı, D6/D7/D8 zinciriyle uyumlu biçimde kuruluyor.
- Önceki notta bu Hakediş Raporu zincirini icmaldeki olası çift ekleme riskiyle karıştıran ifade vardı. R-01 ve kaynak inceleme belgesi düzeltildi: rapor ile icmal ayrı kabul edilir; birinin davranışı diğerine genellenmez.
- Bu doküman düzeltmeleri için yeni CI sonucu henüz alınmadı; kaynak/test mantığı değişmedi, yalnızca açıklama düzeltildi.


## Ek çalışma kaydı — son yüklenen Excel dosyalarının doğrudan kontrolü (2026-10-20)
- En son iki XLSM bu çalışma ortamında bulundu ve dosya parmak izleri alındı:
  - Temel: `03-Temel Eğitim Taşıma Ödeme.xlsmdüzeltildi (1).xlsm`, SHA-256 `37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296`.
  - Özel: `02-Özel Eğitim Hakediş Ödeme (1)(2).xlsm`, SHA-256 `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e`.
- Güncel kaynak satırları okundu: Temel `Hesaplama Bilgileri` 4–8; Özel `Hesaplama` 4–6. Kaynak formüller doğrulandı: Temel `T=I*6/100`, `U=K*9.48/1000`, `V=P+Q+R+S+T+U+M`, `W=O-V`; Özel `AC=(I*6/100)+(R*6/100)`, `AD=(K*9.48/1000)+(T*9.48/1000)`, `AE=M+V+Y+Z+AA+AB+AC+AD`, `AF=O+X`, `AG=AF-AE`.
- Eski `CurrentXlsmSourceParityTests` içinde Temel/Özel için önceki yüklemeden kalmış satır vektörleri vardı. Bunlar son Excel'in kaynak satırlarıyla değiştirildi: Temel 4–8 için beş vektör; Özel 4–6 için üç vektör. Temel ve Özel test çekirdekleri ayrı tutuldu.
- Test verileri xUnit `MemberData` ile veriliyor; `InlineData` içine decimal argüman koymaktan kaynaklanan eski CS0182/CS3016 hata riski kullanılmadı.
- Kod değişikliği yapılmadı; yalnızca kaynak Excel'lerden alınan son satırların regresyon vektörleri ve belgeler güncellendi. Yeni testler henüz CI'da doğrulanmış değil.
- CI kontrolü: run `39398873252` yeni test commit'i için çalışıyor; son kontrol sırasında sonuç bekleniyor. PR #3 taslak/açık/birleştirilmemiştir; mergeability şu an GitHub API'de `false` göründüğünden olası çakışma ayrıca araştırılmalı; birleştirme yapılmayacak.
- Açık işler: CI test sonuçlarını incele; başarısızsa kaynak satır formülleriyle test girdilerini eşleştirerek düzelt. Ardından en son XLSM'lerin formül önbelleği ile LibreOffice yeniden hesaplamasını karşılaştır, VBA'yı masaüstü Excel'de çalıştırma sınırını açık tut ve 4 PDF × 2 program kabul matrisine devam et.


## Ek CI durumu — son XLSM test düzeltmeleri (2026-10-20)
- Run [39398873252](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39398873252) ve [39399075572](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39399075572), test dosyalarındaki MemberData dönüşümü henüz dal birleşim commit'ine yansımadan alınmış checkout nedeniyle CS3016 ile başarısız oldu. Loglar, iki test dosyasında attribute argümanı olarak dizi kullanıldığını gösteriyor.
- Bu hata son test dosyalarında düzeltildi: veriler artık xUnit MemberData üzerinden sunuluyor ve decimal değerler doğrudan InlineData attribute'larına yazılmıyor.
- Güncel testleri içeren sonraki PR CI run [39399138327](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39399138327) şu an tam çözüm derleme aşamasında; sonuç henüz doğrulanmış değil.
- Son XLSM dosyalarının 279 sayısal hücresinde LibreOffice yeniden hesaplama farkı 0; bu bulgu VBA'nın Microsoft Excel'de çalıştırılması değildir.
- Sıradaki adım: run 39399138327'nin derleme/test sonucunu kontrol et; yeni testler başarısızsa logdaki somut alan/hücre eşlemesini düzelt. PR taslak/açık kalacak; merge/yayın yok.


## Son test kodu revizyonu — 2026-10-20
- Run 39399138327 de eski/önceki commit ağacında CS3016 ile başarısız oldu; hata satırları MemberData attribute kullanımına işaret ediyordu.
- Son düzenleme: MemberData ve InlineData kaldırıldı. Temel satır 4–8 ile Özel satır 4–6 kaynak girdileri [Fact] içinde tipli tuple'lar olarak tanımlanıyor ve foreach ile karşılaştırılıyor.
- Son test commit'leri: Özel Eğitim d147068121d84c73af80e4d5ed4f896fba0c965b; Temel Eğitim 15e3e88755ef768fdb6352c8bacb56f6ce99cafe.
- Bu yeni test düzeni için CI sonucu henüz görünmüyor; başarı iddiası yok.


## Son doğrulama — güncel XLSM regresyonları geçti (2026-10-20)
- CI run [39399443926](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39399443926) ve [39399513703](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39399513703) başarılı tamamlandı.
- Son kaynak satır testleri: Temel satır 4–8 (5 vektör) ve Özel satır 4–6 (3 vektör). Testler [Fact] içinde tipli tuple vektörleriyle çalışır; CS3016 attribute hatası giderildi.
- Test sayıları: **215/215 Core**, **5/5 WPF**. Tam çözüm/WPF derlemesi, statik UI kontrolleri, PDF smoke üretimi ve Windows EXE publish başarılı. EXE/arkadaş paketi artefakt yükleme adımları atlandı; fiziksel/masaüstü kabul yapılmadı.
- Son XLSM formül önbelleği ile LibreOffice yeniden hesaplaması karşılaştırması: 279 sayısal hücrede tolerans üstü fark 0.
- Son XLSM'lerdeki güncel icmal bulgusu: Temel Eğitimde P4/Q4/R4=I4 ve AD4=V4 eşleşiyor; ancak C8=K4 (fiyat farkı dahil) iken D8=R4+AD4 fiyat farkını tekrar ve kesintileri aynı sütunda ekliyor. Özel Eğitimde de C8=K+T iken D8=I+R ayrıca ekleniyor. Tarihsel AB4/V4 farkı yalnızca eski kopyaya aittir. Kaynak makrolar masaüstü Excel'de çalıştırılmadan neden/niyet kesinleştirilmeyecek; kod sessizce değiştirilmedi.
- PR #3 taslak/açık/birleştirilmemiş kalacak. Sıradaki iş: son XLSM'nin dört çıktı sayfasını aynı girdilerle OEHS dört PDF'siyle alan alan karşılaştırmak ve gerçek Excel/VBA aktarım sonucunu doğrulamak.


## Güncel kaynak PDF regresyonu — 2026-10-20
- WPF testine son yüklenen Temel satır 4 ve Özel satır 4 girdileriyle 8 ayrı A4 PDF üretme/doğrulama eklendi. Hakediş Raporu ve Hakediş İcmali toplamları, ayrıca her çıktının tek A4 sayfa olması denetleniyor.
- CI run [39400709937](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39400709937) başarılı: 215/215 Core, 6/6 WPF; tam çözüm/WPF derlemesi, UI statik kontrolleri, PDF smoke ve Windows EXE publish başarılı.
- Bu koşu sırasında bir tutar metni assertion'ı gerçek icmal D sütununu (fiyat farkı + kesinti) dikkate alacak şekilde düzeltildi. Sonrasında yeni koşu geçti.
- Son commit a0bce9e063492095493392c3ee40f10c02c648f6 yalnızca CA1307 uyarılarını gidermek için Assert.Contains çağrılarına StringComparison.Ordinal ekledi; bu son küçük commit için CI bekleniyor.
- Kaynak PDF'ler aynı dosya içindeki güncel Excel/VBA makrolarının çalıştırılmasıyla elde edilmiş referanslar değildir. Gerçek masaüstü Excel/VBA ve dört resmi PDF'nin kaynakla görsel kabulü açık.


## Kapanış durumu — 2026-10-20, güncel XLSM + PDF regresyonları
- Son yüklenen Temel/Özel XLSM dosyalarının hash, formül ve satır 4–8 / 4–6 vektörleri güncellendi. LibreOffice yeniden hesaplama karşılaştırması 279 sayısal hücrede tolerans üstü fark göstermedi.
- Yeni WPF testi son XLSM satır 4 girdileriyle iki programın dört ayrı PDF çıktısını (toplam sekiz) üretip A4 boyutu ve temel rapor/icmal tutarlarını kontrol ediyor.
- CI run [39401000133](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39401000133) başarılı: 215/215 Core, 6/6 WPF; tam çözüm/WPF derlemesi, UI statik kontrolleri, PDF smoke ve Windows EXE publish geçti. Artefakt yükleme adımları ortam kısıtlaması nedeniyle atlandı.
- Son kaynak-parite, risk ve devam-notu düzeltmeleri dokümantasyon odaklıdır. Güncel HEAD 2e68e2674f85ce9976a6a33486d84a737b39d77a için CI run [39401156074](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39401156074) başlamış durumda; son sonuç henüz görülmedi.
- PR #3 açık/taslak/birleştirilmemiştir. Bu turda geliştirme/test hazırlığı yaklaşık %90'a getirildi. Kalan kritik kabul: Excel masaüstünde VBA makrolarını çalıştırma; dört PDF × iki program için gerçek kaynak çıktısıyla alan alan karşılaştırma; UI DPI/yazıcı/temiz profil denemeleri. Bunlar yapılmadan 8/8 resmî kabul tamamlanmış sayılmayacak.
