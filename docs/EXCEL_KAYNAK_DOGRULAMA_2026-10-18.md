# Excel kaynak doğrulama notu — 2026-10-18

## Kapsam

Bu not, iki sağlanan XLSM dosyasının OOXML çalışma sayfası kayıtlarından yapılan statik incelemedir. Bu inceleme Excel masaüstü hesaplama motorunu veya VBA makrolarını çalıştırmaz. Kaynak dosyalar değiştirilmemiştir.

## 1. Anasayfa!AA2 — kayıtlı hata sabiti

Her iki kaynak dosyada da `Anasayfa!AA2` hücresinin XML kaydı `t="e"` ve değeri `#VALUE!` biçimindedir; aynı hücre kaydında bir formül (`<f>`) bulunmamaktadır. Dolayısıyla eldeki dosya içeriğinde bu değer, formülün yeniden hesaplanmış sonucu olarak değil, formülsüz hata türünde bir hücre değeri olarak kayıtlıdır.

**Karar:** Hücreye formül eklenmeyecek veya değer sessizce temizlenmeyecek. Hücrenin kullanıcı arayüzünde, VBA'da veya adlandırılmış aralıklarda kullanılıp kullanılmadığı ayrıca izlenmelidir. Bu statik bulgu, tek başına uygulama hatası veya zararsız artifakt sonucuna varmak için yeterli değildir.

## 2. Temel/Ortaöğretim — Hakediş İcmali!E21

Kaynak formül; dönem seçimi için `Anasayfa!E5` ve `I5:I14`, hat seçimi için `K17`, aylık kesinti aralığı olarak `AN:AX` kullanıyor. Buna karşılık Temel/Ortaöğretim dosyasında seçili dönem `E4`, hat numarası `K16` ve aylık kesinti kolonları `AB:AL` olarak belirlenmiş. Bu nedenle kaynak E21 formülü bu programın hücre düzeniyle tutarsız görünüyor.

İnceleme kopyasında, seçili dönemin bir önceki ayını `AB:AL` başlıklarından bulan formül LibreOffice üzerinden kontrol edildi: Kasım 2026 seçiliyken Ekim 2026 kesintisi **24.104,02472 TL** döndü; kaynak örnekte `İcmal Verileri!AC4` değeri de **24.104,02472 TL**. Bu, inceleme kopyası için hücre/formül karşılaştırmasıdır; Excel masaüstünde kabul testi değildir ve orijinal XLSM değiştirilmemiştir.

## 3. Özel Eğitim — Hakediş İcmali!E21 ve aylık başlıklar

Özel Eğitim kaynak formülü `E5`, `I6:I15`, `K17` ve `AN:AX` aralıklarını kullanıyor. Ancak aylık kolon başlıklarının statik eşlemesinde Kasım 2026 dönemi atlanmış ve Aralık etiketi iki kolonda tekrarlanmış görünüyor. Kaynak E21 önbellekli değeri **10.580,582128 TL**; `İcmal Verileri!AO4` değeri de **10.580,582128 TL**.

**Karar:** Gerçek VBA çalıştırması ve dönem dönem Excel sonuçları karşılaştırılmadan aylık kolon eşlemesi ya da hesap davranışı değiştirilmemelidir. Kasım/Aralık ayrımının hangi makro ve veri aktarım zincirinde oluştuğu ayrıca doğrulanmalıdır.

## 4. Ortak Hakediş Raporu!D11

Her iki kaynak dosyada D11 formülü `=IF(D7<0,H2-ABS(D7),H2+D7)` biçimindedir; görünür satır açıklaması ise “Bu Hakedişin Tutarı (C - D)” der. Formülün iş anlamı, H2 hücresinin kaynağı ve D7 fiyat farkı işaretinin etkisi, örnek Excel/VBA sonuçlarıyla doğrulanmadan yeniden yazılmamalıdır.

Önbellekli hücre değerleri bu farkı somutlaştırıyor:
- **Temel/Ortaöğretim:** H2 = 220.490; D7 = 1.500; D11 = 221.990. Kaydedilmiş sonuç, D7 pozitifken H2 + D7 hesabıyla uyumlu.
- **Özel Eğitim:** H2 = 93.211,64; D7 = 3.380; D11 = 96.591,64. Kaydedilmiş sonuç yine H2 + D7 hesabıyla uyumlu.

Bu örnekler formülün mevcut önbellekli aritmetiğini doğrular; görünen “C - D” açıklamasının yanlış olduğunu tek başına kanıtlamaz. H2, D7 ve D11'in gerçek iş anlamı ile D28/AG net ödeme zinciri Excel masaüstünde aynı örneklerle birlikte kontrol edilmelidir. Kodda veya kaynak XLSM'de değişiklik yapılmadı.

## 5. Temel/Ortaöğretim — Ek kesin teminat (T sütunu) ile mevcut hesap motoru

