# Hat bazlı hibrit hakediş girişi — 22.10.2026

Kullanıcının dört maddelik talebi uygulandı:

- Resmî Belge İmzacıları, İhale Bilgileri ile Yüklenici Firmalar arasında ayrı düğme/panel oldu.
- Çıktılar, Hakediş Oluştur altına ve menü ayırıcısından önce taşındı.
- İcmal ve Kabul Tutanağı menü düğmeleri kaldırıldı; çekirdek ve dört PDF üretimi korundu.
- Yeni hakedişte tek veya çok seçili hat için Excel-hibrit tablo hesap kaynağı oldu. Gün, fiyat farkı, iskonto ve kesintiler hat bazında girilir; satır netleri ve toplamlar canlı hesaplanır. Sütun grupları görünümü sadeleştirir. Günlük fiyatlar mevcut hat/sözleşme kaynağından hassasiyet korunarak alınır.

Gizli ortak form yeni kayıtları etkilemez. Seçimden çıkarılıp yeniden eklenen hattın dönem içi taslağı korunur. Hatalı hücre varken tablo yeniden kurulmaz ve kayıt yapılmaz. Geçmiş/arşiv düzeltmesinin tek kayıt akışı korunur.

Doğrulama: standart Release çözüm testleri 395/395 (Core 260, WPF 135); dört CI statik kontrolü geçti. Derleyici analiz uyarıları mevcut, test hatası yok. Tek/çok hat gerçek kayıt entegrasyonu, ortak alanlardan bağımsız hesap, geçersiz hücre, taslakların korunması ve 1024×600/1366×768/1920×1080 yerleşimleri test edildi. Öz inceleme yapıldı; bağımsız inceleme ajanı kullanılamadı.

Yerel test kayıtları ve WPF render: `.superpowers/sdd/2026-10-22-hybrid-route-entry/`. Gerçek kullanıcı veri klasöründe uygulama başlatılmadı; testler geçici veri kökleri kullandı. Mevcut dal kullanıldı; push, PR veya merge yapılmadı.
