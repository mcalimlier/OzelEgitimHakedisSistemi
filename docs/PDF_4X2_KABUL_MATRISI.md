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
