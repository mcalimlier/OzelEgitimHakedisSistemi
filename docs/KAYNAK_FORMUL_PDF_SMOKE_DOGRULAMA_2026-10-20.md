# OEHS Kaynak Formül ve PDF Smoke Doğrulaması — 20.10.2026

## Kapsam
Bu kayıt, son XLSM dosyalarının formül metinleri ve önbelleğe alınmış sonuçları ile son CI PDF smoke arşivinin yapısal kontrolünü belgeler. Microsoft Excel/VBA çalışma zamanı testi veya 8 nihai PDF'nin Excel çıktılarıyla görsel kabulü değildir.

## Kaynak dosyalar
- Temel Eğitim: `03-Temel Eğitim Taşıma Ödeme SON(1).xlsm`
- Özel Eğitim: `02-Özel Eğitim Hakediş Ödeme SON(1).xlsm`
- Özel Eğitim formül düzeltme kopyası ayrıca bulundu; kaynak kabulü olarak ayrı tutuldu.
- Dosya içerikleri bu kontrol sırasında değiştirilmedi.

## Sonuçlar
- Temel Eğitim formül ve cache kontrolleri: T4, U4, W4, Hakediş Raporu D15 ve D28 dahil geçti.
- Özel Eğitim formül ve cache kontrolleri: AC4, AD4, AE4, AF4, AG4, Hakediş Raporu D15 ve D28 dahil geçti.
- Toplam formül/cache kontrolleri: **19/19 PASS**.
- CI smoke arşivi: `OEHS-PDF-Smoke-Review-39480959706.zip`; **16 PDF'nin 16'sı** tek sayfa, A4 ölçüsünde ve çıkarılabilir metin içeriyor.

## Önemli yorum sınırları
- XLSM kontrolleri formül metinlerini ve dosyaya kaydedilmiş hesaplama önbelleklerini doğrular; Excel'de tam yeniden hesaplama yapıldığını veya VBA makrolarının derlenip çalıştığını kanıtlamaz.
- 16 PDF smoke çıktısı, kabul hedefi olan Temel Eğitim 4 PDF + Özel Eğitim 4 PDF ile aynı şey değildir. Bu nedenle **8/8 gerçek Excel PDF kabulü tamamlanmış değildir**.
- Temel Eğitim kaynakta W4 = 332516.074511856 iken Hakediş Raporu D28 = 332516.08 (iki ondalığa yuvarlanmış) olarak kayıtlıdır. Bu fark kaynak formülleriyle uyumludur; değiştirilmedi.
- İcmal C8/D8/E8 zincirinde daha önce işaretlenen olası fiyat farkı tekrar sayımı ayrı karar gerektiren fark olarak kalır; bu testte sessizce değiştirilmedi.
- Temel Eğitim ve Özel Eğitim ayrı hesaplama kuralları olarak korunur.

## VBA yaması
V4 aday yamasının statik invariant test kaydı **46/46 PASS** olarak mevcuttur. Bu sonuç VBA derleyicisi veya runtime testi değildir. Kaynak XLSM'lere otomatik içe aktarım yapılmadı.

## Açık kabul engelleri
1. Microsoft Excel VBA Project Compile ve gerçek makro/event çalışma zamanı.
2. UserForm kontrolleri, Click event eşleşmeleri ve ekran/DPI görsel kabulü.
3. Aynı dönem ve hat için her programın dört nihai PDF'sinin üretilmesi; toplam 8/8 sayısal ve görsel karşılaştırma.
4. Özgün Excel ile OEHS arasındaki bütün tutar/alan farklarının hücre bazında kaydı.

## Sıradaki adım
Excel masaüstü ortamında test kopyalarında derleme ve makro senaryoları; ardından 8 nihai PDF'nin alan alan karşılaştırması. PR açık kalmalı, ana dala merge/yayın yapılmamalıdır.
