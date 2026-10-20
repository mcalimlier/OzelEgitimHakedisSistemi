# Son yüklenen XLSM — kaynak paritesi ve regresyon vektörleri

**Tarih:** 20.10.2026  
**Durum:** Dosya baytları ve hücre formülleri doğrudan incelendi; kaynak satır vektörleri C# regresyon testlerine aktarıldı. CI sonucu ve Microsoft Excel/VBA masaüstü çalıştırması ayrı doğrulama kapılarıdır.

## Yetkili kaynak dosyaları

| Program | Dosya | SHA-256 |
|---|---|---|
| Temel Eğitim / Ortaöğretim | 03-Temel Eğitim Taşıma Ödeme.xlsmdüzeltildi (1).xlsm | 37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296 |
| Özel Eğitim | 02-Özel Eğitim Hakediş Ödeme (1)(2).xlsm | c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e |

Bu dosyalar, eski GitHub dokümanlarındaki tarihsel kaynak kopyalarından üstündür. Aşağıdaki sonuç veya test beklentisi son dosyayla çelişirse bu belge ve testler güncellenecektir.

## Kaynak formülleri

**Temel Eğitim — Hesaplama Bilgileri, satırlar 4–8**

- K=(G*H)+I-J
- L=IF(E="Hayır",K*10/100,0)
- M=L/2
- O=K+L
- T=I*6/100
- U=K*9.48/1000
- V=P+Q+R+S+T+U+M
- W=O-V

**Özel Eğitim — Hesaplama, satırlar 4–6**

- K=(G*H)+I-J; L=K*10/100; M=L/2; O=K+L
- T=(P*Q)+R-S; U=T*20/100; V=U*9/10; X=T+U
- AC=(I*6/100)+(R*6/100)
- AD=(K*9.48/1000)+(T*9.48/1000)
- AE=M+V+Y+Z+AA+AB+AC+AD
- AF=O+X; AG=AF-AE

## Kaynak satırlarından alınan hesap sonuçları

**Temel Eğitim, satırlar 4–8**

| Satır | K — taşıma neti | T — ek teminat | U — damga vergisi | V — toplam kesinti | W — net |
|---:|---:|---:|---:|---:|---:|
| 4 | 319.991,9228 | 441,921 | 3.033,523428144 | 19.475,040568144 | 332.516,074511856 |
| 5 | 112.086,35 | 441,981 | 1.062,578598 | 7.108,877098 | 116.186,107902 |
| 6 | 105.527,35 | 442,041 | 1.000,399278 | 6.718,807778 | 109.361,277222 |
| 7 | 85.082,9116 | 442,101 | 806,586001968 | 5.502,832581968 | 88.088,370178032 |
| 8 | 52.329,35 | 442,161 | 496,082238 | 3.554,710738 | 54.007,574262 |

**Özel Eğitim, satırlar 4–6**

| Satır | K — taşıma neti | T — rehber neti | AC — ek teminat | AD — damga vergisi | AE — kesintiler | AG — net |
|---:|---:|---:|---:|---:|---:|---:|
| 4 | 43.630,00 | 34.037,27 | 661,80 | 736,2857196 | 27.328,7843196 | 61.508,9396804 |
| 5 | 34.354,00 | 33.957,27 | 513,00 | 647,5908396 | 28.045,2494396 | 50.492,8745604 |
| 6 | 37.624,00 | 36.107,27 | 580,80 | 698,9724396 | 35.330,0010396 | 49.385,1229604 |

Tutarlar kaynak hücrelerinin önbelleğe alınmış sonuçlarından aktarılmıştır. C# testleri aynı girdi alanları ve program-özel kesintilerle karşılaştırır. Özel Eğitim satırlarında Y/Z/AA/AB sırasıyla vergi borcu, denetim cezası, SGK borcu ve asgari ücret desteği olarak girişe eşlenmiştir.

## Yeniden hesaplama karşılaştırması

Orijinaller değiştirilmeden LibreOffice Calc ile ayrı XLSX kopyaları üretildi; kaynak dosyanın formül önbelleği ile yeniden hesaplanan kopya karşılaştırıldı.

| Dosya | Formül hücresi | Sayısal karşılaştırma | 0,000001 TL üzeri fark |
|---|---:|---:|---:|
| Temel Eğitim | 297 | 138 | 0 |
| Özel Eğitim | 289 | 141 | 0 |
| **Toplam** | **586** | **279** | **0** |

Bu sonuç LibreOffice yeniden hesaplaması ile kaynak önbelleğinin karşılaştırmasıdır. VBA makrolarının Microsoft Excel masaüstünde çalıştırıldığını veya PDF'lerin resmî kabulden geçtiğini göstermez.

## İlgili testler ve kabul sınırı

