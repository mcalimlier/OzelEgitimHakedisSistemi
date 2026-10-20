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


## Devam turu — eski Excel kuralları ve test dokümanlarının temizliği
- 16.10 ve 18.10 tarihli XLSM analizleri, 11.10 referans-formül/PDF belgesi ve eski Excel V2/akış/uyuşmazlık notlarına açık "tarihsel, güncel kabul kaynağı değil" uyarısı eklendi. Son yüklenen dosyalarla çelişen hücre/adres/formül beklentileri geçerli kural olarak kopyalanmamalı.
- `EXCEL_MASAUSTU_KABUL_LISTESI.md` son dosyaların hash'leri, Temel satır 4–8 ve Özel satır 4–6 formül/vectörleri, icmal çift sayım riskleri ve 4×2 PDF kabul adımlarıyla baştan yazıldı.
- Yinelenen `EXCEL_MASAUSTU_KABUL_KONTROL_LISTESI.md` eski hücre adreslerini tekrar etmemesi için kanonik listeye yönlendirme belgesine dönüştürüldü.
- Bu turdaki değişiklikler belge/kabul-prosedürü güncellemeleridir; hesaplama motoru veya resmi tutar formülleri değiştirilmedi.
- Önceki CI run [39401253948](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39401253948) başarılıydı (215/215 Core, 6/6 WPF). Bu turdaki yeni doküman değişikliklerinin CI sonucu son commit için ayrıca kontrol edilecek.
- Açık kalanlar: gerçek Excel masaüstünde VBA sonrası değerler; kaynak Excel PDF'leriyle 8/8 alan/tutar/görsel karşılaştırması; Windows'ta etkileşimli DPI/yazıcı kabulü. PR taslak kalacak.


## Kalıcı kabul şartı — Hakediş Oluştur (asla unutulmayacak)

Bu bölüm devam notlarının kalıcı ürün kriteridir. Yeni oturumlarda ve her kod değişikliğinden sonra korunmalı; kapsamı daraltılmamalı ve yalnızca belgeye yazılmış olması uygulandığı anlamına gelmemelidir.

- **İKN ve program türü:** Kullanıcı Temel Eğitim/Ortaöğretim ve Özel Eğitim için kendi İKN kayıtlarını oluşturup seçebilmeli. Birleşik İKN listesi iki programı aynı tabloda gösterebilir; her satır program türünü göstermeli ve filtrelenebilmelidir. Bu görünüm hesaplama çekirdeklerini birleştirmez.
- **Firma ve hatlar:** Seçilen İKN altında firmalar/yükleniciler ve onlara bağlı istenen sayıda hat tanımlanabilmeli. Kullanıcı bir hat, seçtiği herhangi sayıda hat veya tüm hatlar için hakediş oluşturabilmeli. Yalnızca seçilen hatlar hesaba ve belge üretimine girmeli; seçilmeyen hatlar için çıktı oluşmamalı.
- **Hat bazlı geçici düzenleme:** Seçili hattın dönemlik iş günü, fiyat farkı, iskonto ve ilgili diğer girişlerinde yapılan özel değişiklik yalnızca o hat + dönem + hakediş taslağına uygulanmalı. Diğer hatlara, başka firmalara, önceki/sonraki dönemlere veya kalıcı ana hat/sözleşme tanımına açık kullanıcı onayı olmadan sızmamalı. Önce/sonra değerleri ve gerekçe denetlenebilir olmalı.
- **Çoklu seçim ve belge kimliği:** Çoklu hat seçimi toplu iş akışı olarak desteklenmeli, fakat hesaplama, hakediş numarası, arşiv kaydı ve dört PDF seti her hat için ayrı kimliği korumalı. Bir hattın tutarı/kimliği başka hattın belgesine karışmamalı.
- **Kaynak Excel adları:** Excel'de alan/değişken hangi tam adla geçiyorsa kaynak eşleme envanterinde ve uygun kullanıcı etiketinde aynen korunmalı. Temel ve Özel Eğitim dosyalarında adlar farklıysa her iki ad kendi çekirdeğinde saklanmalı. Sol menü kaynak çalışma sayfası adlarıyla uyumlu olmalı; iki gerçekten eşdeğer sayfa tek menü öğesinde birleştirilirse etiket her iki kaynak sayfa adını da içermeli. Sayfa adı → program → tam alan adı → hücre/formül → VBA aktarımı → model alanı → PDF hedefi zinciri izlenebilir tutulmalı.
- **Tanıdık iş akışı:** Program/dönem → İKN → firma/yüklenici → hat(lar) → gerekiyorsa hat bazlı düzenleme → hesap ön izlemesi/kontrol → dört resmî PDF. Modern tasarım, kullanıcıların yıllardır bildiği Excel terimlerini veya işlem sırasını belirsizleştirmemeli.
- **Programlara özel hesap:** Temel Eğitim/Ortaöğretim ve Özel Eğitim hesaplama, kesinti, rehber personel ve icmal zincirleri ayrı kalmalı. Yalnızca kanıtlanmış ortak davranış paylaşılmalı; kaynak Excel ile çelişen tutar sessizce değiştirilmemeli.

### Zorunlu regresyon/kabul senaryoları
1. Her iki program için ayrı İKN oluşturma/seçme; birleşik listede program türünün görünmesi ve filtrelenmesi.
2. Tek İKN + tek firma + 5 hat: yalnızca seçilen herhangi bir alt küme (ör. 1, 3 veya 5 hat) için hesap ve dört PDF; seçilmeyenler için sıfır çıktı.
3. Bir seçili hatta yapılan özel değişiklik yalnızca o hattı etkiler; kardeş hatlar ve önceki/sonraki dönem arşivleri değişmeden kalır.
4. Arşivden açma/düzeltme/yeniden hesaplama, İKN-program-firma-hat-dönem ilişkisini ve değişiklik izini korur.
5. Kaynak Excel alan adları ve sol menü eşlemeleri otomatik kontrol edilir; programlar arası farklı adlar kaybolmaz.
6. Çoklu hat için her hattın kendi hakediş numarası, arşiv kimliği ve dört PDF seti doğru kimlik/tutar/dönem taşır.
7. Temel ve Özel hesaplama/rapor/icmal testleri bağımsızdır; birleşik İKN listesi ortak hesap motoru gerekçesi değildir.

### Beş çekirdek — yürütme ve kanıt durumu
- **Çekirdek 1 / Temel Eğitim hesaplama:** son XLSM satır 4–8 kaynak vektörleri var; Microsoft Excel/VBA ile T4/U4/W4 ve Hakediş Raporu D15/D28 eşitliği hâlâ açık.
- **Çekirdek 2 / Özel Eğitim hesaplama:** son XLSM satır 4–6 kaynak vektörleri var; Excel/VBA ile AC4:AG4 ve Hakediş Raporu D28 eşitliği hâlâ açık.
- **Çekirdek 3 / Hakediş rapor zinciri:** C# rapor testleri var; aynı girdi için gerçek Excel raporundaki tüm finansal alanların alan-alan karşılaştırması açık.
- **Çekirdek 4 / İcmal ve aktarım:** program-özel ay aralığı ve kimlik testleri var; C8/D8/E8 olası tekrar sayımının VBA aktarımıyla doğrulanması ve karar kaydı açık.
- **Çekirdek 5 / Hakediş Oluştur + dört PDF + WPF:** kodda çoklu hat seçimi/hat bazlı taslak satırları mevcut; ancak 1/3/tüm hat senaryoları, hat değişikliği izolasyonu, Excel sayfa adları/menü eşlemesi ve 8/8 gerçek Excel PDF kabulü kanıtlanmadan bu çekirdek tamamlandı sayılmaz.
- Bu çekirdeklerin ilerleme yüzdesi, yalnızca kabul kanıtıyla artırılmalı. Otomatik CI, Excel/VBA masaüstü ve görsel PDF kabulünün yerine geçmez.

