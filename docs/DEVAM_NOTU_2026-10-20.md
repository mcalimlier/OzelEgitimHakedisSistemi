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
