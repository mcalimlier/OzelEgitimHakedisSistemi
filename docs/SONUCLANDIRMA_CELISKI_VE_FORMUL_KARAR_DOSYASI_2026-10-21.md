# OEHS Sonuçlandırma Karar Dosyası — 21.10.2026

Dal: `feature/official-payment-document-templates`  
PR #3: açık/taslak; birleştirilmedi.  
Kaynaklar: son XLSM dosyaları ve bu dosyalardan doğrudan okunan formüller.  
Önemli sınır: formül/önbellek statik incelemesi gerçek Excel masaüstü tam hesaplama ve VBA çalışma kabulü değildir.

## 1. Program bazında yetkili kaynaklar

| Program | Dosya | SHA-256 | Temel hücreler |
|---|---|---|---|
| Temel Eğitim | `03-Temel Eğitim Taşıma Ödeme SON(1).xlsm` | `37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296` | `Hesaplama Bilgileri!K4:W8`, `Hakediş Raporu!D15/D28`, `Hakediş İcmali!C8:E8` |
| Özel Eğitim | `02-Özel Eğitim Hakediş Ödeme SON(1).xlsm` | `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e` | `Hesaplama!K4:AG6`, `Hakediş Raporu!D15/D28`, `Hakediş İcmali!C8:E8` |

İki programın sonuçları birbiriyle kıyaslanmaz. Her birinin kendi hücre zinciri, PDF örneği ve regresyon testleri vardır.

## 2. Çelişki A — Temel Eğitim ek kesin teminat T hücresi

### Doğrudan kaynak kanıtı
En güncel Temel Eğitim dosyasında:
- `Hesaplama Bilgileri!K4=(G4*H4)+I4-J4`
- `Hesaplama Bilgileri!T4=I4*6/100`
- `Hesaplama Bilgileri!U4=K4*9.48/1000`
- `Hesaplama Bilgileri!V4=P4+Q4+R4+S4+T4+U4+M4`
- `Hesaplama Bilgileri!W4=O4-V4`

Kayıtlı örnek değerleri: I4=7.365,35 TL; K4=319.991,9228 TL; T4=441,921 TL; U4=3.033,523428144 TL; V4=19.475,040568144 TL; W4=332.516,074511856 TL.

### Karar
**Bu sürüm için T4 elle girilen sabit bir tutar değildir.** Formül açıkça I4'ün %6'sını hesaplıyor. Önceki dokümanda geçen “Temel Eğitim T elle girilir” kuralı son XLSM ile çeliştiğinden güncel kabulden çıkarıldı. Kaynak dosya değiştirilmeyecek; OEHS kodu da güncel formül paritesine göre kontrol edilmeli.

## 3. Çelişki B — Temel Eğitim icmalinde fiyat farkının ikinci kez eklenmesi

Kaynak formüller:
- `K4=(G4*H4)+I4-J4`: K aylık tutarı I fiyat farkını içeriyor.
- `İcmal Verileri!R4=7.365,35 TL` (aylık fiyat farkı bloğu).
- `İcmal Verileri!AD4=19.475,040568144 TL` (kesinti toplamıyla eşleşen değer).
- `Hakediş İcmali!C8` aylık hizmet toplamını getiriyor; kayıtlı C8=319.991,9228 TL.
- `Hakediş İcmali!D8` formülü, aynı satır için 15+MATCH sütununu ve 27+MATCH sütununu topluyor. Kayıtlı D8=26.840,390568144 TL = R4+AD4.
- `Hakediş İcmali!E8=C8+D8`; kayıtlı E8=346.832,313368144 TL.

### Kesin olarak gösterilen ve gösterilmeyenler
- **Gösterilen:** Kaynak formül D8'de fiyat farkı bloğu ile kesinti bloğunu topluyor; C8'in K aylık tutarı da I fiyat farkını zaten içeriyor. Bu nedenle fiyat farkının toplamda ikinci kez yer alma riski formül bağımlılığıyla kanıtlanıyor.
- **Gösterilmeyen:** Bu formülün hukuki/resmî ödeme anlamında yanlış olduğu veya hangi alternatifin idarece kabul edildiği. Yalnızca aritmetik şüpheyle kaynak Excel davranışı değiştirilmez.
- Salt aritmetik senaryo: fiyat farkı ikinci kez eklenmeden, kesinti korunursa C8+AD4=339.466,963368144 TL olur; kaynak E8 ile fark 7.365,35 TL'dir. **Bu bir önerilen resmî tutar değil; teşhis hesabıdır.**

