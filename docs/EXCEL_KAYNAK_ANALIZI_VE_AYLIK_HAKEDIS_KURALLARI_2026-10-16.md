# OEHS — Güncel XLSM Kaynak Analizi ve Aylık Hakediş Kuralları

**Tarih:** 16.10.2026  
**Durum:** Çalışma sayfaları, formül envanteri ve formül örüntüsü taraması tamamlandı; 5.691 formül hücresinin tamamında bağımsız semantik doğrulama ve VBA'nın uçtan uca gerçek senaryo doğrulaması sürüyor. Bu belge bulguları ve doğrulama sınırlarını ayırır.  
**Gizlilik:** Kullanıcı tarafından yüklenen çalışma kitapları, kişisel veriler ve giriş bilgileri nedeniyle bu depoya yüklenmemiştir. Bu belge yalnızca şema, formül, makro davranışı ve doğrulama bulgularını içerir.

## 1. İncelenen kaynakların kimliği

- `03-Temel Eğitim Taşıma Ödeme.xlsm` — SHA-256: `9f89be5a8e0315e177b97c02cb8f146be0040ebc1f4be7d7a3d30b5f57040423`
- `02-Özel Eğitim Hakediş Ödeme.xlsm` — SHA-256: `3c67f22327f9c748208cdcd7f9ed5952e37d514ea75a2043aa2f3d49867f61a4`

Dosyalar ayrı ayrı incelendi: çalışma sayfaları, hücre formülleri ve önbelleğe alınmış değerler, tanımlı adlar, dış bağlantılar, VBA modülleri, olay yordamları ve aktarım/yazdırma makroları.

## 2. İş akışının kaynakta görülen modeli

1. İhale ve hat verileri girilir.
2. Anasayfa üzerinde ödeme dönemi/ayı seçilir; gün sayıları girilir.
3. Hesaplama tablosu her hat için ayrı satırda hesap yapar.
4. Hakediş raporu ve diğer resmi belge sayfaları, seçili hat ve ay verilerine başvurur.
5. Aktarım makroları ilgili ay sütununa **her hat satırının** tutarlarını yazar. Özel Eğitim kitabında taşıma ve rehber fiyat farkları ile kesintiler ayrıca ayrı aylık bloklarda saklanır.
6. İcmal, aylık tutarları ve önceki dönem kesintilerini birleştirir.
7. Yazdırma makrosu dört belgeyi ayrı ayrı basar.

Kaynak Excel tek bir sözleşme dönemini tek bir toplu hakediş olarak hesaplamaz. Hesaplama tablosunda her hat satırı ayrı hesaplanır; seçilen ayın sonuçları hat bazında geçmişe aktarılır.

## 3. Temel/Ortaöğretim formülleri

Excel sayfası: **Hesaplama Bilgileri**. Aşağıdaki harfler bu tablonun sütunlarını gösterir.

| Hücre/sütun | Formül | Anlam |
|---|---|---|
| K | `(G × H) + I − J` | Taşıma hakediş tutarı: iş günü × günlük fiyat + KDV hariç fiyat farkı − iskonto |
| L | `IF(E = "Hayır", K × 10%, 0)` | Muaf değilse taşıma KDV'si |
| M | `L ÷ 2` | KDV tevkifatı; KDV'nin yarısı (5/10) |
| N | `K + M` | Tevkifat sonrası fatura tutarı |
| O | `K + L` | KDV dahil brüt taşıma tutarı |
| T | `I × 6%` | Fiyat farkı teminatı / ek kesin teminat kesintisi |
| U | `K × 9,48 ÷ 1000` | Damga vergisi |
| V | `P + Q + R + S + T + U + M` | Kesintilerin toplamı; vergi borcu + denetim cezası + SGK borcu + asgari ücret desteği + ek kesin teminat + damga vergisi + KDV tevkifatı |
| W | `O − V` | Yükleniciye ödenecek net tutar |

**Muafiyet koşulu:** Kaynak formüllerde `E = "Hayır"` olduğunda KDV hesaplanır. `E = "Evet"` olduğunda KDV ve buna bağlı tevkifat sıfır/boş olur.

**Önemli kaynak bulgusu:** Temel/Ortaöğretim kitabında rehber personel hesabı yoktur; ancak **Asgari Ücret Destek Tutarı (S sütunu) kesintiler toplamına dahildir**. Uygulama bunu Temel/Ortaöğretim programında sıfırlamamalıdır.

## 4. Özel Eğitim formülleri

Excel sayfası: **Hesaplama**. Taşıma sütunları Temel/Ortaöğretim kitabındaki hesapla aynı mantıktadır; ayrıca rehber personel hesaplanır.

### 4.1 Taşıma

| Sütun | Formül | Anlam |
|---|---|---|
| K | `(G × H) + I − J` | Taşıma hizmet tutarı + fiyat farkı − iskonto |
| L | `IF(E = "Hayır", K × 10%, 0)` | Taşıma KDV'si |
| M | `L ÷ 2` | Taşıma KDV tevkifatı (5/10) |
| N | `K + M` | Taşıma fatura tutarı |
| O | `K + L` | Taşıma brüt tutarı |

### 4.2 Rehber personel

| Sütun | Formül | Anlam |
|---|---|---|
| T | `(P × Q) + R − S` | Rehber iş günü × rehber günlük fiyat + rehber fiyat farkı − rehber iskonto |
| U | `IF(E = "Hayır", T × 20%, 0)` | Rehber KDV'si |
| V | `IF(E = "Hayır", U × 9/10, 0)` | Rehber KDV tevkifatı (9/10) |
| W | `T + V` | Tevkifat sonrası rehber fatura tutarı |
| X | `T + U` | KDV dahil rehber brüt tutarı |

### 4.3 Ortak kesintiler ve net ödeme

