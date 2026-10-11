# OEHS — Excel Referans Formülleri ve Dört PDF Şablonu

**Hazırlanma tarihi:** 11.10.2026  
**Amaç:** OEHS hesaplama ve resmî belge üretiminin, kullanıcı tarafından sağlanan makrolu Excel çalışma kitabı ile dört örnek PDF çıktısına dayalı doğrulama kaynağı. Bu belge örnek dosyalardaki kişisel/kurumsal kayıt değerlerini uygulama kaynak koduna taşımayı amaçlamaz.

## 1. Referans kaynakları

| Kaynak | Kullanım amacı | Görülen yapı |
|---|---|---|
| `Özel Eğitim Hakediş Ödeme Programı(3).xlsm` | İş kuralı, hücre formülü, aylık özet ve VBA aktarım akışı | 8 çalışma sayfası; `xl/vbaProject.bin` mevcut (72.192 bayt); bir harici bağlantı girdisi; `forceFullCalc=1` |
| `1(3).pdf` | Hakediş Raporu görsel şablonu | Tek sayfa, A4 dikey, 595 × 842 pt |
| `2(3).pdf` | Hizmet İşleri Hakediş Raporu görsel şablonu | Tek sayfa, A4 dikey, 595 × 842 pt |
| `3(3).pdf` | Hizmet İşleri Kabul Tutanağı görsel şablonu | Tek sayfa, A4 dikey, 595 × 842 pt |
| `4(3).pdf` | Hakediş İcmali görsel şablonu | Tek sayfa, A4 dikey, 595 × 842 pt |

Workbook sayfaları: `Anasayfa`, `İhale Verileri`, `Hesaplama`, `Hakediş Raporu`, `Hizmetişleri Hakediş Raporu`, `Hizmet İşleri K. Tutanağı`, `Hakediş İcmali`, `İcmal Verileri`.

Excel dosyasında hücre formülleri yanında VBA ile yazıldığı görülen değer hücreleri de vardır. Formül metni bulunmayan, makro tarafından doldurulan aylık hücrelerin davranışı sadece hücre formüllerinden çıkarılmamalı; aktarım makroları ayrıca eşleştirilmelidir. Dosyanın hesaplama ayarı tam hesaplamayı istiyor (`forceFullCalc=1`), bu nedenle XML içindeki saklanmış sonuçlar tek başına güncel ve tutarlı sonuçların kanıtı kabul edilmemelidir. `Anasayfa!W2` için saklanmış `#VALUE!` değeri ve bir harici bağlantı girdisi görüldü; Excel'de açılarak ayrıca incelenmeli.

## 2. Hesaplama sayfasındaki temel Excel formülleri

Aşağıdaki formüller workbook'un `Hesaplama` sayfasından okunmuştur. Taşıma hücreleri G:O, rehber personel hücreleri P:X, kesintiler Y:AE, brüt ve net sonuçlar AF:AG alanındadır.

| Excel hücresi | Formül / anlam | OEHS doğrulama kuralı |
|---|---|---|
| K | `(G*H)+I-J` | Taşıma hakediş tutarı |
| L | `IF(E="Hayır",K*10/100,blank)` | Muafiyet cevabı “Hayır” olduğunda taşıma KDV'si |
| M | `L/2` | Taşıma KDV tevkifatı |
| N | `K+M` | Taşıma tevkifat sonrası fatura tutarı |
| O | `K+L` | Taşıma brüt tutarı |
| T | `(P*Q)+R-S` | Rehber personel hakediş tutarı |
| U | `IF(E="Hayır",T*20/100,blank)` | Muafiyet cevabı “Hayır” olduğunda rehber KDV'si |
| V | `IF(E="Hayır",U*9/10,0)` | Rehber KDV tevkifatı |
| W | `T+V` | Rehber tevkifat sonrası fatura tutarı |
| X | `T+U` | Rehber brüt tutarı |
| AC | `(I*6/100)+R*6/100` | Ek kesin teminat: ham taşıma + rehber fiyat farklarının %6'sı |
| AD | `(K*9.48/1000)+T*9.48/1000` | Damga vergisi |
| AE | `M+V+Y+Z+AA+AB+AC+AD` | Kesinti toplamı |
| AF | `O+X` | Brüt tutar |
| AG | `AF-AE` | Hesaplama sayfasındaki ele geçen/net değeri |

**Kritik ayrım:** `I` ve `R` fiyat farkı girişleridir. Hakediş tutarı K/T hesabında J/S iskonto tutarları düşülür; AC ek teminat formülü ise ham `I` ve `R` hücrelerini kullanır. OEHS içinde hesaplamada kullanılan iskonto-düzeltilmiş fark ile resmî rapordaki ham fiyat farkı birbirinin yerine kullanılmamalıdır.