### Bu devam turu — 2026-10-20
- PR #3 başı ve uzak dosyalar yeniden okundu: dal `feature/official-payment-document-templates`, HEAD `ed449a2157765e1d607405b51c284fea3208b7e2`; PR açık/taslak/birleştirilmemiş.
- HEAD için Windows workflow run [39426585405](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39426585405) kontrol sırasında çalışıyor; bu nedenle bu HEAD için CI başarısı henüz iddia edilemez.
- Mevcut WPF kodu `UpdateMultiRouteDraftRows` içinde seçilen hatlardan satır üretiyor; mevcut bir satır aynı route key ile korunuyor, yeni hatlara tek hat için girilmiş fiyat farkı/iskonto/kesinti kopyalanmıyor. Bu iyi bir izolasyon korumasıdır; uçtan uca UI ve arşiv/PDF kabul testleri hâlâ gereklidir.
- Bu nottaki kalıcı kabul şartları kullanıcı talimatının devam notlarına da yazılmıştır. Bu belge değişikliği işlevin tamamlandığını kanıtlamaz.
- Sıradaki adım: hat bazlı izolasyon ve seçilen hat alt kümesi davranışını bağımsız testlerle güçlendir; kaynak sayfa adları/menü eşlemesini denetle; her kod commit'i sonrası Windows CI sonucunu kontrol et. Gerçek Excel/VBA ve 8/8 PDF kabulini açık tut. Ana dala birleştirme/yayınlama yapma.


### Sonraki ilerleme — CI ve hat izolasyon regresyonu (20.10.2026)
- Dokümantasyon commit'i `f8f25f00ae86d20cba376d841a66a21edfa18e72` öncesindeki HEAD `ed449a2157765e1d607405b51c284fea3208b7e2` için Windows CI run [39426585405](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39426585405) **başarılı** tamamlandı: tam çözüm/WPF derlemesi, UI statik/yerleşim kontrolleri, test derlemeleri/çalıştırılması, PDF smoke ve Windows EXE publish başarılı. PDF smoke ve test paketi artefakt yüklemeleri ortam kısıtları nedeniyle atlandı; paket yayımlandığı varsayılmamalı.
- Beş çekirdek çalışması kapsamında Çekirdek 5 için `ProgressPaymentPropagationTests` dosyasına yeni regresyon eklendi: aynı İKN/sözleşme/yüklenici altındaki bir hattın ana hat tanımı değiştiğinde kardeş hattın günlük fiyatı, hat numarası ve hesap sonucu değişmeden kalmalı. Test commit'i `bb3aa6a8441c1ad21a68b02a13e8c95e6fbd75dc`.
- Bu yeni test commit'i için workflow run [39426987510](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39426987510) çalışıyor; bu not yazılırken nihai sonucu doğrulanmış değil. Testin geçtiği veya derlemenin başarılı olduğu iddia edilmemeli.
- Bu regresyon ana hat tanımı değişikliğinin geçmişe yayılımını kontrol eder; dönemlik hakediş taslağındaki geçici hat düzenlemelerinin UI düzeyinde izolasyonu farklı bir kabul senaryosudur ve ayrıca test edilmelidir.
- Güncel PR #3 açık/taslak/birleştirilmemiştir. Merge/release yapılmadı. Sıradaki adım: run 39426987510'un nihai sonucunu kontrol et; ardından menüdeki sayfa adlarını kaynak XLSM sayfa adlarıyla karşılaştırıp kanıtlı eşleme ekle. Excel/VBA masaüstü ve 8/8 gerçek PDF kabulini açık tut.


### Kaynak sayfa adları ve sol menü denetimi — 20.10.2026
- Kaynak VBA metinlerinden doğrudan doğrulanan Temel Eğitim sayfa adları arasında `Anasayfa`, `Hesaplama Bilgileri`, `İcmal Verileri`, `Hakediş Raporu`, `Hizmet İşleri Hakediş Raporu`, `Hizmet İşleri K. Tutanağı` ve `Hakediş İcmali` var. Özel Eğitim kaynaklarında `Anasayfa`, `Hesaplama`, `İcmal Verileri` ve aynı çıktı sayfaları için makro referansları görülüyor. Bu liste tüm çalışma kitabı sayfa envanteri olarak kabul edilmemeli; gizli/yardımcı sayfalar için workbook sayfa listesinin tamamı ayrıca alınmalı.
- Mevcut WPF sol menü etiketleri `ANA EKRAN`, `İHALE BİLGİLERİ`, `YÜKLENİCİ FİRMALAR`, `HATLAR`, `HAKEDİŞ OLUŞTUR`, `ÇIKTILAR`, `HAKEDİŞ GEÇMİŞİ`, `İSTATİSTİKLER`, `ARŞİVLER`, `AYARLAR` ve `GEÇİCİ HESAP TUTUCU`; dolayısıyla kaynak çalışma sayfası adlarıyla birebir eşleşme henüz sağlanmış değildir.
- Bu aşamada menü adları tahminle değiştirilmedi. Önce iki son XLSM'nin tam worksheet listesi ve her sayfanın uygulamadaki işlevi doğrulanmalı; ardından gerçek eşdeğer sayfalar için birleşik etiketler iki kaynak adını da içerecek şekilde XAML ve statik UI testine uygulanmalı. Bu, Çekirdek 5 için açık iş olarak kalır.
- Çekirdek 1–4 için son kaynak formül vektörleri ve mevcut regresyon testleri korunur; Excel/VBA masaüstü kanıtı olmadığı için bu çekirdeklerin kabul durumu yükseltilmez.


### Beş çekirdek devam kaydı — 20.10.2026, son test ekleri
- **Çekirdek 1 (Temel):** Son XLSM satır 4–8 kaynak vektörleri ve hesaplama regresyonları korunuyor; Microsoft Excel/VBA T4/U4/W4 ve D15/D28 karşılaştırması açık.
- **Çekirdek 2 (Özel):** Son XLSM satır 4–6 kaynak vektörleri ve hesaplama regresyonları korunuyor; Microsoft Excel/VBA AC4:AG4 ve D28 karşılaştırması açık.
- **Çekirdek 3 (Rapor):** Mevcut rapor testleri kümülatif hizmet/fiyat farkı ve ihale/hat/yüklenici/program kimlik izolasyonunu kapsıyor. Gerçek kaynak Excel raporlarıyla alan-alan karşılaştırma açık.
- **Çekirdek 4 (İcmal):** `ProgressPaymentIcmalServiceTests` içine Temel Eğitim için Temmuz aramasının geçerli, Özel Eğitim için kaynak arama aralığının dışında olduğuna dair program-özel regresyon eklendi. Commit `5df94a151f873ce0d27b97cd980dc9a2e32a4248`; bu commit'in Windows CI run'ı [39427241216](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39427241216) başladı, nihai sonuç henüz yok.
- **Çekirdek 5 (Hakediş Oluştur/WPF/PDF):** `ProgressPaymentPropagationTests` içine aynı yüklenici ve sözleşme altında bir hattın değiştirilmesinin kardeş hattı etkilememesi için regresyon eklendi. Testi içeren önceki workflow [39426987510](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39426987510) derleme ve `Test Solution` adımlarını geçti; sonradan eklenen Temmuz icmal testi bu eski koşunun kapsamında değildir. En güncel koşu [39427241216](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39427241216) henüz başlangıç aşamasında.
- **Menü/terminoloji açığı:** VBA referansları Temel Eğitimde `Anasayfa`, `Hesaplama Bilgileri`, `İcmal Verileri`, `Hakediş Raporu`, `Hizmet İşleri Hakediş Raporu`, `Hizmet İşleri K. Tutanağı`, `Hakediş İcmali`; Özel Eğitimde `Anasayfa`, `Hesaplama`, `İcmal Verileri` ve çıktı sayfalarını doğruluyor. Sol menüdeki genel etiketler bu kaynak sayfa adlarıyla henüz birebir eşleşmiyor. Tahminle yeniden adlandırma yapılmadı; tam worksheet envanteri ve işlev eşlemesi tamamlanmalı.
- **Son durum:** PR #3 açık/taslak/birleştirilmemiş; HEAD `5df94a151f873ce0d27b97cd980dc9a2e32a4248`. Yeni CI sonucu bekleniyor. Gerçek Excel/VBA masaüstü, hat seçim/override etkileşimli kabul ve 8/8 PDF görsel/finansal parite açık. Ana dala merge/release yok.


