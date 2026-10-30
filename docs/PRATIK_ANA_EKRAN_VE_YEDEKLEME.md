# Ana Ekran, kayıt kontrolü ve yedekleme

## Ana Ekran

Programı, İKN'yi ve hakediş ayını seçin. İKN seçimini boş bırakırsanız seçili programdaki tüm ihaleler gösterilir. Tarih seçiminde gün değil, o tarihin ayı esas alınır.

**Altın Vurgulu Finans** görünümünde üstte dört, altta dört kart bulunur. Net ödeme kartı altın renktir; sekiz kartın da ayrı vektör simgesi vardır. Üst sıra hesaplanan net, brüt tutar, toplam kesinti ve KDV toplamını; alt sıra taşıma tutarı, fiyat farkı, hakedişli hat ve yüklenici firma sayısını gösterir.

Kartlar seçili program, İKN ve ay kapsamındadır. Tutarlar her hattın o aydaki son kayıtlı hakedişinden alınır. Hakedişli hat, kayıtlı / beklenen hat sayısını gösterir; firma sayısı kapsamdaki hatlara bağlı benzersiz firma kimliklerinin sayısıdır. Taşıma tutarı rehber hariç hizmet tutarıdır. Fiyat farkı, taşıma ve rehber için iskontolar sonrası KDV hariç toplamdır; kartların araç ipuçlarında kapsam açıklanır.

Eski üç durum sayacı kaldırıldı. **Listede ara / duruma göre süz** bölümünde firma, hat adı/numarası veya İKN arayabilir, duruma göre listeyi süzebilirsiniz. Bu liste filtreleri üstteki dönem toplamlarını değiştirmez. **Yenile** dosyaların ve kayıtların güncel durumunu yeniden okur.

Bir hat seçtiğinizde alt bölümde o hakedişin günleri, günlük fiyatları, fiyat farkı, kesintileri ve PDF'deki net ödeme gösterilir. Eksik bilgi düğmesi ilgili kaydı açar. **Sonraki işlem** ödeme yoksa oluşturma, ödeme varsa düzenleme/arşivleme akışına götürür. İmzacı ve oranlar açılır ayrıntı bölümündedir.

**Belgeleri önizle** ve **Dört PDF'yi kaydet**, seçtiğiniz hakedişe aittir. Eksik bağlantı/dönem/tarih bilgileri tamamlanmadan çıktı oluşturulmaz. Ayrı bir resmî sözleşme numarası alanı istenmez; İKN kullanılır. Uygulamanın `INTERNAL-…` ilişki kimliği resmî numara gibi PDF'ye basılmaz.

**Tamamlandı**, güncel hakedişin arşiv kaydı ve bağlamı/dosya özetleri doğrulanmış dört PDF'si bulunduğu anlamına gelir. Eski çıktılar doğrulanmış gibi gösterilmez; yeniden üretilebilir. İmzacı veya rapor içeriği değişirse önceki PDF güncel sayılmaz. Bu durum belgenin kurumca onaylandığı veya ödemenin yapıldığı anlamına gelmez.

## Düzeltmeler

Canlı geçmiş veya arşiv düzeltmesinde kaydetmeden önce **Alan / Önce / Sonra** karşılaştırması açılır. Sadece değişen alanlar listelenir. Hesaplanan net, fark ve PDF neti ayrı gösterilir; günlük fiyat hassasiyeti korunur.

**İptal** kaydı değiştirmez. Canlı geçmiş onayla güncellenir. Arşiv düzeltmesinde önceki revizyon korunur ve yeni revizyon oluşur. Oran değişiklikleri karşılaştırmada görünür.

## Geçmiş ve arşiv

Her iki ekranda arama, İKN, firma, ödeme dönemi ve durum filtreleri vardır. Program kapsamı sonuç sayısıyla birlikte gösterilir. **Dönemi belirsiz** eski tarihsiz kayıtları bulur. **Filtreleri temizle** kayıt silmez.

Arşivlerde varsayılan olarak her ödemenin son revizyonu gösterilir. **Önceki revizyonları göster** eski sürümleri de açar. Okunamayan arşivler görünür uyarı satırları olarak kalır; programı belirlenemeyen bozuk dosyalar sessizce gizlenmez.

## Tam yedek

**Ayarlar → Yedekleme → Tam yedek al** ile `.oehs-backup.zip` paketi oluşturulur. Paketi OEHS veri/çıktı klasörlerinin dışında bir yere kaydedin. İki programın kaydedilmiş verileri, oranlar/değişiklik geçmişi, imzacı/pencere ayarları, arşiv revizyonları ve yönetilen çıktılar pakete alınır. Formda henüz kaydetmediğiniz girdiler pakete dahil değildir.

**Yedeği incele ve geri yükle** önce dosya özetlerini, sürümü ve veri biçimini doğrular; tarih ve kayıt sayılarını gösterir. Onaydan sonra mevcut iki programın kayıtları ve yönetilen çıktı klasörü paketteki durumla değiştirilir. Birleştirme yapılmaz. Önce açık düzenlemeleri kaydedin veya formu temizleyin.

Mevcut veriler değiştirilmeden önce `OEHS-Recovery-…oehs-backup.zip` kurtarma paketi oluşturulur. Eski kök klasörler ayrıca `.before-…` adıyla tutulur; hata olursa geri alma denenir. Başka bilgisayara taşımada doğrulanmış çıktı kayıtları göreli dosya yollarını kullanır. Eski tanılama JSON'ları oluşturuldukları bilgisayarın yolunu içerebilir; güncellik kontrolü bu eski mutlak yollara dayanmaz.

Bu yedek, uygulamanın yönetilen güncel veri ve çıktı alanını kapsar. Önceden oluşturulmuş reset/kurtarma klasörleri ve kaydedilmemiş geçici dosyalar yeni pakete eklenmez; geri yükleme öncesi tutulan `.before-…` klasörleri bu ek dosyaları korur.