| Sütun | Formül | Anlam |
|---|---|---|
| AC | `(I × 6%) + (R × 6%)` | Taşıma ve rehber fiyat farkı tutarlarının ayrı ayrı %6'sı |
| AD | `(K × 9,48 ÷ 1000) + (T × 9,48 ÷ 1000)` | Taşıma ve rehber tutarları üzerinden damga vergisi |
| AE | `M + V + Y + Z + AA + AB + AC + AD` | Tevkifatlar + vergi borcu + denetim cezası + SGK borcu + asgari ücret desteği + ek kesin teminat + damga vergisi |
| AF | `O + X` | Taşıma ve rehber brüt tutarlarının toplamı |
| AG | `AF − AE` | Ele geçen/net ödeme |

**Kritik uyum notu:** Kaynak Excel AC formülü fiyat farkı alanlarını doğrudan kullanır; formülde `MAX(0, ...)` yoktur. Uygulamadaki `Math.Max(0, taşıma farkı + rehber farkı) × %6` ifadesi kaynak formülle birebir aynı değildir. Kodun Excel formülünü yansıtması ile bu formülün mevzuat açısından uzman tarafından onaylanması ayrı konulardır. Formülü sessizce değiştirmek yerine bu fark ayrıca raporlanmalı ve uzman onayına sunulmalıdır.

## 5. Resmî rapor formüllerinden çıkan kurallar

- Hakediş raporunda sözleşme fiyatlarıyla yapılan iş ve fiyat farkı ayrı gösterilir.
- Tahakkuk tutarı, rapordaki iş tutarı ile KDV toplamından oluşur.
- Kesinti kalemleri ayrı ayrı listelenir; net ödeme yalnızca tek bir “toplam tutar” değildir.
- Her iki kitapta net ödeme hücresi `ROUND(D13, 2) − ROUND(D26, 2)` mantığını kullanır: tahakkuk ve kesinti toplamları ayrı ayrı iki ondalığa yuvarlanır, sonra çıkarılır.
- Özel Eğitim kitabında taşıma ve rehber KDV/tevkifatları ayrı hesaplanır; tek bir ortak KDV oranı kullanmak doğru değildir.
- Dört çıktı ayrı kalır: Hakediş Raporu, Hizmet İşleri Hakediş Raporu, Hizmet İşleri Kabul Tutanağı ve Hakediş İcmali.

## 6. Aylık tarih ve geçmiş kaydı

- Temel/Ortaöğretim kitabında seçilen ödeme dönemi `Anasayfa!E4`; Özel Eğitim kitabında `Anasayfa!E5` alanıdır.
- Aylık hizmet günü girdileri hesaplamaya doğrudan girer; Özel Eğitimde taşıma günü ve rehber personel günü ayrı girdilerdir.
- `İcmal Verileri` sayfası aylık değerleri hat bazında tutar. Temel/Ortaöğretim kitabında aylık hakediş tutarı, fiyat farkı ve kesinti blokları ayrıdır. Özel Eğitim kitabında bunlara ek olarak rehber fiyat farkı bloğu vardır.
- VBA aktarım yordamları, hesaplama tablosundaki her hat satırını bulup seçilen ayın ilgili sütununa yazar. Özel Eğitim aktarımında aylık hakediş toplamı `K + T` olarak aktarılır; taşıma fiyat farkı `I`, rehber fiyat farkı `R`, kesinti toplamı `AE` kaynak alınır.
- Kaynakta hat numarası geçmiş satırını bulmak için kullanılır. OEHS ise program + İKN/sözleşme + firma + hat kimliğini kullanmalı; aynı hat numarasının iki programda veya iki sözleşmede çakışmasına izin verilmemelidir.

## 7. Kullanıcı tarafından açıklanan yeni ana iş kuralı

**Bir hakediş, sözleşmenin tüm süresi değildir; seçilmiş bir aya ve seçilmiş hat kümesine aittir.**

Örnek kayıt başlığı:

`Özel Eğitim | İKN 2026-454 | A Firması | Q1+Q3 | Ekim 2026`

İş kuralları:

1. Kullanıcı programı seçer, sonra o programın sözleşmesini/İKN'sini seçer.
2. Seçilen sözleşmenin firması ve sözleşmeye ait hatları gösterilir.
3. Kullanıcı hakediş ayını seçer; her ay ayrı kayıt oluşturulur.
4. Kullanıcı bir veya daha fazla hattı seçebilir. Örneğin Eylül'de Q1; Ekim'de Q1+Q3; başka bir ayda Q1+Q2+Q3.
5. Hesaplama seçilen her hat için ayrı yapılır; seçilen hatların toplamı ayrı bir toplam olarak gösterilir. PDF ve arşivde satır bazlı ayrıntı kaybolmaz.
6. Her satırın iş günü, fiyat farkı, iskonto, KDV/tevkifat, kesintiler ve net ödemesi ayrı saklanır. Özel Eğitimde taşıma ve rehber personel kalemleri ayrı tutulur.
7. Kayıt başlığı ve arama anahtarı en az `Taşıma Türü + İKN/Sözleşme + Firma + sıralı Hat Kimlikleri + Yıl/Ay` bileşiminden oluşur.
8. Aynı sözleşme, aynı hat ve aynı ay için mükerrer ödeme engellenmeli; mevcut kaydın düzenlenmesi yeni bir ödeme gibi ikinci kez geçmişe yazılmamalıdır.
9. Arşiv, düzeltme, geri yükleme, aylık geçmiş ve icmal işlemleri bu kimliğin tüm bileşenlerini korumalıdır.

### Hakediş numarası ayrımı

