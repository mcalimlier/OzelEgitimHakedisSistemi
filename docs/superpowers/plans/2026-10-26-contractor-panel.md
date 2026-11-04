# Yüklenici Firmalar paneli uygulama planı

Kullanıcının istediği kapsam: İhale Bilgileri ekranına uygulanan kod incelemesi, hata düzeltmesi, sade form ve okunabilir kayıt listesi yaklaşımını firma ekranına uygulamak. Mevcut WPF ve program ayrımı korunacak. Daha önce kaldırılan adres/telefon/e-posta/yetkili girişleri geri eklenmeyecek; eski kayıtlardaki değerleri korunacak.

Tasarım: üstte bu ekrandan erişilebilen taşıma programı seçimi (mevcut ortak program seçimine bağlı), tam genişlikte firma adı/TC-VKN/banka alanları, kısa KDV açıklaması ve belirgin doğrulama alanı. Altta arama, sonuç sayısı, uzun metinleri saran firma kartları. Yeni zorunlu kimlik/IBAN biçimi kuralları eklenmeyecek; eski hesap numaraları desteklenmeye devam edecek.

- [x] Hakedişten firma düzenlemede programlı anahtar ve KDV durumunu, düzenlemeyi iptal etmeyi ve eksik düzenlenen kayıt davranışını test et; hataları yeniden üret.
- [x] MainWindow.xaml.cs kayıt/düzenleme/iptal akışlarını düzelt; yanlışlıkla yeni firma oluşmasını önle, görünür doğrulama ekle.
- [x] MainWindow.Contractors.cs içinde arama/program filtresi ve sayacı uygula; kaynak ve taslak verilerini koru.
- [x] MainWindow.xaml ContractorsPanel alanını tam genişlikte hizalı form ve okunabilir listeye dönüştür. KDV açıklamasını kısalt, eski alanları ekleme.
- [x] Yeni kayıt, aynı programda mükerrer kayıt, diğer programdaki aynı kimlik, arama, KDV, iptal ve üç ekran genişliğini test et; görüntüleri incele.
- [x] Tüm testleri ve analizörleri açık derlemeyi çalıştır; açık Visual Studio'da tam yeniden derlemede 0 hata/0 uyarıyı doğrula. Bulguları raporla ve mevcut dala kaydet.

Dosyalar: MainWindow.xaml (görünüm), MainWindow.xaml.cs (mevcut akış bağlantıları), MainWindow.Contractors.cs (panel yardımcıları), ContractorPanelTests.cs (WPF regresyon ve yerleşim testleri). Finansal hesaplama formülleri bu çalışmanın kapsamında değil.
