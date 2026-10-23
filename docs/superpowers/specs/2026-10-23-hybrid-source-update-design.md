# Onaylı hibrit tablo güncellemesi

Kullanıcı talebi: rehber yıllık gün tamamen kaldırılacak. Hat kaynak verileri seçilen hatların düzenlenebilir tablosuna gelecek. Tablo altındaki güncelleme butonu değişiklikle etkinleşecek; onayda kalıcı ve eksiksiz güncelleme, vazgeçmede taslağı koruma yapılacak.

Mevcut WPF/JSON mimarisi korunur. Yaklaşım: kayıtlı hat (ad, taşıma günü, taşıma günlük fiyatı, rehber günlük fiyatı) ile döneme özgü ödeme girdilerini ayrı saklayıp aynı onaylı kayıt işleminde yazmak. Doğrudan her tuşta kaynak kayıt değiştirmek onay şartını karşılamaz; yalnız hat kaydetmek ise dönemsel hücreleri kalıcı kaydetmez.

- `Route`, `RouteData`, WPF `RouteDraft` içindeki yıllık `GuideWorkDays` kaldırılır. Eski JSON'daki alan okunurken göz ardı edilir, yeni dosyaya yazılmaz. Hakediş girdisindeki aylık `GuideWorkDays` korunur. Hat/geçmiş/arşiv düzeltmeleri artık yıllık rehber günü okumaz/yazmaz.
- Hat kimlikleri İKN/sözleşme/program/firma/hat numarasıyla eşleşir. Hat numarası kimliktir; tablo satırında sabit kalır. Hattın adı ayrı düzenlenebilir sütundur. Günlük fiyatlar ve günler düzenlenebilir; yeni seçim/değişen seçim kullanıcının düzenlemesini ezmez.
- Taşıma fiyatı sözleşmede başlangıç varsayılanıdır. Tablodan kaydedilen hat fiyatı hat düzeyinde korunur ve Hatlar panelinde gösterilir; hat düzenleme işlemi bu değeri korur; bir hattın fiyatı başka hattın veya tüm sözleşmenin fiyatını değiştirmez.
- Buton seçili satırlarda başlangıç/kayıt anına göre fark varsa etkinleşir. Düzenleme geri alınırsa pasifleşir. Geçersiz hücre, negatif fiyat/gün, boş hat adı, yanlış sahiplik/eksik kaynak veya kayıt hatası varsa hiçbir kaynak güncellenmez, taslak korunur.
- Onay: değişen seçili hat kaynakları ve dönemsel taslaklar tek mevcut atomik program veri kaydı içinde saklanır. Kaynak hat listeleri/seçiciler/özetler yenilenir. Başarıdan sonra yeni temel değerler kabul edilir ve buton pasifleşir. Vazgeçme hiçbir kayda dokunmaz ve buton etkin kalır.
- Dönemsel taslaklar program/hat kimliği/ay ile saklanır; yeniden açılışta aynı ayın taslağı yüklenir. Kaydedilmiş hakediş/geçmiş ve değişmez resmî arşivler bu butonla topluca yeniden hesaplanmaz. Hakediş oluşturma ve hedefli resmî düzeltme mevcut kendi iş akışını kullanır.
- Rehber Personel Günlük Fiyat ↔ Excel-hibrit **Rehber Günlük Fiyat** (`GuideDailyPrice`).

Doğrulama: kaldırılan yıllık alanla eski JSON uyumu; aylık günün hat değişiminde korunması; satır aktarımı/değiştirilebilirlik; pasif/değişiklik/geri alma; iptal/onay; çoklu kaynak/kalıcılık/program ayrımı; başarısız kayıt atomik geri alma; tam Windows Core/WPF/PDF testleri. Gerçek masaüstü/Excel kabulü ayrıca belirtilir.