### Sonuçlandırma yolu
Aynı Temel Eğitim test kopyasında Excel tam hesaplama ve VBA aktarımından önce/sonra R4, AD4, C8, D8, E8, D15, D28, W4 kaydedilecek. Uygulama, onaylanmış kaynak Excel davranışını taklit etmeli; formül hatası iddiası ancak kaynak workbook formülü düzeltilip kabul edilmişse üretim kuralına dönüşmeli. Şimdilik kodda sessiz formül değişikliği yapılmamalı.

## 4. Çelişki C — Özel Eğitim icmalinde fiyat farklarının ikinci kez eklenmesi

Kaynak formüller:
- `Hesaplama!K4=(G4*H4)+I4-J4`
- `Hesaplama!T4=(P4*Q4)+R4-S4`
- `Hesaplama!AC4=(I4*6/100)+(R4*6/100)`
- `Hesaplama!AD4=(K4*9.48/1000)+(T4*9.48/1000)`
- `Hesaplama!AE4=M4+V4+Y4+Z4+AA4+AB4+AC4+AD4`
- `Hesaplama!AF4=O4+X4`; `AG4=AF4-AE4`

İcmal kayıtlı değerleri:
- K4=43.630,00 TL; T4=34.037,27 TL; K4+T4=77.667,27 TL.
- `Hakediş İcmali!C8=77.667,27 TL`.
- `İcmal Verileri!R4=7.530,00 TL`; `İcmal Verileri!AD4=3.500,00 TL` (kaynakta fiyat farkı blokları için ilgili aylık değerler).
- `Hakediş İcmali!D8=11.030,00 TL`; `E8=C8+D8=88.697,27 TL`.

### Karar
**Gösterilen:** C8'in K+T aylık toplamı fiyat farkı bileşenlerini içeriyor; D8 de fiyat farkı bloklarından 7.530+3.500 TL ekliyor. Bu, kaynak icmal formülünde ikinci ekleme riskini gösteriyor.
**Gösterilmeyen:** Kaynak formülünün resmî ödeme amacı bakımından hatalı olduğuna dair idari/hukuki karar. Dolayısıyla üretim kodunda “D8=0” veya “yalnızca kesinti” değişikliği şimdilik yapılmamalı.

Teşhis senaryosu: D8 tekrar eklenmezse toplam C8=77.667,27 TL olur; kaynak E8'den 11.030 TL farklıdır. Bu, alternatif resmî tutar değil, farkın kaynağını gösteren hesaplamadır.

## 5. Çelişki D — Özel Eğitim icmal ay başlığı

Kaynak `İcmal Verileri!AP3` ve `AQ3` formülleri ikisi de `=Anasayfa!I9` (Aralık) olarak okunuyor. VBA/ay sütun eşlemesi AP=Kasım, AQ=Aralık bekliyorsa başlıklar çakışıyor. Ayrı düzeltilmiş XLSM kopyası var; orijinal kaynak değiştirilmedi.

Karar: kaynak dosyayı sessizce değiştirme. Excel masaüstünde test kopyasında AP3'ü Kasım başlığına düzelterek Kasım/Aralık/Ocak seçimini ve E21 ile makro sonrası aktarımı karşılaştır. Orijinal ve düzeltilmiş kopyanın sonuçlarını ayrı kaydet; hangi dosyanın resmî kaynak olacağı değişiklik kaydıyla belirlenmeli.

## 6. Çelişki E — Hesaplama neti ile Hakediş Raporu D28

Kaynak kabul hücreleri:
- Temel Eğitim: `Hesaplama Bilgileri!W4` ve `Hakediş Raporu!D28`.
- Özel Eğitim: `Hesaplama!AG4` ve `Hakediş Raporu!D28`.

Önceki LibreOffice kopya incelemesinde D28 ile W/AG arasında fiyat farkına yakın farklar gözlenmiştir. Bu gözlem gerçek Excel/VBA masaüstü yeniden hesaplaması değildir. Her program için D6/D7/D8/D10/D11/D28 ile W/AG, aynı satır/hat/dönem ve makro öncesi/sonrası değerlerle karşılaştırılmalıdır. D28'i veya W/AG'yi “doğru” ilan edecek kabul kanıtı henüz yoktur.