## 3. Hakediş Raporu hücre eşleştirmesi

| Excel hücresi | Excel formülü | Uygulamadaki anlamı |
|---|---|---|
| H2 | Taşıma K + rehber T (`K+T`) | Geçerli hakedişin KDV öncesi toplamı |
| D6 | `SUM(VLOOKUP(...,'İcmal Verileri'!A4:AY103,{4,5,...,14},FALSE))` | İcmal Verileri aylık hizmet tutarlarının toplamı; rapor A satırı |
| D7 | `VLOOKUP(...,Hesaplama!... ,9,FALSE)+VLOOKUP(...,Hesaplama!... ,18,FALSE)` | Ham `I+R` fiyat farkı; rapor B satırı |
| D8 | `D6+D7` | Rapor C satırı |
| D10 | `VLOOKUP(...,'İcmal Verileri'!A4:AY103,15,FALSE)` | Kaynak workbook'un önceki hizmet tutarı değeri; rapor D satırı |
| D11 | `IF(D7<0,H2-ABS(D7),H2+D7)` | Rapor E satırının gerçek formülü |
| D12 | Taşıma L + rehber U | Rapor F satırı, KDV toplamı |
| D13 | `D11+D12` | Rapor G satırı, tahakkuk tutarı |
| D15 | Hesaplama AD | Damga vergisi |
| D16 | Hesaplama M + V | KDV tevkifatı |
| D17 | Hesaplama AA | SGK kesintisi |
| D21 | Hesaplama AC | Ek kesin teminat |
| D22 | Hesaplama Y | Vergi borcu |
| D24 | Hesaplama Z | Denetim cezası |
| D25 | Hesaplama AB | Asgari ücret desteği kesintisi |
| D26 | `SUM(D14:F25)` | Kesintiler ve mahsuplar toplamı |
| D28 | `ROUND(D13,2)-ROUND(D26,2)` | Bu PDF'nin “Yükleniciye Ödenecek Tutar” değeri |

**Başlık-formül farkı korunarak test edilmelidir:** PDF/Excel'de E satırının açıklaması “C - D” şeklindeyken D11 formülü `H2 ± ABS(D7)` mantığındadır. Kaynak formül sessizce `C-D` biçimine çevrilmemeli; kullanıcı tarafından onaylanacak bir iş kuralı kararı olmadan birebir formül davranışı korunmalıdır.

**Ayrı net değerler:** `Hesaplama!AG = AF-AE` ile `Hakediş Raporu!D28 = ROUND(D13,2)-ROUND(D26,2)` farklı formüllerdir ve farklı sonuç verebilir. Resmî Hakediş Raporu PDF'sinin I satırı D28'e göre oluşturulmalı; hesaplama ekranındaki AG değeri onun yerine kullanılmamalıdır.

## 4. İcmal dönemleri ve kesintiler

`İcmal Verileri` 11 aylık çıktı düzenine sahiptir: Eylül–Haziran dönemleri ile ayrıca Temmuz sütunu. Ana sayfadaki `I6:I15` 10 aylık Eylül–Haziran adlarını sağlar; `İcmal Verileri` içinde Temmuz ayrıca tanımlıdır. Dolayısıyla PDF şablonu 11 dönem satırı içermelidir.

- `Hakediş İcmali` satırları 6–16: 11 aylık dönem satırı.
- C sütunu: aylık hizmet tutarı (fiyat farkı ayrı tutulur).
- D sütunu: ilgili ayın taşıma ve rehber personel ham fiyat farkları toplamı.
- E sütunu: `C+D`.
- E17: aylık toplamlar.
- E21: hemen önceki takvim ayındaki kesinti değeri (önceki bir ödeme bulunmazsa 0).
- E22: geçerli hakedişin kesinti toplamı.
- E23: iki kesintinin toplamı.
- E25: `E17-E23`.

**Kaynak workbook anomalileri (yayın öncesi iş kuralı kararı gerekiyor):**

1. `Anasayfa!I6:I15` yalnızca Eylül–Haziran (10 ay) içerirken `Hakediş İcmali` PDF formunda 11 satır (Eylül–Temmuz) vardır. C/D satır formülleri ay adını bu 10 hücrelik aralıkta `MATCH` ile arar; Temmuz satırının tutarları mevcut formülde `IFERROR(...,0)` nedeniyle sıfır olur. OEHS bu davranışı şimdilik birebir taklit ediyor: Temmuz satırı gösteriliyor ancak tutarları sıfır.