### Devam turu — CI sonucu doğrulaması (20.10.2026)
- Run [39427241216](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39427241216) **başarılı** tamamlandı. Bu koşu Temel/Özel program için Temmuz icmal aralığı regresyon testini de içeriyordu; tam çözüm ve WPF derlemesi, UI sözleşme/yerleşim kontrolleri, test projelerinin derlenmesi, `Test Solution` ve PDF smoke üretim/doğrulama adımları geçti. Bu, Excel/VBA masaüstü ve gerçek kaynak PDF eşitliğini kanıtlamaz.
- Run [39427323818](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39427323818) güncel dokümantasyon HEAD'i için aynı derleme, test, UI ve PDF smoke adımlarını geçti; Windows EXE yayımlama/artefakt adımları son kontrol sırasında hâlâ sürüyordu. Nihai run sonucu ayrıca kontrol edilmeli.
- XAML kontrolünde hat listesinin `SelectionMode="Extended"` olduğu ve çoklu hat giriş tablosunun bulunduğu doğrulandı. Bu, tekli/çoklu seçim arayüz sözleşmesini destekler; gerçek kullanıcı etkileşimi ve 1/alt küme/tüm hatların uçtan uca PDF üretimi ayrıca kabul edilmelidir.
- Güncel kod commit'leri: kardeş hat izolasyon regresyonu `bb3aa6a8441c1ad21a68b02a13e8c95e6fbd75dc`; program-özel Temmuz icmal testi `5df94a151f873ce0d27b97cd980dc9a2e32a4248`. Her ikisi de PR #3 dalında; ana dala birleştirme/yayınlama yapılmadı.
- Açık işler değişmedi: tam XLSM worksheet envanteri ve sol menü eşlemesi; gerçek Excel/VBA hücre paritesi; 8/8 PDF'nin finansal/görsel kabulü; geçici hat bazlı değişikliklerin etkileşimli ve arşivli izolasyon testi.

### Otonom devam turu — hat seçimi taslak koruması (20.10.2026)
- Başlangıç HEAD'i fca459eff3deadbfe4be40000031800216fc9c7c ve bu commit'e ait Windows CI run 39427613237 GitHub API üzerinden doğrulandı: tüm çözüm/WPF derleme, Test Solution, PDF smoke doğrulama ve Windows EXE yayımlama adımları başarılı.
- MainWindow.xaml.cs içinde çoklu hat tablosu gizlenirken _multiRoutePaymentRows.Clear() çağrısının kullanıcı taslaklarını sildiği doğrulandı.
- Düzeltme commit'i aa6221fc387b1a7f074036946277b55dc6c611f1: çoklu hat tablosu tekli/boş seçimde gizlendiğinde taslak satırları korunuyor; görünür tekli giriş alanlarındaki değerler tam route key eşleşen taslağa geri yazılıyor. Ek asgari ücret desteği bayrağı, ilgili hattın program türü ve kontrol durumu ile güncelleniyor. Yeni seçimlerde taslaklar yalnızca tam hat anahtarı eşleşirse yeniden kullanılıyor.
- Doğrulama durumu: Kod GitHub dalına yazıldı. Bu yeni commit için CI sonucu henüz bu not hazırlanırken gözlenmedi; başarı varsayılmamalı. Yeni davranış için ayrı etkileşimli WPF otomasyon testi henüz eklenmedi; CI derleme/regresyon sonucu ve gerçek kullanıcı akış testi sonraki kabul adımlarıdır.
- Açık risk: Tekli giriş alanlarından alınan değerler yalnızca _lastProgressDefaultsRouteKey ile eşleşen taslağa aktarılır; başka hatların taslakları değiştirilmez. Arşiv/düzenleme kipinde bu aktarım bilerek kapalıdır.
- Kalıcı kabul kriterleri değişmedi: Temel ve Özel hesaplamalar ayrı; IKN/firmaya bağlı istenen herhangi sayıda hat seçilebilir; yalnız seçilen hatlar için ödeme ve dört PDF; hat bazlı geçici değişiklikler yalnız ilgili hat+dönem+taslakta; Excel alan adları tam korunur; menü kaynak sayfa adlarına göre eşlenir; her program için dört PDF olmak üzere 8 çıktı birlikte doğrulanır; PR #3 taslak kalır ve ana dala birleştirilmez.
- Sıradaki adım: aa6221f... commit'inin Windows CI sonucunu ve test adımlarını kontrol et; ardından aynı seçim/değer koruma akışı için otomatik regresyon testi eklenmesini değerlendir ve tam XLSM worksheet envanteri/menü eşlemesine geç.

### CI düzeltme turu — ReadDecimal kapsam hatası (20.10.2026)
- Önceki CI run [39428899521](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39428899521), HEAD 01569bc24c2423ca58f42332c69fce401e50182a üzerinde `MainWindow.xaml.cs(6547,29)` için CS0136 verdi: `ReadDecimal` yerel fonksiyonu aynı kapsamda yeniden tanımlanmıştı.
- Düzeltme commit'i [8ec7b62bf76b005efc043d2a044d67941c02eb10](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/commit/8ec7b62bf76b005efc043d2a044d67941c02eb10), yinelenen yerel tanımı kaldırdı ve mevcut `ReadDecimal` fonksiyonunu kullandı.
- Güncel Windows CI run [39431932622](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39431932622) HEAD 8ec7b62... için başarılı tamamlandı: tam çözüm ve WPF derlemesi, UI sözleşme/yerleşim kontrolleri, test projeleri derlemesi, Test Solution, PDF smoke üretim/doğrulama, Windows EXE ve izole test paketi artefakt adımları başarılı.
- Bu CI sonucu kod derleme ve mevcut otomatik testler için kanıttır; çoklu/tekli hat geçişini gerçek WPF etkileşimiyle test eden özel regresyonun eklendiği anlamına gelmez. Bu davranış için hedefli test hâlâ açık.
- Açık kabul riskleri aynı: Microsoft Excel masaüstü/VBA ile yeniden hesaplama yapılmadı; kaynak örnekleriyle 8/8 PDF alan ve görsel kabulü tamamlanmadı; XLSM sayfa envanteri ve sol menü eşlemesi tamamlanmalı. PR #3 taslak/açık kalacak; merge veya release yok.
- Sıradaki adım: test mimarisini inceleyip hat taslağı koruma mantığını güvenilir biçimde otomatik test edilebilir kılmak; kırılgan metin taraması yerine mümkünse küçük, bağımsız bir taslak eşleme/koruma bileşenini test etmek.


## Çoklu hat taslak önbelleği — 20.10.2026

- `MainWindow.xaml.cs` içinde seçimi geçici kaldırılan hatların kaydedilmemiş aylık taslaklarının kaybolması riski için kimlik anahtarlı `_multiRouteDraftCache` eklendi.
- Görünen `_multiRoutePaymentRows` koleksiyonu hâlâ yalnızca seçili hatları taşır; taslak önbelleği ise aynı hakediş formunda seçimi değiştirilen hatların değerlerini korur. Yeni hat satırları sıfır aylık girişlerle başlar; mevcut satır değerleri yalnızca tam `RouteKey` eşleşmesinde geri yüklenir.
- Form temizlenirken önbellek de temizlenir. Aylık değişim akışı mevcut aylık giriş sıfırlama davranışını kullanır.
- Kod commit'i: `7da224211daaeb45c49938ff29681f28ba2495fc`.
- Doğrulama: bu değişiklikten sonra yeni Windows CI koşusu henüz sonuçlanmadı. Bu nedenle derleme/test başarısı şimdilik iddia edilmiyor. Sonraki adım CI sonucunu kontrol etmek ve gerekiyorsa düzeltmektir.
- Açık risk: gerçek etkileşimli WPF testiyle A/B seç → A/C seç → B taslağını geri getir senaryosu henüz çalıştırılmadı. Çoklu hakediş kaydetme yolu seçili hat listesini ve görünür satırları kullanmaya devam eder; bu davranış CI ve testle teyit edilmelidir.


