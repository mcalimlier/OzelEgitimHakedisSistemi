# Hakediş Oluştur yayın öncesi denetim devri

Kullanıcı, kaynak Excel deneme verileriyle gerçek panel hesabı/kaydının karşılaştırılmasını, kart girişlerinin hizalanmasını ve çekirdek ilerlemesinin yüzdelerle raporlanmasını istedi. Çalışma mevcut `feature/official-payment-document-templates` dalında yapıldı; yayın/push/PR/merge yok.

Üretim değişikliği yalnız Hakediş Oluştur'un İKN/firma/dönem kartlarında: ortak SharedSizeGroup başlık/etiket/giriş satırları, eşit 34 DIP seçim kutuları, eksik firma etiketi. Dar ekrandaki yaklaşık 8 DIP fark giderildi. Alt formun altı textbox çifti de üç çözünürlükte ölçülerek doğrulandı.

Yeni kalıcı denetimler: `PaymentReleaseAuditTests.cs`, iki kimliksiz sayısal JSON fixture'ı, `scripts/Verify-PaymentPanelExcelSources.ps1`. Betik Excel 16.0 ile iki SON.xlsm dosyasını salt okunur açar, makroları kapatır, tam hesaplar, 24 senaryo/236 alan ve 20 kaynak SUM toplamını kaydeder. Kaynak dosya hash'leri önce/sonra eşittir.

Önemli denetim düzeltmesi: G17/Value2 ikili sayı artığını kullanıcı girdisine çevirmek, özellikle 5 Temel Eğitim hattının toplam teminatında yapay yarım-kuruş farkı yaratabiliyordu. Girdiler Excel'in G15 hassasiyetiyle alınır; çıktının ham Value2'si korunur, para beklentisi Excel ROUND'dan alınır. Uygulamanın decimal hesap formülleri değiştirilmedi, erken iki ondalık yuvarlama eklenmedi.

Ayrı `.superpowers/release-2026-10-25/qa-app` uygulamasına hayalî firma/hat adlarıyla sekiz sayısal kaynak satır hazırlandı. Gerçek pencerenin ihale/firma/ay/hat seçimi ve Hesapla/Kaydet düğmesi klavyeyle çalıştırıldı. Kaydedilmiş 76 satır alanı ve 20 toplu alan bağımsız karşılaştırmada geçti. Özel Eğitim toplamı 161.386,94 TL; Temel/Ortaöğretim 700.159,40 TL. QA kapatıldı; kullanıcının normal verileri değiştirilmedi.

Standart Release: 439/439 (Core 265, WPF 174). Son fixture/20 toplam ve alt textbox hizalama kontrolleri de tam çözümde geçti. Dört CI kontrolü ve 24/24 PDF smoke dosyası başarılı. Analiz uyarıları mevcut. Öz inceleme yapıldı; bağımsız ajan aracı mevcut değildi.

Ana rapor: `docs/YAYIN_ONCESI_HAKEDIS_DOGRULAMA_2026-10-25.md`. Fiziksel Core kütüphanesi 1, önceki kabul panosundaki işlevsel çekirdek 5. Kanıt kapılarıyla ölçülen yayın kabulü sırasıyla %80/%80/%60/%60/%60, genel 17/25 = %68. Bu, hata olasılığı veya kod satırı yüzdesi değildir.

Açık kabul: kaynak VBA aktarımı sonrası kontrol; iki program × dört resmî PDF'nin Excel kaynak çıktılarıyla tüm alan/görsel kabulü; gerçek %100/%125/%150 DPI. Mevcut 4 icmal farkı kaynak fiyat farkı tekrarına ilişkin önceki karardır; tam eşleşme diye sayılmadı. Hesap neti ile rapor netinin program-özel yuvarlama zinciri korunur.