Kaynak Excel'deki `F` sütunu `COUNTIF` ile ilgili hattın aylık geçmişinde pozitif tutar bulunan dönemleri sayar. Bu, sözleşmenin takvimsel dönem sıra numarasıyla aynı kavram değildir. Çoklu hatlı tek bir OEHS kaydında her hattın geçmiş sayısı farklı olabileceğinden:
- iç kayıt kimliği / kayıt başlığı,
- kayıt grubunun kullanıcıya görünen aylık numarası,
- resmî PDF'de hat bazında gösterilen hakediş numarası

kavramları birbirine karıştırılmamalıdır. Kaynak Excel çoklu hatları tek bir resmî belge kaydında birleştirmediği için bu yeni birleşik belge numarasının kuralı ayrı bir tasarım kararı ve kabul testi gerektirir. Kodda bu belirsizlik gizlice varsayılmamalıdır.

## 8. VBA makro incelemesi

### Temel/Ortaöğretim kitabı
- `Workbook_Open`: Excel görünürlüğünü gizler ve giriş formunu açar.
- `LoginForm`: kullanıcı adı/parola karşılaştırmasını `Users` sayfasındaki değerlerle yapar; rolüne göre sayfaları görünür/gizli yapar.
- `Worksheet_Change`: seçilen ayı kontrol eder, son ay hakedişi olup olmadığını sorar ve gerekirse Kabul Tutanağı'ndaki süre bitiş metnini günceller.
- `HakedisTutarlariniAySutununaAktar`: hesaplama tablosundaki her hattın K sütununu seçili ayın aylık geçmiş sütununa aktarır.
- `TasimaFiyatFarkiAktar`: her hattın I sütununu fiyat farkı geçmişine aktarır.
- `KesintiToplamlariniAktar`: her hattın V sütununu kesinti geçmişine aktarır.
- `HesapKontrol`: Hakediş Raporu D8, D10 ve D11 değerlerinin sayısal olduğunu kontrol eder; `D8 − D10 = D11` olup olmadığını sınar.
- `SayfaYazdir`: dört resmi belgeyi ayrı ayrı yazdırır.
- `AyVerileriniSil`: aylık geçmiş bloklarını temizler. **Kontrol edilmesi gereken makro kusuru:** fiyat farkı temizliği `P4:Y103`, kesinti temizliği `AB4:AK103` ile sınırlıdır; Temmuz sütunları Z ve AL kapsam dışında kalır. Bu sütunlarda eski veri varsa temizleme işleminden sonra kalabilir.

### Özel Eğitim kitabı
- `Worksheet_Change`: seçilen ayı kontrol eder ve son ay hakedişi için kabul tutanağındaki metni günceller.
- `HakedişToplamlariniAktar`: her hat için `K + T` toplamını ayın D:N sütunlarına aktarır.
- `FiyatFarkıVerileriniAktar`: taşıma fiyat farkı I sütununu P:Z bloğuna aktarır.
- `RehberPersonelFiyatFarkıVerileriniAktar`: rehber fiyat farkı R sütununu AB:AL bloğuna aktarır.
- `KesintiBilgileriniAktarAktar`: kesinti toplamı AE sütununu AN:AX bloğuna aktarır.
- `SayfaYazdir`: dört resmi belgeyi ayrı ayrı yazdırır.
- `HesapKontrol`: önceki ve mevcut tutarlar üzerinden rapor kontrolü yapar.
- `AyVerileriniSil`: hakediş, taşıma fiyat farkı, rehber fiyat farkı ve kesinti aylık bloklarını temizler.

## 9. Kaynak dosyalarda görülen teknik riskler

1. Her iki kitabın `Anasayfa!AA2` hücresinde `#VALUE!` hata değeri var. Bu hücre hesaplama/arayüz açısından incelenmeli.
2. Özel Eğitim kitabında eski bir OneDrive/yerel Excel dosyasına dış bağlantı kaydı bulunuyor. Güncel uygulama bu dış dosyaya bağlı olmamalı; örnek değerler dış bağlantı önbelleğinden gelmiş olabilir.
3. Özel Eğitim `İcmal Verileri!AP3` ve `AQ3` başlıklarının ikisi de `Anasayfa!I9` hücresine bağlı. Ay sırasına göre AP Kasım, AQ Aralık olmalı; mevcut formül Kasım başlığını atlayıp Aralık'ı iki kez gösteriyor. VBA hedef sütun eşlemesi ise AP'yi Kasım, AQ'yu Aralık kabul ediyor. Bu, kaynak kitabın başlık formülünde muhtemel bir kopyalama hatasıdır. OEHS ay eşlemesini açık tarih/ay anahtarıyla yapmalı ve bu hatayı taşımamalıdır.
4. Kaynak geçmişi hat numarasına göre bulur. OEHS geçmiş anahtarı yalnızca hat numarası olamaz; program/sözleşme/firma/hat/ay kimliği kullanılmalıdır.
5. Excel'deki `Hakediş No` formülü, hat bazında aylık geçmişin pozitif tutarlarını sayar; mevcut uygulamadaki sözleşme dönem sırasına bağlı numaralandırma ile aynı değildir.
6. Kaynak Excel formüllerinin bir kısmı tutarları hücrelerde değer olarak saklar; bu nedenle VBA aktarım makroları ve geçmiş hücreleri formüller kadar önemlidir.
7. Kaynak dosyalar örnek/önceden doldurulmuş veriler içeriyor. Bunlar üretim verisi olarak otomatik içe aktarılmamalıdır.

## 10. Kaynakla eşleştirme kapsamında uygulanan kod değişiklikleri

Aşağıdaki değişiklikler `feature/official-payment-document-templates` dalına gönderildi. Her değişikliğin başarılı olduğu henüz en güncel CI sonucu ile doğrulanmalıdır.

