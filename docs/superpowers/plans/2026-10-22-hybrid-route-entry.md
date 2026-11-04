# Kullanıcının menü ve hat bazlı hakediş isteği

Kaynak: 22.10.2026 tarihli kullanıcı talimatı ve TEHYS Hakediş Yönetim Paneli SON.png.

- İhale Bilgileri ile Yüklenici Firmalar arasına Resmî Belge İmzacıları düğmesi ve ayrı panel.
- Çıktılar, Hakediş Oluştur'dan hemen sonra ve orta menü ayırıcısından önce.
- İcmal ve Kabul Tutanağı gezinme düğmeleri kaldırılır. Dört resmî belgenin üretimi/çekirdek hesapları korunur.
- Yeni hakedişte tek veya çok hat için aynı Excel-hibrit tablo kullanılır. Gün, fiyat farkı, iskonto ve kesintiler her hatta ayrıdır. Kayıtlı günlük fiyatların hassasiyeti korunur.
- Ortak form alanları satır hesabını engelleyemez veya satır değerlerini otomatik ezemez. Hatalı hücre varken kayıt yok. Yalnız seçili satırlar kayda geçer; seçimden çıkarılıp geri eklenen hattın aynı aydaki taslağı korunur. Dönem değişince aylık girdiler sıfırlanır.
- Satır neti ve toplamlar canlıdır. Arşiv/geçmiş düzeltmesinin mevcut tek kayıt akışı korunur.

## Uygulama ve doğrulama
1. Menü sırası, imzacı paneli ve satır davranışları için RED testler.
2. XAML/gezinti, satır modeli ve güvenli commit/hesap akışı.
3. Tek/çok hat, yeniden seçim, farklı satır değerleri, geçersiz hücre, Temel/Özel kapsamı ve çözünürlük testleri.
4. Release tüm testler, CI statik kontrolleri, ekran görüntüsü/ölçüm, öz inceleme ve mevcut dala commit. Bağımsız ajan aracı yok; push/PR/merge yapılmaz.

## Sonuç

Uygulama tamamlandı. Standart Release: 395/395 test (260 Core, 135 WPF). Dört CI statik kontrolü başarılı. Geçersiz hücre ve gizli ortak formdan veri sızması regresyonları önce başarısız testle doğrulanıp düzeltildi. Üç çözünürlükte yerleşim testi ve WPF render alındı.
