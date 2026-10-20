# Hakediş Deneme Vektörleri

Bu dosya, OEHS geliştirme çalışması sırasında sentetik hesaplama girdilerini ve C# hesaplama çekirdeği için beklenen sonuçları kaydetmek amacıyla oluşturulmuştur. **Yalnızca sentetik regresyon vektörüdür; son yüklenen Excel'lerin resmî test verisi, kaynak satırı veya kabul kuralı değildir.** Son XLSM/VBA ile çelişen herhangi bir tutar, oran veya davranış burada kalıcı kural sayılmaz; vektör güncellenmeli veya kaldırılmalıdır. Kaynak Excel/VBA ile bire bir kabul testi değildir.

## Temel / Ortaöğretim

- Çalışma günü: 1
- Günlük taşıma fiyatı: 100,00 TL
- İskonto: 0,00 TL
- Fiyat farkı: 20,00 TL
- Sözleşme bedeli: 1.000,00 TL
- KDV istisnası: Hayır
- Vergi borcu, SGK borcu, asgari ücret desteği ve denetim cezası: 0,00 TL
- Program türü: Temel / Ortaöğretim

Beklenen C# çekirdek sonuçları:

| Kalem | Tutar |
|---|---:|
| Hizmet tutarı | 100,00 TL |
| Fiyat farkı dâhil matrah | 120,00 TL |
| KDV (%10) | 12,00 TL |
| Tevkifat (KDV'nin %50'si) | 6,00 TL |
| Ek kesin teminat (20 × %6) | 1,20 TL |
| Damga vergisi (120 × 9,48/1000) | 1,1376 TL |
| Toplam kesinti | 8,3376 TL |
| Net ödenecek | 123,6624 TL |

Temel / Ortaöğretim hesabında rehber personel alanları girilse de bu programın hesaplamasına dâhil edilmemelidir. Bu vektör, sentetik bir C# kontrolüdür; Excel/VBA kabulü değildir.

## Özel Eğitim

- Çalışma günü: 1
- Günlük taşıma fiyatı: 100,00 TL
- Taşıma fiyat farkı: 100,00 TL
- Rehber fiyat farkı: 50,00 TL
- İskontolar ve diğer kesintiler: 0,00 TL
- KDV istisnası: Hayır
- Program türü: Özel Eğitim

Kaynak formül davranışı: `AC = (I × 6%) + (R × 6%)`.

| Kalem | Tutar |
|---|---:|
| Taşıma matrahı | 200,00 TL |
| Rehber matrahı | 50,00 TL |
| Taşıma KDV'si (%10) | 20,00 TL |
| Rehber KDV'si (%20) | 10,00 TL |
| Taşıma tevkifatı | 10,00 TL |
| Rehber tevkifatı | 9,00 TL |
| Ek kesin teminat | 9,00 TL |
| Damga vergisi | 2,37 TL |
| Toplam kesinti | 30,37 TL |
| Net ödenecek | 249,63 TL |

Rehber fiyat farkı sıfırlanırsa ek kesin teminat `100 × 6% + 0 × 6% = 6,00 TL` olur. Bu örnek de sentetik C# kontrolüdür; Excel/VBA ile doğrulanmış resmî örnek değildir.

## Negatif fiyat farkı kontrolü — Temel / Ortaöğretim

- Çalışma günü: 10
- Günlük taşıma fiyatı: 1.000,00 TL
- Fiyat farkı: -500,00 TL
- İskonto: 0,00 TL
- Sözleşme bedeli: 10.000,00 TL
- KDV istisnası: Hayır
- Diğer kesintiler: 0,00 TL

Mevcut kaynak-formül davranışını karşılaştırmak için beklenen sentetik C# sonuçları: toplam matrah 9.500,00 TL; KDV 950,00 TL; tevkifat 475,00 TL; ek kesin teminat -30,00 TL; damga vergisi 90,06 TL; brüt 10.450,00 TL; toplam kesinti 535,06 TL; net 9.914,94 TL. Negatif teminatın resmî ödeme bakımından uygulanabilirliği ayrıca mevzuat ve sözleşme açısından karara bağlanmalıdır.

## Doğrulama sınırı

Bu vektörler kod/test geliştirmesine yardımcı olmak için hazırlanmıştır. Microsoft Excel'de makroları çalıştırarak alınmış sonuçlar veya dört resmî PDF'nin kaynak Excel ile bire bir eşleştiğine dair kanıt değildir. Resmî kabul için aynı girdilerin Excel/VBA sonuçları ve dört PDF çıktısı ile karşılaştırılması gerekir.