- Temel/Ortaöğretim hesaplama motorunda Asgari Ücret Desteği Excel'deki S sütunu gibi V kesinti toplamına eklenir. WPF ekranında varsayılan tutar 0'dır; kullanıcı “Uygula” kutusunu işaretlemeden tutar alanı açılmaz ve hesaplamaya sıfır aktarılır. Çoklu hat girişinde aynı kural hat bazında uygulanır. Kayıtlı pozitif tutarlar düzenleme ekranına etkin biçimde geri yüklenir; sözleşme/hat kaynaklı yeniden hesaplamalar desteği silmez.
- Ek kesin teminat kodu, kaynak Excel'in `I × %6 + R × %6` formülünü doğrudan uygular; negatif toplamı `MAX(0,...)` ile sıfırlamaz. Mevzuat onayı ayrı açık konu olarak tutulur.
- Hakediş raporu A/B ayrımı düzeltildi. Excel D7 yalnızca mevcut ayın ham I+R fiyat farkını gösterir; D6 ise önceki ve mevcut aylık K+T toplamından mevcut ayın I+R tutarı çıkarılarak hesaplanır. D8 = D6 + D7 olur. Rapor PDF testi bu hücre zincirini kontrol eder.
- Tek hat ekranında taşıma ve rehber iş günü alanları artık seçilen aya ait kullanıcı girdisidir. Sözleşmedeki hat tanımının yıllık/sözleşmesel gün sayısı aylık iş günü yerine kullanılmaz. Günlük fiyatlar hat/sözleşme tanımından okunur.
- Çoklu hat seçimi için ayrı satır bazlı giriş tablosu eklendi. Her hat için iş günü, iskonto, fiyat farkı ve kesintiler ayrı hesaplanır; toplam, satır sonuçlarının toplamıdır. Tek bir hat için yazılan parasal değerler diğer seçili hatlara kopyalanmaz.
- Her aylık kayıt için ortak `BatchId`, `BatchDisplayName` ve sıralı `BatchRouteNumbers` metadata alanları eklendi. Görünen ad; program, İKN, firma, seçilen hat kümesi ve ay/yıl bilgisini içerir.
- Hakediş numarası, Excel'in hat bazlı geçmiş sayımına uygun olarak aynı program/sözleşme/firma/hat için seçilen aydan önceki pozitif aylık K+T kayıtlarını sayıp bir ekler. Çoklu kayıtta hatların numaraları farklı olabilir; arayüz bunu hat bazında gösterir.
- Aynı sözleşme/hat/ay için ikinci ödeme kaydı hâlâ reddedilir; bu, Excel'deki hat başına tek aylık hücre yapısını korur.
- Arşivleme artık çoklu kayıtları ortak BatchId ile gruplar; her hat satırı ve kendi hakediş numarası ayrı kalır. Arşiv adlarında program, seçili hat kümesi ve ay/yıl bulunur.
- PDF çıktı klasörleri program ve aylık hat kümesi/ay bazında ayrılır; aynı ihale/hat/firma adı iki programda kullanılsa bile çıktı yolları çakışmaz.
- Ay değiştirildiğinde önceki aya ait iş günü ve parasal girdiler sıfırlanır; yeni ay için yanlışlıkla önceki ay tutarları kullanılmaz.

## 11. Güncel doğrulama ve sıradaki aşamalar

### Aşama 1 — Formül eşitliği
- [x] Temel/Ortaöğretim Asgari Ücret Desteği kaynak formülle eşleştirildi.
- [x] Ek kesin teminat formülü kaynak Excel'le eşleştirildi.
- [x] Rapor D6/D7/D8/D10/D11 aylık formül zinciri kod ve testlerde eşleştirildi.
- [ ] En son commit'teki tüm CI/test/PDF smoke sonucu başarılı olarak doğrulanmalı.
- [ ] Gerçek örnek kayıtlarla Excel ara hücreleri ve PDF değerleri uzman tarafından karşılaştırılmalı.

### Aşama 2 — Aylık/çoklu hat veri modeli
- [x] Aylık batch kimliği ve görünen bileşik kayıt adı eklendi.
- [x] Her hat için aylık iş günü ve ayrı parasal giriş tablosu eklendi.
- [x] Hakediş numarası hat bazlı geçmişe bağlandı.
- [ ] Çoklu kayıt düzenleme/geri yükleme akışı, batch içindeki bütün hatların tutarlılığını koruyacak biçimde ayrıca kabul testinden geçirilmeli.
- [ ] Aynı hat/sözleşme/ay için mükerrer ödeme engeli; birleştirme, düzenleme ve arşivden geri yükleme senaryolarıyla test edilmeli.

### Aşama 3 — Resmî belgeler ve kabul
- [x] Çıktı klasörleri ve dosya adları program + aylık hat kümesi + ay/yıl ile ayrıştırıldı.
- [ ] Her seçili hat için dört resmî PDF'nin üretilmesi, doğru batch adı ve doğru hat numarasıyla kaydedilmesi test edilmeli.
- [ ] Dört belgenin her biri resmî XLSM hücreleriyle karşılaştırılmalı.
- [ ] Bir hatlı ve çok hatlı senaryolar; iki program, KDV muaf/KDV'li, negatif/pozitif fiyat farkı, iskonto ve tüm kesinti kalemleriyle doğrulanmalı.
- [ ] Gerçek Windows kullanım ve görsel kabulünden önce sürüm/EXE yayımlanmamalı.

## 12. Uzmanla onaylanacak açık noktalar

- Çoklu hatlı tek PDF'de resmî hakediş numarası: her hat için ayrı Excel tarzı sıra mı gösterilecek, yoksa birleştirilmiş kayıt için yeni bir sıra mı tanımlanacak?
- Ek kesin teminatın negatif fiyat farkında Excel formülündeki gibi işaretli hesaplanması ile mevzuat uygulaması arasındaki fark.
- Resmî rapor toplamlarında her ara kalemde mi, yoksa yalnızca Excel'de açıkça ROUND uygulanan hücrelerde mi yuvarlama yapılacağı.
- Kaynak Excel'in ay başlığı hataları ve dış bağlantı kayıtlarının uzman tarafından teyidi.