## Dönem değişiminde önbellek izolasyonu — 20.10.2026

- İnceleme sırasında ek bir risk bulundu: dönem değişiminde görünen satırlar sıfırlanırken geçici olarak seçili olmayan hatların önbellekteki aylık değerleri korunabiliyordu. Bu değerler aynı formda tekrar seçilince sonraki döneme taşınabilirdi.
- `ResetMonthlyInputsForNewMonth` artık önbellekteki tüm hat taslaklarını sıfırlıyor; asgari ücret desteği davranışı her satırın kendi program türü üzerinden belirleniyor. Böylece seçili olmayan satır da dönem sınırını geçemez.
- Kod commit'i: `449537fa6f18dd2354a8129c7f7259f1d64ed088`.
- Doğrulama: bu düzeltmenin CI koşusu henüz sonuçlanmadı; sonuç görülmeden başarı beyanı yapılmayacak.
- Açık risk: A/B → A/C seçimi ve dönem değişiminde B taslağının sıfırlanması için gerçek etkileşimli regresyon testi henüz eklenmedi/çalıştırılmadı.


## Devam notu — hat taslakları, CI ve çekirdek ilerlemesi (2026-10-20, 14:xx +03:00)

### GitHub ve doğrulama
- PR #3: açık, taslak, birleştirilmemiş. Dal: `feature/official-payment-document-templates`; gözlenen PR head `76d118107c2d77b4b5bf38774c1c298425b9774c`; base `feature/wpf-contract-entry-v0.5`.
- Kod commit'i `449537fa6f18dd2354a8129c7f7259f1d64ed088` için Windows Build run [39433352479](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39433352479) ve PR run [39433360848](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39433360848) başarıyla tamamlandı.
- Bu sonuçlar derleme/CI test akışının başarılı olduğunu gösterir; Microsoft Excel/VBA masaüstü paritesi, etkileşimli WPF/DPI kabulü veya 8/8 gerçek Excel PDF karşılaştırması anlamına gelmez.

### Hat taslak ön incelemesi
- `ResetMonthlyInputsForNewMonth()`, yalnızca görünür satırları değil `_multiRouteDraftCache.Values` içindeki seçimi geçici kaldırılmış hatları da sıfırlıyor. Hat program türüne göre asgari ücret desteği varsayılanı yeniden kuruluyor.
- Ay değişimi algısı `ProgressPeriodMonthComboBox_SelectionChanged` üzerinden `_lastSelectedPaymentMonthKey` karşılaştırmasına bağlı. `ClearProgressEntryFields()` hem görünür satırları hem taslak cache'ini temizliyor.
- `UpdateMultiRouteDraftRows()` görünür listeyi seçili hatlarla sınırlandırıyor ve satırları tam rota anahtarıyla geri yüklüyor. Bu, taslak izolasyonu için doğru yönde; ancak A/B → A/C → A/B etkileşim senaryosunun otomatik regresyon testi henüz mevcut değil.
- Açık test: seçili hat değişimi sırasında tek-hat global giriş alanlarının önceki hat taslağına doğru kaydedilmesi; ay değişince seçili olmayan hat taslaklarının da sıfırlanması; çıktıya yalnızca seçili hatların girmesi. Bu davranışlar WPF seviyesinde veya test edilebilir ayrı bir sınıf üzerinden doğrulanmalı; statik kaynak metni araması tek başına kabul kanıtı değildir.

### Çekirdek ilerleme tahmini
Yüzdeler mühendislik tahminidir; resmî kabul/sertifika değildir.

| Çekirdek | İlerleme | Kalan ana kabul kapısı |
|---|---:|---|
| Temel Eğitim / Ortaöğretim hesaplama | %85 | Excel/VBA sonrası T4, U4, W4, D15, D28 eşleşmesi |
| Özel Eğitim hesaplama | %87 | Excel/VBA sonrası AC4:AG4 ve D28 eşleşmesi |
| Hakediş raporu ve formül zinciri | %82 | Kaynak Excel raporundaki tüm finansal alanların alan-alan karşılaştırması |
| İcmal ve VBA aktarımı | %78 | Makro sonrası İcmal Verileri/İcmal hücreleri ve olası tekrar sayımı kararı |
| Dört PDF × iki program | %72 | Gerçek Excel PDF'leriyle 8/8 görsel ve finansal kabul |
| Çoklu hat / taslak izolasyonu | %78 | A/B→A/C→A/B ve dönem sınırı regresyonları |
| Arşiv, kimlik ve veri modeli | %86 | Hat/dönem bazlı uçtan uca arşiv ve denetim izi kabulü |
| WPF / iş akışı / DPI | %83 | Gerçek Windows masaüstü, DPI, klavye ve etkileşim kabulü |
| Test, CI ve dağıtım | %96 | Masaüstü kabulü ve kullanılabilir artefaktın ayrıca doğrulanması |
| **Genel mühendislik ilerlemesi** | **%82** | Excel/VBA paritesi ve 8 PDF kabul matrisi |

### Sıradaki teknik iş
1. Çoklu hat taslak cache'ini test edilebilir hale getirecek en küçük güvenli tasarım belirlenmeli; test yalnızca kaynak kodu metni aramamalı.
2. Ay seçimi temizlenip yeniden seçildiğinde cache'in önceki döneme ait değer taşıyıp taşımadığı ve seçili hat değişimindeki tek-hat giriş aktarımı doğrulanmalı.
3. Temel ve Özel Eğitim için kaynak workbook sayfa/alan → hücre/formül → VBA aktarımı → model → PDF alanı envanteri tamamlanmalı.
4. Gerçek Microsoft Excel/VBA masaüstü çalıştırması ve kaynak Excel'den üretilmiş sekiz PDF bulunmadığı sürece bu kabul kapıları açık işaretlenmeli.
5. PR #3 taslak kalmalı; ana dala birleştirme veya yayınlama yapılmamalı.


## Devam notu — taslak cache regresyon testleri (2026-10-20, 14:52 +03:00)

### Kod değişiklikleri
- `src/OzelEgitimHakedis.Core/Services/RouteDraftCache.cs` eklendi. `RouteDraftCache<T>`, kararlı rota anahtarıyla taslakları saklayan `Dictionary<string,T>` tabanlı bir sınıf; `ResetAll(Action<T>)` görünürlükten bağımsız bütün cache girdilerini sıfırlamaya olanak veriyor. Commit: `1a20cc2cf10ecc3e956baa0b553ba697672cff44`.
- `tests/OzelEgitimHakedis.Tests/RouteDraftCacheTests.cs` eklendi. İki test; A/B→A/C→A/B senaryosunda rota taslağının izolasyonu/geri yüklenmesi ve seçimi kaldırılmış rota dahil dönem reseti davranışını sınamayı amaçlıyor. Commit: `94e5b5273f3b9093a87e5bd069a990b819d75c90`.
- `MainWindow.xaml.cs` içindeki cache yeni sınıfa bağlandı ve ay değişimi sıfırlaması `ResetAll` üzerinden çalışıyor. Commit: `42f01c2234208f0cc65b139c13d6ddd87f5025dc`.

