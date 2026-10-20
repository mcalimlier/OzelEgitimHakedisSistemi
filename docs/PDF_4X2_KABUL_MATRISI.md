# PDF 4 × 2 Kabul Matrisi

> **GÜNCEL DURUM — 20.10.2026, HEAD `b6db48694d957f8ca1eaa003e57bceb617da8795`:** En güncel Windows CI [39464941718](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39464941718) başarılı tamamlandı: tam çözüm/WPF derlemesi, test çözümü, PDF smoke üretimi ve Windows EXE publish adımları başarılı. Bu CI, sentetik/regresyon PDF doğrulamasıdır; gerçek Microsoft Excel/VBA çıktılarıyla 8/8 resmî PDF kabulü hâlâ açık ve tamamlanmış sayılmaz. Son XLSM hash/formül/vectör kaynağı [SON_XLSM_KAYNAK_PARITESI_2026-10-20.md](SON_XLSM_KAYNAK_PARITESI_2026-10-20.md), masaüstü kabul prosedürü [EXCEL_MASAUSTU_KABUL_LISTESI.md](EXCEL_MASAUSTU_KABUL_LISTESI.md) dosyasıdır. Aşağıdaki eski CI atıfları tarihsel kayıt olarak korunur.

---

Tarih: 2026-10-19  
Dal: `feature/official-payment-document-templates`

## Kapsam

Kaynak Excel sayfaları 4/5/6/7, uygulamanın sol menüsüne eklenecek ekranlar değildir. Her program için bu sayfalar, OEHS'nin ürettiği dört PDF'nin kabul referanslarıdır.

| Kaynak program | PDF 4 — Hakediş Raporu | PDF 5 — Hizmet İşleri Hakediş Raporu | PDF 6 — Hizmet İşleri Kabul Tutanağı | PDF 7 — Hakediş İcmali |
|---|---|---|---|---|
| Temel Eğitim | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı |
| Özel Eğitim | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı | Açık — Excel referansıyla karşılaştırılmalı |

## Her hücre için kabul kontrolü

1. Aynı ihale, sözleşme, yüklenici, hat, hakediş numarası ve dönem kullanılmış olmalı.
2. PDF metin alanları kaynak formun karşılık gelen alanlarıyla karşılaştırılmalı: dönem/tarih, numara, yüklenici, hat/güzergâh, sözleşme bilgileri ve işin süresi.
3. Tutarlar kaynak Excel'in gerçek hesaplanmış hücreleriyle karşılaştırılmalı. Yuvarlama farkları, formül farkları ve yalnızca biçim farkları ayrı sınıflandırılmalı.
4. İcmalde ay sırası, önceki hakediş aktarımı, fiyat farkı ve kesinti davranışı ayrı kontrol edilmeli.
5. İmza/komisyon alanları, metin taşması, sayfa boyutu/yönü ve hücre hizaları görsel olarak kontrol edilmeli.
6. Sonuç her belge için ayrı kaydedilmeli; bir PDF'nin geçmesi diğerlerini geçirmez.

## Şu an eldeki kanıt ve sınırı

GitHub Actions run [39303608475](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39303608475) başarıyla tamamlandı. `OEHS-PDF-Smoke-Review` artefaktı indirildi ve sekiz sentetik PDF incelendi. Sekizinin de sayfa sayısı 1 ve ölçüsü A4 portre.

Sentetik örneklerde görülen program farkları:
- Özel Eğitim kabul tutanağı süreyi `180 İş Günü` olarak gösteriyor; Temel/Ortaöğretim örneği `180` gösteriyor.
- Ekim icmal toplamı Özel Eğitim örneğinde 120,00 TL; Temel/Ortaöğretim örneğinde 125,00 TL.

Bunlar sentetik smoke verisinin gözlemleridir; tek başına kaynak Excel'e uygunluğu kanıtlamaz ve otomatik olarak hata sayılmaz. İncelemede ayrıca:
- Hakediş Raporu'ndaki KDV açıklaması Özel Eğitim'de taşıma %10 + rehber %20, Temel/Ortaöğretim'de yalnızca taşıma %10 olarak üretiliyor.
- Sentetik örnekte damga vergisi tutarı Özel Eğitim'de 1,00 TL, Temel/Ortaöğretim'de 1,04 TL.
- Bu farklar programların ayrı hesaplama zincirleriyle ilişkili olabilir; gerçek aynı-girdi kaynak Excel sonucu görülmeden değişiklik yapılmayacak.

