# Yüklenici Firmalar paneli incelemesi — 26 Ekim 2026

İhale Bilgileri panelindeki inceleme ve sadeleştirme yaklaşımı firma paneline uygulandı. Çalışma dalı: `feature/official-payment-document-templates`.

## Kod incelemesi ve hata düzeltmeleri

- **Hakedişten düzenlemeye geçişte KDV seçimi:** canlı kayıt ve arşiv akışları firmanın KDV istisnasını kutuya yüklemiyordu. Önceki formun seçimi kalabiliyordu. İki akış da kendi firma durumunu yükler; eski arşivde firma durumu belirtilmemişse o arşivin kayıtlı vergi girdisi kullanılır.
- **Hakedişten düzenlemede kimlik anahtarı:** yalnız TC/vergi numarası kullanılıyordu. Bu, program içeren firma anahtarıyla karşılaştırmada kaydın kendisinin mükerrer sayılmasına, aynı kimlik iki programda varsa yanlış firma seçilmesine yol açabiliyordu. Artık program ve kimlik birlikte kullanılır; mükerrer kontrolünde düzenlenen nesnenin kendisi dışarıda tutulur.
- **Kaybolan düzenleme kaydı:** düzenlenen firma bulunamazsa yeni firma eklenebiliyordu. Artık işlem durur ve iptal edip yeni kayıt açılması istenir.
- **Arşiv düzeltmesi sırasında liste işlemleri:** firma listesinin düzenle/sil düğmeleri devam eden arşiv düzeltmesini bırakamaz veya canlı firma silemez.
- **İptal ve silme sonrası form:** iptal edilen taslak temizlenir, kayıtlı firma korunur. Düzenlenen firma kullanıcı onayıyla silinirse artık formda eski düzenleme anahtarı bırakılmaz.
- Mevcut disk hatası geri alma mekanizmaları, bağlı ihale/hat/hakediş güncellemeleri ve kullanılan firmaların silinmesini engelleyen kontroller korunur. Boş zorunlu alan ve mükerrer kayıt açıklamaları panel içinde gösterilir.

## Arayüz

- Dar iki sütunlu ekran yerine tam genişlikte, üstten etiketli ve aynı yükseklikte girişler kullanıldı.
- Taşıma programı bu panelden de seçilebilir; İhale Bilgileri ekranındaki mevcut seçimle aynı değeri paylaşır. Düzenlemede firma programı ayrı ve değiştirilemez biçimde gösterilir.
- KDV istisnasının yeni hesaplamalarda KDV/tevkifata etkisi kısa açıklamayla belirtildi.
- Kayıt listesinin üstüne arama ve gösterilen/toplam sayacı eklendi. Firma adı, boşluklu TC/vergi numarası ve banka bilgisiyle arama yapılabilir. Program seçili değilse iki programın firmaları da gösterilir.
- Uzun firma unvanları ve banka bilgileri kartlarda satıra sarılır. Düzenle/sil düğmeleri metinden ayrıldı; sonuç bulunamadığında açıklama gösterilir.
- Önceden kaldırılmış iletişim alanları geri eklenmedi. Eski kayıtlardaki adres, telefon, e-posta ve yetkili değerleri korunur. Banka alanı hem hesap numarasını hem eski IBAN kayıtlarını kabul etmeye devam eder.

## Testler

- Açık Visual Studio'da son **Tümünü Yeniden Derle / Debug Any CPU** sonucu: **4 başarılı proje, 0 başarısız; Hata Listesi: 0 hata, 0 uyarı**. Son görsel düzeltmeler de bu derlemeye dahildir. Kanıtlar: `.superpowers/contractor-vs-final-build.log` ve `.superpowers/contractor-vs-final-state.txt`.
- Hata senaryolarının ilk çalıştırmasında **5 başarısız, 1 başarılı** test görüldü: dört KDV yükleme senaryosu ve kaybolan düzenleme kaydı hatası yeniden üretildi; mevcut iptal modu kontrolü zaten geçiyordu.
- Son çalıştırmalarda **270 çekirdek + 245 WPF = 515 test başarılı**, başarısız ve atlanan test yok. Test kanıtları `.superpowers/contractor-tests.log`, `.superpowers/contractor-wpf-final-tests.log` ve son derleme üzerindeki tekrarın `.superpowers/contractor-final-tests.log` dosyalarındadır.
- **19 yeni WPF senaryosu:** canlı/arşiv KDV ve anahtar (4), iptal (1), kayıp düzenleme kaydı (1), 1024/1366/1920 genişlikleri (3), arama/program ayrımı (1), iki KDV durumuyla diske kayıt ve aynı kimliğin diğer programda bulunması (2), aynı programda mükerrer kayıt (1), zorunlu alanlar (3), arşiv düzeltmesi korumaları (2), kullanılan firmayı silme koruması (1).
- Üç genişlikte gerçek XAML oluşturuldu; giriş yükseklikleri, asgari kullanılabilir genişlikler ve uzun unvanın küçük ekranda satıra sarılması denetlendi. `.superpowers/payment-details-2026-10-26/contractor-*.png` görüntüleri görsel olarak incelendi.
- Ayrı deneme klasöründe gerçek `App` ve `MainWindow` ile yerel/ortak program seçiminin iki yönlü eşleşmesi, gerçek Kaydet/Düzenle/İptal düğmeleri, firma KDV seçimi ve diske yazılan program kaydı kontrol edildi. Son görsel düzeltmelerle tekrarlandı. Kabul kaydı: `.superpowers/contractor-final-native-2026-10-26/app/contractor-smoke-result.txt`; gerçek pencere görüntüsü aynı klasördeki `contractor-native.png` dosyasıdır. Ölçülen gerçek DPI: 96. Normal kullanıcı verileri yerine bu çalıştırıcının `DenemeVerisi` klasörü kullanıldı.

Bu çalışma finansal hesaplama formüllerini veya `dist` dağıtım paketini yenilemez. Genişlik testleri, henüz gerçekleştirilemeyen gerçek %150 / 144 DPI kabulü yerine sayılmaz.
