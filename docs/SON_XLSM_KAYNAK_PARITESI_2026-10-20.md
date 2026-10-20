# Son yüklenen XLSM — kaynak paritesi ve regresyon vektörleri

**Tarih:** 20.10.2026  
**Durum:** Dosya baytları ve hücre formülleri doğrudan incelendi; kaynak satır vektörleri C# regresyon testlerine aktarıldı. CI sonucu ve Microsoft Excel/VBA masaüstü çalıştırması ayrı doğrulama kapılarıdır.

## Yetkili kaynak dosyaları

| Program | Dosya | SHA-256 |
|---|---|---|
| Temel Eğitim / Ortaöğretim | 03-Temel Eğitim Taşıma Ödeme.xlsmdüzeltildi (1).xlsm | 37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296 |
| Özel Eğitim | 02-Özel Eğitim Hakediş Ödeme (1)(2).xlsm | c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e |

Bu dosyalar, eski GitHub dokümanlarındaki tarihsel kaynak kopyalarından üstündür. Aşağıdaki sonuç veya test beklentisi son dosyayla çelişirse bu belge ve testler güncellenecektir.

## Kaynak formülleri

**Temel Eğitim — Hesaplama Bilgileri, satırlar 4–8**

- K=(G*H)+I-J
- L=IF(E="Hayır",K*10/100,0)
- M=L/2
- O=K+L
- T=I*6/100
- U=K*9.48/1000
- V=P+Q+R+S+T+U+M
- W=O-V

**Özel Eğitim — Hesaplama, satırlar 4–6**

- K=(G*H)+I-J; L=K*10/100; M=L/2; O=K+L
- T=(P*Q)+R-S; U=T*20/100; V=U*9/10; X=T+U
- AC=(I*6/100)+(R*6/100)
- AD=(K*9.48/1000)+(T*9.48/1000)
- AE=M+V+Y+Z+AA+AB+AC+AD
- AF=O+X; AG=AF-AE

## Kaynak satırlarından alınan hesap sonuçları

**Temel Eğitim, satırlar 4–8**

| Satır | K — taşıma neti | T — ek teminat | U — damga vergisi | V — toplam kesinti | W — net |
|---:|---:|---:|---:|---:|---:|
| 4 | 319.991,9228 | 441,921 | 3.033,523428144 | 19.475,040568144 | 332.516,074511856 |
| 5 | 112.086,35 | 441,981 | 1.062,578598 | 7.108,877098 | 116.186,107902 |
| 6 | 105.527,35 | 442,041 | 1.000,399278 | 6.718,807778 | 109.361,277222 |
| 7 | 85.082,9116 | 442,101 | 806,586001968 | 5.502,832581968 | 88.088,370178032 |
| 8 | 52.329,35 | 442,161 | 496,082238 | 3.554,710738 | 54.007,574262 |

**Özel Eğitim, satırlar 4–6**

| Satır | K — taşıma neti | T — rehber neti | AC — ek teminat | AD — damga vergisi | AE — kesintiler | AG — net |
|---:|---:|---:|---:|---:|---:|---:|
| 4 | 43.630,00 | 34.037,27 | 661,80 | 736,2857196 | 27.328,7843196 | 61.508,9396804 |
| 5 | 34.354,00 | 33.957,27 | 513,00 | 647,5908396 | 28.045,2494396 | 50.492,8745604 |
| 6 | 37.624,00 | 36.107,27 | 580,80 | 698,9724396 | 35.330,0010396 | 49.385,1229604 |

Tutarlar kaynak hücrelerinin önbelleğe alınmış sonuçlarından aktarılmıştır. C# testleri aynı girdi alanları ve program-özel kesintilerle karşılaştırır. Özel Eğitim satırlarında Y/Z/AA/AB sırasıyla vergi borcu, denetim cezası, SGK borcu ve asgari ücret desteği olarak girişe eşlenmiştir.

## Yeniden hesaplama karşılaştırması

Orijinaller değiştirilmeden LibreOffice Calc ile ayrı XLSX kopyaları üretildi; kaynak dosyanın formül önbelleği ile yeniden hesaplanan kopya karşılaştırıldı.

