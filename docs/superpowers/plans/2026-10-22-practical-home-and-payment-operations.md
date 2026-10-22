# Pratik Ana Ekran ve Hakediş İşlemleri Implementation Plan

**Goal:** İKN ile çalışan sade aylık merkez, güvenli tam yedek, düzeltme karşılaştırması ve geçmiş/arşiv filtreleri.
**Architecture:** Mevcut ilişki anahtarlarını koru. Yedek, takip, kontrol, filtre ve fark mantığı bağımsız servisler; WPF adaptörleri ayrı partial dosyalar. PDF üretiminin staging/rollback davranışı korunur.
**Tech Stack:** .NET 8, WPF/XAML, xUnit, System.IO.Compression, SHA256.
**Spec:** docs/superpowers/specs/2026-10-22-practical-home-and-payment-operations-design.md

## Global Constraints
- Yeni resmî sözleşme numarası alanı yok; yalnız İKN. İç kimlikler korunur, INTERNAL değerleri PDF'ye basılmaz.
- Tam decimal hassasiyeti, program ayrımı, arşiv revizyonları korunur.
- Mevcut aktif dalda commit; push/PR/merge yok.
- Ana Ekran: bağlam/sayaç, aylık iş listesi, seçili ödeme kontrolü. Gereksiz eski kartlar kaldırılır.

## Review Focus
- Program/hat/dönem karışması, eski seçimle PDF üretimi.
- Eski JSON ve arşivlerin okunması; bozuk arşivlerin görünür kalması.
- ZIP yol kaçışı, bozuk dosya, başka köke taşınabilirlik, yarım geri yükleme.
- Sıfır gün/fiyat farkı ayları, mükerrer ödeme ve eski PDF'nin tamamlandı sayılmaması.
- Filtre sonrası doğru kayıt kimliğiyle işlem ve Türkçe harfler.

### Task 1: Ortak iş servisleri ve testler
- [ ] Kontrol, Türkçe arama/filtre, aylık durum ve alan farkı testlerini önce yaz, eksik servis nedeniyle RED doğrula.
- [ ] `PaymentOperationsService` ve veri nesnelerini ekle; gerçek kimlik/dönem sınırları, sıfır gün, fark hassasiyeti, güncel çıktı kontrollerini doğrula.
- [ ] PDF çıktı kanıtı servisi: göreli dosya yolları, SHA256 ve bağlam parmak izi; eksik/değişmiş dosya ve bağlamı reddet.
- [ ] Core testlerini GREEN çalıştır, commit.

### Task 2: Tam yedek ve geri yükleme
- [ ] Geçici veri kökleriyle roundtrip, bozuk paket, ZIP path traversal ve geri yükleme güvenliği testlerini RED çalıştır.
- [ ] Paket bildirimi/sürüm/özet, sınırlı çıkarma, JSON doğrulama, otomatik kurtarma yedeği ve rollback ile `OehsBackupService` ekle.
- [ ] WPF Ayarlar bölümüne yedek al/incele/geri yükle işlemlerini bağla; düzenleme sırasında engelle; geri yükleme sonrası veriyi yenile.
- [ ] Core testlerini ve WPF derlemesini doğrula, commit.

### Task 3: Sade Ana Ekran ve ortak PDF kontrolü
- [ ] Yeni Ana Ekran WPF sözleşme/ölçüm testlerini RED çalıştır.
- [ ] Eski Home kartlarını kaldır; filtreler, üç sayaç, aylık tablo, seçili ödeme kontrolü, hedefe git/önizle/kaydet ekle.
- [ ] Eski Home güncelleme metotlarını yeni partial adaptöre taşı; kaldırılan kontrollere başvuru bırakma.
- [ ] PDF yollarını seçili ödeme bağlamına bağla, çıktılar ekranında da kontrol uygula; başarılı dört PDF için bağlam kanıtı yaz.
- [ ] INTERNAL kimliğini PDF'de gösterme; gerçek İKN'yi koru. CI yol haritası kontrolünü yeni davranışla değiştir.
- [ ] WPF testlerini GREEN çalıştır, commit.

### Task 4: Karşılaştırma ve geçmiş/arşiv filtreleri
- [ ] Gerçek önce/sonra alanları, oran değişikliği ve Türkçe arama/dönem/program/revizyon testlerini RED doğrula.
- [ ] Ortak kaydırılabilir karşılaştırma iletişim penceresini mevcut iki onay yoluna bağla; iptalde yazma yok.
- [ ] Geçmiş ve arşiv filtrelerini ekle; mevcut kalıcı satır kimliği ve bozuk arşiv satırını koru; sonuç sayısı ve boş durum.
- [ ] WPF ve Core testlerini GREEN çalıştır, commit.

### Task 5: Son kontrol
- [ ] Tüm Release testleri, CI statik kontrolleri, diff kontrolü ve yeni ekranların ölçüm/yerleşim doğrulaması.
- [ ] Ayrı öz inceleme: özellikle restore rollback, çıktı bağlamı ve filtrelenmiş satır kimliği. Bağımsız ajan erişimi yoksa bunu raporla.
- [ ] Devam notunu gerçek sonuçlarla güncelle, commit ve temiz çalışma ağacını doğrula.
