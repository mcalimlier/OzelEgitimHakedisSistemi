# PDF 4 × 2 Kabul Matrisi

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
- **4 × 2 gerçek kaynak PDF kabul matrisi: 0/8 kapalı.** Kalan iş; en yeni kaynak dosyanın aynı girdi ve aynı dönemle yeniden hesaplanmış sonuçlarıyla dört OEHS PDF'sinin alan alan karşılaştırılmasıdır.
- Excel/VBA makroları masaüstü Excel'de çalıştırılmadı. LibreOffice ile formül hesaplama, VBA olay/makro davranışını doğrulamaz.


### Temel/Ortaöğretim tarihsel XLSM icmal formül kusuru — karar bekliyor

Aynı tarihsel Temel Eğitim kaynak kopyasının LibreOffice PDF dışa aktarımı ve hücre formülleri birlikte incelendiğinde ek bir kaynak uyuşmazlığı bulundu:

- `İcmal Verileri!P4` (Eylül 2026 ham fiyat farkı): **7.365,35 TL**.
- `İcmal Verileri!AB4` (başlık: `HAKEDİŞLERDEN YAPILAN KESİNTİ TUTARLARI`): **12.304,954557692 TL**.
- `Hakediş İcmali!D6` formülü, `15+MATCH(...)` ile P:Z fiyat farkı sütununu **ve** `27+MATCH(...)` ile AB:AL kesinti sütununu topluyor.
- Bu nedenle `Hakediş İcmali!D6` sonucu **19.670,304557692 TL**; PDF'de **19.670,30 TL** basılıyor. `E6=C6+D6` sonucu **219.045,63 TL**.
- Aynı kaynakta `Hakediş Raporu` brüt/tahakkuk tutarı **219.312,85 TL**; bu tutar hesaplama sayfasındaki KDV dâhil brüt zincirle uyumlu. İcmal toplamı bu değerle **267,23 TL** fark gösteriyor.

Bu, Temel/Ortaöğretim icmalinde Özel Eğitim için kullanılan iki fiyat-farkı bloğunu toplayan formülün, Temel Eğitim'de ikinci blok kesinti sütunları olduğu hâlde aynen kalmış olabileceğini gösterir.

Ayrımı açık tutmak gerekir:
- `ProgressPaymentReportService`, Hakediş Raporu için ham fiyat farkını `I4` değerinden alır; yeni `HistoricalBasicWorkbookRow4_ShouldMatchReportCellsWithoutImportingIcmalColumnDefect` testi bu rapor alanlarını doğrular.
- PDF icmal üreticisi `OehsOfficialDocumentFactory` ise kaynak `D6` formülünün davranışını korumak için Temel/Ortaöğretim'de ham fiyat farkına mevcut hakedişin hesaplanmış kesintisini ekler. Bu nedenle uygulama, kaynak formül kusurunun yapısını taklit eder; ancak tarihsel dosyadaki `AB4=12.304,954557692` değeri hesaplama sayfasındaki `V4=12.300,765146612` değerinden **4,18941108 TL** farklıdır. Bu sabit aylık veri farkı nedeniyle aynı hesaplama girdileri tek başına tarihsel PDF'deki `D6` değerini üretmeye yetmez.
- **Kod davranışı bu bulgu üzerine değiştirilmedi.** Kaynak formülün düzeltilmesi mi yoksa mevcut Excel çıktısının birebir korunması mı gerektiği açık bir karar noktasıdır.

Bu bulgu 17.10.2026 tarihli tarihsel dosya kopyasına aittir. En yeni Temel Eğitim XLSM'de `Hakediş İcmali!D6`, AB:AL başlıkları ve aylık veri aktarım makrosu ayrıca doğrulanmadan tüm sürümlere genellenmemelidir.


### Hizmet İşleri Hakediş Raporu örnek numarası — kaynakla hizalandı

Tarihsel kaynak PDF'lerinde hem Temel/Ortaöğretim (LibreOffice dışa aktarım sayfa 20) hem Özel Eğitim (sayfa 43) Hizmet İşleri Hakediş Raporu alt bilgisi **`M.Y.H.B.Y. Örnek No: 4`** olarak basılıyor. OEHS şablonunda ve testinde önce **5** bekleniyordu. Kaynak örnekleriyle doğrudan karşılaştırınca bu etiket farkı doğrulandı; `OehsOfficialDocumentFactory` **4** olarak düzeltildi ve WPF regresyon testi 4'ü bekleyip 5'i reddedecek şekilde güncellendi.

Bu düzeltme yalnızca belge alt bilgisini hizalar; hesaplama davranışını değiştirmez. Yeni kod ve test CI sonucu henüz bekleniyor. Aynı girdilerle sekiz PDF'nin tüm alanlarının karşılaştırılması yine ayrı kabul kapısıdır.


### Özel Eğitim icmalinde fiyat farkının iki kez toplanması

Tarihsel Özel Eğitim kaynak PDF'sinde Eylül satırı `İş Tutarı 54.725,90 TL` ve `Fiyat Farkı 1.403,57 TL`; Ekim satırı `İş Tutarı 93.318,27 TL` ve `Fiyat Farkı 1.403,57 TL` olarak gösteriliyor. Kaynak makro aylık tutar kolonuna `Hesaplama!K + Hesaplama!T` yazıyor; bu iki hücre ham `I/R` fiyat farklarını zaten içeriyor. İcmal formülü fiyat farkını ayrıca eklediğinden satır toplamı ham farkı ikinci kez içeriyor. OEHS icmal üreticisi şu an bu kaynak davranışını koruyor. Bu, kaynak PDF ile tutarlılık bulgusudur; hesaplama matrahının doğru olduğu yönünde bağımsız mevzuat onayı değildir. İcmal davranışı değiştirilmeden önce en yeni XLSM ve VBA makrosu aynı girdiyle tekrar doğrulanmalıdır.

### Hizmet İşleri Hakediş Raporu — alan eşlemesi düzeltildi

Kaynak XLSM sayfalarında ve tarihsel PDF'lerde şu alanlar doğrulandı:
- Alt bilgi `M.Y.H.B.Y. Örnek No: 4`.
- `Tarihi` alanı seçili ödeme ayının son günü değil, raporun üretildiği tarihtir (`TODAY()`).
- `Sözleşme Tarihi ve No.su` etiketi korunur.
- Temel/Ortaöğretim'de `Sözleşmeye Göre İşin Süresi` başlangıç ve bitiş tarihlerini `gg.aa.yyyy-gg.aa.yyyy Tarihleri Arasıdır.` biçiminde birleştirir; Özel Eğitim'de kaynak hücre `İş Günü` sayısını metne ekler.

OEHS `OehsOfficialDocumentFactory` bu alanlara göre düzeltildi; WPF testleri numara, etiket, üretim tarihi ve program bazlı süre metnini kontrol ediyor. [İlgili kod commit'i](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/commit/d22a1e8346f25e77c15e95f2bab769bf41a192d4). Güncel test/CI durumu aşağıdaki son doğrulama kaydında belirtilmelidir.