Güncel `Temel Eğitim Taşıma Ödemes Programı En Güncel(1).xlsm` dosyasının `Hesaplama Bilgileri` sayfasında T4:T6 hücreleri sırasıyla sabit `1500` değerleridir; hücrelerde `I*6%` formülü bulunmuyor. Buna karşılık OEHS hesap motoru ek kesin teminatı fiyat farkının %6'sı olarak hesaplıyor ve eski/transkribe edilmiş test vektörleri de bu davranışı bekliyor. Özel Eğitim dosyasında ise `Hesaplama!AC4` hücresinin formülü açıkça `=(I4*6/100)+R4*6/100` biçiminde.

Aynı satırlarda `U4` damga vergisi formülü `=(K4*9.48/1000)+T4*9.48/1000` biçimindedir; yani kaynak formül T sütunundaki ek kesin teminat tutarını damga vergisi matrahına ekliyor. OEHS hesap motoru damga vergisini Temel/Ortaöğretim için yalnızca K tutarından hesaplıyor. Örnek satırda K4=220.490 ve T4=1.500 olduğundan kaynak U4=2.104,4652; yalnız K üzerinden hesaplanan tutar 2.090,2452 olur. Aradaki 14,22 TL fark, T sütununun kaynakta gerçek hesap zincirine dahil edildiğini gösteriyor.

**Sonuç:** Temel/Ortaöğretim için hem %6 ek teminat üretimi hem de bu tutarın damga vergisi hesabına katılması, güncel kaynak XLSM ile eşdeğer kabul edilemez. T4:T6 sabitlerinin kaynağı ve bu tutarların resmi ödeme hesabındaki anlamı VBA/form giriş akışı ve Excel masaüstü senaryosuyla belirlenmeli. Bu karar alınmadan resmi hesap davranışı değiştirilmedi; mevcut kod yorumundaki yanlış “kaynak formülü birebir” iddiası kaldırılarak uyuşmazlık açıkça belirtildi.

## 6. Hakediş Raporu D6/D8/D10/D11 — formül ve önbellek aritmetiği uyuşmazlığı

İki güncel XLSM'de D6 dizi formülü `SUM(VLOOKUP(...,{4,5,...,14},FALSE))` ile `İcmal Verileri` aylık D:N sütunlarını topluyor. D7 mevcut fiyat farkını, D8 ise `D6+D7` değerini alıyor. D10 önceki dönem toplamına, D11 ise `IF(D7<0,H2-ABS(D7),H2+D7)` formülüne bağlı.

Dosyaların önbellekli örneklerinde:
- **Temel/Ortaöğretim:** D6=836.742; D7=1.500; D8=838.242; D10=616.252; H2=220.490; D11=221.990. Dolayısıyla D6-D10=220.490=H2; D8-D10=221.990=D11.
- **Özel Eğitim:** D6=277.026,66; D7=3.380; D8=280.406,66; D10=183.815,02; H2=93.211,64; D11=96.591,64. Dolayısıyla D6-D10=93.211,64=H2; D8-D10=96.591,64=D11.

D6 formülü aylık K(+T) toplamını kullanıyor ve kaynak aktarım yordamları da K(+T) değerini aylık sütunlara yazıyor. K/T formülleri mevcut fiyat farkını içerdiğinden, D6'nın D10'dan farkı H2'ye eşitken D8/D11'e ayrıca D7 eklenmesi, aynı fiyat farkının iki kez sayılıyor olabileceği yönünde **ciddi bir kaynak zinciri çelişkisi** oluşturuyor. Önbellekli değerler güncel formülün Excel'de yeniden hesaplanmış sonucunu kanıtlamaz; dosyalar tam yeniden hesaplama istiyor.

Aynı örneklerde Hakediş Raporu net tutarı ile Hesaplama sayfası net tutarı da ayrışıyor:
- **Temel/Ortaöğretim:** `Hesaplama Bilgileri!W4=221.910,0348` iken `Hakediş Raporu!D28=223.410,03`; fark 1.500 TL, yani D7 fiyat farkı.
- **Özel Eğitim:** `Hesaplama!AG4=95.527,1346528` iken `Hakediş Raporu!D28=98.907,13`; fark 3.380 TL, yine D7 fiyat farkı.

Bu, kaynak Excel'in kendi hesaplama sayfası ile resmî Hakediş Raporu net ödeme tutarının önbellekli örneklerde aynı olmadığını gösteriyor. OEHS'nin rapor neti hesaplaması bu örneklerde Hesaplama sayfası zincirine yakın davranabilir; ancak kaynak Excel'in resmî rapor formülü farklı sonuç üretiyor. İki kaynaktan hangisinin beklenen resmi sonuç olduğu kullanıcı/Excel masaüstü kararı gerektiriyor.


