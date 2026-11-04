# OEHS devam notu — 17.10.2026

## Bu çalışma turunda yapılanlar
- Etkin çalışma dalı ve son commit GitHub üzerinden doğrulandı; ana dala geçiş veya birleştirme yapılmadı.
- Mimari değerlendirme belgesi güncellendi: `docs/OEHS_CEKIRDEK_MIMARISI_DEGERLENDIRME.md`.
- PDF testlerinin mevcut kapsamı belgelendi: dört belge ve iki program için yapısal/A4/metin/koordinat kontrolleri var; piksel tabanlı kaynak şablon karşılaştırması henüz yok.
- Kabul Tutanağı'ndaki `takvim günü` etiketi ile `İş Günü` değerinin birlikte görünmesi açık doğrulama konusu olarak kaydedildi; bu turda hesap veya belge davranışı değiştirilmedi.
- Kaynak rapor servisinde D6/D7/D8/D10/D11 zinciri ve Özel Eğitim rapor örneği mevcut testlerle incelendi. Temel/Ortaöğretim için kaynak hücre değerlerini yalnızca cache'e dayanarak yeni golden-master testine dönüştürmedik.

## Doğrulama durumu
- Önceki commit `9b832e411bcd85b25fe1b0baa10cf5ca89f6abe2` için Windows CI başarılıydı: https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39054744710
- Bu not ve mimari belge güncellemesinin yeni commit'i: `691b34062e69605054c42d4500f6f308b0a6ab0f`.
- Bu commit için CI başlatıldı: https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39055083007. Bu not yazılırken sonuç henüz tamamlanmamıştı; tamamlandıktan sonra yeniden kontrol edilmelidir.
- Bu turda C# hesaplama kodu değiştirilmedi; yeni test sonucu iddia edilmiyor.

## Açık riskler / karar bekleyen noktalar
1. XLSM dosyalarının kaynak formülleri ile önbellekteki sonuçlar arasında masaüstü Excel/VBA ile taze hesaplama kanıtı henüz yok.
2. Dört PDF için piksel/görsel karşılaştırma ve metin taşması analizi henüz kurulmadı.
3. Kabul Tutanağı süre etiketi ve gün değeri kaynak şablon/kurumca onaylı örnek üzerinden netleştirilmeli.
4. Ek kesin teminatın negatif fiyat farkında kaynak formülüyle mevzuat yorumu arasındaki olası fark sessizce değiştirilmemeli.

## Sıradaki adım
1. Commit `691b340` CI sonucunu kontrol et.
2. Rapor formül zinciri için Temel/Ortaöğretim gerçek girişlerini yeniden kurabilecek kaynak kanıtını belirle; kanıt yoksa test eklemek yerine eksik girdiyi belgele.
3. `ProgressPaymentCalculator` çağrı noktalarını ve program türüne özgü dalları haritalandır; önce karakterizasyon testleri, sonra küçük refaktör.
4. PDF veri aktarım sözleşmesini tanımla ve dört belge için kaynak görsel karşılaştırmasını ayrı aşamada kur.
5. Her değişikliği ayrı commit/CI ile doğrula; ana dala birleştirme veya yayın için açık onay bekle.
