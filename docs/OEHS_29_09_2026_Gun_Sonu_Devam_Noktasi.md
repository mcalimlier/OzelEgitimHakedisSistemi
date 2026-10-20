# OEHS — 29.09.2026 Gün Sonu / Devam Noktası

Bugünkü OEHS geliştirmelerini, Excel tersine mühendislik sonuçlarını, UI/çıktı değişikliklerini, derleme düzeltmelerini ve yarınki başlangıç adımlarını içerir.

## Bugünkü ana çalışmalar
- Ana ekran çok sayıdaki karttan tek kompakt hakediş özet paneline sadeleştirildi.
- Taşıma günlük fiyatı Hatlar ekranından kaldırılarak İhale Bilgileri içine taşındı.
- 4 belge tek birleşik önizleme ve tek yazdırma akışına alındı.
- 4 belge artık tek OEHS_Hakediş_Belgeleri.xlsx dosyasında 4 çalışma sayfası olarak arşivleniyor.
- A4 kalıbı 210x297 mm, dört kenarda 1 cm olacak şekilde sabitlendi.
- Eski 540 DIP dar içerik kalıbı kaldırıldı; 190 mm yazdırılabilir içerik genişliği hedeflendi.
- CS0120 static/instance hatası düzeltildi.
- Excel exporter içindeki CA1305, CA1307, CA1847 uyarıları düzeltildi.
- JsonSerializerOptions için CA1869 düzeltildi.
- MarkProgressInvalid için CA1822 düzeltildi.

## Excel doğrulama özeti
- Adli Excel denetimi: 3.399 benzersiz formül hücresi.
- Taşıma: %10 KDV.
- Rehber: %20 KDV.
- Taşıma tevkifat: %5.
- Rehber tevkifat: %18.
- Fiyat farkı teminatı: fiyat farklarının toplamının %6'sı.
- Damga vergisi: toplam hakedişin %0,948'i.
- KDV istisnası iş kuralı korunuyor.

## Ana test senaryosu
- Taşıma iş günü: 29
- Taşıma günlük fiyat: 2.608,26 TL
- Taşıma fiyat farkı: 1.500,00 TL
- Rehber iş günü: 30
- Rehber günlük fiyat: 1.362,49 TL
- Hakediş No: 1
- Dönem: 30.09.2026
- Sözleşme bedeli: 816.805,03 TL
- KDV istisnası: Hayır

Beklenen sonuç:
- Taşıma hizmet: 75.639,54 TL
- Taşıma hakediş: 77.139,54 TL
- Rehber hizmet/hakediş: 40.874,70 TL
- Toplam: 118.014,24 TL
- KDV: 15.888,89 TL
- Tevkifat: 11.214,42 TL
- Fatura: 129.228,66 TL
- Fiyat farkı teminatı: 90,00 TL
- Damga vergisi: 1.118,77 TL
- Toplam kesinti: 12.423,20 TL
- Brüt: 133.903,13 TL
- Net ödenecek: 121.479,94 TL

## Arşiv
Desktop/OzelEgitimHakedisSistemi/ altında Arsiv/Hakedisler ve Belgeler/[hakediş] klasörleri kullanılıyor.
Belgeler klasöründe OEHS_Hakediş_Belgeleri.xlsx ve OEHS_Hakediş_Bilgi.json bulunuyor.

## Yarın ilk iş
1. Git Pull.
2. Visual Studio Clean Solution.
3. Rebuild Solution.
4. Core testlerini çalıştır.
5. F5.
6. Ana ekranın tamamen sığdığını kontrol et.
7. İhale Bilgileri içindeki taşıma günlük fiyatını kontrol et.
8. 4 belgeyi tek önizlemede kontrol et.
9. Excel dosyasını açıp A4/1 cm yazdırma kalıbını kontrol et.
10. 121.479,94 TL net sonucunu gerçek UI senaryosunda doğrula.
11. Kalan hata/warning varsa sıfırla.
12. Sonrasında Excel resmi şablonlarına ve eksik İhale Verileri alanlarına geç.

## Derleme notu
Visual Studio'dan gelen hata/warning kayıtları kod üzerinden düzeltildi. Bu sohbet ortamında gerçek Windows Visual Studio/F5 derlemesi çalıştırılamadığı için son temiz Windows build sonucu henüz bağımsız olarak doğrulanmış değildir. Yarın ilk doğrulama budur.

## GitHub
- Repo: mcalimlier/OzelEgitimHakedisSistemi
- Branch: feature/wpf-contract-entry-v0.5
- Açık PR: #2