| Dosya | Formül hücresi | Sayısal karşılaştırma | 0,000001 TL üzeri fark |
|---|---:|---:|---:|
| Temel Eğitim | 297 | 138 | 0 |
| Özel Eğitim | 289 | 141 | 0 |
| **Toplam** | **586** | **279** | **0** |

Bu sonuç LibreOffice yeniden hesaplaması ile kaynak önbelleğinin karşılaştırmasıdır. VBA makrolarının Microsoft Excel masaüstünde çalıştırıldığını veya PDF'lerin resmî kabulden geçtiğini göstermez.

## İlgili testler ve kabul sınırı

- CurrentBasicWorkbookDiscrepancyTests: Temel Eğitim son dosyasının satır 4–8 vektörleri.
- CurrentXlsmSourceParityTests: Özel Eğitim son dosyasının satır 4–6 vektörleri.
- Testler [Fact] altında doğrudan kaynak satırlarından kurulan tuple vektörlerini döngüyle doğrular; attribute argümanı olarak dizi veya decimal veri kullanılmaz.
- Windows CI run 39399443926 ve 39399513703 başarılı tamamlandı: tam çözüm/WPF derlemesi, test adımları, PDF smoke ve Windows EXE publish başarılı. Son XLSM regresyonları ve güncel kaynak-PDF testi bulunan run 39400709937 de başarılı: 215/215 Core ve 6/6 WPF testi geçti; PDF smoke ve Windows EXE publish başarılı. Sonraki commit yalnızca Assert.Contains çağrılarına açık StringComparison.Ordinal ekledi; bu küçük uyarı temizliği için CI yeniden çalıştırılıyor. Bu otomatik CI kanıtı masaüstü Excel/VBA veya 4×2 PDF kabulü değildir.
- Dört PDF × iki program için aynı girdiyle alan/tutar/güzergâh/dönem/imza/sayfa karşılaştırması açık kalır.
- VBA'nın Excel masaüstünde çalıştırılması ve gerçek yazıcı kabulü ayrıca yapılmalıdır.

## Sonraki adımlar

1. Yeni regresyon testlerinin Windows CI sonucunu kontrol et; başarısızsa yalnızca son XLSM hücreleri ve gerçek formül eşlemesiyle düzelt.
2. Her iki kaynak workbook'un dört çıktı sayfasını aynı kaynak satırı/dönemle PDF'ye aktar ve OEHS'nin dört PDF'siyle ayrı ayrı karşılaştır.
3. Kaynak Excel icmal formülü veya makro aktarımı ile OEHS arasında fark bulunursa hücre formülü, VBA aktarımı, örnek sonuç ve test kanıtını birlikte kaydet; sessiz düzeltme yapma.


## Son test uygulama notu — 20.10.2026
İlk MemberData düzenlemesi de Windows CI'da CS3016 verdi; bu nedenle testler daha basit ve derleyici açısından güvenli bir düzene alındı: tek [Fact], tipli tuple kaynak vektörleri ve foreach ile hücre sonuçlarının doğrulanması. Güncel kaynak test düzenlemesini izleyen CI kontrolleri daha sonraki commitlerde tamamlandı; en güncel kontrol durumu aşağıdaki ek nottadır.


## Son XLSM'lerdeki icmal riskleri — formül zinciri ve aktarım ayrı ayrı

**Özel Eğitim**
- Anasayfa!E5 = Kasım 2026.
- Hesaplama!K4 = 43.630,00 TL; Hesaplama!T4 = 34.037,27 TL; K+T = 77.667,27 TL.
- Hakediş Raporu!D11 = 77.667,27 TL; D8−D10 da aynı değeri verir.
- Hakediş İcmali!C8 = 77.667,27 TL ve D8 = 11.030,00 TL; E8 = C8+D8 = 88.697,27 TL. D8, İcmal Verileri'ndeki taşıma fiyat farkı I=7.530 TL ile rehber fiyat farkı R=3.500 TL'yi toplar. Böylece fiyat farkı, C8 içindeki K+T tutarında zaten bulunmasına rağmen ayrı sütunda bir kez daha eklenir.

