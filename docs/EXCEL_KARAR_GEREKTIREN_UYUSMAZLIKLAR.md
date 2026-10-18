# Excel Kaynak Uyuşmazlıkları — Karar Kaydı

Tarih: 18.10.2026  
Kapsam: güncel Temel Eğitim ve Özel Eğitim XLSM'leri ile OEHS hesap/rapor zinciri.

**Kural:** Aşağıdaki başlıklarda kod veya kaynak XLSM davranışı karar alınmadan değiştirilmez. Örnek değerler orijinal dosyaların salt okunur incelenmesinden ve ayrı LibreOffice XLSX kopyalarının formül yeniden hesaplamasından alınmıştır. Bu sonuçlar gerçek Excel/VBA masaüstü kabulü değildir.

## Karar 1 — Ek kesin teminat / kesin teminat alanlarının anlamı (iki program)

**Kullanıcının güncel kaynak Excel kontrolüyle doğruladığı iş kuralları (öncelikli):**
- **Temel/Ortaöğretim:** Ek kesin teminat tutarı kullanıcı tarafından elle girilir ve hesaplamada girilen tutar aynen kullanılır; bu alana ayrıca %6 uygulanmaz.
- **Özel Eğitim:** Ek kesin teminat `=(I4*6/100)+R4*6/100` formülüyle hesaplanır; iskonto alanları bu formüle eklenmez.
- Bu iki programın kuralları ayrı uygulanır. Temel Eğitimde sabit 1.500 TL varsayılmaz; tutar kullanıcı girdisidir.
- Temel Eğitim damga vergisi formülünde `T` hücresinin ek kesin teminat tutarı olarak kullanıldığı kaynak zinciri de hesaba katılır ve ayrıca test edilir.

**Önceki incelemenin düzeltmesi:** Eski metin, Temel Eğitim `Hesaplama Bilgileri!T4:T6` hücrelerindeki 1.500 değerlerini ve OEHS'nin fiyat farkı üzerinden yaptığı %6 hesabını aynı kalemin doğrudan karşılaştırması olarak ele aldı. Bu çıkarım alanların anlamını doğru ayırmıyordu ve geçerli kabul edilmemelidir. Önceki satır bazlı farklar yalnızca mevcut hücre/kod çıktılarının aritmetik farklarıdır; doğru iş kuralını belirlemez.

**Statik kaynak bulguları ve kalan doğrulama:**
- Temel Eğitim `Hesaplama Bilgileri!T4:T6` hücrelerinde 1.500 örnekleri ve `U4=(K4*9.48/1000)+T4*9.48/1000` formülü statik olarak gözlendi. Kullanıcının güncel açıklamasına göre T tutarı elle girilen ek kesin teminattır; OEHS bu tutarı aynen kullanır ve damga vergisi matrahına K+T olarak dahil eder.
- Özel Eğitim `Hesaplama!AC4` formülü `=(I4*6/100)+R4*6/100` olarak statik incelendi ve kullanıcı tarafından teyit edildi. İskonto alanları J/S bu formüle dahil değildir.
- Kod ve regresyon testleri bu iki program kuralına göre güncellendi. VBA'daki alan aktarım zinciri, gerçek Excel masaüstü hesaplaması ve dört PDF'nin örneklerle karşılaştırılması hâlâ açık kabul kapılarıdır.
- Kaynak XLSM'ler değiştirilmedi. Bu kayıt, kaynak dosyaların Excel/VBA masaüstü kabulünden geçmiş olduğu anlamına gelmez.

**Sonraki doğrulama:**
1. Temel Eğitimde örneğin 1.500 TL girildiğinde ek kesin teminat kesintisinin 1.500 TL olarak kaldığını ve damga vergisi matrahının kaynak K+T formülünü izlediğini test et.
2. Özel Eğitimde örneğin I=100 TL ve R=50 TL iken ek kesin teminatın 9 TL olduğunu; J/S iskontolarının bu formüle katılmadığını test et.
3. Her iki program için kesinti/net ödeme zincirini ve dört PDF'yi aynı örnek verilerle kontrol et.
4. Excel masaüstü/VBA çalıştırması mümkün değilse bu kabul kapısını açık bırak; LibreOffice/statik incelemeyi onun yerine geçmiş gibi gösterme.


