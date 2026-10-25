# En düşük kabul düzeyindeki çekirdeklerin kapatılması

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Önce icmal/aktarım, sonra rapor zinciri ve PDF/WPF alanlarında açık kabul kapılarını gerçek kaynak kanıtıyla kapatmak.

**Architecture:** Finansal formüller ve daha önce onaylanan kaynak düzeltmeleri korunur. Testler kullanıcı verisinden ayrı kopyalarda çalışır. Excel/VBA çalışması, formül hesabı, otomatik PDF üretimi ve görsel/DPI kabulü ayrı kanıtlar olarak tutulur.

**Tech Stack:** .NET 8/WPF, xUnit, Microsoft Excel COM, Python statik VBA incelemesi.

**Spec:** `docs/YAYIN_ONCESI_HAKEDIS_DOGRULAMA_2026-10-25.md` içindeki beş çekirdek/25 kabul kapısı ve kullanıcının en düşükten başlayarak %100 isteği.

## Kısıtlar

- Orijinal XLSM, normal OEHS kayıtları ve kullanıcı güvenlik ayarları değiştirilmez.
- Temel/Özel programları karıştırılmaz; AP3 ve icmal çift fiyat farkı için kayıtlı kararlar korunur.
- Makro adı ve çağrı zinciri incelenmeden makro çalıştırılmaz; silme/temizleme veya dış sisteme erişen makrolar çalıştırılmaz.
- Kanıtı eksik kapı %100 diye işaretlenmez. Yayın/push/merge yapılmaz; tamamlanan çalışma mevcut dala commitlenir.

## İnceleme odağı

1. Kasım–Aralık–Ocak ve okul yılı sınırında önceki ay kesintisi.
2. Temel/Özel için farklı ay sütunları ve Temmuz/Ağustos sınırları.
3. Birden fazla ihale/firma/hat arasında veri taşınmaması; aynı ay revizyonunun iki kez sayılmaması.
4. Eksik kimlik/geçersiz dönem veya aktarım hatasında önceki kayıtların korunması.
5. Belge alanlarının kaynak hücreler, uygulama toplamları ve PDF'lerde tutarlılığı.

## Görevler

- [x] Güncel XLSM içindeki gerçek VBA aktarım yordamlarını ve bağımlılıklarını salt okunur çıkarıp incele.
- [x] Ayrı kopyalarda izin verilen kaynak düzeltmelerini kayıt altına al; makro öncesi/sonrası ay aktarımı ve E21 kanıtını üret.
- [x] Bulgular için RED test ekle; gerekli çekirdek düzeltmelerini yap; alan bazlı pariteyi doğrula.
- [x] İcmal ve rapor zinciri için çok dönem/kimlik/revizyon kapsamını tamamla.
- [ ] Gerçek Excel/OEHS belge karşılaştırmasını ve otomatik yapılabilen DPI/yerleşim kontrollerini tamamla; donanım/izin gerektiren gerçek engelleri açık bırak.
- [ ] Release testleri, kanıt raporu, kabul tablosu ve commit.

## 25 Ekim uygulama kaydı

- Gerçek VBA 49/49, 14 dönem/E21; 285 sayısal parite alanı ve iki Temmuz bağımsız geçmiş toplamı.
- Sekiz Excel/OEHS belge çifti üretildi; 74 alan, A4, imzacılar, dönemler ve sekiz çiftin render'ı incelendi.
- Kaynak sorunları: AP3 Kasım; Temel tam Temmuz başlığı; Özel Haziran MATCH aralığı; çift fiyat farkı; teslim tarihi; eski dönem/altbilgi tarihi. Asıl dosyalar değişmedi.
- Kaynak Temmuz başlığı olumsuz denemesi gerçek VBA'da hedefe yazmadan reddedildi. Yeni kaynak düzeltmesi önce başarısız parite testiyle görüldü, sonra Excel tekrar çalıştırıldı.
- Üretim hesaplama kodunda değişiklik gerekmedi. Yeni üç regresyon testi ve iki yeniden üretme aracı eklendi.
- Tam Release: 442/442 (265 + 177). Bağımsız ajan yok; öz inceleme yapıldı.
- Gerçek 96/120 DPI + altı hat doğrulandı. Kullanıcı %125 geçişini yaptı; cihazda %150 seçeneği yok. %100 geri dönüşü istendi ve kullanıcı hazır olduğunu bildirdi. Tek açık madde 144 DPI; toplam kabul 24/25 = %96.
- Güncel ayrıntılar: `docs/YAYIN_KABUL_VBA_PDF_DPI_2026-10-25.md`.