Ek çapraz kontrol: Her iki orijinal XLSM'nin **ayrı, dönüştürülmüş XLSX kopyası** LibreOffice Calc ile yeniden hesaplatıldı. Kopyalarda D6/D7/D8/D10/D11/D28 ile Temel Eğitim K4/T4/U4/V4/W4 ve Özel Eğitim K4/T4/AC4/AD4/AE4/AF4/AG4 sonuçları yukarıdaki değerlerle aynı kaldı. Orijinal XLSM'lere dokunulmadı. Bu kontrol, çelişkinin yalnızca eski önbellek değeri olma ihtimalini azaltır; LibreOffice VBA makrolarını çalıştırmadığı ve Excel masaüstü motoru olmadığı için gerçek Excel/VBA kabulünün yerine geçmez.

**Karar:** Bu farkı sessizce OEHS'ye taşımayın veya mevcut kodu tahminle değiştirmeyin. Excel masaüstünde aynı hat/ay için K/T, aylık D:N aktarım hücreleri, D6, D7, D8, D10, D11, Hesaplama sayfası neti, D28 ve makro kontrol sonucu birlikte kaydedilmeli. D11'in ve resmî net ödemenin beklenen iş anlamı ancak bu zincir doğrulandıktan sonra kesinleştirilebilir.

## 7. Mevcut regresyon vektörlerinin kaynak sürümü

`tests/OzelEgitimHakedis.Tests/ExcelSourceParityTests.cs` içindeki bazı testler kaynak olarak açıkça önceki dosya adlarını kullanıyor: `03-Temel Eğitim Taşıma Ödeme.xlsm` ve `02-Özel Eğitim Hakediş Ödeme.xlsm`. Örneğin eski Temel Eğitim row 5 vektöründe G=13, H=6545, I=3659,2 ve K=88.744,2; güncel yüklenen Temel Eğitim çalışma kitabının `Hesaplama Bilgileri` satır 5 değerleri ise farklıdır. Güncel Özel Eğitim `Hesaplama` satır 4–6 değerleri de bu eski vektörlerle aynı değildir.

Bu testler faydalı eski regresyon vektörleri olarak korunuyor, fakat adları ve açıklamaları **legacy** olacak şekilde düzeltildi. Artık en son yetkili XLSM'lerle doğrulanmış golden-master testi gibi yorumlanmamalıdır. Güncel dosyalardan çıkarılan değerler ve çözülememiş farklar bu belgedeki ayrı başlıklarda kayıtlı; güncel workbook hücreleriyle yeni parity fixture'ları ancak formül/VBA davranışı doğrulandıktan sonra eklenmeli.

## 8. Güncel Özel Eğitim satır 4–6 için yeni regresyon vektörleri

`tests/OzelEgitimHakedis.Tests/CurrentXlsmSourceParityTests.cs` dosyasına güncel yüklenen Özel Eğitim XLSM'sinin `Hesaplama` satır 4, 5 ve 6 hücrelerinden çıkarılan üç ayrı hesap testi eklendi. Bu satırlarda taşıma/rehber neti, ek teminat, damga vergisi, toplam kesinti, brüt ve net tutarların kodla eşleşmesi bekleniyor. Kaynak değerleri önce LibreOffice ile yeniden hesaplatılan **ayrı XLSX inceleme kopyasından** da okundu; bu, Excel/VBA masaüstü çalıştırması değildir.

Bu testler eski vektörleri güncel kaynak gibi sunan durumu düzeltmek için eklenen ilk güncel kaynak kapsamıdır. Windows CI run `39139111316` bu üç güncel Özel Eğitim vektörünü de içeren test çözümünde başarılı oldu: 214 Core testi ve 4 WPF testi geçti; PDF smoke ve EXE publish adımları da başarılı. Bu, kaynak hücrelerinden transkribe edilen hesap zincirinin OEHS ile eşleştiğini gösterir; Excel masaüstü/VBA çalıştırması değildir. Temel Eğitim için benzer golden-master vektörü henüz parite sayılmaz; ek teminat T sütunu ve damga vergisi U sütunu farkı açık karar kapısıdır.

## 9. Temel Eğitim güncel satır 4 — açık uyuşmazlığı koruyan test

`tests/OzelEgitimHakedis.Tests/CurrentBasicWorkbookDiscrepancyTests.cs` güncel Temel Eğitim satır 4'te OEHS'nin halen ürettiği sonuç ile güncel XLSM hücrelerinin farklı olduğunu açıkça test eder. Kaynakta K=220.490, L=22.049, M=11.024,5, T=1.500 (sabit), U=2.104,4652, V=20.628,9652, W=221.910,0348. OEHS'de aynı G/H/I/J girdileri K, KDV, tevkifat ve brüt tutarı eşleştiriyor; ancak %6 hesabıyla ek teminatı 90 TL, yalnız K üzerinden damga vergisini 2.090,2452 TL hesaplıyor. Bu nedenle kesinti/net sonuçları kaynakla farklı kalıyor.