## Mevcut otomatik test kapsamı (Excel kabulü yerine geçmez)

- `tests/OzelEgitimHakedis.Wpf.Tests/OfficialDocumentOutputTests.cs`: dört ayrı belge üretimi, A4 portre/tek sayfa, temel alanlar, imza metinleri, icmalde Eylül–Ekim takvim sırası ve sentetik tutar kontrolleri.
- `tests/OzelEgitimHakedis.Tests/ProgressPaymentIcmalServiceTests.cs`: Eylül–Temmuz dönemi, Kasım için önceki ay aktarımı, eksik başlık/lookup sınırı, program ve hat izolasyonu.
- `tests/OzelEgitimHakedis.Tests/CurrentXlsmSourceParityTests.cs` ve `CurrentBasicWorkbookDiscrepancyTests.cs`: kodun kaydedilmiş kaynak hücre sonuçlarına karşı regresyon vektörleri. Test açıklamaları bunların masaüstü Excel/VBA yeniden hesaplaması olmadığını açıkça belirtiyor.

Güncel doğrulama: commit `a6127422655c6c55b0d1470c936c16bc4febfef3` için [Windows Build CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39307338034) **başarıyla tamamlandı**. Tam çözüm/WPF derlemesi, UI kontrolleri, test derlemeleri, `Test Solution`, PDF smoke üretimi ve Windows EXE/izole deneme paketi artefakt adımları başarılı. Bu, otomatik CI kanıtıdır; kaynak Excel/VBA ile sekiz PDF'nin alan alan kabulü değildir.

## Kapanış koşulu

Matristeki sekiz hücrenin her biri için gerçek Excel masaüstü/VBA ile yeniden hesaplanmış referans sonuçlar ve aynı girdilerle OEHS PDF'si mevcut olmalı. Her eşleşme alan bazında kayıt altına alınmalı. Excel formülü ile uygulama arasında fark bulunursa, kaynak hücre/formül, gerçek sonuç ve test kanıtı belgelenmeden hesaplama davranışı değiştirilmemeli.

CI başarısı, sentetik PDF smoke sonucu veya yalnızca PDF'nin açılması bu kabul kapısını kapatmaz.


## Devam notu — 2026-10-19

- **Tamamlandı:** `FindPreviousDeduction_ShouldUseLatestSavedRecordWhenPreviousMonthHasDuplicates` regresyon testi; aynı ayda birden fazla Ekim kaydı varsa en son kaydedilen kaydın kesintisinin Kasım'a taşındığını denetler.
- **Doğrulama:** commit `a6127422655c6c55b0d1470c936c16bc4febfef3`, Windows CI run `39307338034` başarılı.
- **Açık risk/engel:** sekiz PDF için gerçek masaüstü Excel/VBA yeniden hesaplanmış aynı-girdi referansları ve aynı verilerle OEHS PDF'leri karşılaştırması hâlâ yok. Sentetik smoke örnekleri bu boşluğu kapatmaz.
- **Sıradaki adım:** kaynak XLSM'nin gerçek yeniden hesaplama sonuçlarına erişilebilen ilk fırsatta 4×2 matriste her belgeyi dönem, yüklenici, hat, tutarlar, imza alanları ve sayfa yerleşimiyle ayrı ayrı kapatmak; formül uyuşmazlıklarını test ve karar notuyla izlemek.


## Devam notu — 2026-10-19 (son doğrulama)

