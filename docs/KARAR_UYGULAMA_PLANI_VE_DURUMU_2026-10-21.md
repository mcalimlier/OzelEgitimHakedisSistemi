# OEHS Karar Uygulama Planı ve Durum Kaydı — 21.10.2026

**Kullanıcı kararı:** 21.10.2026 tarihinde 9 karar için seçenekler açıkça seçildi.  
**Dal:** `feature/official-payment-document-templates`  
**PR:** #3 açık/taslak kalacak. Ana dala birleştirme ve yayınlama yapılmayacak.

## Seçilen uygulama davranışı

| No | Seçim | Uygulama |
|---|---|---|
| 1 | Temel Eğitim icmalinde kanıtlanan mükerrer bileşeni düzelt | “İş Tutarı” sütununda K-I baz tutarı; “Fiyat Farkı” sütununda I bir kez + kaynak kesinti bileşeni. A+B artık I'yi iki kez saymaz. |
| 2 | Özel Eğitim icmalinde kanıtlanan mükerrer bileşeni düzelt | “İş Tutarı” sütununda (K+T)-(I+R); “Fiyat Farkı” sütununda I+R bir kez. A+B = K+T. |
| 3 | Temel Eğitim W4/D28: D28=W4 ise eşle | Eşitlik yalnızca formül/yuvarlama zinciriyle kanıtlanırsa aynı alan sayılacak; D28 farklı formülle hesaplanıyorsa ayrı alan olarak korunacak. |
| 4 | Özel Eğitim AG4/D28: D28=AG4 ise eşle | AC4:AG4 zinciri ve D28 formülü birlikte kontrol edilecek; yalnızca etiket veya yaklaşık tutar benzerliği yeterli değil. |
| 5 | Özel Eğitim AP3/AQ3: önce 12 aylık eşleme testi | Özgün XLSM değiştirilmeden ay başlığı → sütun → aktarım → sonuç testi; ardından kaynak mı VBA mı hatalı kararı. |
| 6 | Tüm alanlar için tam matris | Hücre, formül, hesaplanan değer, uygulama alanı, PDF/bölüm, gerçek PDF değeri ve fark. Her program için ayrı. |
| 7 | Önce otomatik tutar/kimlik karşılaştırması | Önce dönem, İKN, yüklenici, hat ve mali alanlar; sonra PDF sayfa/görsel kabulü. |
| 8 | Statik + masaüstü + regresyon | Statik kod kontrolü tek başına çalışma kabulü sayılmaz. Excel masaüstü/VBA ve uygulama regresyonları ayrı raporlanır. |
| 9 | CI + regresyon + sekiz PDF kanıtı | Aynı commit SHA'sına ait CI, program bazlı regresyon ve sekiz gerçek Excel karşılaştırması olmadan genel kabul tamamlanmış sayılmaz. |

## Uygulanan ilk kod düzeltmesi: icmal fiyat farkı

### Kaynak kanıtı
Temel Eğitim kaynak XLSM SHA-256:
`37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296`

- `Hesaplama Bilgileri!K4=(G4*H4)+I4-J4`: K aylık tutarı I fiyat farkını içeriyor.
- `Hakediş İcmali!C8=319.991,9228 TL`.
- `Hakediş İcmali!D8=R4+AD4=7.365,35+19.475,040568144=26.840,390568144 TL`.
- `Hakediş İcmali!E8=C8+D8=346.832,313368144 TL`.
- R4'teki 7.365,35 TL, I4 ile aynı fiyat farkı bileşenidir; AD4 ise 19.475,040568144 TL kesinti toplamıyla eşleşir.

Özel Eğitim kaynak XLSM SHA-256:
`c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e`

- `Hesaplama!K4=(G4*H4)+I4-J4`.
- `Hesaplama!T4=(P4*Q4)+R4-S4`.
- K4=43.630,00 TL, T4=34.037,27 TL, C8=77.667,27 TL.
- D8=11.030,00 TL; bu değer 7.530,00 + 3.500,00 TL I/R fiyat farkı bloklarına eşit.
- E8=88.697,27 TL.
- C8 zaten K+T olduğundan I/R fiyat farkı bileşenleri C8 içinde bulunur.

### Üretim kodu değişikliği
Dosya: `src/OzelEgitimHakedis.Wpf/OehsOfficialDocumentFactory.cs`  
Metot: `BuildHakedisIcmali`

