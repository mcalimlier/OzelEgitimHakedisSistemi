# OEHS — Denenebilir hakediş örnekleri

Bu örnekler uygulamanın hesaplama davranışını hızlıca denemek için hazırlanmış **sentetik test verileridir**. Resmî hakediş veya mevzuat onayı yerine geçmez. Gerçek ödeme dosyasına aktarmayın. Hesaplamalar program türüne göre ayrıdır; aynı verileri iki programa girince aynı sonuç beklenmemelidir.

## Örnek 1 — Temel / Ortaöğretim

Taşıma programı: **Temel / Ortaöğretim**

| Girdi | Değer |
|---|---:|
| Çalışma günü | 1 |
| Günlük taşıma fiyatı | 100,00 TL |
| İskonto | 0,00 TL |
| Fiyat farkı (KDV hariç) | 20,00 TL |
| Sözleşme bedeli | 1.000,00 TL |
| KDV istisnası | Hayır |
| Vergi borcu | 0,00 TL |
| SGK borcu | 0,00 TL |
| Asgari ücret desteği | 0,00 TL |
| Rehber personel gün/fiyat/fiyat farkı | 20 gün / 200,00 TL / 50,00 TL (Temel programda hesaba katılmaması kontrolü için) |

Beklenen temel kontroller (varsayılan hesaplama sabitleriyle):

| Sonuç | Beklenen |
|---|---:|
| Taşıma hizmeti | 100,00 TL |
| Taşıma KDV'si | 10,00 TL |
| Taşıma tevkifatı | 5,00 TL |
| Ek kesin teminat | 1,20 TL |
| Damga vergisi | 1,1376 TL |
| Toplam kesinti | 8,3376 TL |
| Net ödenecek | 123,6624 TL |
| Rehber hizmeti / rehber KDV'si / rehber tevkifatı | 0,00 TL |

Kaynak davranış kontrolü: Temel/Ortaöğretim için (T=I\times6\%), (U=K\times9{,}48/1000); rehber alanları bu programın hesabına eklenmez. Ekranda para yuvarlaması görülebilir; test karşılaştırması yuvarlanmamış ara değerler üzerinden yapılır.

## Örnek 2 — Özel Eğitim

Taşıma programı: **Özel Eğitim**

| Girdi | Değer |
|---|---:|
| Çalışma günü | 1 |
| Günlük taşıma fiyatı | 100,00 TL |
| İskonto | 0,00 TL |
| Taşıma fiyat farkı (KDV hariç) | 100,00 TL |
| Sözleşme bedeli | 1.000,00 TL |
| KDV istisnası | Hayır |
| Vergi borcu / SGK borcu / asgari ücret desteği | 0,00 TL |
| Rehber çalışma günü | 0 |
| Rehber günlük fiyat / iskonto | 0,00 TL / 0,00 TL |
| Rehber fiyat farkı (KDV hariç) | 50,00 TL |

Beklenen temel kontroller (varsayılan hesaplama sabitleriyle):

| Sonuç | Beklenen |
|---|---:|
| Ek kesin teminat | 9,00 TL |

Kaynak davranış kontrolü: Özel Eğitim için (AC=(I\times6\%)+(R\times6\%)); bu örnekte (I=100), (R=50), dolayısıyla ek kesin teminat (9,00) TL olmalıdır. Rehber fiyat farkı sıfırlanırsa bu değer (6,00) TL olur. Bu örnek, program ayrımını ve rehber fiyat farkının ek kesin teminata etkisini sınamak içindir; bütün ödeme belgesi tutarlarının kabulü değildir.

## Örnek 3 — Negatif fiyat farkı sınaması

Bu, sınır davranışını denetleyen sentetik bir testtir; gerçek ödeme kararında ayrıca kaynak Excel, sözleşme ve mevzuat incelenmelidir.

| Girdi | Değer |
|---|---:|
| Program | Temel / Ortaöğretim |
| Çalışma günü / günlük fiyat | 10 gün / 1.000,00 TL |
| İskonto / fiyat farkı | 0,00 TL / -500,00 TL |
| Sözleşme bedeli | 10.000,00 TL |
| Vergi borcu / SGK borcu / destek | 0,00 TL / 0,00 TL / 0,00 TL |

Mevcut kaynak formül davranışına göre test beklentileri: toplam tutar 9.500,00 TL; ek kesin teminat -30,00 TL; damga vergisi 90,06 TL; tevkifat 475,00 TL; brüt tutar 10.450,00 TL; toplam kesinti 535,06 TL; net ödenecek 9.914,94 TL. Negatif ek kesin teminatı sıfırlayan bir varsayım eklenmemiştir.

## Nasıl denenecek?

1. Uygulamayı test verisiyle açın; gerçek kurum/kişi bilgisi girmeyin.
2. Taşıma programını açıkça seçin.
3. Önce örnek 1'i kaydetmeden hesaplayın ve beklenen değerlerle karşılaştırın.
4. Yeni bir test kaydında programı **Özel Eğitim** seçerek örnek 2'yi deneyin.
5. Çıktı üretirseniz dört PDF'nin dönem, yüklenici/hat, tutar ve sayfa düzenini ayrı ayrı kontrol edin. Bu örnekler için PDF çıktısının kaynak XLSM ile birebir aynı olduğu iddia edilmez.
6. Herhangi bir farkta program türünü, tüm girdileri, hesaplama sabitlerini ve sonuç ekranını kaydedin; resmî tutarları değiştirmeden önce farkın kaynağı belirlenmelidir.