**Güvenlik ilkesi:** Excel'deki kullanıcı/parola bilgileri, kişisel kimlik/banka verileri veya önceden doldurulmuş yüklenici kayıtları Git deposuna kopyalanmayacaktır.

## 12. Son kod incelemesinde bulunan ek uyumsuzluklar (16.10.2026)

- **Rapor geçmişi kapsamı:** Önceki aylık tutarlar toplanırken yalnızca dönem ve mali yıl filtresi vardı. Bu, farklı hat/program/sözleşme/yüklenicinin aynı rapora karışmasına neden olabilirdi. Önceki kayıtlar artık program + İKN + sözleşme + yüklenici kimliği + hat anahtarıyla eşleştiriliyor; buna karşı regresyon testi eklendi.
- **Asgari Ücret Desteği giriş alanı:** Hesaplama motoru tutarı doğru kesinti toplamına ekliyordu; ancak WPF ekranında destek yalnızca rehber alanlarıyla birlikte okunuyordu. Bu yüzden Temel/Ortaöğretim'de rehber personel bulunmadığında kullanıcı girişi hesaplamaya ulaşmıyordu. Giriş ayrıştırması rehber kontrolünden çıkarıldı; Temel/Ortaöğretim için varsayılan 0 TL + onay kutusu, çoklu hat satırları için ayrı etkinleştirme kutusu eklendi.
- **Aylık batch kimliği:** Çoklu hat kaydı ortak BatchId ile gruplanır; görünen isim program, İKN, sözleşme, firma, seçili hat kümesi ve ay/yıl bilgisini içerir. Her hattın Excel uyumlu hakediş numarası ve hesaplama satırı ayrı saklanır.
- **Son doğrulama durumu:** Bu ek düzeltmeler için GitHub Actions yeniden çalışıyor. En son commit'in test sonucu tamamlanmadan bu düzeltmeler başarılı doğrulandı diye işaretlenmemeli. Resmî Excel dosyalarıyla gerçek örnek değer karşılaştırması ve Windows WPF kabul testi hâlâ zorunlu.


## 13. 16.10.2026 — hücre düzeyinde formül örüntüsü denetimi

Yüklenen iki kaynak dosyanın SHA-256 değerleri bu belgedeki kaynak kimlikleriyle yeniden eşleşti. Temel hesaplama sayfalarındaki formüller, satır 4–103 boyunca sütun bazında karşılaştırıldı.

### Özel Eğitim kitabında satır bazlı formül sapmaları

- `Hesaplama!K4:K6` formülü `=(G*H)+I-J`; fakat `K7:K103` hücrelerinde formül `=(G*H)+I` biçimine dönüyor. Bu, **97 satırda J sütunundaki taşıma iskontosunun formülde çıkarılmadığı** anlamına gelir.
- `Hesaplama!T4:T102` formülü `=(P*Q)+R-S`; fakat `T103` hücresinde `=(P*Q)+R` kullanılır. Son satırda S sütunundaki rehber iskontosu formülde çıkarılmaz.
- `AD4:AD6` ile `AD7:AD103` arasında parantez yazımı farklıdır; bu fark cebirsel olarak eşdeğerdir ve tek başına hesap farkı üretmez.
- Temel/Ortaöğretim `Hesaplama Bilgileri` sayfasındaki ana hesaplama sütunları satır 4–103 boyunca aynı formül örüntüsünü korur.

### OEHS için karar

Bu sapmalar, Excel'in amaçlanan formül tanımı olan `K=(G×H)+I−J` ve `T=(P×Q)+R−S` ile çelişir. OEHS hesaplama motoru tüm hatlarda bu standart formülleri uygular; kaynak kitabın yanlış kopyalanmış 97+1 satır formülünü uygulamaya taşımamalıdır. `ExcelDiscountIsSubtractedFromPriceDifference` regresyon testi taşıma ve rehber iskontosunun ilgili hizmet tutarlarından çıkarıldığını doğrular.

### Ek teknik bulgular

- Her iki dosyada `Anasayfa!AA2` değeri `#VALUE!` türünde hata hücresi olarak saklanmıştır; hücrede çalıştırılabilir bir formül yoktur. Bu alanın işlevi Excel masaüstünde doğrulanmadan OEHS hesabına bağlanmamalıdır.
- Özel Eğitim kitabında `İcmal Verileri!AP3` ve `AQ3` aynı `Anasayfa!I9` hücresine bağlıdır; böylece ay başlıkları Kasım'ı atlayıp Aralık'ı iki kez gösterir. VBA'nın hedef sütun eşlemesi AP'yi Kasım, AQ'yu Aralık saydığından uygulama tarih/ay anahtarıyla eşleştirmeye devam etmelidir.
- Özel Eğitim dosyasında eski bir dış çalışma kitabı bağlantısı vardır. Uygulama bu dış bağlantıya bağımlı olmamalıdır.
- Temel Eğitim `Hakediş İcmali!E3` ve `E4` formülleri aramayı `Hesaplama Bilgileri!A3:O52` aralığıyla sınırlar; buna karşılık hesaplama tablosu 100 satırlık giriş alanına sahiptir. 52. satırdan sonraki hatlarda başlıkta hakediş/hat bilgisi bulunamama riski vardır.
- Temel Eğitim `Hizmet İşleri Hakediş Raporu!F16` formülü `İhale Verileri!A3:L25` aralığını kullanırken aynı belgedeki `F17` formülü `A2:L102` kullanır. F16'daki daha kısa aralık, 25. satırdan sonraki kayıtlar için olası bir arama sınır hatasıdır.
- Temel Eğitim `Hakediş Raporu!D10` formülü `VLOOKUP(...,15,)` biçiminde eşleşme türünü boş bırakarak yaklaşık eşleşmeye düşer; `Hakediş İcmali!E22` ise açıkça `TRUE` kullanır. Özel Eğitim'de başlık amaçlı `A31`, `Hizmet İşleri Hakediş Raporu!F6`, `Hizmet İşleri K. Tutanağı!B2` ve `Hakediş İcmali!A2` formüllerinde de yaklaşık eşleşme görülür. Uygulama, seçili kaydı program + İKN + sözleşme + yüklenici + hat kimliğiyle kesin eşleştirmeli; bu yaklaşık eşleşme davranışını kopyalamamalıdır.
- Kaynak VBA özetindeki `AyVerileriniSil` yordamı seçili ayla sınırlı kalmadan bütün aylık aktarım bloklarını temizler ve önceden onay istemez. OEHS bu davranışı kopyalamamalı; seçili döneme sınırlı ve geri alınabilir işlem kullanmalıdır.