## 7. Çelişki F — Önceki hakediş E21 ve ay başlıkları

- Temel Eğitim `Hakediş İcmali!E21` kaynak formülü E4, I5:I14, K16 ve İcmal Verileri AB:AL eşlemesini kullanıyor; kayıtlı önbellek 26.448,176274448 TL.
- Özel Eğitim `Hakediş İcmali!E21` kaynak formülü E5, I6:I15, K17 ve İcmal Verileri AN:AX eşlemesini kullanıyor; kayıtlı önbellek 29.963,898156 TL.
- Özel Eğitim AP3/AQ3 Aralık tekrar sorunu, önceki hakediş ay eşlemesini etkileyebilir.
- Bunlar kayıtlı formül/önbellek gözlemleridir; tam Excel/VBA kabulü değildir.

## 8. PDF kabulü — 8 ayrı kapı

Her program için ayrı ayrı şu dört PDF doğrulanmalı:
1. Hakediş Raporu
2. Hizmet İşleri Hakediş Raporu
3. Hizmet İşleri Kabul Tutanağı
4. Hakediş İcmali

Her belge aynı programın gerçek Excel PDF'siyle dönem, yüklenici, İKN/sözleşme, hat, brüt tutar, fiyat farkı, KDV, tevkifat, ek kesin teminat, damga vergisi, kesintiler, net tutar, imza alanları, A4 ve tek sayfa açısından karşılaştırılmalı. Bir belgenin geçmesi diğerlerini geçirmez. Sentetik PDF smoke testleri gerçek Excel PDF kabulü değildir.

## 9. Otomatik test ve gerçek kabul durumu

Son doğrulanmış CI: commit `983ccf4a1155cb61e7598787a5cf1411c7b67436`, run [39588156139](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39588156139), başarılı. Bu commit'ten sonra yeni dokümantasyon commit'i oluştuğundan yeni commit'in CI sonucu ayrıca izlenmelidir.

- CI/derleme/test: başarılı olan run mevcut.
- Gerçek Excel/VBA tam yeniden hesaplama: açık.
- Gerçek makro/UserForm çalışma testi: açık.
- Gerçek Excel PDF'leriyle 8/8 alan bazlı karşılaştırma: açık.
- Kaynak formülünün resmî anlamı konusunda icmal kararı: açık; aritmetik risk kanıtlandı ama alternatif resmî formül onaylanmadı.

## 10. Kapatma planı

1. Son XLSM hash'lerini doğrula ve orijinalleri salt okunur koru.
2. Her program için ayrı test kopyası aç; Excel tam hesaplama yap.
3. Aynı seçili hat/dönemle makro öncesi/sonrası hücreleri kaydet.
4. W/AG, D15/D28, C8/D8/E8 ve E21/ay aktarım zincirlerini karşılaştır.
5. Aynı test verisiyle program başına dört Excel PDF'si ve OEHS PDF'si üret; 8 belgeyi ayrı ayrı kabul et.
6. Yalnızca kaynağın açıkça desteklediği kod düzeltmelerini uygula; her değişiklik sonrası test/CI çalıştır.
7. Testi geçmeyen veya masaüstü Excel erişimi olmayan kapıları tamamlandı olarak işaretleme. PR açık kalır; merge/release yok.

## 11. Devam notu

Yapılan: en güncel XLSM formülleri doğrudan kontrol edildi; Temel Eğitim T4=I4*6% olduğu doğrulandı ve eski “elle giriş” kuralı güncel kabulden çıkarıldı. İcmal çift ekleme riski her program için ayrı aritmetik zincirle belgelendi. Kaynak XLSM'ler değiştirilmedi; üretim hesap formülleri sessizce değiştirilmedi.

Doğrulama: GitHub run 39588156139 başarılı, ancak yalnızca o commit için. Yeni dokümantasyon commit'inin CI sonucu ayrıca kontrol edilmeli.

Açık risk: gerçek Excel/VBA yeniden hesaplama, makro/UserForm çalışması, icmal resmî iş kuralı ve 8/8 gerçek Excel PDF kabulü.

Sıradaki adım: gerçek Excel masaüstü kanıtıyla açık kapıları kapat; test/CI başarısı görülmeden başarı iddia etme.
