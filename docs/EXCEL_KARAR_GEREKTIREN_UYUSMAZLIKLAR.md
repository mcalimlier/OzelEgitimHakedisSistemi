# Excel Kaynak Uyuşmazlıkları — Karar Kaydı

Tarih: 18.10.2026  
Kapsam: güncel Temel Eğitim ve Özel Eğitim XLSM'leri ile OEHS hesap/rapor zinciri.

**Kural:** Aşağıdaki başlıklarda kod veya kaynak XLSM davranışı karar alınmadan değiştirilmez. Örnek değerler orijinal dosyaların salt okunur incelenmesinden ve ayrı LibreOffice XLSX kopyalarının formül yeniden hesaplamasından alınmıştır. Bu sonuçlar gerçek Excel/VBA masaüstü kabulü değildir.

## Karar 1 — Temel Eğitim ek kesin teminat ve damga vergisi

**Güncel kaynak kanıtı**
- Hesaplama Bilgileri!T4:T6: sabit 1.500 değerleri, I*6% formülü yok.
- U4=(K4*9.48/1000)+T4*9.48/1000: damga vergisi matrahına T dahil.
- Satır 4: K=220.490; T=1.500; U=2.104,4652; V=20.628,9652; W=221.910,0348.
- OEHS aynı girdiyle T=90 (I'nin %6'sı), damga=2.090,2452; toplam kesinti=19.204,7452; net=223.334,2548. Bu fark için CurrentBasicWorkbookDiscrepancyTests regresyon koruması mevcut.

- Aynı kaynak satırlarının 5 ve 6 vektörleri de kapsama alındı (LibreOffice ile yeniden hesaplanmış kopya, Excel/VBA masaüstü kabulü değil):
  - Satır 5 kaynak W=95.940,52152; OEHS net=97.364,68152; fark=1.424,16 TL. Kaynak T=1.500 ve U=956,77848; OEHS T=90,06 ve U=942,55848.
  - Satır 6 kaynak W=89.459,12244; OEHS net=90.883,22244; fark=1.424,10 TL. Kaynak T=1.500 ve U=897,72756; OEHS T=90,12 ve U=883,50756.
  - Farkın her satırda sabit 1.424,22 TL olduğu varsayılmamalı: OEHS'nin ek teminatı I*6% hesabıyla fiyat farkı değiştikçe değişiyor. Bu üç vektör iş kuralını seçmez; kaynak XLSM ve OEHS farklı davranışını korur.

**Karar gerektiren soru:** T, her hakediş/hat için elle girilecek tutar mı, kaynak dosyada kalan sabit/örnek veri mi, yoksa eski formülün eksikliği mi?

**Olası yollar**
1. T'yi açık kullanıcı girdisi olarak modelle ve Basic için damga vergisine T'yi dahil et.
2. T'nin aslında %6 hesaplanması gerekiyorsa kaynak XLSM/VBA'nın neden formül üretmediğini kanıtla; ardından kullanıcı onayıyla iki sistemi eşleştir.
3. T değerleri örnek/stale veri ise gerçek çalışma örneğiyle yeni beklenen değerleri doğrula.

Şu an hiçbir seçenek seçilmedi.

## Karar 2 — Hakediş Raporu D28 ile hesaplama sayfası neti

**Güncel kaynak kanıtı**
- Temel Eğitim: Hesaplama Bilgileri!W4=221.910,0348; Hakediş Raporu!D28=223.410,03; fark 1.500 TL.
- Özel Eğitim: Hesaplama!AG4=95.527,1346528; Hakediş Raporu!D28=98.907,13; fark 3.380 TL.
- Fark, ilgili satırın D7 fiyat farkına yakın; LibreOffice ile yeniden hesaplanan kopyada D28 iki ondalığa gösterildiği için küçük küsurat farkları var.
- Seçili hatı K16/K17 üzerinden 1, 2 ve 3 olarak değiştirdiğim ayrı LibreOffice kopyalarında D28-W/AG farkları:
  - Temel Eğitim: satır 4 `1.499,9952` TL; satır 5 `1.500,98848` TL; satır 6 `1.501,97756` TL. Fiyat farkları sırasıyla 1.500 / 1.501 / 1.502 TL.
  - Özel Eğitim: satır 4 `3.379,9953472` TL; satır 5 `3.755,9964312` TL; satır 6 `4.075,009606` TL. Taşıma + rehber fiyat farkları sırasıyla 3.380 / 3.756 / 4.075 TL.
- Bu çoklu hat incelemesi formül zincirinin tekrar eden örüntüsünü güçlendiriyor; yine de Excel masaüstü/VBA çıktısı değil. Makroların aktarım ve rapor değerleriyle son doğrulama açık.
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