- Özel Eğitim `Hesaplama!H104:AG104` toplam satırı 100 hatlık giriş alanının tamamı yerine yalnızca 23 hatlık `4:26` aralığını topluyor. Bu, toplam satırındaki taşıma/rehber hizmet tutarları, fiyat farkları, KDV/tevkifat ve kesintilerin 27–103. satırlardaki hatları dışarıda bırakmasına yol açar. OEHS raporları seçilen hat kayıtlarından hesaplandığı ve çoklu hat toplamını yalnızca seçili hatlardan topladığı için bu hatalı Excel toplam satırını kopyalamamalıdır. Otomatik formül taramasında `H104:AG104` boyunca aynı kısa aralık doğrulandı.
- Özel Eğitim `İcmal Verileri!O4:O103` önceki dönem toplamı formülünün dış koşulundaki `MATCH` aralığı `Anasayfa!I6:I14` (9 ay), içteki `MATCH` aralığı ise `I6:I15` (10 ay) olarak farklıdır. Bu nedenle seçili dönem Haziran olduğunda dış `MATCH` hata verir ve `IFERROR` önceki toplamı boş döndürebilir. OEHS, raporun kümülatif tutarlarını `ProgressPaymentReportService.Calculate` içindeki tarih/dönem anahtarıyla hesaplamalı; kaynak aralık hatasını kopyalamamalıdır. `ProgressPaymentIcmalServiceTests.MonthLookupRange_ShouldIncludeSeptemberThroughJuneButNotJulyPlaceholder` Haziran'ı geçerli, Temmuz'u yer tutucu olarak doğrular; `FindPreviousDeduction_ShouldCarryMayDeductionIntoJune` ise Haziran'da önceki ay kesintisinin taşınmasını ayrı olarak doğrular.
- Temel Eğitim `Hakediş İcmali!E21` önceki kesintiyi `İcmal Verileri!AN3:AX3` başlıklarında arar; ancak Temel Eğitim `İcmal Verileri` sayfası AM sütununda biter ve aylık kesintiler AB:AL, toplam AM sütunundadır. Bu yüzden E21'deki ay eşleşmesi başarısız olur ve `IFERROR(...,0)` önceki kesintiyi sıfır gösterir. OEHS'nin tarih ve kimlikle önceki aya ait gerçek kesintiyi bulması korunmalı; bu kaynak kusurunu kopyalamamak için Temel/Ortaöğretim regresyon testi eklendi.
- Temel Eğitim `Hakediş Raporu!D15` damga vergisini `IF(D11<0,0,D11*9,48/1000)` ile sınırlar; hesaplama sayfasındaki `U` hücresi ise işaretli `K*9,48/1000` sonucunu korur. Bu nedenle negatif K tutarında resmî Hakediş Raporu'nun kesinti toplamı, hesaplama sayfasındaki V toplamından farklıdır. OEHS'de yalnızca Hakediş Raporu çıktısı için rapor-damga-vergisi ve rapor-kesinti toplamı yardımcıları eklendi; Hakediş İcmali kaynak Excel'deki V toplamını kullanmaya devam eder. Negatif K senaryosu için regresyon testi eklendi.

### Uygulama doğrulaması

- Temel/Ortaöğretim asgari ücret desteğinin varsayılan 0 TL + kullanıcı onayı davranışı WPF tek-hat ve çoklu-hat girişlerine eklendi.
- Sözleşme ve hat değişimlerinde destek tutarını sıfırlayan iki yeniden-hesaplama yolu düzeltildi; bu davranış için iki regresyon testi eklendi.
- En son kod commit'inin CI sonucu tamamlanmadan bu değişiklikler için son doğrulama başarılı sayılmamalıdır. Gerçek Windows Excel ve WPF kabul testleri ayrıca gereklidir.


## 14. Kullanıcı tarafından teyit edilen Excel formül niyeti (16.10.2026)

Kullanıcı, Özel Eğitim kitabındaki aşağı kopyalanmamış formüllerle ilgili denetim bulgusunu netleştirdi: **K4 satırındaki formül ve hesaplama mantığı doğrudur; aşağı satırlara formüllerin kopyalanmaması Excel çalışma kitabındaki bir doldurma/aktarımı eksikliğidir.** Bu nedenle önceki bölümde listelenen K7:K103 ve T103 farklılıkları, OEHS'nin hesaplama kuralını değiştirme gerekçesi değildir.

OEHS'nin standart hesaplama kuralı kullanıcı teyidine göre korunur:
- Taşıma: `K=(G×H)+I−J`
- Rehber: `T=(P×Q)+R−S`