- **Tamamlandı:** `AdditionalGuarantee_ShouldPreserveSignedSourceFormulaForNegativePriceDifferences` regresyon testi eklendi. Temel Eğitim için I=-100 TL olduğunda T=-6 TL; Özel Eğitim için I=-100 TL ve R=-50 TL olduğunda AC=-9 TL beklenir. Bu, doğrudan kaynak formül davranışını kilitler; hukuki/ihale yorumu hakkında karar vermez.
- **Doğrulama:** commit `f891895c6e4b0fcc92c0f7d252f23face7fe261e` için [Windows Build CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39309542369) **başarılı**. PR #3 hâlâ açık/taslak ve birleştirilmedi.
- **Kaynak durumu:** etkin dalın Git ağacında `.xlsm`, `.xlsx` veya `.xlsb` dosyası bulunmadı. Bu nedenle bu turda masaüstü Excel/VBA yeniden hesaplaması ve gerçek referanslı 4×2 PDF kabulü yapılmadı.
- **Açık risk/engel:** sekiz PDF için aynı girdilerle gerçek Excel/VBA referansları yok; sentetik smoke çıktıları kabul kanıtı sayılmaz. Negatif fiyat farkında ek kesin teminatın işaretli sonuç üretmesi Excel formülüne sadakattir, ancak mevzuat/sözleşme değerlendirmesi gerektiren noktadır.
- **Sıradaki adım:** dört belgeyi iki program için aynı hakediş girdisiyle üretip dönem, yüklenici, hat, her tutar, imza alanları ve sayfa yerleşimini karşılaştıran kabul kaydını doldurmak; Excel/VBA sonuçları olmadan hesap davranışını değiştirmemek.


## Devam notu — 2026-10-19 (bağımsız XLSM formül ve PDF ön incelemesi)

### Kaynak formül doğrulaması — tarihsel dosya kopyaları

Bu oturumda kişisel dosya alanındaki 17.10.2026 tarihli tarihsel XLSM kopyaları ayrıca yeniden hesaplandı:

- `03-Temel Eğitim Taşıma Ödeme(1).xlsm`
- `02-Özel Eğitim Hakediş Ödeme(1).xlsm`

Dosyaların kopyaları LibreOffice Calc ile XLSX'e dönüştürülerek yeniden hesaplandı; orijinal önbellek değerleri, yeniden hesaplanan hücreler ve aşağıdaki kaynak formül aynası karşılaştırıldı. Aktif satırlar için tolerans **0,000001 TL**:

| Program | Satırlar | Hücre eşlemesi | Karşılaştırma | Fark |
|---|---:|---|---:|---:|
| Temel/Ortaöğretim | 4–8 | K, L, M, N, O, T, U, V, W | 45 hücre | 0 uyumsuzluk |
| Özel Eğitim | 4–6 | K, L, M, N, O, T, U, V, W, X, AC, AD, AE, AF, AG | 45 hücre | 0 uyumsuzluk |
| **Toplam** | **8 kaynak satırı** | **90 formül sonucu** | **90 hücre** | **0 uyumsuzluk** |

Bu değerler aynı tarihsel kaynak kopyalarında formül zincirinin LibreOffice yeniden hesaplamasıyla tutarlı olduğunu gösterir. Bunlar masaüstü Excel/VBA çalıştırması değildir ve 19.10.2026 tarihli en yeni XLSM'lerin yerine geçmez.

Yeni `HistoricalWorkbookFormulaParityTests.cs` testi, bu 8 kaynak satırının aynı temel hesaplama alanlarını OEHS `ProgressPaymentCalculator` ile karşılaştırır. CI:
- [Push CI — 39311836817](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39311836817): başarılı.
- [PR CI — 39311842694](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39311842694): başarılı.

### Tarihsel kaynak PDF'lerinde bulunan dönem metni kusuru

LibreOffice ile alınan tarihsel kaynak PDF'lerinde sekiz resmi sayfa bulundu ve metin/tutarlar incelendi. Özel Eğitim dosyasında `Hakediş Raporu!A4` hücresi formül değil, sabit metin olarak `30.04.2026 TARİHİNE KADAR YAPILAN İŞİN` içeriyor. Aynı dosyanın `Anasayfa!E5` dönemi Ekim 2026; kabul tutanağındaki dönem 01.10.2026–31.10.2026; icmal Eylül ve Ekim 2026 satırlarını gösteriyor. Kaynak PDF altındaki rapor tarihi de 24.06.2026, hizmet işleri raporunun tarihi 10/19/2026.

