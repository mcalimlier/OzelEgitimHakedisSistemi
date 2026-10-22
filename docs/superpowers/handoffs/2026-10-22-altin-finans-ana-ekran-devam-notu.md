# Altın Vurgulu Finans ana ekranı

Kullanıcı 3. tasarımı seçti; özellikle sembollerin de uygulanmasını istedi. Ana ekran gerçek WPF olarak yeniden düzenlendi: lacivert zemin, altın net kartı, üstte dört ve altta dört eşit kart, alt sırada altın sol çizgi, sekiz ayrı vektör simge. Simgeler font/emoji bağımlılığı olmadan Geometry + Path ile çizilir: para kesesi, para yığını, kesinti belgesi, yüzde, otobüs, fiyat farkı belgesi, güzergâh ve firma binası. Altın işlem düğmesine de belge/ekle simgesi eklendi.

Eski üç durum düğmesi kaldırıldı. Liste arama/durum seçenekleri açılır bölüme taşındı. Tablo koyu temaya uyarlandı. Belge kontrolü yalnız bir hat seçilince görünür; önizleme, dört PDF, geçmiş/arşiv düzeltmesi korunur. İKN listesindeki hatlardan türetilen iş adı bağlantısı korundu.

Kartlar yalnız üstteki program / İKN / ay kapsamını izler. Kaydedilmiş son hat hakedişlerinin mevcut hesapları toplanır; formüller tekrar yazılmaz. Liste araması/durum filtresi bu toplamları değiştirmez. Hat oranı kayıtlı/beklenen, firma sayısı kapsamdaki hatların benzersiz firma kimlikleridir. Taşıma tutarı rehber hariç hizmet tutarı; fiyat farkı taşıma + rehber, iskontolar sonrası KDV hariçtir. Kart araç ipuçları kapsamı açıklar. Boş seçim/dönem veya yükleme hatasında eski toplamlar tutulmaz.

Yeni kaynak: `MainWindow.HomeFinance.cs`; yerleşim/simgeler `MainWindow.xaml` içindeki HomePanel.Resources ve HomeFinanceCards. Kapsam bağlantısı `MainWindow.Operations.cs` içindedir.

Doğrulama: standart Release çözüm testleri 397/397 (260 Core, 137 WPF), dört CI statik kontrolü başarılı. Yeni testler sekiz simgeyi, kartların yerleşimini, İKN/program/ay filtrelerini, son kaydın bir kez sayılmasını, liste aramasından bağımsız toplamları ve boş dönemi kapsar. Dört çözünürlükte yerleşim sınandı (1024×600, 1280×720, 1366×768, 1920×1080). Simgeler ve altın düğme gerçek WPF render ile görsel olarak kontrol edildi. Derleyici analiz uyarıları mevcut; test hatası yok. Öz inceleme yapıldı; bağımsız inceleme ajanı aracı yoktu.

Render/test kayıtları yerel ve Git dışında: `.superpowers/gold-finance/`, `.superpowers/gold-finance-verified.log`, `.superpowers/gold-finance-layout-final.log`. Render örnek rakamlarla gerçek panel XAML'inden alınır; kullanıcı verisi değildir. Gerçek uygulama başlangıcı/veri göçü çalıştırılmadı. Mevcut dala commit; push, PR veya merge yok.