Bu formüller tüm ilgili hatlarda uygulanır; Excel'de eksik kopyalanmış formüller uygulamaya taşınmaz. Bu bulgu, hesaplama kuralı bakımından artık kullanıcı tarafından teyit edilmiş kabul edilir. Diğer kaynak Excel aralıkları, ay başlıkları, yaklaşık eşleşmeler ve VBA davranışları hakkındaki bulgular ayrı inceleme konuları olmaya devam eder.


## 15. Sayısal referans vektörleri — kaynak XLSM ön doldurulmuş örnekleri (16.10.2026)

Aşağıdaki sayılar yüklenen çalışma kitaplarının `data_only` görünümünden, 4. satırdaki örnek kayıttan alınmıştır. Bunlar **kaynak Excel'in saklanmış sonuç değerleridir**; LibreOffice veya masaüstü Excel'de yeniden hesaplanmış, OEHS ile karşılaştırılmış sonuç olarak sunulmamaktadır. İlk aşama referans vektörleri olarak tutulur; otomatik testte aynı girdiler hesaplama motoruna aktarılıp toleranslı sayısal karşılaştırma yapılmalıdır.

### Temel/Ortaöğretim — `Hesaplama Bilgileri`, satır 4

Girdi alanları:
- İş günü `G4 = 9.82693033060048`
- Günlük taşıma fiyatı `H4 = 19539.1608`
- KDV hariç fiyat farkı `I4 = 7365.35`
- İskonto `J4` boş/0 olarak okunuyor
- KDV muafiyeti `E4 = "Hayır"`

Kaynak kitapta saklanan ara/sonuç değerleri:
- Hakediş tutarı `K4 = 199375.32189999995` (formül: `G×H + I − J`)
- KDV `L4 = 19937.532189999994`
- Tevkifat `M4 = 9968.766094999997`
- Fatura tutarı `N4 = 209344.08799499995`
- Brüt tutar `O4 = 219312.85408999995`
- Ek kesin teminat `T4 = 441.921`
- Damga vergisi `U4 = 1890.0780516119996`
- Toplam kesinti `V4 = 12300.765146611997`
- Net tutar `W4 = 207012.08894338796`

### Özel Eğitim — `Hesaplama`, satır 4

Girdi alanları:
- Taşıma iş günü `G4 = 22`
- Günlük taşıma fiyatı `H4 = 2320`
- Taşıma fiyat farkı `I4 = 1403.573`
- Taşıma iskontosu `J4 = 0`
- Rehber iş günü `P4 = 30`
- Rehber günlük fiyat `Q4 = 1362.49`
- Rehber fiyat farkı `R4` boş/0 olarak okunuyor
- Rehber iskontosu `S4 = 0`
- KDV muafiyeti `E4 = "Evet"`

Kaynak kitapta saklanan ara/sonuç değerleri:
- Taşıma hakedişi `K4 = 52443.573`
- Rehber hakedişi `T4 = 40874.7`
- Ek kesin teminat `AC4 = 84.21438`
- Damga vergisi `AD4 = 884.6572280400001`
- Toplam kesinti `AE4 = 968.8716080400001`
- Brüt tutar `AF4 = 93318.27299999999`
- Net tutar `AG4 = 92349.40139195998`

### Karşılaştırma protokolü ve kalan iş

1. Aynı girdi alanlarını uygulamanın gerçek hesaplama servisine veren test/fixture oluştur.
2. Excel hücreleriyle karşılaştırılacak alanları açık eşleştir; boş hücreleri sıfır varsaymadan önce formülün boş hücre davranışını kontrol et.
3. Para hesaplarında Excel'in hücre bazlı `ROUND` kullandığı yerleri aynen modelle; yalnızca görüntüleme katmanındaki biçimlendirmeyi hesaplama yuvarlaması sayma.
4. Başlangıçta saklanmış örnek sonuçları karşılaştır; sonra Excel'de yeniden hesaplatılmış kopyada aynı girdileri kullanarak ikinci kez doğrula. Kaynakta saklanan değerlerin eski olma ihtimali nedeniyle iki kanıt türünü ayrı tut.
5. Bu belgeye girilen sayısal vektörler henüz OEHS ile test edilmiş değildir. Programın aynı sonuçları verdiği ancak testler çalıştırılıp sonuçlar incelendikten sonra işaretlenebilir.


## 16. Sayısal örneklerin bağımsız aritmetik kontrolü (16.10.2026)

Bölüm 15'teki iki örneğin temel formülleri kaynak dosyadan bağımsız olarak yüksek hassasiyetli ondalık aritmetik ile yeniden hesaplandı. Bu kontrol, yalnızca belirtilen girdiler ve yazılı formüller için matematiksel çapraz kontroldür; **OEHS uygulama servisi çalıştırılmadı ve uygulama testi yerine geçmez.**

### Temel/Ortaöğretim — satır 4

- `K = G×H + I − J` → 199375.321899999939...
- `L = K×10%` → 19937.532189999993...
- `M = L/2` → 9968.766094999996...
- `N = K+M` → 209344.087994999936...
- `O = K+L` → 219312.854089999933...
- `T = I×6%` → 441.921
- `U = K×9.48/1000` → 1890.078051611999...
- `V = T+U+M` (bu örnekte diğer kesintiler sıfır) → 12300.765146611996...
- `W = O−V` → 207012.088943387936...

Kaynak Excel'in saklanan hücre sonuçları bu aritmetik sonuçlarla kayan nokta hassasiyeti düzeyinde uyuşuyor.

### Özel Eğitim — satır 4

- `K = G×H + I − J` → 52443.573
- `T = P×Q + R − S` → 40874.70
- `AC = 6%×I + 6%×R` → 84.21438
- `AD = (K+T)×9.48/1000` → 884.65722804
- `AE = AC+AD` (bu örnekte diğer kesintiler sıfır) → 968.87160804
- `AF = K+T` → 93318.273
- `AG = AF−AE` → 92349.40139196