## Karar 2 — Hakediş Raporu D28 ile hesaplama sayfası neti

**Güncel kaynak kanıtı**
- Temel Eğitim: Hesaplama Bilgileri!W4=221.910,0348; Hakediş Raporu!D28=223.410,03; fark 1.500 TL.
- Özel Eğitim: Hesaplama!AG4=95.527,1346528; Hakediş Raporu!D28=98.907,13; fark 3.380 TL.
- Fark, ilgili satırın D7 fiyat farkına yakın; LibreOffice ile yeniden hesaplanan kopyada D28 iki ondalığa gösterildiği için küçük küsurat farkları var.
- Seçili hatı K16/K17 üzerinden 1, 2 ve 3 olarak değiştirdiğim ayrı LibreOffice kopyalarında D28-W/AG farkları:
  - Temel Eğitim: satır 4 `1.499,9952` TL; satır 5 `1.500,98848` TL; satır 6 `1.501,97756` TL. Fiyat farkları sırasıyla 1.500 / 1.501 / 1.502 TL.
  - Özel Eğitim: satır 4 `3.379,9953472` TL; satır 5 `3.755,9964312` TL; satır 6 `4.075,009606` TL. Taşıma + rehber fiyat farkları sırasıyla 3.380 / 3.756 / 4.075 TL.
- Bu çoklu hat incelemesi formül zincirinin tekrar eden örüntüsünü güçlendiriyor; yine de Excel masaüstü/VBA çıktısı değil. Makroların aktarım ve rapor değerleriyle son doğrulama açık.
- Aynı satırlar için OEHS `CalculateReportNetPayable` regresyon vektörleri: satır 4 = 95.527,13 TL; satır 5 = 97.595,37 TL; satır 6 = 87.543,81 TL. LibreOffice kopyasındaki D28'ler sırasıyla 98.907,13 / 101.351,37 / 91.618,81 TL. Satır 6'da ham AG=87.543,800394 TL iken OEHS rapor servisi bileşenleri ayrı yuvarladığı için 87.543,81 TL üretiyor; bu 0,01 TL yuvarlama farkı da kaydedildi. Bunlar bilinen fark koruma testleridir, resmî parite iddiası değildir.
- Temel Eğitim satır 5 ve 6 için de OEHS rapor neti regresyon koruması eklendi: OEHS 97.364,68 / 90.883,22 TL; kaynak Hakediş Raporu D28 97.441,51 / 90.961,10 TL. Bu değerler kaynak workbook'un ayrı LibreOffice kopyasında alınmıştır; rapor D28 ile hesaplama W farkı satır 5'te yaklaşık 1.501 TL, satır 6'da yaklaşık 1.502 TL'dir. Dört belge kabulüyle eşleşme henüz yapılmadı.
- D11 formülü IF(D7<0,H2-ABS(D7),H2+D7); D8=D6+D7; makro kontrolü D8-D10=D11 eşitliğini arıyor. D6 aylık D:N toplamına, VBA ise K(+T) aylık aktarımına dayanıyor.

**Karar gerektiren soru:** Resmî ödenecek tutar olarak hesaplama sayfası W/AG neti mi, Hakediş Raporu D28 mi esas alınacak; kaynak workbook içindeki çift fiyat farkı etkisi nasıl yorumlanacak?

**Olası yollar**
1. Excel masaüstünde makroyla yeniden hesaplanan rapor D28'i resmî sonuç kabul et ve OEHS raporunu birebir eşleştir.
2. Hesaplama sayfası W/AG netini beklenen kabul et; kaynak rapor formülünün farkını hata olarak belgeleyip kaynak Excel düzeltmesi için ayrı onay al.
3. Mevzuat/şartname ve idare onayıyla resmi ödeme formülünü belirle; iki Excel çıktısındaki çelişkiyi sessizce uygulama.

Şu an hiçbir seçenek seçilmedi.

## Karar 3 — Temel Eğitim Hakediş İcmali!E21

