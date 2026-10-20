# OEHS / TEHYS — VBA kaynak ve Excel formül doğrulaması
**Tarih:** 20.10.2026

## VBA sayfa referansları
- Temel Eğitim kaynak arşivinde 11 VBA modül/form/sayfa sınıfı, çalışma kitabında 11 sayfa bulundu.
- Özel Eğitim kaynak arşivinde 3 VBA modül/sayfa sınıfı, çalışma kitabında 8 sayfa bulundu.
- Kaynak kodundaki sabit Sheets(...) / Worksheets(...) referanslarıyla çalışma kitabı sayfa adları karşılaştırıldı. İki programda da eksik sabit sayfa referansı: **0**.
- Bu kontrol dinamik adları, buton atamalarını ve çalışma zamanı erişimlerini garanti etmez.

## Kritik formül ve hücre sonuçları (salt okunur XLSM kontrolü)

### Temel Eğitim
| Hücre | Formül / cached sonuç |
|---|---|
| Hesaplama Bilgileri!T4 | `=I4*6/100` → 441.921 |
| Hesaplama Bilgileri!U4 | 3033.523428144 |
| Hesaplama Bilgileri!W4 | 332516.074511856 |
| Hakediş Raporu!D15 | 3033.523428144 |
| Hakediş Raporu!D28 | 332516.08 |

### Özel Eğitim — AP3 düzeltilmiş kopya
| Hücre | Formül / cached sonuç |
|---|---|
| Hesaplama!AC4 | 661.8 |
| Hesaplama!AD4 | 736.2857196 |
| Hesaplama!AE4 | 27328.7843196 |
| Hesaplama!AF4 | 88837.724 |
| Hesaplama!AG4 | 61508.9396804 |
| Hakediş Raporu!D15 | 736.2857196 |
| Hakediş Raporu!D28 | 61508.94 |
| İcmal Verileri!AP3 | `=Anasayfa!I8` → Kasım 2026 |
| İcmal Verileri!AQ3 | `=Anasayfa!I9` → Aralık 2026 |

## Yama ve statik testler
- Güvenli ay bazlı silme, başlık doğrulama, Temel Eğitim yazdırma hata yolunda ScreenUpdating geri yükleme ve iki programın Module2 dışa aktarma hata raporlaması için aday `.bas` kaynakları pakete eklendi.
- Statik regresyon kontrolü: **27/27 PASS**.
- Bunlar VBA derlemesi veya gerçek Excel çalışma zamanı testleri değildir. Kaynak XLSM'ler bu aday modüllerle değiştirilmedi.
- Temel Eğitim Module2 içindeki kaynakta gömülü kullanıcı adı/şifre korunmuştur ve güvenlik riski olarak kaydedilmiştir.

## Açık kabul kapıları
1. Microsoft Excel VBA IDE `Debug > Compile`.
2. Kopya XLSM üzerinde boş/geçersiz ay, bozuk başlık, seçili ay silme ve önceki/sonraki ayların korunması testleri.
3. Temel Eğitim `T4/U4/W4`, rapor `D15/D28`; Özel Eğitim `AC4:AG4`, rapor `D15/D28` değerlerinin gerçek yeniden hesaplamadan sonra kaydı.
4. Her iki programın dört PDF'sinin, toplam **8 PDF**, gerçek Excel çıktılarıyla tutar, dönem, yüklenici, hat, imza ve sayfa düzeni açısından karşılaştırılması.
5. PR #3 açık/taslak kalmalı; açık onay olmadan merge/release yok.