- CurrentBasicWorkbookDiscrepancyTests: Temel Eğitim son dosyasının satır 4–8 vektörleri.
- CurrentXlsmSourceParityTests: Özel Eğitim son dosyasının satır 4–6 vektörleri.
- Testler [Fact] altında doğrudan kaynak satırlarından kurulan tuple vektörlerini döngüyle doğrular; attribute argümanı olarak dizi veya decimal veri kullanılmaz.
- Windows CI run 39399443926 ve 39399513703 başarılı tamamlandı: tam çözüm/WPF derlemesi, test adımları, PDF smoke ve Windows EXE publish başarılı. Son test sonucu 215/215 Core ve 5/5 WPF testtir. Bu otomatik CI kanıtı masaüstü Excel/VBA veya 4×2 PDF kabulü değildir.
- Dört PDF × iki program için aynı girdiyle alan/tutar/güzergâh/dönem/imza/sayfa karşılaştırması açık kalır.
- VBA'nın Excel masaüstünde çalıştırılması ve gerçek yazıcı kabulü ayrıca yapılmalıdır.

## Sonraki adımlar

1. Yeni regresyon testlerinin Windows CI sonucunu kontrol et; başarısızsa yalnızca son XLSM hücreleri ve gerçek formül eşlemesiyle düzelt.
2. Her iki kaynak workbook'un dört çıktı sayfasını aynı kaynak satırı/dönemle PDF'ye aktar ve OEHS'nin dört PDF'siyle ayrı ayrı karşılaştır.
3. Kaynak Excel icmal formülü veya makro aktarımı ile OEHS arasında fark bulunursa hücre formülü, VBA aktarımı, örnek sonuç ve test kanıtını birlikte kaydet; sessiz düzeltme yapma.


## Son test uygulama notu — 20.10.2026
İlk MemberData düzenlemesi de Windows CI'da CS3016 verdi; bu nedenle testler daha basit ve derleyici açısından güvenli bir düzene alındı: tek [Fact], tipli tuple kaynak vektörleri ve foreach ile hücre sonuçlarının doğrulanması. Güncel commit: 15e3e88755ef768fdb6352c8bacb56f6ce99cafe. Bu commit için yeni CI sonucu henüz görünmüyor.


## Son XLSM'lerdeki icmal riskleri — formül zinciri ve aktarım ayrı ayrı

**Özel Eğitim**
- Anasayfa!E5 = Kasım 2026.
- Hesaplama!K4 = 43.630,00 TL; Hesaplama!T4 = 34.037,27 TL; K+T = 77.667,27 TL.
- Hakediş Raporu!D11 = 77.667,27 TL; D8−D10 da aynı değeri verir.
- Hakediş İcmali!C8 = 77.667,27 TL ve D8 = 11.030,00 TL; E8 = C8+D8 = 88.697,27 TL. D8, İcmal Verileri'ndeki taşıma fiyat farkı I=7.530 TL ile rehber fiyat farkı R=3.500 TL'yi toplar. Böylece fiyat farkı, C8 içindeki K+T tutarında zaten bulunmasına rağmen ayrı sütunda bir kez daha eklenir.

**Temel Eğitim**
- Hesaplama Bilgileri!I4 = 7.365,35 TL; K4 = 319.991,9228 TL; V4 = 19.475,040568144 TL.
- İcmal Verileri!P4/Q4/R4 = 7.365,35 TL; AD4 = 19.475,040568144 TL. Bu son dosyada ilgili aktarılmış değerler I4 ve V4 ile eşleşir.
- Hakediş İcmali!C8 = 319.991,9228 TL; D8 = 26.840,390568144 TL (= R4 + AD4 = fiyat farkı + kesintiler); E8 = 346.832,313368144 TL. C8'deki K4 aylık hizmet toplamı I4 fiyat farkını zaten içerdiğinden, D8'de fiyat farkının ayrıca eklenmesi kaynak formül zincirinde tekrar sayım oluşturur. D sütununun aynı zamanda AD4 kesintisini içermesi de sütun başlığıyla anlam farkı doğurur.

Önceki tarihsel kopyalarda raporlanan AB4/V4 farkı 4,19 TL bu en son dosyaya doğrudan taşınmamalıdır: son dosyada AD4=V4 görülüyor. Tarihsel bulgu yalnızca eski kopya için geçerlidir.

Bu bulgular, son XLSM'lerde **icmal formül zincirinin ayrı kabul edilmesi gerektiğini** gösterir. Kaynak Excel'in davranışını sessizce düzeltmeyin. Aynı hat/dönemle VBA aktarım makroları masaüstü Excel'de çalıştırılmalı, dört PDF yeniden üretilmeli ve kaynak formülünün iş sahibince onaylanmış davranışıyla OEHS çıktısı karşılaştırılmalıdır.
