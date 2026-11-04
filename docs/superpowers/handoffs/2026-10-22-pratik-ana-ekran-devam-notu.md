# Pratik Ana Ekran — devam notu

- Çalışma alanı: `C:\Users\MuratPC\Documents\GitHub\OzelEgitimHakedisSistemi`
- Dal: `feature/official-payment-document-templates`
- Kullanıcı ayrı resmî sözleşme numarasını kapsamdan çıkardı: İKN kullanılacak. Diğer beş madde uygulandı.
- Tasarım: `docs/superpowers/specs/2026-10-22-practical-home-and-payment-operations-design.md`
- Plan: `docs/superpowers/plans/2026-10-22-practical-home-and-payment-operations.md`
- Kullanım: `docs/PRATIK_ANA_EKRAN_VE_YEDEKLEME.md`

## Uygulananlar

1. Ana Ekran: program/ihale/ay bağlamı, üç durum sayacı, aranabilir aylık hat listesi, seçili ödeme kontrolü, ilgili kayda geçiş, önizleme/dört PDF kaydetme. Eski tekrarlayan kartlar ve yol haritası kaldırıldı.
2. Ayarlar: doğrulanmış tam yedek ZIP, içerik incelemesi, otomatik kurtarma yedeği ve iki veri kökü için geri alma. Geçici klasörlerle roundtrip/bozuk ZIP/yol kaçışı/hatalı oran/ikinci kökte hata testleri var. Başarılı geri yükleme de `.before-…` kurtarma klasörlerini korur.
3. Düzeltme: canlı geçmiş ve arşivde yalnız değişen alanlarla önce/sonra penceresi, hassas girdiler, oran değişikliği, hesap ve PDF netleri.
4. Geçmiş/arşiv: Türkçe arama, İKN/firma/dönem/durum filtreleri, arşivde varsayılan son revizyon ve eski revizyon seçeneği. Bozuk arşivler görünür.
5. PDF durumu: dört dosya SHA256'sı, rapor bağlamı parmak izi, göreli yollar ve programa göre çıktı kanıtı. INTERNAL kimliği PDF sözleşme satırından gizlenir; eski açık numaralar korunur.

Yeni sorumluluklar `MainWindow.Operations`, `.RecordFilters`, `.Backup`, `.Comparison` partial dosyalarına ve ayrı Core servislerine ayrıldı.

## Doğrulama

- Son `dotnet test .\OzelEgitimHakedisSistemi.sln -c Release --no-restore`: çıkış 0; Core 260/260, WPF 130/130, toplam **390/390**, başarısız/atlanan 0.
- CI'nin dört `Verify` PowerShell adımı yerelde başarılı.
- `git diff --check` temiz. CA analiz uyarıları sürüyor.
- Ana Ekran 1024×600, 1280×720, 1366×768, 1920×1080'de gerçek WPF ölçüm testinden geçti; XAML parçasının RenderTargetBitmap çıktısı gözle incelendi.
- Ana pencere gerçek kullanıcı verileriyle başlatılmadı; testler constructor/fresh-start işlemi olmadan çalıştı. Gerçek kullanıcı verilerinde yedek geri yükleme yapılmadı.
- Bağımsız ajan aracı yoktu; son inceleme öz inceleme olarak yapıldı. Önceki CLI denemeleri model erişimi hatası vermişti.
- Son incelemede aynı GUID'nin iki programda bulunması hâlinde yanlış geçmiş satırının seçilmesi yakalandı; test önce SpecialEducation/BasicOrSecondary uyuşmazlığıyla RED oldu, programlı seçim/düzeltme/filtre/çıktı kanıtı ayrımı sonrası tüm testler GREEN.

## Kararlar ve sonraki iş

- Kullanıcının mevcut dal tercihi korundu; yeni worktree açılmadı.
- Başarılı geri yüklemede `.before-…` klasörleri otomatik silinmez: eski reset yedekleri veya paket kapsamı dışındaki dosyalar kaybolmasın. Diskte ek alan kullanır.
- Yeni büyük özellik gerekmiyor. Kullanıcı isterse gerçek günlük iş akışıyla etkileşimli kabul testi ve bağımsız inceleme yapılabilir.
- Push, PR veya merge yapılmadı. Kullanıcı ayrıca isterse yapılır.