2. `İcmal Verileri!AN3:AX3` dönem başlıklarında `AP3` ve `AQ3` ikisi de Aralık'a bağlanıyor; Kasım başlığı eksik. `Hakediş İcmali!E21` önceki ay adını bu başlık aralığında aradığı için Aralık hakedişinin önceki Kasım kesintisi bulunamaz ve `IFERROR(...,0)` sıfır döndürür. OEHS şu anda kaynak formül davranışını testle koruyor.

Bunlar istenen “Excel formülleriyle birebir uyum” için kaynak davranışı olarak kaydedilmiştir, fakat doğru iş kuralı oldukları onaylanmış değildir. Excel'in kendisi düzeltilirse OEHS kodu ve referans testleri de aynı kararla birlikte güncellenmelidir; C# içinde sessizce tarih bazlı başka sonuç üretmemelidir.

## 5. Dört PDF'nin görsel sözleşmesi

Örneklerin tamamı tek sayfa A4 dikeydir. İçerik ve sayfa ölçüsü birinci sınıf doğrulama ölçütüdür; örneklerin içindeki ad, numara, tutar ve tarihler test fixture'ı olarak kopyalanmamalıdır.

### 5.1 `1(3).pdf` — Hakediş Raporu

- Üstte ortalanmış kalın başlık.
- Hemen altında sol tarafta uzun hat/iş açıklaması, sağda “Hakediş No” ve “Hat No” kutuları.
- Tarihe kadar yapılan iş başlığı; A/B/C satır tablosu.
- D/E/F/G satırları ve kesinti/mahsup bölümünde solda dikey birleşik başlık.
- H ve I toplam satırları gri dolgulu; örnekte E satırı kırmızı dolgu ile gösteriliyor (bunun koşullu uyarı mı yoksa sabit şablon stili mi olduğu kaynak workbook stiliyle ayrıca kontrol edilmelidir).
- Yüklenici alanı, iki imza alanı, tarih/onaylayan ve örnek numarası alt bölümü.

### 5.2 `2(3).pdf` — Hizmet İşleri Hakediş Raporu

- Ortalanmış başlık.
- Tarih, hakediş no, uygulama yılı ve işin adı; işin adı uzun olduğunda çok satıra sarılmalı.
- Proje, yüklenici, sözleşme bedeli ve tarihleri, tescil durumu, işyeri teslimi, süre, bitiş, avans satırları.
- Sözleşme bedeli/artış/ek sözleşme/toplam sözleşme tablosu.
- Ayrı “Süre Uzatım Kararlarının” tablosu.
- Alt bölümde Düzenleyen/Kontrol Eden ve onaylayan imza alanları ile örnek no.

### 5.3 `3(3).pdf` — Hizmet İşleri Kabul Tutanağı

- Ortalanmış başlık.
- Sol etiket/sağ değer çift sütunlu bilgi tablosu; uzun iş adı satırı yüksekliği içeriğe göre büyümeli.
- Sözleşme bilgisi, süre, bitiş tarihi ve süre uzatımı satırları.
- Geniş metinli kabul paragrafı.
- Ayrı yüklenici imzası; altında Düzenleyen/Kontrol Eden ve onaylayan imzaları, tarih ve unvan.

### 5.4 `4(3).pdf` — Hakediş İcmali

- Ortalanmış başlık; iş açıklaması geniş sol hücrede, sayfa/hakediş/hat kutuları sağda.
- 5 sütun: Sıra No, İşin Cinsi, İş Tutarı (a), Fiyat Farkı Tutarı (b), Toplam (a+b).
- 11 aylık satır Eylül–Temmuz; ardından gri toplam satırı ve A “TOPLAM” satırı.
- “Hakediş İçeriğinden Yapılan Kesintiler” bölümü; önceki ve bu hakedişten kesinti, B toplam kesinti, C net toplam satırları gri dolgu ile.
- Alt imza alanları ve M.Y.H.B.Y. örnek no.

## 6. DoğruTemin ile belge altyapısı karşılaştırması — güncel

DoğruTemin WPF'deki DocumentPreviewWindow/DocumentViewer, TextSharpnessService ve PrintDialog + DocumentPaginator yaklaşımı OEHS'ye taşındı. DoğruTemin'deki PdfVectorWriter, OEHS namespace'ine uyarlanarak FixedDocument/FixedPage içindeki metin, kenarlık ve temel şekilleri vektör olarak PDF'ye yazacak şekilde entegre edildi.