Bu, tarihsel dosya kopyasında belgeler arası **kaynak metin/tarih tutarsızlığıdır**. OEHS'nin seçili dönemden türettiği başlığı bu sabit metinle değiştirmedik; kaynak kusuru kayıt altına alındı. En yeni XLSM'de aynı hücre kusurunun sürüp sürmediği ayrı doğrulanmalıdır.

### PDF kabul kapısının durumu

- Kaynak Excel'in tarihsel kopyalarından sekiz belge sayfası LibreOffice ile dışa aktarıldı ve metin/yerleşim ön incelemesi yapıldı.
- OEHS smoke ZIP'indeki sekiz sentetik PDF'nin tamamı tek sayfa A4 portre; ancak bu PDF'ler aynı gerçek XLSM girdileriyle üretilmiş olmadığından tutar kabulünü kapatmaz.
- **En yeni XLSM ve gerçek Excel/VBA ile resmi kabul: 0/8 kapalı.** Buna ek olarak 17.10.2026 tarihli tarihsel kaynak kopyaları için sekiz OEHS PDF'si gerçek hücre değerlerinden türetilen vektörlerle üretildi; test edilen finansal/şablon alanlarında 4/8 belge tam eşleşti; dört istisna aşağıda belgelendi (iki tarih/formül kusuru, bir sabit tarih metni, bir makro sonrası yenilenmemiş aylık veri). Bu tarihsel kontrol en yeni XLSM kabulünün yerine geçmez.
- Excel/VBA makroları masaüstü Excel'de çalıştırılmadı. LibreOffice ile formül hesaplama, VBA olay/makro davranışını doğrulamaz.


### Temel/Ortaöğretim tarihsel XLSM icmal formül kusuru — karar bekliyor

Aynı tarihsel Temel Eğitim kaynak kopyasının LibreOffice PDF dışa aktarımı ve hücre formülleri birlikte incelendiğinde ek bir kaynak uyuşmazlığı bulundu:

- `İcmal Verileri!P4` (Eylül 2026 ham fiyat farkı): **7.365,35 TL**.
- `İcmal Verileri!AB4` (başlık: `HAKEDİŞLERDEN YAPILAN KESİNTİ TUTARLARI`): **12.304,954557692 TL**.
- `Hakediş İcmali!D6` formülü, `15+MATCH(...)` ile P:Z fiyat farkı sütununu **ve** `27+MATCH(...)` ile AB:AL kesinti sütununu topluyor.
- Bu nedenle `Hakediş İcmali!D6` sonucu **19.670,304557692 TL**; PDF'de **19.670,30 TL** basılıyor. `C6` ise `Hesaplama!K4` değerini (fiyat farkı zaten dâhil) kullanıyor; `E6=C6+D6` sonucu **219.045,63 TL** oluyor ve fiyat farkı ikinci kez ekleniyor.
- Aynı kaynakta `Hakediş Raporu` brüt/tahakkuk tutarı **219.312,85 TL**; bu tutar hesaplama sayfasındaki KDV dâhil brüt zincirle uyumlu. İcmal toplamı bu değerle **267,23 TL** fark gösteriyor.

Bu, Temel/Ortaöğretim icmalinde Özel Eğitim için kullanılan iki fiyat-farkı bloğunu toplayan formülün, Temel Eğitim'de ikinci blok kesinti sütunları olduğu hâlde aynen kalmış olabileceğini gösterir.

