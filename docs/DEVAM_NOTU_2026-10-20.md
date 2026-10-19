# OEHS devam notu — 2026-10-20

## Çalışma sınırı
- Temel Eğitim/Ortaöğretim ve Özel Eğitim ayrı hesaplama çekirdekleri olarak ele alınacaktır. Sırf ortaklaştırmak için kod veya hesap kuralı birleştirilmeyecektir.
- Kullanıcının aktardığı iki programın VBA kaynakları aktif/çalışır kabul edilir; kodlar aktarımın kaynak davranışını anlamak için referanstır. Buradaki inceleme Excel masaüstünde makro çalıştırıldığı anlamına gelmez.
- Hesap davranışı, kaynak hücre/aktarım zinciri ve regresyon kanıtı olmadan değiştirilmeyecektir.
- PR #3 taslak ve birleştirilmemiş kalacaktır; ana dala birleştirme veya yayınlama yapılmayacaktır.

## Mevcut uzak GitHub durumu
- Depo: `mcalimlier/OzelEgitimHakedisSistemi`
- Dal: `feature/official-payment-document-templates`
- İncelenen PR: [#3](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/pull/3), taslak/açık.
- Bu not oluşturulmadan önce görülen dal başı: `4c9bdf753cdec142b379e200dddc05e1b84c7433`. Bu notun oluşturulması yeni bir dokümantasyon commit'i üretir; sonraki oturumda gerçek güncel baş SHA ve CI yeniden kontrol edilmelidir.
- Son kanıtlanmış kod/test CI'sı: [run 39317706602](https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39317706602): 223/223 Core ve 5/5 WPF test; çözüm derlemesi, PDF smoke ve Windows EXE publish başarılı. Daha sonraki dokümantasyon commit'leri için ayrı CI sonucu ayrıca kontrol edilmelidir.

## Kaynak ve kabul bulguları
- Özel Eğitim VBA aktarım haritası: `Hesaplama!K+T -> İcmal Verileri!D:N`; `I -> P:Z`; `R -> AB:AL`; `AE -> AN:AX`.
- Temel Eğitim aktarım makroları ayrı kaynak haritasıyla ele alınır; iki programın hesap çekirdekleri birbirine dönüştürülmez.
- Önceki formül denetiminde erişilebilir tarihsel XLSM kopyalarının LibreOffice yeniden hesaplamasıyla 5.375 sayısal hücresi karşılaştırıldı; tolerans üstü fark 0, yalnızca TODAY() kaynaklı 10 tarih farkı vardı. Bu Microsoft Excel/VBA çalışma kabulü değildir ve 19 Ekim'de yüklenen en yeni ham dosyaların doğrudan yeniden hesaplandığı anlamına gelmez.
- Tarihsel Temel Eğitim icmalinde yaklaşık 4,19 TL farkı, kaynak `AB4`/`V4` uyuşmazlığı ve makronun son aktarım durumu açısından açık tutulur; otomatik düzeltme yapılmaz.
- Kaynak formül zincirinde icmale fiyat farkının ikinci kez eklenmesi riski belgelenmiştir. Yetkili karar ve aynı-girdi doğrulaması olmadan hesap formülü sessizce değiştirilmez.
- Dört belge × iki program için (Hakediş Raporu, Hizmet İşleri Hakediş Raporu, Hizmet İşleri Kabul Tutanağı, Hakediş İcmali) tutar, dönem, yüklenici, hat, imza ve sayfa düzeni kabulü açık kalır. Sentetik PDF smoke gerçek Excel çıktısına birebir parite kanıtı değildir.

## Sonraki oturumun iş sırası
1. PR #3 başını, en güncel commit SHA'sını ve CI durumunu yeniden oku; bu nottaki SHA'yı varsayma.
2. Her programı kendi VBA/Excel kaynak zinciri üzerinden incele. Özel Eğitim için `K/T`, `I`, `R`, `AE` aktarım bloklarını; Temel Eğitim için kendi `K`, `I`, `V` aktarımını kaynak ay başlıklarıyla eşleştir.
3. Var olan testleri yeniden kullan; eksik kalan yerde yalnızca kaynak vektörüne dayalı, program-özel regresyon testleri ekle.
4. 4×2 PDF kabul matrisinde yalnızca gerçek kanıtı bulunan hücreleri kapat. PDF görsel kabulü/gerçek Excel masaüstü sonucu yoksa açık yaz.
5. Test ve CI tamamlanmadan başarı iddiasında bulunma. Genel ilerleme tahminini kanıt olmadan yükseltme.

## İlerleme tahmini (son belgelenmiş)
| Alan | Tahmin |
|---|---:|
| Temel Eğitim/Ortaöğretim hesaplama | %85 |
| Özel Eğitim hesaplama | %87 |
| Hesaplama motoru | %85 |
| Dört PDF / icmal | %87 |
| Arşiv ve veri modeli | %86 |
| WPF / ekran akışı | %83 |
| Test / CI / dağıtım | %97 |
| **Genel proje** | **%85** |

Bu oranlar mühendislik tahminidir, kabul sertifikası değildir. Genel proje %90 hedefine ulaşmamıştır.

## Kullanıcının GitHub Desktop'ta yapacağı
1. Yerel değişiklikleri kontrol et; korunması gereken değişiklik varsa önce commit et veya yedekle.
2. Current branch `feature/official-payment-document-templates` olmalı.
3. **Fetch origin** yap; ardından yerel değişiklikler güvence altındaysa **Pull origin** yap.
4. PR #3'ü taslak bırak; **Merge** veya yayınlama yapma.
5. Pull sonrasında yerel commit varsa veya GitHub Desktop çakışma gösterirse çözmeden devam etme; sonraki oturumda durumunu bildir.