Dört resmi form artık OehsOfficialDocumentFactory tarafından sabit A4 FixedPage şablonları olarak oluşturuluyor. Önizleme ve yazdırma aynı birleşik dört sayfalı FixedDocument'i, “PDF OLARAK KAYDET” ise aynı şablon üreticilerinden dört ayrı tek sayfalı belgeyi kullanıyor. Eski FlowDocument rapor üreticileri ve raster PDF dışa aktarma yolu kaldırıldı.

Mevcut CI smoke testi PDF'nin oluşmasını, A4 boyutunu ve yeniden açılabilirliğini kontrol ediyor. Bu henüz referans PDF'lerle piksel eşitliği kanıtlamıyor. Son smoke dosyaları GitHub Actions artifact alanına yazılmaya çalışıldı; GitHub artifact depolama kotası dolu olduğundan yükleme gerçekleşmedi. Kota temizlenince dört PDF indirilip kaynak örneklerle aynı DPI'da render ve görsel fark karşılaştırması yapılmalı.

## 7. GitHub / GitHub Desktop durumu — 11.10.2026

- [OEHS Windows Build #1186](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/38158074721): üç proje derlemesi, 93 Core testi, 1 WPF PDF smoke testi ve Windows x64 dotnet publish başarılı; derleme hatası 0.
- Smoke PDF artifact upload adımı dosya yüklemeye ulaştı ancak GitHub “Artifact storage quota has been hit” yanıtı verdi. Bu nedenle PDF'ler test koşucusunda üretildi ama indirilebilir artifact olarak saklanmadı.
- Ana uygulama OEHS-Windows-x64 artifact'i PR tetiklemelerinde bilerek atlanıyor; başarılı publish tek başına indirilebilir EXE olduğu anlamına gelmez.
- Branch, son karşılaştırmada main'den 1 commit geride ve çok sayıda commit ilerideydi. PR #2 açık; main'e merge etmeden önce fark ve geride kalan commit elle gözden geçirilmeli.
- GitHub Desktop'taki .sln VisualStudioVersion 17/18 değişikliği gibi makineye özel metadata değişiklikleri iş gereği değilse kaynak değişikliği olarak eklenmemeli. Stash içeriği ayrıca incelenmeden düşürülmemeli.

## 8. Yol haritası ve kabul kriterleri

1. **Kaynak davranışını sabitle:** Bu belgeyi hücre formülleri ve resmi PDF görsel sözleşmesi için kaynak kabul et; Excel kaynak anomalilerini sessizce “düzeltme”.
2. **Hesaplama eşitliği:** KDV muaf/muaf değil, taşıma ve rehber, iskonto, pozitif/negatif fiyat farkı, kesintiler ve kuruş yuvarlama için Excel hücre referanslı golden-master testleri ekle.
3. **İcmal eşitliği:** Eylül–Temmuz 11 çıktı satırı, Eylül–Haziran kaynak lookup aralığı, ham I+R fiyat farkı, hemen önceki ay kesintisi ve Kasım/Aralık başlık kusuru için testleri koru; kaynak workbook kararı yazılı alınsın.
4. **PDF görsel doğrulama:** Artifact kotası temizlenince dört gerçek smoke PDF'yi indir; örneklerle aynı DPI'da karşılaştır. Başlıklar, satırlar, imza/footer ve döndürülmüş kesinti etiketi kontrol edilmeden “birebir” kabul etme.
5. **Veri/arşiv güvenliği:** İlk açılış temizliği yedekli çalışmalı; farklı GUID'li aynı ticari anahtara sahip hakediş tekrar eklenmemeli; bozuk JSON/yedek, erişim reddi, kilitli temp ve disk dolu senaryoları test edilmeli.
6. **Önizleme/yazdırma:** DocumentViewer, Microsoft Print to PDF ve en az bir gerçek yazıcıda A4 kenar boşlukları, sayfa sırası, taşma ve Türkçe karakter kontrolü.
7. **Arayüz:** kayıt durumunu statik “Hazır” yerine gerçek kayıt durumuna bağla; kurum imza/unvan metinlerini onaylat; 1366×768/1920×1080 ve %100–%200 DPI test et.
8. **Windows paketleme:** artifact kotası açıldıktan sonra indirilebilir self-contained win-x64 paketini doğrula; temiz kullanıcı profilinde ilk açılış, kayıt, yeniden başlatma, PDF/yazıcı ve kaldırma testi yap.

**Yayın kapısı:** 93 Core + 1 PDF smoke testi başarılı olması önemli; ancak görsel karşılaştırma, Excel iş sahibi onayı, gerçek yazıcı ve installer/update/uninstall kabul testleri bitmeden üretime hazır kararı verilmemeli.