Ayrımı açık tutmak gerekir:
- `ProgressPaymentReportService`, Hakediş Raporu için ham fiyat farkını `I4` değerinden alır; yeni `HistoricalBasicWorkbookRow4_ShouldMatchReportCellsWithoutImportingIcmalColumnDefect` testi bu rapor alanlarını doğrular.
- PDF icmal üreticisi `OehsOfficialDocumentFactory` kaynak `D6` formülünün davranışını korumak için Temel/Ortaöğretim'de ham fiyat farkına hesaplanmış kesintiyi ekler. Tarihsel dosyada `AB4=12.304,954557692`, hesaplama sayfasında `V4=12.300,765146612` olduğundan **4,18941108 TL** fark var. İncelenen VBA modülündeki `KesintiToplamlariniAktar` makrosu seçili ay için `Hesaplama Bilgileri!V` değerini `İcmal Verileri!AB:AM` aralığına yazar; makro çalıştırılırsa AB4'ün V4 ile yenilenmesi ve `D6=I4+V4=19.666,12 TL` oluşması beklenir. Bu oturumda VBA makrosu çalıştırılmadı; kaynak PDF'deki 19.670,30 TL, makrodan sonra güncellenmemiş tarihsel AB4 değerini yansıtıyor. Dolayısıyla 4,19 TL farkı **eski veri durumu ile yeniden hesaplanan OEHS çıktısı** arasındadır; en yeni XLSM'de aynı davranış makro çalıştırılarak doğrulanmalıdır.
- **Kod davranışı bu bulgu üzerine değiştirilmedi.** Kaynak formülün düzeltilmesi mi yoksa mevcut Excel çıktısının birebir korunması mı gerektiği açık bir karar noktasıdır.

Bu bulgu 17.10.2026 tarihli tarihsel dosya kopyasına aittir. En yeni Temel Eğitim XLSM'de `Hakediş İcmali!D6`, AB:AL başlıkları ve aylık veri aktarım makrosu ayrıca doğrulanmadan tüm sürümlere genellenmemelidir.


### Hizmet İşleri Hakediş Raporu örnek numarası — kaynakla hizalandı

Tarihsel kaynak PDF'lerinde hem Temel/Ortaöğretim (LibreOffice dışa aktarım sayfa 20) hem Özel Eğitim (sayfa 43) Hizmet İşleri Hakediş Raporu alt bilgisi **`M.Y.H.B.Y. Örnek No: 4`** olarak basılıyor. OEHS şablonunda ve testinde önce **5** bekleniyordu. Kaynak örnekleriyle doğrudan karşılaştırınca bu etiket farkı doğrulandı; `OehsOfficialDocumentFactory` **4** olarak düzeltildi ve WPF regresyon testi 4'ü bekleyip 5'i reddedecek şekilde güncellendi.

Bu düzeltme yalnızca belge alt bilgisini hizalar; hesaplama davranışını değiştirmez. Güncel kod ve testler push CI'da başarılıdır: [run 39314524004](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39314524004). En yeni XLSM'nin sekiz PDF alanının tamamını karşılaştırma ayrı kabul kapısıdır.


### Özel Eğitim icmalinde fiyat farkının iki kez toplanması

Tarihsel Özel Eğitim kaynak PDF'sinde Eylül satırı `İş Tutarı 54.725,90 TL` ve `Fiyat Farkı 1.403,57 TL`; Ekim satırı `İş Tutarı 93.318,27 TL` ve `Fiyat Farkı 1.403,57 TL` olarak gösteriliyor. VBA `HakedişToplamlariniAktar` makrosu aylık tutar kolonuna `Hesaplama!K + Hesaplama!T` yazıyor; `FiyatFarkıVerileriniAktar` ve `RehberPersonelFiyatFarkıVerileriniAktar` ise ham `I` ve `R` değerlerini ayrı bloklara yazıyor. K+T zaten I/R farklarını içerdiğinden icmal formülü toplamda fiyat farkını ikinci kez ekliyor. OEHS icmal üreticisi şu an bu kaynak davranışını koruyor.

Mantıksal ayrımda aylık `İş Tutarı` fiyat farkı hariç temel hizmet tutarı olmalı, `Fiyat Farkı` yalnızca ham `I/R` olmalı; toplam da `K+T` ile eşleşmeli. Tarihsel kaynak PDF ise Eylül toplamında **56.129,48 TL**, Ekim toplamında **94.721,85 TL** göstererek her iki ham farkı ikinci kez ekliyor. Bu, kaynak PDF ile tutarlılık bulgusudur; hesaplama matrahının doğru olduğu yönünde bağımsız mevzuat onayı değildir. İcmal davranışı değiştirilmeden önce en yeni XLSM ve VBA makrosu aynı girdiyle tekrar doğrulanmalıdır.

### Hizmet İşleri Hakediş Raporu — alan eşlemesi düzeltildi