Bu test bir parite başarı testi değil, çözülmemiş farkın sessizce kaybolmamasını sağlayan **bilinen uyuşmazlık korumasıdır**. T4'ün neden sabit olduğu ve beklenen resmi davranış Excel/VBA ile doğrulanmadan hesap motoru değiştirilmemelidir. Windows CI run `39139237528` bu bilinen-fark testini de içeren çözümde başarılı oldu: **215 Core + 4 WPF testi geçti**, PDF smoke ve Windows EXE publish adımları da başarılı. Test, güncel kaynakla parite sağlandığını değil, bilinen farkın kodda açıkça görünür kaldığını doğrular.

## 10. Güncel Temel Eğitim fark testi — CI sonucu

`CurrentBasicWorkbookDiscrepancyTests.cs` güncel Temel Eğitim satır 4 için kaynak K/KDV/tevkifat/brüt hücrelerini ve OEHS'nin mevcut ek teminat/damga/kesinti/net sonuçlarını ayrı ayrı kontrol eder. Testin amacı farkı “doğru” ilan etmek değil; kaynak T4=1.500 ve U4'ün T'yi içeren formülüyle OEHS'nin %6/T-matrahı davranışı arasındaki ayrışmayı regresyon halinde görünür tutmaktır.

Windows CI run `39139237528`: **215 Core testi ve 4 WPF testi geçti**; çözüm/WPF derlemesi, PDF smoke ve Windows EXE publish adımları başarılı. Bu, testlerin beklenen bilinen-fark vektörünü koruduğunu kanıtlar; Temel Eğitim hesap davranışının doğru olduğu veya Excel masaüstüyle eşleştiği anlamına gelmez.

## 11. Güncel Özel Eğitim rapor D28 fark testi

`CurrentXlsmReportNetDiscrepancyTests.cs` güncel Özel Eğitim satır 4'ün hesaplama sayfası neti ile rapor neti arasındaki farkı OEHS rapor servisinde de görünür tutar: hesaplama `AG4=95.527,1346528`, kaynak `Hakediş Raporu!D28=98.907,13`; OEHS rapor servisi iki ondalığa yuvarlayarak 95.527,13 TL üretir. Test, D28 sonucuyla eşitlik iddiasında bulunmaz; bu farkı karar verilene kadar korur.

Windows CI run `39139686716` başarılı: **216 Core + 4 WPF testi geçti**, PDF smoke ve Windows EXE publish adımları başarılı. Bu test de bilinen uyuşmazlık korumasıdır; kaynak formülünün beklenen resmî sonuç olduğunu veya OEHS'nin rapor netiyle parite sağladığını kanıtlamaz.

## Sonraki doğrulama adımları

1. `AA2` referanslarını formüller, tanımlı adlar, VBA ve sayfa kodlarında aramak.
2. Her iki XLSM için seçili dönem/hat örneklerinde aylık aktarım makrosunu çalıştırıp E21 ve aylık kolon sonuçlarını kaydetmek.
3. Temel/Ortaöğretim E21 inceleme kopyasını Excel masaüstünde yeniden hesaplatıp LibreOffice sonucuyla karşılaştırmak.
4. Kaynak Excel'in dört PDF çıktısını OEHS'nin aynı senaryodaki dört PDF'siyle tutar, dönem, yüklenici, hat, imza ve sayfa düzeni bakımından birlikte karşılaştırmak.

## Doğrulama sınırı

Bu belge statik XML incelemesini ve bir inceleme kopyasındaki LibreOffice formül sonucunu kaydeder. VBA çalışma zamanı testi, Excel masaüstü kabulü veya kaynak Excel ile OEHS arasında uçtan uca eşdeğerlik iddiası değildir.


## Ek statik tarama — AA2 referansları

2026-10-18'deki ek taramada, iki XLSM'nin çalışma sayfası XML'lerinde `Anasayfa!AA2` hücresine doğrudan başvuran formül bulunmadı; `workbook.xml` tanımlı adlarında da AA2 referansı görülmedi. VBA ikili dosyasında yapılan basit metin taramasında da açık `AA2` dizgesi bulunmadı.

Bu sonuç, AA2'nin kesin olarak kullanılmadığını kanıtlamaz: VBA derlenmiş/akışlara bölünmüş kaynak içerdiğinden basit dizge araması tam VBA kaynak incelemesinin yerine geçmez. Bu yüzden AA2, şu aşamada **düşük öncelikli ama kapatılmamış** risk olarak kalır; kullanıcıya görünen bir hata olmadığı ve tam VBA referans incelemesiyle doğrulandığı varsayılmaz.
