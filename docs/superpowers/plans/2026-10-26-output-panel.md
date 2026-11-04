# Çıktılar paneli inceleme ve sadeleştirme planı

Kullanıcı Çıktılar panelinin kodunun incelenmesini ve daha sade, kaliteli bir görünümle düzenlenmesini istedi. Mevcut WPF/lacivert-altın tasarım ve dört A4 belge şablonu korunacak. Önceki panel çalışmalarındaki yetkiyle aynı dalda ilerlenir; finansal formüller değiştirilmez.

Tasarım: üstte tam genişlikte ihale ve hat/firma seçimi; ardından seçili hakedişin brüt/net özeti ve hat kayıt sayısı. Dönem kayıtları seçilebilir listede, seçilen kaydın bilgileri ve dört belge açıklaması yanında. Önizle, Yazdır, Dört PDF'yi Kaydet ve Klasörü Aç tek işlem alanında. Geçersiz seçimde işlemler kapalı; açıklama panel içinde gösterilir. Kayıt listesi ve dosya hedefi aynı hakedişe bağlı olmalıdır.

- [x] Program karışması, boş seçimde global kayıtların gösterilmesi ve yanlış PDF hedefi sorunlarını regresyon testleriyle yeniden üret.
- [x] MainWindow.Outputs.cs içinde kayıt kapsamını ve seçili ödeme kimliğini merkezileştir; dönem seçimi, özet, ön kontrol ve dosya hedefini bağla.
- [x] ReportsPanel XAML'sini hizalı seçimler, seçilebilir kayıt listesi ve tek işlem alanıyla düzenle. Dört belge ve ilgili kontrollerin adlarını koru.
- [x] Önizle/yazdır/PDF kaydet akışını seçili ödeme üzerinde çalıştır; dosya üzerine yazma onayını ve dört dosyanın birlikte kaydedilme/geri alınma korumasını koru. Kaydetme bağlamı hazırlama hatasını da hata sınırına al.
- [x] İki program, iki dönem, boş/uyumsuz seçimler ve 1024/1366/1920 genişlikleri için testleri ve görüntüleri doğrula. Gerçek pencerede seçili eski/yeni dönemi PDF olarak kaydet ve belge metinlerini kontrol et.
- [x] Tüm testleri, açık Visual Studio derlemesini ve yerel CI kontrollerini doğrula; raporla. GitHub CI sonuçlarını push sonrasında izle.

İlk bulgular: UpdateReportView program filtresi kullanmıyor; hat yokken tüm geçmişi gösteriyor. GetSelectedRouteLatestPayment ve GetSelectedRoutePayments boş seçimde global geçmişe dönüyor. Kayıt listesi seçilemediği için işlemler daima son ödeme üzerinden çalışıyor. Gösterilen PDF klasörü güncel hat kartındaki firma bilgileriyle, gerçek kayıt yolu ise hakedişteki firma bilgileriyle hesaplanıyor. Kaydetme bağlamı try/catch dışında hazırlanıyor.
