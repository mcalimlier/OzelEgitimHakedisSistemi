# OEHS devam notu — 17.10.2026 (hesaplama regresyon testleri)

## Bu turda yapılanlar
- Önceki CI `39055380669` tamamlandı ve başarılı oldu. Bu, commit `5ec9b388fc179d6da5970a5148a1deb27750f836` için test/derleme hattının geçtiğini gösterir.
- Temel/Ortaöğretim rapor hücre eşlemesi için bir sentetik regresyon testi eklendi. İncelemede örnek girdinin kaynak Excel'in 5. satırının tamamı yeniden kurulamadığı fark edildi; test adı/açıklaması bu nedenle düzeltilerek sentetik test olduğu açıkça belirtildi. Bu değer kaynak Excel golden-master sonucu olarak sunulmuyor.
- Temel/Ortaöğretim hesaplamasında rehber alanlarının ek kesin teminat hesabına taşınmadığını açıkça güvenceye alan bir assertion eklendi.
- Hesaplama motoru davranışını değiştiren üretim kodu yok; yalnızca test kapsamı genişletildi.

## Son doğrulama durumu
- Commit `5ec9b388fc179d6da5970a5148a1deb27750f836` CI başarılı: https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39055380669
- Test adını/açıklamasını düzelten commit: `322f2c71f636700780abf4ea3db4e15d534ed356`, CI çalışması: https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39055536490
- Rehber ek teminat izolasyon assertion'ı commit'i: `95beaab49935d8abb66487ffe332857d14975923`, CI: https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39055555375
- Bu son iki CI çalışması not hazırlanırken tamamlanmamıştı. Son commit'in sonucu ayrıca kontrol edilmelidir.

## Açık riskler
- Temel/Ortaöğretim rapor hücrelerini kaynak XLSM'den baştan sona kuracak tüm girişler doğrulanmadı; sentetik rapor testi gerçek kaynak örneği yerine geçmez.
- XLSM formüllerinin güncel Excel/VBA hesaplamasıyla taze sonuçları doğrulanmış değil.
- PDF testleri yapısal kontroller içeriyor; piksel düzeyinde kaynak şablon karşılaştırması henüz yok.
- Ek kesin teminatın negatif fiyat farkındaki kaynak formül/mevzuat yorumu farkı karar gerektiren açık konu olmaya devam ediyor.

## Sıradaki adım
1. En son commit `95beaab` için CI sonucunu doğrula.
2. `ProgressPaymentCalculator` için program türüne göre alanların girişten hesaplama ve PDF modeline kadar hangi çağrı zincirinden geçtiğini haritala.
3. Programlar arası veri izolasyonu ve dört PDF veri sözleşmesi için mevcut testleri incele; bir sonraki küçük değişikliği ancak davranış testle sabitlendikten sonra yap.
4. Ana dala birleştirme/yayın yok; kullanıcı açık onayı gerekir.
