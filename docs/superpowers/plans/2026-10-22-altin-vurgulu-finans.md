# Altın Vurgulu Finans — onaylanan ana ekran

Kullanıcı, üretilen 3. görseli açıkça seçti ve sembollerin korunmasını istedi. Uygulama: lacivert zemin, altın net kartı, 4 üstte + 4 altta eşit kart, alt sırada altın sol çizgi, sekiz anlamlı vektör simge. Kartlar: hesaplanan net, brüt tutar, toplam kesinti, KDV toplamı; taşıma tutarı, fiyat farkı, hakedişli hat, yüklenici firma.

Veri kapsamı üstteki program / İKN / aydır. Alt tablo araması ve durum seçimi yalnız listeyi süzer. Kart tutarları mevcut kayıtlı hesaplardan alınır, yeni hesap formülü kurulmaz. Her hattın dönemdeki son kaydı sayılır. Hat oranı kayıtlı / beklenen hat; firma sayısı dönem kapsamındaki benzersiz firma kimlikleridir. Taşıma kartı rehber hariç taşıma hizmeti; fiyat farkı taşıma + rehber, iskontolar sonrası KDV hariçtir. Bu ayrımlar araç ipucunda açıklanır.

Adımlar: önce filtre/tutar ve sekiz simgeli kart testleri; mevcut durum düğmelerini kaldırıp kartları ve gerçek veri bağlantısını ekle; ekranı gerçek WPF olarak render et; 1024×600 ve üzeri boyutları kontrol et; Release testleri ve CI kontrolleri; mevcut dala commit. Ana ekranın belge kontrolü ve çıktı işlemleri korunur, yeni tasarıma uyarlanır. Push / PR / merge istenmedi.

Sonuç: tasarım ve semboller uygulandı. Standart Release çözüm testleri 397/397; dört CI kontrolü geçti. Gerçek WPF render üzerinden simge/yerleşim denetlendi. Son görsel düğme hizalaması ayrıca ilgili ekran testleriyle doğrulanır.