**Güncel kaynak kanıtı**
- Seçili dönem Anasayfa!E4=Kasım 2026; İcmal Verileri!AC4=24.104,02472 TL (Ekim aylık kesinti).
- Kaynak E21 formülü E5 (bugünün tarihi) ve K17 kullanıyor; aylık makro E4 ve AB:AM kullanıyor. Kaynak E21 önbellek değeri 0.
- OEHS testi, aylık makro düzenine göre önceki ay AC4 değerinin taşınmasını bekliyor. Test adı bu beklentinin literal E21 paritesi olmadığını açıkça belirtiyor.

**Karar gerektiren soru:** OEHS, kaynak E21 formülündeki davranışı mı aynen taklit etmeli, yoksa aylık aktarım makrosunun AB:AM düzenini ve Kasım için Ekim kesintisini mi izlemeli?

**Olası yollar**
1. Excel masaüstünde makroları çalıştırıp E21 ve aylık aktarım hücrelerini karşılaştır; makro çıktısını esas al.
2. Kaynak E21'i bugünkü tarih/yanlış anahtar yüzünden hatalı kabul et; OEHS'de aylık aktarım mantığını uygula ve kaynak farkını resmî karar olarak kaydet.

Kullanıcının önceki kararı: ay takvim sırası Eylül–Temmuz olarak korunacak; gerçek Excel/VBA karşılaştırması olmadan davranış değiştirilmemeli. Nihai karar hâlâ Excel yürütme kanıtı bekliyor.

## Karar 4 — Özel Eğitim icmal ay başlıkları

**Güncel kaynak kanıtı**
- İcmal Verileri!AN3:AX3 dizisinde Kasım başlığı atlanıyor, Aralık başlığı iki kez görünüyor.
- Kasım seçiliyken E21 önbellek değeri 10.580,582128 TL ve AO4 Ekim değeriyle eşleşiyor.
- Bu tek örnek Aralık ve sonraki ayların eşleşmesini kanıtlamıyor.

**Karar gerektiren soru:** Başlık kusuru gerçek Excel/VBA aktarım çıktısında da var mı ve düzeltme kaynak XLSM'de mi, OEHS eşlemesinde mi yapılmalı?

**Olası yollar**
1. Excel'de Kasım, Aralık, Ocak, Haziran ve Temmuz seçip aktarım makroları sonrası AN:AX hücrelerini kaydet.
2. Kanıtlanan sütun kaymasına göre OEHS eşlemesini düzelt, ancak kaynak farkını ayrı karar kaydı olarak tut.
3. Kaynak XLSM formül/başlık düzeltmesi için ayrı onay al.

## Kabul öncesi gerekli kanıt

Her program için Excel masaüstünde:
- tam yeniden hesaplama,
- seçili hat ve dönem için makro öncesi/sonrası hücre değerleri,
- D6/D7/D8/D10/D11/D28 ve Hesaplama W/AG neti,
- E21 ile aylık aktarım hücreleri,
- dört resmî PDF (Hakediş Raporu, Hizmet İşleri Hakediş Raporu, Kabul Tutanağı, Hakediş İcmali) ve aynı hakediş bağlamındaki OEHS PDF'leri.

Bu kanıtlar gelmeden genel ilerleme %90 olarak işaretlenmeyecek. PR #3 birleştirilmez ve kaynak XLSM'ler değiştirilmez.


### 18.10.2026 — Program bazlı ek kesin teminat kuralı düzeltmesi

- Temel/Ortaöğretim: ek kesin teminat tutarı elle girilir ve hesapta aynen kullanılır; bu tutara ikinci kez %6 uygulanmaz.
- Özel Eğitim: ek kesin teminat `=(I4*6/100)+R4*6/100` formülüyle hesaplanır.
- Temel Eğitim damga vergisi zincirinde `U4=(K4*9.48/1000)+T4*9.48/1000` olduğundan elle girilen ek kesin teminat T bileşeniyle matraha katılır.
- Canlı hesap tutucu için program seçimi, giriş alanları, formül ara değerleri ve nihai kesinti/net ödeme izlenir; eksik/geçersiz girdilerde eski sonuç gösterilmez.
- Testler güncellendi; Windows CI ve Excel/VBA masaüstü kabulü çalıştırılmadan başarılı kabul edilmeyecek.