### Doğrulama
- PR #3 güncel head: `42f01c2234208f0cc65b139c13d6ddd87f5025dc`; açık/taslak/birleştirilmemiş.
- CI run [39434121985](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39434121985) son kontrolde `in_progress`; .NET 8 kurulumu sürüyor, derleme ve test adımları henüz başlamamıştı. Önceki test commit'inin run'ı [39434092509](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39434092509) da son kontrolde `in_progress`.
- Bu nedenle yeni testler henüz başarılı olarak işaretlenmemeli. CI tamamlanınca sonuç kontrol edilmeli; başarısızlık varsa loglardan düzeltme yapılmalı.
- Testler saf cache davranışını doğrular; gerçek WPF kullanıcı etkileşimi, seçili hatların uçtan uca PDF üretimi, Excel masaüstü/VBA paritesi ve sekiz gerçek Excel PDF karşılaştırması hâlâ ayrı kabul kapılarıdır.

### Çekirdek ilerleme durumu
- Çoklu hat/taslak izolasyonu: önceki tahmin `%78`; testler eklenmiş olsa da CI henüz tamamlanmadığından kabul oranı şimdilik değiştirilmedi.
- Genel mühendislik tahmini: `%82`; resmî kabul oranı değildir.
- Sıradaki adım: run `39434121985` ve ilişkili PR CI sonuçlarını kontrol etmek; ardından gerçek WPF entegrasyon testine ve XLSM→PDF alan envanterine devam etmek.


## Devam notu — cache regresyonu ve CI teyidi (2026-10-20, 14:xx +03:00)

- PR #3 dalı `feature/official-payment-document-templates` üzerinde `RouteDraftCache` değişikliklerini içeren kod commit'i `42f01c2234208f0cc65b139c13d6ddd87f5025dc` için Windows Build run [39434130917](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39434130917) **başarılı** tamamlandı. Tam çözüm/WPF derlemesi, UI sözleşme/yerleşim kontrolleri, test projeleri, Test Solution, PDF smoke ve EXE publish adımları geçti; artefakt yüklemeleri ortam kısıtları nedeniyle atlandı.
- Cache regresyon testlerini içeren commit `94e5b5273f3b9093a87e5bd069a990b819d75c90` için PR workflow [39434099709](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39434099709) **başarılı**. En güncel devam-notu commit'i `cb2291107527ea93c968528e02452be6e2023595` için run [39434257910](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39434257910) da kontrol edildi; tüm zorunlu build/test/PDF smoke/publish adımları başarılı, artifact yükleme adımları ortam kısıtları nedeniyle atlandı.
- Kod/test sonucu, cache'in seçimi geçici kaldırılan rotaları koruması ve dönem değişiminde tüm cache kayıtlarını sıfırlaması için otomatik testlerin geçtiğini doğrular. Bu testler gerçek WPF seçim etkileşimini ve üretilen her PDF'nin yalnız seçili rotaya ait olmasını uçtan uca kanıtlamaz.
- PR #3 açık/taslak/birleştirilmemiştir; merge/release yapılmadı.

### Çekirdek ilerlemesi — kanıta göre
Yüzdeler mühendislik tahminidir; resmi kabul oranı değildir.

| Çekirdek | İlerleme | Sonraki kabul kapısı |
|---|---:|---|
| Temel Eğitim / Ortaöğretim hesaplama | %85 | Excel/VBA T4, U4, W4, D15, D28 karşılaştırması |
| Özel Eğitim hesaplama | %87 | Excel/VBA AC4:AG4 ve D28 karşılaştırması |
| Hakediş raporu / formül zinciri | %82 | Kaynak Excel raporlarıyla alan alan karşılaştırma |
| İcmal / VBA aktarımı | %78 | Kaynak makro aktarımı ve muhtemel tekrar sayımı kararının doğrulanması |
| Dört PDF × iki program | %72 | Gerçek Excel örnekleriyle 8/8 finansal ve görsel kabul |
| Çoklu hat / taslak izolasyonu | %80 | Saf cache testleri CI'da geçti; WPF uçtan uca seçim/çıktı kabulü açık |
| Arşiv / kimlik / veri modeli | %86 | Arşivden geri yükle/düzenle/yeniden hesapla ve denetim izi |
| WPF / iş akışı / DPI | %83 | Windows masaüstünde etkileşim, DPI ve klavye kabulü |
| Test / CI / dağıtım | %97 | Güncel CI geçti; gerçek masaüstü kabulü ve indirilebilir artefakt hâlâ açık |
| **Genel mühendislik tahmini** | **%83** | Excel/VBA paritesi, gerçek WPF kabulü ve 8 PDF kabul matrisi |

### Sıradaki adım
1. Seçili rotaların hesaplama/kaydetme/PDF üretim zincirine nasıl aktarıldığını inceleyip cache testinin kapsamadığı sınırı netleştir.
2. Test edilebilir en küçük saf seçim→çıktı filtrelemesi bileşeni gerekiyorsa ekle ve otomatik test et; WPF koduna bağla.
3. Tam XLSM worksheet envanteri ve kaynak sayfa/alan → hücre/formül → VBA → model → PDF eşlemesini yalnız kaynak dosyalar mevcutsa tamamla.
4. Microsoft Excel/VBA masaüstü kabulü ve gerçek Excel kaynaklı 8 PDF karşılaştırmasını tamamlanmış gibi göstermeden açık tut.


## Devam notu — seçili hatların çıktı sınırı (20.10.2026)

- Yeni Core servisi `SelectedRouteDrafts.SelectExact` eklendi. Seçili hat kimliklerine ait taslakları tam ve kararlı anahtar üzerinden çözümler; seçili olmayan ama cache'te duran taslakları çıktı listesinin dışında bırakır. Eksik taslak veya yinelenen seçili kimlikte işlem kapalı hata verir.
- WPF çoklu-hakediş kaydetme yolunda DataGrid düzenlemeleri commit edildikten sonra görünür taslaklar cache'e yazılıyor ve hesaplama/çıktı servisine yalnızca seçili hatların taslakları geçiriliyor. Taslak çözümlemesi başarısızsa hakediş üretimi durdurulup doğrulama mesajı gösteriliyor.
- Yeni regresyon testleri: seçili A/C rotalarında cache'te bulunan B rotasının hariç tutulması; seçili rota taslağı eksikse başarısız-kapalı davranış; yinelenen rota kimliğinin reddi.
- Kod commit'leri: Core `d4f1455779d0864b629dbc396cd7d0ff1f6e58c8`, WPF `4db5571ea9720b52c0e9e2f6549d9db3f4d29126`, test `f13d6c7606884808045733d2deaca377b691c40c`.
- CI durumu bu not yazılırken çalışıyor: WPF commit'i için run [39435034032](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435034032), test commit'i için run [39435071706](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435071706). Bu yeni değişiklikler için başarı henüz iddia edilmiyor.
- Kapsam sınırı: bu saf servis testleri seçili rota kimliklerini sınar; Windows üzerinde gerçek WPF etkileşimi ve sekiz PDF'nin Excel/VBA çıktısıyla alan alan karşılaştırılması ayrı kabul kapısıdır.

