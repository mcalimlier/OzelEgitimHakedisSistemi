# VBA Statik Envanter ve Refactoring Kararları — 20.10.2026

## Kaynak ve kapsam
- Repo: `mcalimlier/OzelEgitimHakedisSistemi`
- Çalışma dalı: `feature/official-payment-document-templates`
- İncelenen aday yama: `OEHS_VBA_Guvenli_Ay_Aktarim_Yamasi_v4_2026-10-20`
- Güncel doğrulanmış HEAD başlangıçta: `182fccfb2de9c7db746d462f2f50bf0fded86ebd`
- Kaynak XLSM dosyaları ayrı tutulmuştur; aday yama özgün XLSM'lere uygulanmış sayılmaz.

## Otomatik statik envanter
15 VBA kaynak dosyası incelendi: 14 modül/sayfa/form kaynak dosyası ile `.bas/.cls/.frm` envanterindeki toplam 15 dosya (formların ikili `.frx` kaynakları ayrıca paket içindedir).

| Kontrol | Sonuç |
|---|---:|
| `Option Explicit` bulunan kaynak dosyaları | 15/15 |
| `Debug.Print` eşleşmesi | 0 |
| Tanımlanmış prosedür sayısı (regex tabanlı, yaklaşık) | 56 |
| `On Error GoTo` satırı (yaklaşık) | 37 |
| `MsgBox` eşleşmesi | 109 |

Sayımlar metinsel/statik taramadır; VBA derleyicisinin ayrıştırması veya tüm çalışma zamanı çağrılarını kanıtlamaz. Prosedür sayısı regex tabanlıdır; özellik prosedürleri ve devam satırları nedeniyle kesin VBA AST sayımı olarak yorumlanmamalıdır.

## Refactoring kararları
- `Debug.Print` satırı bulunmadığından bu turda debug satırı silinmedi.
- `MsgBox` kullanımları topluca silinmedi: kaynakta bunlar arasında doğrulama, hata, işlem sonucu ve kullanıcı onayı bildirimleri bulunuyor; yalnızca metinsel eşleşmeyle geçici test uyarısı diye sınıflandırılamaz.
- Kullanılmayan prosedür/değişken silinmedi. Excel buton atamaları, `Application.Run`, workbook/sheet/form eventleri, adlandırılmış aralıklar ve dışarıdan çağrılan makrolar tam çağrı grafiği olmadan güvenle dışlanamaz. Yanlış silme kullanıcı işlevlerini bozabilir.
- Kaynak XLSM'ler üzerine aday yamayı otomatik yazma işlemi yapılmadı. Geri alınabilir aday yama ayrı tutuldu.
- `Option Explicit` kontrolü yalnızca bildirim satırının mevcutluğunu doğrular; tüm değişkenlerin tanımlı olduğunu veya derlemenin başarılı olduğunu doğrulamaz.

## Önceki test kanıtları
- Aday VBA yaması v4 için önceki statik kontroller: **46/46 PASS** (yalnızca statik/invariant kontrolleri).
- GitHub Windows CI run [39477148942](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39477148942): **success**; Core **238/238**, WPF çıktı testleri **6/6**, PDF smoke artifact üretimi/aktarımı tamamlandı.
- Bu CI, kaynak XLSM içindeki VBA projesini Microsoft Excel'de derlemez veya çalıştırmaz.
- Gerçek Excel/VBA Compile + Runtime, UserForm etkileşimli testleri ve 8 PDF'nin gerçek Excel çıktılarıyla görsel kabulü hâlâ açık.

## Kabul engelleri / riskler
1. Windows Excel kurulu/etkileşimli ortamda VBA projesi derlenmedi ve makrolar gerçek workbook kopyalarında çalıştırılmadı.
2. Form kontrollerinin bir bölümü `.frx` ikili kaynaklarında olduğundan metin kaynakları tek başına kontrol adlarını/ölçülerini eksiksiz doğrulamaz.
3. Özgün kaynakta kimlik doğrulama bilgileriyle ilgili sabit değerler bulunduğu önceki incelemede not edildi; davranışı değiştirmeden önce kimlik doğrulama tasarımı ayrı güvenlik işi olarak ele alınmalı.
4. Dört PDF × iki program için gerçek Excel/VBA çıktısı ile alan alan görsel ve sayısal karşılaştırma tamamlanmadı.
5. Temel Eğitim ve Özel Eğitim formül/aktarımı birbirinden ayrı doğrulanmalıdır; özellikle son XLSM icmal zincirindeki açık farklar sessizce düzeltilmemelidir.

## Sıradaki işlem
1. Windows Excel test ortamında kaynak dosyaların kopyalarını aç; VBA Project Compile, event/makro senaryoları ve hatalı girdi senaryolarını çalıştır.
2. Buton atamaları, `Application.Run`, event çağrıları, adlandırılmış aralıklar ve workbook/sheet isimlerini kapsayan gerçek çağrı envanteri çıkar; ancak bundan sonra ölü kod sil.
3. Temel/Özel Eğitim için kaynak formül hücreleri ve 8 PDF çıktısını karşılaştır.
4. Bu doğrulamalar görülmeden %100 kabul veya hatasızlık beyan etme.

## Değişiklik özeti
Bu envanter güncellemesi VBA kodunu değiştirmez; aday yama ve resmi XLSM dosyalarını korur. PR açık/taslak kalır; ana dala merge veya yayın yapılmaz.
