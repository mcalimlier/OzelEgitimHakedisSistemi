# Program Bazlı Excel Kabul Kuralları — 2026-10-21

## Bağlayıcı kabul ilkesi

Temel Eğitim ve Özel Eğitim farklı hesaplama programlarıdır. Bir programın tutarları, diğer programın tutarlarıyla karşılaştırılarak doğrulanamaz. Her program yalnızca kendi en güncel XLSM kaynağı, formül zinciri, VBA aktarımı ve kendi gerçek örnek sonuçlarıyla kabul edilir.

## Temel Eğitim

- Kaynak: en güncel Temel Eğitim XLSM dosyası.
- Temel kontrol noktaları: `Hesaplama Bilgileri!T4`, `U4`, `W4`; `Hakediş Raporu!D15`, `D28`; ilgili icmal hücreleri.
- `Hesaplama Bilgileri!K4` fiyat farkını içeriyorsa bu bilgi, Temel Eğitim içindeki icmal hücrelerinin kendi formülleriyle birlikte değerlendirilir.
- Kaynak Excel'in ürettiği sonuçtan farklı bir ödeme tutarı, yalnızca olası mükerrer ekleme şüphesiyle uygulanmaz. Düzeltme için aynı programın formül/aktarım zinciri ve beklenen çıktı arasında açık, tekrarlanabilir kanıt gerekir.
- Kesintilerin fiyat farkı alanında görünmesi gibi alan anlamı sorunları, kaynak Excel'in davranışından ayrı bir bulgu olarak kaydedilir; tutar hesabı sessizce değiştirilmez.

## Özel Eğitim

- Kaynak: en güncel Özel Eğitim XLSM dosyası.
- Temel kontrol noktaları: `Hesaplama!AC4:AG4`; `Hakediş Raporu!D15), `D28`; ilgili icmal hücreleri.
- `K4+T4` aylık aktarımı ve icmaldeki fiyat farkı hücreleri yalnızca Özel Eğitim formülleri ve aynı programa ait beklenen sonuçlarla incelenir.
- Kaynak Excel'in ürettiği sonuçtan farklı bir ödeme tutarı, yalnızca olası mükerrer ekleme şüphesiyle uygulanmaz. Düzeltme için aynı programın formül/aktarım zinciri ve beklenen çıktı arasında açık, tekrarlanabilir kanıt gerekir.
- Alanların anlamı/etiketiyle ilgili sorunlar tutar hesabından ayrı kaydedilir.

## Test ve çekirdek ilerleme kuralı

Bir çekirdek ancak o çekirdeğe ait kabul testleri başarıyla tamamlandığında %100 olarak raporlanabilir. CI'ın başarılı olması tek başına gerçek Excel/VBA masaüstü kabulünü veya sekiz nihai PDF'nin birebir uygunluğunu kanıtlamaz. Kanıtlanmamış testler tamamlandı gibi sayılmaz.

- Temel Eğitim testleri ve PDF'leri ayrı raporlanır.
- Özel Eğitim testleri ve PDF'leri ayrı raporlanır.
- Dört belge türünün her program için dönem, yüklenici, hat, tutar, imza alanı ve sayfa düzeni kontrol edilir.
- Resmî ödeme hesaplarını etkileyen her kod değişikliği, program bazlı regresyon testleriyle doğrulanır.
- Bu kural, eski ve programlar arası tutar karşılaştırmalarının kabul kanıtı sayılmasını engeller.

## Son CI durumu

2026-10-21 tarihinde `983ccf4a1155cb61e7598787a5cf1411c7b67436` commit'ine ait GitHub Actions işi başarıyla tamamlandı. Bu, mevcut iş akışının derleme/test adımlarının geçtiğini gösterir; gerçek Excel/VBA yeniden hesaplaması veya sekiz nihai PDF'nin gerçek Excel çıktılarıyla birebir kabulü olarak yorumlanamaz.
