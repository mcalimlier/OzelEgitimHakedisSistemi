# Bordro doğrulaması ve kayıtlı hakediş ayrıntıları

Amaç: Kullanıcının iki bordrosundaki tüm veri satırlarını çekirdekle karşılaştırmak; kayıtlı hakediş tablosunda Düzenle, Sil, Hesaplamayı Görüntüle eylemlerini sunmak.

Kullanıcı mevcut Ek Ders uygulamasındaki satır eylemlerini istedi. Mevcut düzeltme/silme akışları korunacak; ayrıntılar salt okunur ayrı pencere olacak. Yeni bir düzenleme/silme altyapısı kurulmayacak. Ek Ders deposu yalnızca okunacak.

- [x] Kaynak dosya hashleri, hücre adresleri, ham girişler ve önbellekteki Excel sonuçları ile tekrar çalıştırılabilir karşılaştırma verisi çıkar.
- [x] Kaynakta boş fatura referanslarını ve alternatif fatura gösterimini ayrı raporla; aynı hattın parçalı fiyat dönemlerini birleştirme. Kullanıcı açıklamasıyla boş KDV/tevkifat hücreleri hata sayılmadı.
- [x] Kullanıcının onayladığı dört ondalık giriş desteğini form, hat ve hibrit doğrulamalarında test et ve uygula; çıktılar iki ondalık kalsın.
- [x] Kaydedilmiş hesap ve AppliedConstants üzerinden kapsamlı hesap dökümü oluştur; eksik eski oranlar için açık açıklama göster, güncel ayarları eski hesaba uygulama.
- [x] Kayıtlı hakedişleri kaydırılabilir tabloda göster; satır kimliği ve program türüyle mevcut düzenle/sil akışlarını kullan.
- [x] Hesap dökümü ve satır eylemlerini STA testleriyle, bordroyu sayısal karşılaştırmayla, son durumu tam test paketiyle doğrula (270 çekirdek + 213 WPF testi).

Sınırlar: Gerçek kullanıcı kayıtları değiştirilmez. Ekli Excel dosyaları değiştirilmez. Eski EXE/GitHub sürümü otomatik yayımlanmaz. Önceki 150% DPI testi kapanmış sayılmaz.
