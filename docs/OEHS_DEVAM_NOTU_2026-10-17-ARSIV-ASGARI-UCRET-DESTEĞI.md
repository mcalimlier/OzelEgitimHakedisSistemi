# OEHS devam notu — arşiv düzeltmelerinde asgari ücret desteği

## Yapılan inceleme
- Güncel dal ve son commit doğrulandı: `feature/official-payment-document-templates`, başlangıç commit'i `75f23d7937cfe4a092574d45458392b6247327ba`.
- Önceki devam notu CI'si `39055576182` başarıyla tamamlandı.
- Kod izinde `ProgressPaymentCalculator` hem kayıt hem düzenleme/yeniden hesaplama yollarından çağrılıyor; belge üretimi `BuildOfficialDocumentContextForPayment` üzerinden rapor toplamlarını ve icmalin önceki kesinti tutarını alıp dört PDF oluşturucusuna veriyor.
- İncelemede arşiv sözleşmesi ve hat düzeltmelerinde `MinimumWageSupport` değerinin `hasGuide` koşuluna bağlandığı bulundu. Bu, rehber personel olmayan Temel/Ortaöğretim programında daha önce kaydedilmiş asgari ücret desteğini arşiv düzenlemesi sırasında sıfırlayabiliyordu. Asgari ücret desteği rehber personel kaleminden ayrı bir kesintidir; hesaplama çekirdeği ve mevcut propagation testleri Temel/Ortaöğretim için de bu tutarı koruyor.
- `MainWindow.xaml.cs` içindeki üç arşiv düzeltme ataması düzeltildi: mevcut hakedişin ve önceki hakedişlerin asgari ücret desteği artık rehber varlığına göre silinmiyor. Rehber iş günü/fiyat/iskonto/fiyat farkı alanları program türüne göre ayrılmaya devam ediyor.

## Doğrulama
- Değişiklik commit'i: `ec9888250e7c5c019050df2dc9fef40409b9ebc0`.
- CI çalışması: https://github.com/mcalimlier/OzelEgitimHakedisSistemi/actions/runs/39055918738
- Not hazırlanırken CI çalışması henüz devam ediyordu; başarılı olduğu varsayılmamalıdır.
- Core propagation için mevcut testler Temel/Ortaöğretim sözleşme/hat değişikliğinde asgari ücret desteğinin korunmasını kontrol ediyor. WPF arşiv düzeltme yoluna doğrudan çalışan bir UI testi tespit edilmedi; bu yol için ek regresyon kapsamı hâlâ gerekli.

## Açık riskler ve sonraki adım
1. CI tamamlanınca derleme, test ve PDF smoke sonuçlarını kontrol et.
2. Arşiv düzeltme yolunu kapsayan bir regresyon testi ekleme olanağını değerlendir; UI davranışı test edilmeden tamamen doğrulandı deme.
3. Özel Eğitim ve Temel/Ortaöğretim için kaynak Excel ile tüm girdi/hesap/rapor zincirini ayrı ayrı eşle.
4. Dört PDF'nin görsel karşılaştırmasını ve kaynak Excel'in taze yeniden hesaplama doğrulamasını sürdür.
5. Ana dala birleştirme/yayın yapılmadı; kullanıcı onayı gerekir.
