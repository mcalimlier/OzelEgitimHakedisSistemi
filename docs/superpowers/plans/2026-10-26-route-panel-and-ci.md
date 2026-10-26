# Hatlar paneli ve GitHub kontrol düzeltmesi

Kullanıcı, ihale/firma panellerindeki kod incelemesi ve sade arayüz yaklaşımının Hatlar paneline uygulanmasını, ayrıca GitHub Desktop'taki PR #3 durum işaretinin düzeltilmesini istedi. Aynı çalışma dalı korunur; PR kapatılmaz, birleştirme veya yayın yapılmaz. CI düzeltmesinin GitHub'da doğrulanabilmesi için mevcut dala normal push yapılır.

- [x] PR #3'ün son GitHub kontrol hatasını ve eski UI denetimlerini yeniden üret; kontrolleri kaldırmadan güncel kodun gerçek davranışına bağla, yerelde çalıştır.
- [x] Hat kaydında farklı programdaki firma seçimini, eksik düzenleme kaydını, geçersiz tarih metnini ve arşiv düzeltmesi korumalarını regresyon testleriyle denetle.
- [x] MainWindow.Routes.cs yardımcıları ve mevcut MainWindow.xaml.cs akışlarında doğrulama/iptal/silme korumalarını düzelt; 4 basamak fiyat hassasiyetini, bağlı hakedişleri ve geri alma mekanizmalarını koru.
- [x] LinesPanel'i tam genişlikte seçim/form ve okunabilir, aramalı kayıt listesiyle düzenle. Gereksiz akış kartını ve boş üçüncü sütunu kaldır; rehber alanını programa göre göster.
- [x] Üç genişlikte XAML görüntüleri, gerçek pencere ve test verisiyle kaydet/düzenle/iptal, tüm testler ve açık Visual Studio yeniden derlemesini doğrula.
- [ ] Kaydet, GitHub'a gönder, PR kontrollerinin başarılı sonucunu izle; raporla.

İlk bulgular: CI'nin son koşusu 40388126660, 'Expected one dynamic header save-status label; found 2' hatasıyla durdu; derleme 0 uyarı/0 hata ile geçmiş. Sonraki arşiv kontrolü de artık statik olan SaveRevision çağrısının eski metnini arıyor. Hat formunda sabit ve büyük kayıt sütunları dar görünümde taşma riski taşıyor. Tarih doğrulaması yalnız SelectedDate'e bakıyor; firma/ihale program tutarlılığı kaydetme aşamasında doğrulanmıyor. Kayıp düzenleme nesnesi başarı mesajıyla sessizce atlanabiliyor. Liste düzenle/sil düğmeleri arşiv düzeltmesini korumuyor.
