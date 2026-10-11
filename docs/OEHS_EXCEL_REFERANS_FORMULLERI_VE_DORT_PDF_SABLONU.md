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

**Kaynak workbook anomalisi:** `İcmal Verileri` satır 3'te kesinti dönem başlıklarında `AP3` ve `AQ3` ikisi de Aralık'a bağlanıyor; Kasım başlığı eksik görünüyor. OEHS aylık kesinti aramasını gerçek tarih/yıl/ay üzerinden yapmalıdır; aksi durumda Aralık ve Kasım dönemleri yanlış sütuna bağlanabilir. Bu, kaynağın davranışını sessizce kopyalamak yerine hedeflenen “bir önceki takvim ayı” mantığıyla düzeltilmiş bir uyumsuzluk olarak test edilmelidir.

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

## 6. DoğruTemin ile belge altyapısı karşılaştırması

DoğruTemin WPF'de `DocumentPreviewWindow` `FixedDocument` kabul eder, WPF `DocumentViewer` kullanır ve `TextSharpnessService` uygular. Yazdırma `PrintDialog` + `DocumentPaginator` ile çalışır. `PdfVectorWriter` `FixedDocument` sayfalarındaki metin ve temel şekilleri PDFSharp-WPF üzerinden vektör olarak işler.

OEHS'nin mevcut yaklaşımı ise raporları `FlowDocument` ile üretiyor ve `OehsPdfExportService` bunları 200 DPI bitmap sayfalara dönüştürerek PDF'ye gömüyor. Bu, DoğruTemin ile aynı altyapı değildir; metin seçilebilirliği ve çizgi/kenar netliği kaybolabilir. Sadece DoğruTemin sınıfını kopyalamak yeterli olmaz: dört resmi şablonun `FixedDocument`/`FixedPage` tabanında oluşturulması, önizleme/yazdırma/PDF'nin aynı belge nesnelerini paylaşması gerekir.

Kullanıcı örnekleri dört ayrı, tek sayfalı PDF dosyasıdır. OEHS'nin mevcut “PDF OLARAK KAYDET” iş akışı birleşik FlowDocument/PDF oluşturuyor; hedef davranış dört şablonun her birini kendi tek sayfalı PDF dosyası olarak üretmek, önizleme ve toplu yazdırmada ise aynı dört sayfayı sırayla kullanmaktır.

## 7. GitHub / GitHub Desktop durumu

- PR #2 iş akışı `OEHS Windows Build` içinde WPF derlemesi, test derlemesi, 83 test ve Windows x64 `dotnet publish` başarılıydı.
- İş akışının başarısız olan tek adımı `actions/upload-artifact@v4` idi: GitHub Actions artifact depolama kotası dolmuştu. Bu kaynak kod hatası değildir; indirilebilir CI artifact'i oluşmadığı anlamına gelir.
- Workflow, PR doğrulamasını artifact yüklemesinden ayıracak şekilde güncellendi: PR'larda upload atlanır; push/manuel çalıştırmada upload başarısız olursa iş akışı test/derleme sonucunu kırmızıya çevirmek yerine açık uyarı üretir. Bu kota kullanımını temizlemez; gerçek binary artifact'in saklanıp saklanmadığı ayrıca izlenmelidir.
- GitHub Desktop ekranında yerel `.sln` farkı yalnızca VisualStudioVersion satırının 17'den 18'e değişmesi gibi görünüyordu. Bu makineye özgü Visual Studio metadata değişikliğini kaynak çözüm dosyasına bilerek dahil etmeyin. Stash'te saklanan değişiklikler ayrıca incelenmeden düşürülmemeli/uygulanmamalıdır.

## 8. Yol haritası ve kabul kriterleri

1. **Kaynak davranışını sabitle:** Bu belgeyi kaynak sözleşmesi olarak kullan; workbook'taki hücre formülleri ile PDF görsel sözleşmesini ayrı doğrula.
2. **Hesaplama eşitliği:** KDV muaf/muaf değil, taşıma ve rehber, iskonto, pozitif/negatif fiyat farkı, kesintiler, önceki ay ve kuruş yuvarlama senaryoları için Excel formülüne dayalı otomatik testler.
3. **İcmal eşitliği:** 11 dönem Eylül–Temmuz, hizmet tutarının indirim/fiyat farkı ayrımı, ham I+R fiyat farkı, hemen önceki ay kesintisi ve eksik ay senaryoları.
4. **FixedDocument mimarisi:** DoğruTemin'deki DocumentViewer/PrintDialog/PdfVectorWriter yaklaşımını OEHS'ye aktar; dört şablonu sabit A4 sayfalarında düzenle.
5. **Şablon doğrulama:** Üretilen her PDF bir sayfa A4 dikey; metin/kenar/kolon/imza/footer/örnek no, Türkçe karakterler, uzun iş adı, uzun tutarlar ve boş/veri eksik durumları kontrol edilir.
6. **Veri ve arşiv güvenliği:** temiz başlangıç veri silme davranışı, bozuk JSON/yedek kurtarma, arşiv çakışma/geri yükleme, birden fazla hakediş ve kayıt silme senaryoları.
7. **Arayüz kalite kontrolü:** farklı çözünürlük ve DPI, ekran dışına taşma, statik “Hazır” durumu, tekrar eden kayıt durumu etiketi, imza alanlarının sabit kodlanması ve doğrulama mesajları.
8. **Windows CI:** restore/build/test başarıları ile artifact depolama sonucu ayrı raporlanır; uyarılar ve analyzer bulguları hedefli olarak giderilir.
9. **Kurulum paketi:** yalnız tüm otomatik ve görsel testler geçtikten sonra self-contained Windows x64 paket/kurulum, temiz makinede açılış, ilk kayıt, yeniden açılış, PDF, yazıcı ve kaldırma testi.

**Yayın kapısı:** “83 test geçti” iyi bir başlangıçtır ancak dört örnekle görsel regresyon, Excel formül paritesi, Windows yazıcı ve PDF doğrulanmadan kurulum dosyası yayınlanmış sayılmaz.
