# Hakediş kayıtları ve gerçek pencere bordro kabulü

Kullanıcı isteği: kayıt butonları tablonun altında, seçim yoksa pasif; hücre metinleri satıra sarılarak tam okunmalı. İki bordronun sayısal değerleri uygulamaya girilmeli, gerçek kayıtlardan üretilen çıktılar kaynağa göre denetlenmeli.

- [x] Önce seçim/alt eylem ve metin sarma testi yaz; eski yerleşimde başarısız olduğunu doğrula.
- [x] İşlemleri tek alt çubuğa taşı; Tag bağını seçili kayıt üzerinden kur. Satır yüksekliğini içeriğe bırak, uzun hücreleri sar.
- [x] Gerçek App/MainWindow başlangıcı, gerçek DataGrid düzenleme bağları ve Hesapla/Kaydet olayıyla 42 satır gir.
- [x] Program depolamasını tekrar aç; ham ve kuruş sonuçlarını kaynağa göre doğrula.
- [x] Her kayıt için dört PDF üret; PDF metnini tekrar okuyarak para alanlarını ve sayfa sınırlarını denetle, temsilî görselleri incele (168 PDF, 336 kritik hücre, sekiz render).
- [x] Başlangıç/bitiş veri hashleriyle normal kullanıcı verilerinin korunmasını doğrula (8 dosyada 0 fark).
- [x] Regresyon paketini çalıştır; kanıtları ve sonuçları kaydet (213/213 WPF testi).

Kabul ölçütü: Kullanıcı 1 kuruşluk yuvarlama farklarını kabul etti; mevcut resmî rapor yuvarlama yöntemi korunacak. Anlamlı PDF net farkı >0,01 TL başarısızdır. Ham hesap ve kayıt kontrolleri 0,0000001 TL toleransında kalır. Eksik fatura referansları ve alternatif fatura sunumu önceki raporda açıklandığı şekilde ayrı tutulur. Boş KDV/tevkifat hücreleri bilinçlidir; KDV yoksa tevkifat yok, KDV varsa tevkifat var.

Test kimlikleri ve Ekim 2026 dönemi, kaynak dosyalarda bu metadata tam bulunmadığı için test kurulumu olarak kullanılır; gerçek kullanıcı kayıtlarına yazılmaz. Yeni kabul çalıştırıcısının kendi çıktı dizini zorunlu deneme işaretiyle izole edilir.