**Temel Eğitim**
- Hesaplama Bilgileri!I4 = 7.365,35 TL; K4 = 319.991,9228 TL; V4 = 19.475,040568144 TL.
- İcmal Verileri!P4/Q4/R4 = 7.365,35 TL; AD4 = 19.475,040568144 TL. Bu son dosyada ilgili aktarılmış değerler I4 ve V4 ile eşleşir.
- Hakediş İcmali!C8 = 319.991,9228 TL; D8 = 26.840,390568144 TL (= R4 + AD4 = fiyat farkı + kesintiler); E8 = 346.832,313368144 TL. C8'deki K4 aylık hizmet toplamı I4 fiyat farkını zaten içerdiğinden, D8'de fiyat farkının ayrıca eklenmesi kaynak formül zincirinde tekrar sayım oluşturur. D sütununun aynı zamanda AD4 kesintisini içermesi de sütun başlığıyla anlam farkı doğurur.

Önceki tarihsel kopyalarda raporlanan AB4/V4 farkı 4,19 TL bu en son dosyaya doğrudan taşınmamalıdır: son dosyada AD4=V4 görülüyor. Tarihsel bulgu yalnızca eski kopya için geçerlidir.

Bu bulgular, son XLSM'lerde **icmal formül zincirinin ayrı kabul edilmesi gerektiğini** gösterir. Kaynak Excel'in davranışını sessizce düzeltmeyin. Aynı hat/dönemle VBA aktarım makroları masaüstü Excel'de çalıştırılmalı, dört PDF yeniden üretilmeli ve kaynak formülünün iş sahibince onaylanmış davranışıyla OEHS çıktısı karşılaştırılmalıdır.


## CI doğrulaması — güncel durum 20.10.2026 14:06 +03:00

- Etkin PR dalı: `feature/official-payment-document-templates`; HEAD `101cef71e39dc59200d517fef2167b27b81942f3`.
- HEAD için Windows workflow run [39411288804](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39411288804) tamamlandı ve **başarılı**. Çözüm/WPF derleme, UI statik kontrolleri, test derlemeleri ve çalıştırılması, PDF smoke üretimi ve Windows EXE publish adımları başarılı.
- CI raporunda artefakt yükleme adımları ortam sınırlaması nedeniyle atlandı; bu durum derleme/test başarısından ayrı tutulmalıdır.
- Bu CI sonucu otomatik testlerin başarılı olduğunu doğrular; Microsoft Excel masaüstünde VBA çalıştırılması, kaynak Excel'den dört gerçek PDF'nin üretilmesi ve OEHS ile 4×2 alan alan kabul karşılaştırması hâlâ yapılmadı.


## Devam notu — TEHYS Prestij Kart Stili ve hakediş oluşturma (2026-10-20)

### Tasarım kararı
- Kullanıcı, hakediş oluşturma ekranı için lacivert ağırlıklı, vurucu altın sarısı başlık/kenar/eylem vurguları taşıyan **1. tasarım: Prestij Kart Stili** yaklaşımını seçti.
- Görsel hedef: İKN/sözleşme, firma, dönem, hat seçimi ve hesap özeti panellerinin birbirinden net ayrılması; yatay alanın etkin kullanılması; yoğun Excel benzeri kontrol edilebilirliğin korunması.
- Mevcut WPF ana penceresi zaten TEHYS markasını, Navy/Gold kaynaklarını ve İKN seçimi/ihale özeti kontrollerini içeriyor. Tasarım mevcut kontrol ağacına uygulanmalı; yeni, bağımsız ve işlevsiz bir ekran olarak bırakılmamalı.

### Uygulama kuralları
- XAML kontrol adları, event handler'lar ve bağlı ViewModel/hesaplama davranışı değiştirilmeden önce bağımlılıkları incelenmeli.
- Temel Eğitim ve Özel Eğitim hesaplama çekirdekleri ayrı kalmalı. Kaynak XLSM sayfa adları, hücre adresleri ve değişken isimleri yeniden adlandırılmamalı; ortaklaştırma yalnızca arayüz etiket/yerleşiminde yapılmalı.
- Hat bazlı düzenleme yalnızca o hakediş taslağına uygulanmalı; sözleşme/ana hat tanımı açık bir kullanıcı işlemi olmadan sessizce değiştirilmemeli.
- Görsel tutar örnekleri gerçek hesaplama sonucu gibi gösterilmemeli.