### İlerleme güncellemesi
- Çoklu hat / taslak izolasyonu: %82 (çıktı sınırı kodlandı; yeni CI ve WPF uçtan uca kabul bekleniyor).
- Test / CI / dağıtım: %97 (önceki koşular başarılı; yeni kod/test CI'sı bekleniyor).
- Genel mühendislik tahmini: %83 (Excel/VBA paritesi ve 8 PDF kabulü değişmedi, açık).


## Beş çekirdekte paralel çalışma özeti — 20.10.2026

İş akışı beş eş zamanlı teknik hatta bölünerek incelendi; değişiklikler tek feature dalına güvenli sırayla yazıldı:

1. **Hesaplama / XLSM paritesi:** Güncel Temel ve Özel Eğitim formül zinciri ile kaynak satır vektörleri tekrar incelendi. Mevcut testler hesap makinesi ara/sonuçlarını kapsıyor; LibreOffice karşılaştırması Microsoft Excel/VBA kabulü değildir. XLSM dosyaları mevcut Git dalında bulunmadığından uygulama içi ham workbook okuma doğrulaması henüz bağlanmış değil.
2. **Çoklu hat / taslak izolasyonu:** Seçili hat taslaklarını çözümleyen servis artık eksik, yinelenen ve null taslakları reddediyor. Null taslak için regresyon testi eklendi. Seçili olmayan cache kayıtları hesaplama/çıktı listesine giremez.
3. **PDF üretimi / kabul:** Windows CI'da PDF smoke üretim adımı ve WPF çıktı testleri geçti. Bu, gerçek Excel/VBA kaynak PDF'leriyle görsel alan alan karşılaştırma anlamına gelmez; 8/8 kabul kapısı açık.
4. **WPF / UI:** Tam çözüm ve WPF derleme, UI sözleşmesi ve kompakt yerleşim kontrolleri son doğrulama koşularında geçti. Gerçek masaüstü, farklı DPI ve klavye etkileşimi hâlâ ayrıca denenmeli.
5. **Arşiv / kimlik / CI:** Arşiv iş akışı statik kontrolleri ve çözüm testleri CI'da geçti. Gerçek Windows üzerinde arşiv geri yükleme/düzenleme/yeniden hesaplama ve denetim izi kabulü açık.

### CI teyidi
- WPF değişikliği: [39435034032](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435034032) — **başarılı**.
- Seçili hat regresyon testleri: [39435071706](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435071706) — **başarılı**.
- Önceki devam notu: [39435193729](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435193729) — **başarılı**.
- Bu turdaki son koruma/test değişiklikleri: Core commit `cb129ea528fa88a18a8cb190e726ce5b0d443973`; test commit `966dbce5fe484f3d5f073da770b9b486791bed61`. Bu iki yeni commit için CI sonucu henüz alınmadı.

### Güncel ilerleme
| Çekirdek | Tahmini ilerleme | Açık kabul kapısı |
|---|---:|---|
| Temel Eğitim / Ortaöğretim hesaplama | %85 | Microsoft Excel/VBA T4, U4, W4, D15, D28 |
| Özel Eğitim hesaplama | %87 | Microsoft Excel/VBA AC4:AG4 ve D28 |
| Hakediş raporu / formül zinciri | %82 | Kaynak Excel raporuyla alan alan karşılaştırma |
| İcmal / VBA aktarımı | %78 | VBA masaüstü aktarımı ve tekrar sayımı riski |
| Dört PDF × iki program | %72 | Gerçek Excel çıktılarıyla 8/8 karşılaştırma |
| Çoklu hat / taslak izolasyonu | %84 | Yeni CI ve WPF uçtan uca etkileşim |
| Arşiv / kimlik / veri modeli | %86 | Windows uçtan uca arşiv kabulü |
| WPF / iş akışı / DPI | %83 | Gerçek masaüstü/DPI/klavye kabulü |
| Test / CI / dağıtım | %97 | Son commit CI ve gerçek masaüstü kabulü |
| **Genel mühendislik tahmini** | **%83** | Excel/VBA ve 8 PDF kabul kapıları |

**Sonraki yürütme sırası:** (a) son iki commit'in CI'sını kontrol et ve hataları gider; (b) XLSM kaynağı mevcutsa otomatik workbook/formül/önbellek doğrulayıcısını gerçek dosyaya bağla, değilse sahte doğrulama iddiası üretme; (c) WPF çıktı testlerini seçili rota sayısı 1/3/5 ve farklı program türleri için genişlet; (d) 8 PDF kabul matrisini gerçek Excel çıktılarıyla kapat. PR #3 taslak kalır; ana dala birleştirme veya yayınlama yapılmaz.


## Hızlandırılmış tur ek notu — seçim sınırı fail-closed (20.10.2026)

- Seçili hat sınırı ikinci kez sıkılaştırıldı: artık seçili rota taslağı null ise veya hiç rota seçilmemişse çıktı kümesi reddedilir. Böylece boş/bozuk seçimle sessizce boş hakediş oluşturulmaz.
- Ek testler: `OutputSelection_ShouldRejectNullDraftForSelectedRoute` ve `OutputSelection_ShouldRejectEmptySelection`.
- Son kod/test commit'leri: Core `abe06d26544830ea5520d1b76bb4176b98735b62`; test `457139d845cb702e641a8686be1e611ec63c9748`.
- Yeni CI koşuları: [39435976702](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435976702) ve [39436015389](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39436015389) bu not yazılırken çalışıyordu. Önceki paralel koşular [39435827410](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435827410) ve [39435857812](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435857812) da derleme/EXE yayın adımlarını geçerek sonlandırma aşamasındaydı; son durumları nihai başarı olarak burada varsayılmıyor.
- Beş teknik hat paralel incelendi: hesaplama/XLSM, çoklu hat, PDF üretimi, WPF/UI ve arşiv/CI. Kod değişiklikleri tek feature dalında sıralı olarak kaydedildi; beş bağımsız ajan/branch varmış gibi sunulmaz.
- Açık engel: kaynak XLSM dosyaları bu GitHub dalında yok; formül/önbellek vektör testleri mevcut olsa da programın ham XLSM dosyasını kendiliğinden okuyup Microsoft Excel/VBA ile bire bir kıyasladığı iddia edilemez. Gerçek workbook dosyaları uygulama/repo erişimine alınabildiğinde okuyucu ve otomatik karşılaştırma hattı bağlanmalıdır.


## Hızlandırılmış devam — hatalı rota anahtarı koruması (20.10.2026)

- WPF çoklu-hat kaydetme yolundaki hata yakalama genişletildi: seçili rota anahtarı boş/bozuksa `ArgumentException` artık arayüz olay akışından dışarı taşmak yerine kullanıcıya doğrulama mesajı olarak gösterilir; hakediş üretilmez.
- `OutputSelection_ShouldRejectBlankRouteKey` regresyon testi eklendi.
- Commit'ler: WPF `bf05a6fd63bdb792d9b93819626937319c74bd1f`; test `164cea0044227431887d1a6bfd5c645d0481e9c5`.
- İlgili önceki koşular [39435909317](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435909317) ve [39435976702](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435976702) başarılı tamamlandı. Yeni iki commit için henüz yeni CI sonucu görünmüyor; başarı iddiası yapılmıyor.
- Devam eden öncelikler değişmedi: son commit CI; 1/3/5 hat WPF uçtan uca seçim/çıktı testi; XLSM ham kaynak erişimi; iki program için 8 PDF'nin Excel/VBA karşılaştırması. PR #3 açık/taslak kalır.


## Gün sonu devam notu — 20.10.2026

### Bugün son yapılanlar
- Çoklu-hat çıktı sınırında boş rota anahtarı senaryosu için WPF hata yakalama eklendi: `bf05a6fd63bdb792d9b93819626937319c74bd1f`.
- Bu davranışın regresyon testi eklendi: `OutputSelection_ShouldRejectBlankRouteKey`, commit `164cea0044227431887d1a6bfd5c645d0481e9c5`.
- İlgili Windows CI run [39436417301](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39436417301) **başarılı**: tam çözüm/WPF derlemesi, UI sözleşmesi ve kompakt yerleşim kontrolleri, test derlemesi ve test çözümü, PDF smoke üretimi/doğrulaması ve Windows EXE publish adımları geçti. Artefakt yükleme adımlarının bir kısmı ortam kısıtı nedeniyle atlandı; indirilebilir EXE paketi hazır olduğu varsayılmamalı.
- Önceki doğrulama run'ları [39435909317](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435909317) ve [39435976702](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39435976702) da başarılı tamamlandı.

### Durum ve açık riskler
- Seçili rota anahtarlarının boş, yinelenen veya taslağı eksik olması durumunda hakediş üretimi engellenir. Yeni boş anahtar testi CI'da geçti.
- CI başarısı gerçek masaüstü Windows etkileşim/DPI kabulü veya kaynak Excel/VBA paritesi yerine geçmez.
- Kaynak XLSM dosyaları bu dalda görünmediği için Microsoft Excel/VBA'da yeniden hesaplama ve iki program için 8 PDF'nin kaynak PDF/Excel ile alan alan görsel kabulü hâlâ açık.
- İcmal riskleri (Özel Eğitimde K+T ile I+R'nin ayrı toplanması; Temel Eğitimde C8/D8 fiyat farkı/aktarımı olasılığı) karar verilmeden değiştirilmemeli.
- PR #3 açık/taslak; ana dala birleştirme ve yayınlama yapılmadı.

### Bir sonraki oturum için kısa sıra
1. PR dalındaki en son commit ve CI durumunu yeniden doğrula; devam notu commit'inin CI sonucunu ayrıca kontrol et.
2. Çoklu-hat 1/3/5 rota seçimlerinde WPF uçtan uca test kapsamını güçlendir; seçilmeyen taslakların hesap/PDF çıktısına girmediğini kanıtla.
3. İki XLSM kaynağının erişilebilir konumunu doğrula; bulunursa Excel/VBA sonuçlarını hücre düzeyinde otomatik karşılaştır.
4. Temel Eğitim ve Özel Eğitim için dört PDF'den oluşan 4×2 kabul matrisini gerçek Excel/VBA çıktılarıyla tamamla.
5. Gerçek Windows masaüstü, DPI ve arşiv düzeltme kabulünü kaydet.
6. Formül farklarını sessizce değiştirme; kanıt ve karar gerektiren icmal konularını ayrı tut.

**Oturum sonu:** Bu not güncellendi. Kullanıcı bu akşam devam etmeyebilir; sonraki oturum bu nottan başlamalı. Herhangi bir merge/release yapılmamalı.


## Hakediş Oluştur ana iş akışı — gereksinim ve kabul sözleşmesi (20.10.2026)

- Kullanıcının kritik ürün beklentisi netleştirildi: **Hakediş Oluştur programın kalbidir**; Temel Eğitim ve Özel Eğitim İKN'leri, yüklenicileri, hatları ve dönemleri karıştırmadan hem tek hat hem seçili hat grubu hesaplamasını desteklemelidir.
- Yeni belge: [HAKEDIS_OLUSTUR_URUN_KABUL_SOZLESMESI_2026-10-20.md](HAKEDIS_OLUSTUR_URUN_KABUL_SOZLESMESI_2026-10-20.md). İçerik: IKN/ihale-yüklenici-dönem akışı, ortak tabloda iki programı gösterme, beş hattan seçilen üç hattı hesaplama, hat bazında bağımsız taslak ve değişiklik, seçilmeyen hatların hesap/PDF/arşive girmemesi, dönem izolasyonu, fail-closed doğrulamalar ve zorunlu kabul testleri.
- Kaynak adı kuralı kayda alındı: Excel'deki alan/değişken adları tam metinle korunmalı; Temel ve Özel Eğitim farklı ad kullanıyorsa kendi çekirdeğinde ayrı saklanmalı. Kaynak sayfa adları değiştirilmeyecek; sol menü eşlemesi iki son XLSM'nin gerçek sayfa envanteri doğrulandıktan sonra yapılacak.
- Yeni gereksinim belgesi commit'i: `54b8f2c250fb0de33f8a12b095d50f0480c57c30`. Bu dokümantasyon commit'ine özel CI henüz doğrulanmadı; önceki CI [39436841637](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39436841637) başarılı ve son görülen kod akışını doğruluyor.
- Mevcut WPF'de ihale/dosya seçimi, yüklenici bilgisi, çoklu hat seçimi, `MultiRoutePaymentGrid`, `RouteDraftCache` ve `SelectedRouteDrafts.SelectExact` var. Bu, gereksinimlerin tümünün uçtan uca kabul edildiği anlamına gelmez.
- Açık geliştirme işi: 1/3/5 hat WPF uçtan uca senaryoları; her hatta ayrı değişiklik izolasyonu; seçilmeyen hatların dört PDF ve arşive girmediği doğrulaması; ortak İKN tablosunda program türü ayrımı; tam XLSM sayfa/alan envanteri ve menü eşlemesi.
- Bir sonraki adım: güncel branch HEAD/CI'yi yeniden doğrula; sonra çoklu-hat akışının WPF testlerini ekle. Kod değişiklikleri ve testler tek feature dalında sıralı ilerletilsin. PR #3 taslak kalacak; merge/release yok.


## Hakediş Oluştur — seçili hat sayısı regresyonu (20.10.2026)

- `RouteDraftCacheTests.cs` içine `OutputSelection_ShouldIncludeExactlyOneThreeOrFiveSelectedRoutes` teorik testi eklendi: 1, 3 ve 5 seçili hat için yalnızca seçili anahtarların sonuç listesine girmesi, seçili hat sayısının korunması ve seçili olmayan taslakların toplamı etkilememesi doğrulanıyor.
- Test commit'i: `0256ef5ed5dd710d43148f9d0398588b5c203712`.
- İlk CI sorgusunda bu commit için workflow run henüz görünmedi; test sonucu **beklemede/doğrulanmamış**. CI görülmeden başarı iddiası yapılmamalı.
- Bu, çekirdek servis katmanı regresyonudur; WPF etkileşiminden PDF ve arşive kadar uçtan uca kabul testi değildir.
- Sonraki adım: commit için CI run'ını tekrar kontrol et. CI başarılı olursa WPF tarafında 1/3/5 hat seçimi, hat bazlı giriş izolasyonu ve dört PDF'de seçili hatların kapsamı test edilsin. CI başarısızsa logdan nedeni belirle ve düzelt.


## Güncel son XLSM'ler alındı ve kaynak sayfa adları doğrulandı

- Kullanıcı son kaynak dosyaları yeniden sağladı: `02-Özel Eğitim Hakediş Ödeme SON.xlsm`, `03-Temel Eğitim Taşıma Ödeme SON.xlsm`, ayrıca iki VBA RAR arşivi. XLSM'ler doğrudan açılarak sayfa adları okundu.
- **Özel Eğitim** sayfaları (kaynakta tam yazıldığı biçimde): `Anasayfa`, `İhale Verileri`, `Hesaplama`, `Hakediş Raporu`, `Hizmetişleri Hakediş Raporu`, `Hizmet İşleri K. Tutanağı`, `Hakediş İcmali`, `İcmal Verileri`.
- **Temel Eğitim** sayfaları (kaynakta tam yazıldığı biçimde): `Anasayfa`, `İhale Verileri`, `Hesaplama Bilgileri`, `Hakediş Raporu`, `Hizmet İşleri Hakediş Raporu`, `Hizmet İşleri K. Tutanağı`, `Hakediş İcmali`, `İcmal Verileri`, `Ayarlar`, `Data`, `Users`.
- Menü uyumu için belirgin kritik fark: Özel Eğitim sayfası `Hizmetişleri Hakediş Raporu` (bitişik yazım); Temel Eğitim sayfası `Hizmet İşleri Hakediş Raporu` (ayrı yazım). Bu fark kaynak metni olarak korunmalı; ortak menü varsa birleşik etiket iki kaynak adını da açıkça göstermeli. `Hesaplama` ve `Hesaplama Bilgileri` de ayrı çekirdek etiketleridir.
- VBA RAR dosyaları mevcut ancak bu ortamda RAR çıkarma aracı yok; arşiv içindeki kodlar henüz okunmadı. VBA parity iddiası için bu kodlar açılmalı ve XLSM'deki gerçek hücre/formül davranışlarıyla karşılaştırılmalı.
- Hat seçimi testi güncellendi: sabit 1/3/5 varsayımı kaldırıldı. `OutputSelection_ShouldIncludeExactlyTheRequestedNumberOfSelectedRoutes` artık 12 mevcut taslaktan 1, 2, 3, 4, 5, 8 ve 12 hat seçimini sınar. Bu, seçimin bu sayılara sabitlenmediğini regresyon testinde kontrol eder; üretimdeki hat sayısına sabit bir limit getirmez.
- Yeni test commit'i: `90c68135fa954bb6ac39a6ddfbf4ccef730fabfe`. İlk CI sorgusunda run henüz görünmedi; test sonucu beklemede/doğrulanmamış.
- Önceki commit `0256ef5ed5dd710d43148f9d0398588b5c203712` için Windows CI run `39447861062` görülmüştü; yeni commit için ayrıca sonuç alınmalı.
- Devam: yeni commit CI sonucunu kontrol et; ardından gerçek workbook alan/etiket envanterini ve WPF çoklu hat seçiminden dört PDF'ye kapsam aktarımını uçtan uca doğrula. PR #3 taslak kalacak, merge/release yapılmayacak.


## Hat sayısı testi derleme düzeltmesi — son durum

- CI run `39448833948` başarısız oldu. Log, `RouteDraftCacheTests.cs(146,6)` satırında `CS3016: Arrays as attribute arguments are not CLS-compliant` gösterdi. Önceki `MemberData` denemesi de aynı derleyici hatasını çözmedi.
- Test, attribute tabanlı parametreli test yerine tek `[Fact]` içinde farklı seçili hat sayılarını döngüyle doğrulayacak biçimde yeniden yazıldı.
- Yeni commit: `c9a424270bc261e41362347ef035d521a6a1f626`.
- Bu commit için CI run `39449190810` kuyruğa alındı; nihai sonuç henüz doğrulanmadı.
- Bir sonraki adım: run `39449190810` sonucunu ve gerekirse logunu kontrol et. Başarılı değilse düzeltmeden sonra yeniden doğrula. CI başarılı olduktan sonra hat seçimi servis testinden WPF uçtan uca kapsamına geç.


## Eşzamanlı teknik çalışma ve son CI durumu

- Beş inceleme hattı paralel yürütüldü: CI/build, hat seçim güvenliği, WPF hakediş akışı, Excel alan/sayfa adları, PDF/Excel-VBA kabulü. Bunlar aynı feature dalında sıralı commit'lerle ilerleyen teknik iş hatlarıdır; ayrı ajan/branch olarak sunulmamalıdır.
- Test düzeltme commit'i `c9a424270bc261e41362347ef035d521a6a1f626` için run `39449190810` başarılı tamamlandı. Tam çözüm/WPF, UI kontrolleri, test derlemesi, Test Solution, PDF smoke ve Windows EXE publish adımları başarılıdır. Artefakt upload adımları ortam kısıtı nedeniyle atlandı.
- Yakınındaki run `39449236230` da başarılı tamamlandı; aynı şekilde EXE/deneme paketi artefakt upload'ı atlandı.
- Sonraki test genişletmesi commit'i `458daa9df4d4d4ba5010c81095a18573c43f155c`: hat seçimi regresyon testi 12 yerine 37 kayıtlı hatla, 1,2,3,5,8,12,20,36 ve 37 seçim sayısını deniyor. Bu commit için workflow sorgusunda henüz run görünmüyor; **bu yeni test genişletmesinin CI sonucu doğrulanmış değildir**.
- Testlerin amacı herhangi bir sabit hat sınırı koymak değildir; üretimde seçilebilir hat sayısı veri modelindeki gerçek hat sayısına göre dinamik olmalıdır.
- Açık kritik işler: WPF'de İKN/program türü/firma/dönem/hat akışının uçtan uca doğrulanması; hat başına giriş izolasyonu; seçilmeyen hatların toplam, arşiv ve dört PDF'ye girmediğinin testi; gerçek Excel/VBA ile 8 PDF'nin alan alan kabulü; kaynak değişken/sayfa adlarının tam envanter eşlemesi.
- Sonraki adım: commit `458daa9` için CI sonucunu al; ardından WPF çoklu hat akışında test edilebilir en küçük bağımsız doğrulama katmanını belirleyip regresyon testlerini ekle. PR #3 taslak kalacak; merge/release yok.


## 37 hatlık kabul testi — CI doğrulandı (20.10.2026)

- Commit `458daa9df4d4d4ba5010c81095a18573c43f155c` için Windows CI run [39449499001](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39449499001) başarılı tamamlandı.
- Tam çözüm ve WPF derlemesi, UI sözleşmesi/yerleşim kontrolleri, test derlemeleri, Test Solution, PDF smoke ve Windows EXE publish adımları başarılıdır.
- Hat seçimi testi 37 farklı hat taslağında 1, 2, 3, 5, 8, 12, 20, 36 ve 37 seçiminin yalnızca seçilen anahtarları ve toplamı döndürdüğünü doğrular. Bu, servis düzeyi regresyonudur; WPF arayüzü üzerinden gerçek kullanıcı akışının uçtan uca otomasyonu değildir.
- Bu CI, yalnızca commit `458daa9` kapsamını doğrular. Sonraki devam notu commit'i için workflow sonucu ayrıca kontrol edilmelidir.
- **Hakediş Oluştur kabul durumu %100 değildir.** Açık kritik kapsam: İKN altındaki firma seçiminin bağımsız ve anlaşılır olması; seçilen firma/hatların sözleşme ve program türüyle doğru filtrelenmesi; dönem değişiminde görünür ve gizli tüm taslakların izolasyonu; seçilmeyen hatların hesap, arşiv ve dört PDF'ye hiç girmediğinin uçtan uca testi; Temel/Özel değişken ve Excel sayfa adlarının tam eşleme envanteri; gerçek Excel/VBA ile 8 PDF alan bazlı karşılaştırması.
- Sonraki adım: WPF hakediş ekranında İKN ve firma seçim modelini inceleyip, mevcut veri modelinin aynı İKN altında birden çok firmayı destekleyip desteklemediğini doğrula. Desteklemiyorsa sözleşme/hat kimliğini bozmadan ilişki modelini tasarla ve testle; Excel hesap çekirdeklerini birleştirme.
- PR #3 taslak kalır; ana dala merge/release yok.

## 2026-10-20 — Firma seçimi ve CI doğrulaması devamı

- Hakediş Oluştur ekranına `ProgressContractorComboBox` eklendi. Seçilen İKN/sözleşmenin firma seçenekleri, o sözleşmeye bağlı hatlardan ve program türünden türetiliyor.
- Birden fazla firma varsa varsayılan firma seçilmiyor; kullanıcı açık seçim yapmadan hat listesi boş kalıyor. Tek firma varsa otomatik seçiliyor.
- Firma değiştiğinde hat listesi seçili firmanın kimlik numarasına göre filtreleniyor; önceki seçili hatlar yalnızca aynı firma/hat anahtarıyla eşleşirse korunuyor.
- Son kod commit'i: `96751fb40ce1f74a5bdb1f917aa1fb57eeaaf25c`.
- Windows CI run `39450875012` tamamlandı: **success**. WPF Release derlemesi ve mevcut workflow adımları geçti. Derleyici/analiz uyarıları mevcut; bunlar bu değişiklikte derlemeyi durdurmadı.
- Bu CI sonucu, masaüstü UI E2E testinin, gerçek Excel/VBA hesaplama eşitliğinin veya sekiz PDF'nin görsel kabulünün tamamlandığı anlamına gelmez.
- Açık kabul işleri: firma kimliği eksik hat verisinin davranışı; farklı firma/İKN/dönem değişiminde gerçek UI senaryoları; seçilen hatların yalnızca hesaplama/arşiv/PDF'ye gittiğinin uçtan uca doğrulanması; iki güncel Excel'in gerçek Excel/VBA sonuçlarıyla karşılaştırma; dört PDF türünün her programda tutar/dönem/yüklenici/hat/imza/sayfa düzeni kontrolü.
- PR #3 açık/taslak. Ana dala birleştirme veya yayınlama yapılmadı.