Kaynak Excel'in saklanan hücre sonuçları bu aritmetik sonuçlarla uyuşuyor.

### Teste geçişte dikkat edilecek konu

Excel dosyasında `ROUND` veya hücre biçimlendirmesiyle ilişkili yuvarlama davranışları olan başka alanlar bulunuyor. Bu iki örnek, ara hesaplamaların ham hassasiyetini doğrular; resmî PDF'de iki ondalığa yuvarlanmış değerlerin karşılaştırması ayrı test edilmelidir. Uygulama servisinden sonuç alınmadan “OEHS ile eşleşti” durumu verilmemelidir.


## 17. Kritik hesaplama farkı için karar kaydı — ek kesin teminat tabanı

Kaynak formül ile önceki kod incelemesi arasında önemli bir fark kayıtlıdır:
- Kaynak Özel Eğitim Excel'i: `AC=(I×6%)+(R×6%)`
- Önceki kod incelemesinde görülen uygulama yaklaşımı: `Math.Max(0, taşıma fiyat farkı + rehber fiyat farkı)×6%`

Bu iki ifade, fiyat farklarından biri negatif olduğunda aynı sonucu vermeyebilir. Ayrıca kaynak formül, iki kalemin ayrı ayrı yüzde altısını alır; uygulama yaklaşımı ise önce farkları toplar ve toplamı sıfırın altına düşürmez. Bu nedenle konu yalnızca yazım farkı değildir; net hakedişi etkileyebilir.

**Karar:** Kaynak Excel formülünü mevzuat/ihale uzmanı teyidi olmadan uygulamaya sessizce taşımak veya mevcut davranışı sessizce değiştirmek doğru değildir. Test matrisi şu üç senaryoyu ayrı kapsamalıdır:
1. İki fiyat farkı da pozitif.
2. Taşıma veya rehber fiyat farkından biri negatif, diğeri pozitif.
3. İki fiyat farkı da negatif.

Her senaryoda Excel AC, uygulama ek kesin teminatı ve net ödeme ayrı ayrı raporlanmalı; nihai iş kuralı onaylanana kadar fark görünür tutulmalıdır. Pozitif örnekte sonuçlar eşleşebilir; negatif örnekler ayırt edici testlerdir.

### CI durumu notu

Bu belge güncellemesi için sorgulanan commit üzerinde GitHub birleşik durum API'si herhangi bir durum kontrolü döndürmedi. Bu, testlerin başarılı olduğu anlamına gelmez; doğrulanabilir CI sonucu mevcut değildir. Windows derlemesi ve testler başarıyla tamamlanmadan üretim kabulü verilmemelidir.


## 18. Sonraki otomatik kabul testi için ayırt edici senaryo matrisi

Bu matris, kaynak formüllerinin yalnızca olağan pozitif örnekte değil, davranış farkı ortaya çıkaran sınır durumlarında da kontrol edilmesi için hazırlanmıştır. Henüz uygulama test sonucu değildir.

| Senaryo | Girdi/koşul | Beklenen doğrulama |
|---|---|---|
| Normal Temel Eğitim | Pozitif gün, fiyat, fiyat farkı; KDV dahil | K, L, M, O, T, U, V, W zinciri Excel formül kurallarıyla uyuşmalı |
| Temel Eğitim desteği | Rehber alanı yok; asgari ücret desteği pozitif | Destek V toplamına dahil olmalı; rehber alanı olmaması desteği sıfırlamamalı |
| KDV muaf Temel Eğitim | E = Evet | KDV ve KDV tevkifatı sıfır olmalı |
| İskontolu Özel Eğitim taşıma | J > 0 | K = G×H + I − J; iskonto net taşıma hesabından düşmeli |
| İskontolu rehber | S > 0 | T = P×Q + R − S; rehber iskontosu taşıma tutarını etkilememeli |
| Özel Eğitim KDV dahil | E = Hayır | Taşıma KDV %10, taşıma tevkifatı %50 KDV; rehber KDV %20, rehber tevkifatı %90 KDV |
| Negatif fiyat farkı | I < 0 ve/veya R < 0 | AC formülü ile uygulamanın ek teminat davranışı ayrı karşılaştırılmalı; varsayımla eşit sayılmamalı |
| Negatif rapor fiyat farkı | Rapor D11 < 0 | Kaynak rapor damga vergisi kuralı, hesaplama sayfasındaki işaretli tutardan ayrı doğrulanmalı |
| Önceki dönem kesintisi | Mayıs kesintisi > 0, Haziran hakedişi var | Haziran icmalinde önceki kesinti tarih/ay anahtarıyla taşınmalı; ay indeksiyle yanlış eşleşmemeli |
| Çok hatlı toplam | Hat sayısı 26'dan fazla | Kaynak Excel'in kısa toplam aralığına bağlı kalmadan seçili tüm hatlar dahil edilmeli |
| Ay sınırı | Temmuz ve Kasım/Aralık kayıtları | Ay anahtarı benzersiz olmalı; boş başlık/tekrarlı başlık geçmişi yanlış aya yazmamalı |
| Yuvarlama | Kuruş sınırında tahakkuk ve kesinti | Tahakkuk ve kesinti ayrı ayrı iki haneye yuvarlanıp netleştirilmeli; ham kayan nokta sonucu PDF değeriyle karıştırılmamalı |

### Kabul kriteri

Her testte (1) girişler, (2) kaynak Excel hücre/formülü, (3) OEHS hesaplama servisi sonucu, (4) fark, (5) geçme/kalma durumu kaydedilmelidir. Sonuç yalnızca net tutarla sınırlı kalmamalı; ara değerler de karşılaştırılmalıdır. Testlerin çalıştırıldığı commit SHA ve CI koşusu rapora eklenmeden matris “geçti” olarak işaretlenmemelidir.
