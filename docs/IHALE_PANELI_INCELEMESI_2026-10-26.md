# İhale Bilgileri paneli incelemesi — 26 Ekim 2026

İhale panelinin kayıt doğrulaması, görünümü ve çözümdeki analizör uyarıları incelendi. Çalışma dalı: `feature/official-payment-document-templates`.

## Düzeltilen sorunlar

- Eski kayıtlardan gelen iki gizli tutar alanında `TryParse` sonucu dikkate alınmıyordu. Bozuk veya negatif değer artık kaydı durdurur; yeni kayıtta boş değer, bilinmeyen tutar olarak 0 saklanır.
- WPF tarih kutusu geçersiz tarih metnini önceki seçime döndürebiliyordu. Üç tarih alanında doğrulama hatası izlenir ve kayıt engellenir. Takvimden geçerli tarih seçilerek düzeltilebilir; isteğe bağlı ihale tarihi ayrıca × düğmesiyle temizlenebilir.
- Küçük ekranda sabit genişlikli etiketler giriş alanlarını daraltıyordu. Etiketler alanların üstüne taşındı; form üç eşit sütun ve iki satırdan oluşur, giriş yükseklikleri 34 pikseldir.
- Özette gizli kartın bıraktığı boşluk kaldırıldı. Üç kart kayıt durumunu ve hizmet tarihlerini gösterir.
- Kayıt listesindeki yinelenen İKN kaldırıldı. Taşıma programı, hatlardan türetilen iş adı ve hizmet tarihleri ayrı, okunabilir satırlardadır.

## Kullanım iyileştirmeleri

- İKN, iş adı ve bağlı hat adı üzerinden arama; gösterilen/toplam kayıt sayısı ve sonuç bulunamadı açıklaması eklendi.
- Program seçiliyse kayıtlar o programa göre süzülür; seçim yoksa iki programın kayıtları da gösterilir. Arama taslak alanları veya saklanan koleksiyonları değiştirmez.
- Sözleşme iş günü bilinmiyorsa boş bırakılabilir; kayıt karşılığı 0'dır. Negatif ve sayı olmayan girişler reddedilir.
- Kaydetme, düzenleme ve iptal düğmeleri açık şekilde adlandırıldı. Mevcut kayıtların iç kimlikleri ve eski verileri korunur.

## Uyarıların giderilmesi

İlk envanterde 67 ayrı uyarı konumu vardı. Sabit test dizileri ve JSON seçenekleri yeniden kullanılacak alanlara alındı; metin karşılaştırma kuralları açık belirtildi; durum taşımayan metotlar ve çağrıları statik hale getirildi; koleksiyon erişimleri düzeltildi. Doğrulamada göz ardı edilen dönüş değerleri ele alındı. Analizörler kapatılmadı ve uyarı bastırma kuralı eklenmedi. xUnit test derlemesi, dizi bağımsız değişkenli öznitelikleri nedeniyle CLS uyumlu olmayan bir test API'si olarak açıkça tanımlandı.

## Doğrulama

- Açık Visual Studio oturumunda **Tümünü Yeniden Derle / Debug Any CPU**: **4 başarılı proje, 0 başarısız; Hata Listesi: 0 hata, 0 uyarı**. Derleme 21:54'te tamamlandı. Kanıt: `.superpowers/tender-vs-final-build.log` ve `.superpowers/tender-vs-final-state.txt`.
- Standart analizör ayarlarıyla `dotnet test OzelEgitimHakedisSistemi.sln --no-restore -c Debug`: **270 çekirdek + 226 WPF = 496 başarılı, 0 başarısız, 0 atlanan test**.
- 13 yeni panel testi: bozuk/negatif eski tutarlar (4), geçersiz tarih (3), üç ekran genişliği (3), arama/program filtresi (1), boş isteğe bağlı alanlarla gerçek geçici veri deposuna kayıt (1), isteğe bağlı hatalı tarihi temizleme (1).
- Hatalı tutar/tarih senaryolarının ilk yedi testi düzeltmeden önce başarısız oldu; düzeltmeden sonra geçti.
- Gerçek WPF XAML'si 1024, 1366 ve 1920 piksel ekran genişliklerine karşılık gelen içerik alanlarında oluşturuldu. Tarih kutularının en az 140 piksel olduğu ve giriş yükseklikleri test edildi. Üç PNG ayrıca görsel olarak incelendi.
- Yeni derleme `.superpowers/tender-native-2026-10-26/app` altında, izolasyon işareti başlatmadan önce yazılarak çalıştırıldı. Gerçek ana pencerenin başlığı ve yanıt vermesi doğrulandı, ardından pencere kapatıldı. Deneme verisi bu klasörün `DenemeVerisi` altına ayrıldı.
- Yerel kanıtlar: `.superpowers/tender-red.log`, `.superpowers/tender-final-tests.log`, `.superpowers/payment-details-2026-10-26/tender-*.png`.

Ekran genişliği kontrolleri gerçek Windows %150 / 144 DPI kabulünün yerine geçmez. Bu çalışma hesaplama formüllerini veya yayımlanmış `dist` paketini yenilemez.
