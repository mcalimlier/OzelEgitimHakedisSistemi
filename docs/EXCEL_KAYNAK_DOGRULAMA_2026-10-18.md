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

**Karar:** Bu farkı sessizce OEHS'ye taşımayın veya mevcut kodu tahminle değiştirmeyin. Excel masaüstünde aynı hat/ay için K/T, aylık D:N aktarım hücreleri, D6, D7, D8, D10, D11, Hesaplama sayfası neti, D28 ve makro kontrol sonucu birlikte kaydedilmeli. D11'in ve resmî net ödemenin beklenen iş anlamı ancak bu zincir doğrulandıktan sonra kesinleştirilebilir.

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