Kaynak XLSM sayfalarında ve tarihsel PDF'lerde şu alanlar doğrulandı:
- Alt bilgi `M.Y.H.B.Y. Örnek No: 4`.
- `Tarihi` alanı seçili ödeme ayının son günü değil, raporun üretildiği tarihtir (`TODAY()`).
- `Sözleşme Tarihi ve No.su` etiketi korunur.
- Temel/Ortaöğretim'de `Sözleşmeye Göre İşin Süresi` başlangıç ve bitiş tarihlerini `gg.aa.yyyy-gg.aa.yyyy Tarihleri Arasıdır.` biçiminde birleştirir; Özel Eğitim'de kaynak hücre `İş Günü` sayısını metne ekler.

OEHS `OehsOfficialDocumentFactory` bu alanlara göre düzeltildi; WPF testleri numara, etiket, üretim tarihi ve program bazlı süre metnini kontrol ediyor. [İlgili kod commit'i](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/commit/d22a1e8346f25e77c15e95f2bab769bf41a192d4). Güncel test/CI durumu aşağıdaki son doğrulama kaydında belirtilmelidir.


## Son doğrulama kaydı — 2026-10-19, commit `a5eb7c1121e24620669fd9f535b45793e6713723`

- [Windows push CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39314524004): başarılı.
- [Windows PR CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39314531090): bu kayıt yazılırken çalışıyor.
- Core testleri: **222/222 geçti**. WPF çıktı testleri: **5/5 geçti**.
- Smoke artefaktı artık **16 PDF** içerir: sekiz temel sentetik çıktı ve sekiz tarihsel XLSM kaynak vektöründen üretilen çıktı. [PDF artefaktı](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39314524004/artifacts/12217118649).
- Tarihsel Özel Eğitim örneğinde dört PDF'nin finansal alanları kaynak PDF değerleriyle eşleşiyor; önceki hakediş toplamının üç ondalık basamaklı gösterimi de kaynakla hizalandı. Kaynak Hakediş Raporu'ndaki sabit `30.04.2026` başlığı ve `24.06.2026` alt tarihi kusur olarak kalır; OEHS seçili dönem ve üretim tarihini kullanır.
- Tarihsel Temel/Ortaöğretim örneğinde Hakediş Raporu ve Kabul Tutanağı test edilen alanlarda eşleşiyor. Hizmet İşleri raporunda `İşyeri Teslim Tarihi` kaynak formül kusuruyla farklı; OEHS `İşe Başlama Tarihi`ni kullanıyor. İcmal PDF'sindeki `AB4` değeri makrodan sonra yenilenmediği için kaynak görüntüsü OEHS çıktısından 4,19 TL farklı; statik VBA modülü `V` değerinin `AB:AM` alanına aktarıldığını gösteriyor, fakat makro çalıştırılmadı.
- WPF şablonundaki paylaşılan gri fırça da donduruldu (`Freeze()`); yeni tarihsel PDF testi ayrı STA iş parçacığında çalıştığında ortaya çıkan çapraz iş parçacığı hatası giderildi.

### Sıradaki adım

1. PR CI tamamlandıktan sonra son commit'in her iki CI sonucunu kaydet.
2. En yeni XLSM dosyalarının hücre ve VBA akışını aynı örnek üzerinden yeniden doğrula; özellikle `Hakediş İcmali!D6`, `İcmal Verileri!AB4:AM4` ve Özel Eğitim'in `AN:AX` kesinti bloğu.
3. Temel/Ortaöğretim icmalindeki 4,19 TL fark için kaynak Excel'in hatalı formülünü aynen koruma, mantıksal fiyat farkını düzeltme veya aylık icmal değerini arşivde ayrıca saklama seçeneklerinden biri açıkça kararlaştırılmadan hesaplama davranışını değiştirme.
4. Ana dala birleştirme/yayınlama yapma; PR taslak kalacak.


### Hizmet İşleri raporunda hat numarası, güzergâh ve hizmet adı ayrımı

Tarihsel kaynak hücreleri `Hizmet İşleri Hakediş Raporu!F4` için Temel/Ortaöğretim'de `Anasayfa!K16`, Özel Eğitim'de `VLOOKUP(Anasayfa!K17, 'İhale Verileri'!A:R, 1, FALSE)` kullanıyor. İki formül de **hat numarasını** veriyor; hakediş numarasını değil. OEHS'nin `No.su` alanı bu nedenle `ProgressPaymentNumber` yerine `RouteNumber` kullanacak şekilde düzeltildi.

Aynı kaynak sayfasında hat başlığı `Hattın Adı` sütunundan, `Yapılan Hizmetin Adı` ise ayrı bir alan/şekilden geliyor. OEHS bağlamına `RouteName` alanı eklendi; dört belgenin başlıkları güzergâh adını, Hizmet İşleri raporundaki `Yapılan Hizmetin Adı` satırı ise sözleşmenin `WorkName` değerini kullanıyor. Tarihsel PDF testi, `No.su: 1` ile `ProgressPaymentNumber: 2` ayrımını ve iki farklı metin alanını doğruluyor.

### Kaynak formül kusuru: İşyeri teslim tarihi

Tarihsel Temel/Ortaöğretim ​`Hizmet İşleri Hakediş Raporu!F15` formülü `İhale Verileri` aralığının 11. sütununu; Özel Eğitim'de aynı alan 16. sütunu çekiyor. Her iki aralıkta bu sütun `İşin Bitiş Tarihi` olduğu hâlde etiket `İşyeri Teslim Tarihi`; kaynak PDF bu nedenle **25.06.2027** gösteriyor. Aynı tabloda `İşe Başlama Tarihi` 14.09.2026. OEHS mevcut bağlamdaki `ContractStartDate` değerini (14.09.2026) kullanıyor; bu mantıksal alan eşlemesidir, fakat tarihsel Excel PDF'siyle birebir aynı değildir. Kaynak formülün düzeltilmesi veya aynen taklit edilmesi en yeni XLSM/VBA ile doğrulanmadan kararlaştırılmamalıdır.


## Güncel devam kaydı — 2026-10-19

- Güncel kod doğrulaması: [commit `9e09adca`](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/commit/9e09adca88a5ce4e19fe5837e776656045cf1190) için [push CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39316589255) ve [PR CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39316596025) başarılı. Son dokümantasyon commit'i `2c449019` için [push CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39316639093) ve [PR CI](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39316648124) de başarılı.
- Hizmet İşleri raporundaki `No.su` alanı artık `RouteNumber` ile doldurulur; `RouteName` dört belgenin güzergâh başlıklarında, `WorkName` ise ayrı `Yapılan Hizmetin Adı` satırında kullanılır. 16 PDF smoke paketi ve tarihsel kaynak vektör testi bu alanları kapsar.
- Tarihsel kaynak PDF'lerinde Temel/Ortaöğretim Hakediş Raporu alt tarihi `16.10.2026`, diğer raporlardaki üretim tarihi `19.10.2026`; Özel Eğitim Hakediş Raporu ise sabit `24.06.2026` alt tarihi gösteriyor. OEHS `GeneratedDate` kullanarak güncel üretim tarihini tutarlı gösterir. Bu tarih farkları kaynak dosya durumuna aittir; resmî hesap tutarlarına müdahale edilmedi.
- Kaynak `İşyeri Teslim Tarihi` formülü bitiş tarihi sütununu çekiyor; OEHS başlangıç tarihini kullanıyor. Bu kaynak formül kusuru açıkça belgeli ve karar verilene kadar değiştirilmedi.
- **Sıradaki iş:** en yeni XLSM'nin aynı örneği gerçek Excel'de yeniden hesaplanıp VBA makroları çalıştırılarak tarihsel/yenilenmiş değer farkları doğrulanmalı. `Hakediş İcmali`ndeki fiyat farkını ikinci kez ekleme davranışı için kaynak davranışını koruma veya düzeltme kararı alınmadan kod değişikliği yapılmamalı. PR taslak ve birleştirilmemiş kalacak.


## Ek güncel kaynak kanıtı — 20.10.2026 XLSM'leri

- Son yüklenen dosyalar artık doğrudan okunup SHA-256 ile sabitlendi. Yetkili kaynak vektörleri ve formül özeti: [SON_XLSM_KAYNAK_PARITESI_2026-10-20.md](SON_XLSM_KAYNAK_PARITESI_2026-10-20.md).
- Temel Eğitim satır 4–8 ve Özel Eğitim satır 4–6 kaynak sonuçları C# regresyon testlerine aktarıldı. Eski yüklemeye dayanan test vektörleri yenileriyle değiştirildi; program çekirdekleri ayrı tutuldu.
- Her iki son XLSM'nin formül önbelleği, LibreOffice Calc ile ayrı XLSX kopyalarında yeniden hesaplanan sayısal sonuçlarla karşılaştırıldı: 279 sayısal hücre, 0 fark (tolerans 0,000001 TL). Bu sonuç masaüstü Microsoft Excel/VBA makro kabulü değildir.
- Windows CI run [39398873252](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39398873252) önceki test commit'inde CS3016 nedeniyle başarısız oldu. İlk MemberData denemesi de CS3016 ile başarısız oldu. Son düzeltmede attribute tabanlı veri sağlayıcıları kaldırılarak her program için [Fact] içinde typed tuple vektörleri döngüyle doğrulanıyor. Güncel test commit'i için yeni CI sonucu henüz görünmüyor.
- Sekiz PDF kabul hücresinin durumu değişmedi: gerçek aynı-girdi Excel/VBA PDF referansı ve alan alan karşılaştırma olmadan hiçbiri kapatılmış sayılmaz.


- **Test düzeltmesi güncellemesi:** CS3016 sonrasında MemberData da kaldırıldı; kaynak vektörleri artık [Fact] içinde tipli tuple listesi ve foreach ile test ediliyor. Güncel test commit'i 15e3e88755ef768fdb6352c8bacb56f6ce99cafe; CI sonucu henüz alınmadı.


## Güncel Windows CI sonucu — 20.10.2026

- [Windows CI run 39399443926](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39399443926) ve [run 39399513703](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39399513703) **başarılı** tamamlandı.
- Son doğrulanan testler: **215/215 Core**, **5/5 WPF çıktı testi**. Tam çözüm ve WPF derlemesi, UI statik kontrolleri, PDF smoke üretimi ve Windows EXE publish adımları başarılı.
- Eski source-vector test metotları son XLSM satırlarıyla değiştirildiği için test sayısı önceki CI'daki 223'ten 215'e indi; son testlerde 8 gerçek kaynak satırı 2 program-özel test içinde döngüyle doğrulanıyor. Eski yükleme vektörleri artık geçerli kabul edilmiyor.
- Arkadaş test paketi ve EXE artefakt yükleme adımları CI ortamı kısıtlaması nedeniyle atlandı; hazırlama adımları başarılı. CI, masaüstü Excel/VBA veya 4×2 PDF görsel kabulü yerine geçmez.
- Güncel PR başı bu ekleme öncesinde bfe1c5d7... idi; son kaynak ve test düzeltmeleri dalda. PR #3 açık/taslak/birleştirilmemiş durumda.


## Son yüklenen XLSM'lerle PDF üretim regresyonu — 20.10.2026

- WPF testine son Temel satır 4 ve Özel satır 4 girdileri eklendi; iki program için dört ayrı PDF çıktısı oluşturuluyor (toplam sekiz), içerik tutarları ve A4 ölçüleri denetleniyor.
- [Windows CI run 39400709937](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39400709937) başarılı: 215/215 Core, 6/6 WPF test; tam çözüm/WPF derlemesi, UI statik kontrolleri, PDF smoke ve Windows EXE publish geçti.
- Son commit a0bce9e063492095493392c3ee40f10c02c648f6 yalnızca xUnit string karşılaştırmalarına StringComparison.Ordinal ekleyerek CA1307 uyarılarını gideriyor; bu commit için yeni CI sonucu bekleniyor.
- Kaynak Excel icmal formül riskleri ve masaüstü Excel/VBA kabulü açık; bu test otomatik PDF üretimini doğrular, kaynak Excel'deki makroların çalıştırıldığını kanıtlamaz.