### Doğrulama durumu ve açık işler
- Bu not tasarım kararını ve uygulama ölçütlerini kaydeder; Prestij Kart Stili'nin tüm hakediş oluşturma ekranına uygulandığını veya yeni testlerin çalıştığını iddia etmez.
- Son gözlenen PR #3 açık/taslak/birleştirilmemiş durumda. HEAD: `460538f2fd7e20b6d5a043870c42786e738e856e`.
- HEAD için workflow/status sorgusunda yeni CI kaydı görünmedi; dolayısıyla bu HEAD'in CI başarısı doğrulanmış değildir.
- Sonraki teknik adım: mevcut XAML'de hakediş oluşturma görünümünün kontrol hiyerarşisini ve ilgili event handler/ViewModel bağlantılarını saptamak; küçük, geriye uyumlu bir görsel değişiklik yapmak; Windows CI ve WPF testlerini çalıştırıp sonuçları buraya kaydetmek.
- Microsoft Excel/VBA ile aynı girdiden 8 PDF'nin alan alan kabulü hâlâ açık; icmaldeki olası fiyat farkı tekrar sayımı kaynak davranışıyla karşılaştırılmadan değiştirilmemeli.
- Ana dala merge veya yayınlama yapılmayacak.

## Beş çekirdekli geliştirme ve kabul panosu — 20.10.2026

Bu pano işleri ayrı doğrulama alanlarına böler; aynı formül veya kabul ölçütü programlar arasında kanıt olmadan ortaklaştırılmaz. Yüzdeler otomatik test sayısından türetilmez; ilgili kabul kapısı kanıtlanmadan %90 kabul edilmez.

| Çekirdek | Kapsam | Mevcut kanıt | %90 kabul kapısı | Durum |
|---|---|---|---|---|
| 1 — Temel Eğitim hesaplama | Son XLSM satır 4–8; K/L/M/O/T/U/V/W; rapor D15/D28 zinciri | Kaynak vektörleri ve C# regresyon testi; CI koşusu 39423199212 başarılı | Son dosya hash'iyle Excel masaüstü tam hesaplama ve VBA sonrası T4/U4/W4, D15/D28 değerleri eşleşmeli | Otomatik kaynak vektörleri var; Excel/VBA kapısı açık |
| 2 — Özel Eğitim hesaplama | Son XLSM satır 4–6; K/T/AC/AD/AE/AF/AG ve rapor D28 | Kaynak vektörleri ve C# regresyon testi; CI koşusu 39423199212 başarılı | Excel masaüstü tam hesaplama ve VBA sonrası AC4:AG4 ile D28 eşleşmeli | Otomatik kaynak vektörleri var; Excel/VBA kapısı açık |
| 3 — Hakediş rapor zinciri | Kümülatif dönem, ham fiyat farkı, D6/D7/D8/D10/D11 ve net ödeme | `ProgressPaymentReportService` kaynak formül açıklamaları | Aynı kimlik/dönem için Excel raporu ve OEHS raporundaki her finansal alan karşılaştırılmalı; D28 ayrı doğrulanmalı | Kaynak zinciri kodda belgeli; alan-alan Excel karşılaştırması açık |
| 4 — İcmal ve aktarım | Program-özel ay aralığı, önceki ay kesintisi, C8/D8/E8 ve aktarım makroları | `ProgressPaymentIcmalService` testleri; kaynakta olası fiyat farkı tekrar sayımı belgeli | Makro sonrası İcmal Verileri ve İcmal hücreleri karşılaştırılmalı; çift sayım kararı kaynak sahibiyle kayıt altına alınmalı | Otomatik testler var; kaynak davranışına ilişkin karar ve Excel/VBA kanıtı açık |
| 5 — Dört PDF ve WPF akışı | İki program × dört PDF; dönem, yüklenici, hat, imza, tutar, A4/sayfa ve arayüz | Windows CI: tam derleme/test/PDF smoke başarılı | Aynı test girdisiyle Excel PDF'si ve OEHS PDF'si 8/8 görsel ve finansal olarak karşılaştırılmalı; Windows DPI/masaüstü kontrolü yapılmalı | Otomatik smoke var; 8/8 gerçek PDF ve masaüstü kabulü açık |