- Aylık hizmet tutarı mevcut programa özel transfer davranışını korur: Temel Eğitim K; Özel Eğitim K+T.
- Temel Eğitim fark sütununda kesinti bileşeni korunur, ancak K'ye zaten dahil olan ham fiyat farkı ikinci kez eklenmez.
- Özel Eğitim fark sütununa I/R fiyat farkları ikinci kez eklenmez; değer 0 olur.
- Alt bölümdeki kesinti çıkarma mantığı korunur.
- Kaynak XLSM dosyalarının hiçbirinde değişiklik yapılmadı.

### Test değişikliği
Dosya: `tests/OzelEgitimHakedis.Wpf.Tests/OfficialDocumentOutputTests.cs`

- Özel Eğitim icmalinde baz hizmet tutarı ile I/R farkının ayrı gösterilmesi, her satırın a+b toplamının K+T ile eşleşmesi ve mükerrer 140/260 TL toplamların oluşmaması için sentetik PDF regresyon beklentileri güncellendi.
- Bu test sentetik uygulama regresyonudur; gerçek Excel masaüstü kabulünün yerine geçmez.

### Kaynak Excel ile açık fark
Bu düzeltme kaynak XLSM'nin mevcut D8/E8 formül davranışından bilerek ayrılabilir. Kullanıcı kararı programdaki kanıtlanmış çift eklemeyi düzeltmek yönündedir. Dolayısıyla:
- Programın icmal A sütunu kaynak C8'den fiyat farkı kadar düşük, D sütunu ise fiyat farkını bir kez içerir; toplam E8 kaynak E8'den mükerrer fiyat farkı kadar düşük olabilir.
- Bu fark gizlenmeyecek; tam alan eşleme matrisinde “bilinçli program düzeltmesi / kaynak Excel uyuşmazlığı” olarak gösterilecek.
- Kaynak XLSM veya resmî ödeme kuralı bu işlemde değiştirilmiş sayılmayacak.
- Gerçek Excel/VBA ve PDF örnekleri incelenmeden “resmî kabul tamamlandı” denmeyecek.

## Kalan uygulama sırası

1. [Kod ve test commit'i için Windows CI sonucunu doğrula](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions).
2. Temel Eğitim ve Özel Eğitim için ayrı test vektörlerinde yeni icmal sütun/alt toplam değerlerini doğrula.
3. W4/D28 ve AG4/D28 için formül/yuvarlama eşlemesini ayrı ayrı çıkar; kaynak testleri yalnızca doğru olduğu kanıtlanan alanları eşleştirsin.
4. Özel Eğitim için Eylül-Ağustos 12 ay tablosunu üret: ay adı, kaynak başlık hücresi, kaynak başlık değeri, VBA/uygulama sütun indeksi, E21 önceki ay sonucu, aylık tutar ve test sonucu. AP3/AQ3 çakışması çözülene kadar kaynak XLSM'yi değiştirme.
5. Her iki program için tam Excel→uygulama→PDF alan matrisi oluştur.
6. Önce otomatik kimlik/tutar karşılaştırması; sonra her PDF için A4, tek sayfa, taşma, imza ve görsel kontrol.
7. Masaüstü Excel'de VBA derlemesi, makro ve UserForm testi yapılabiliyorsa kanıtları kaydet. Yapılamıyorsa kapıyı OPEN/BLOCKED bırak.
8. Her düzeltmeden sonra aynı commit'e ait CI/regresyon sonucunu doğrula.
9. Son devam notunda yapılan iş, test kanıtı, açık risk ve sonraki adımı yaz.

## Durum sözlüğü

- **PASS:** İlgili test aynı commit/kaynak örneği üzerinde çalıştı ve geçti.
- **FAIL:** Test çalıştı ve başarısız oldu.
- **OPEN:** Gerekli test/kanıt henüz yok.
- **BLOCKED:** Gerekli ortam veya veri erişimi yok; engel açıkça belirtilmiş.

## Durum — bu kayıt oluşturulurken

- İcmal düzeltme kodu ve ilgili sentetik PDF test beklentileri yazıldı.
- Son test çalışmaları bazı eski test beklentilerinin yanlış program türü veya eski icmal sütun dağılımını kullandığını gösterdi; bunlar düzeltilip yeniden CI'ya gönderildi. En güncel test commit'i için sonuç bekleniyor.
- Sonraki yalnız-belge commit'leri için ayrı CI sonucu bulunmayabilir. Son kod/test commit'inin run sonucu, test edilen SHA ile birlikte raporlanmalı.
- Masaüstü Excel/VBA kabulü: OPEN.
- 8/8 gerçek Excel PDF alan/görsel kabulü: OPEN.
- AP3/AQ3 12 aylık test ve kaynak/VBA kararının kapanışı: OPEN.
- PR #3 açık/taslak; merge/release yok.