### Ortak test yürütme kuralları

1. **Kaynak vektörü çekirdeği:** Testler yalnızca son yüklenen iki XLSM'nin hash'i ve yukarıdaki satır sonuçlarıyla güncellenir. Tarihsel kopyadan gelen beklentiler güncel kaynak gibi kullanılmaz.
2. **Rapor çekirdeği:** Hesaplama sayfası AG/W ile Hakediş Raporu D28 aynı sonuç varsayılmaz. Hücre zincirleri ayrı test edilir.
3. **İcmal çekirdeği:** Kaynak Excel'deki şüpheli formül sessizce düzeltilmez. Excel formülü, VBA'nın aktardığı değer ve OEHS sonucu ayrı ayrı kaydedilir.
4. **PDF çekirdeği:** PDF smoke üretimi dosyanın üretilebildiğini gösterir; kaynak Excel PDF'siyle görsel/finansal eşitlik kanıtı değildir.
5. **Arayüz çekirdeği:** Derleme ve statik XAML kontrolleri gerçek Windows masaüstü, DPI, klavye/odak ve iş akışı kabulünün yerine geçmez.

### Güncel doğrulama notu

- PR #3 başlığı açık/taslak ve ana dala birleştirilmemiştir.
- HEAD `4e1102e2afcbc9bf5f57069dce474c623dcfa6a5` için Windows CI run [39423199212](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39423199212) tamamlandı: tam çözüm/WPF derleme, UI statik kontrolleri, test çözümü, PDF smoke ve Windows EXE publish başarılı. Paket/artifact yükleme adımları ortam kısıtları nedeniyle atlanmıştır.
- Bu CI sonucu Microsoft Excel/VBA masaüstü çalıştırması, yazıcı kabulü veya 8/8 gerçek PDF karşılaştırması olarak yorumlanamaz.
- Ana dala birleştirme ve yayınlama için açık onay gerekir.

## Devam notu — CI ve fiyat farkı regresyonu doğrulaması (20.10.2026)

- Etkin PR #3 dalı: `feature/official-payment-document-templates`; HEAD `1ce386eec50aa875995d7ace62a709ef53b47ddc`. PR açık/taslak ve birleştirilmemiş durumda.
- Güncel HEAD için Windows workflow run [39423654004](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39423654004) başarılı tamamlandı. Tam çözüm ve WPF derlemesi, UI statik kontrolleri, test projelerinin derlenmesi/çalıştırılması, PDF smoke üretimi ve Windows EXE publish adımları başarılı.
- Fiyat farkının taşıma matrahına dâhil olduğu sentetik Temel/Ortaöğretim regresyonuna açık assertion'lar eklendi: taşıma neti 120, KDV 12, tevkifat 6, brüt 132 TL; ek teminat 1,20, damga vergisi 1,1376, toplam kesinti 8,3376 ve net ödeme 123,6624 TL. Bu sentetik regresyondur; resmî hakediş örneği değildir.
- CI çıktısının yüklenmesi ve EXE artefaktı bazı adımlarda ortam kısıtı nedeniyle atlanmıştır. Bu, build/test başarısını değiştirmez; ancak indirilebilir paket yayımlandığı anlamına gelmez.
- Açık kabul kapıları değişmedi: Microsoft Excel masaüstünde VBA'nın çalıştırılması; Temel için T4/U4/W4 ve D15/D28; Özel için AC4:AG4 ve D28; kaynak Excel ile OEHS arasında sekiz PDF'nin alan/tutar/görsel karşılaştırması; Windows DPI ve etkileşimli masaüstü kabulü.
- İcmal C8/D8/E8 içindeki olası fiyat farkı tekrar sayımı kaynak davranışı olarak belgeli kalır. Formül/makro çıktısı ve iş sahibi kararı olmadan hesaplama değiştirilmez.
- Ana dala birleştirme veya yayınlama yapılmadı; bunun için açık onay gerekir.