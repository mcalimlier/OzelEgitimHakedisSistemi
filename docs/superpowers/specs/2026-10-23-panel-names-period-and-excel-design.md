# Kaynak adları, hakediş dönemi ve Excel dışa aktarımı

Kullanıcı talebi doğrudan uygulanır; mevcut WPF ve .NET 8 yapısı korunur.

1. Aynı kaynak alanı bütün panellerde ve tabloda aynı tam adla gösterilir. Rehber Personel Günlük Fiyat, Taşıma Günlük Fiyatı, Döneme Esas Taşıma Hakediş Gün Sayısı (Döneme Esas THGS), Rehber İş Günü (bu ay) ve Ele Geçen başlıkları kaynak adlarıyla eşleştirilir. İç hesaplama özellikleri ve ödeme formülleri değişmez.
2. Ana ekrandaki serbest tarih seçici kaldırılır. Hakediş Dönemi, Hakediş Oluştur ile aynı ProgressPaymentMonthOption / ProgressPaymentMonthService.BuildOptions zincirini kullanır. Seçili program ve varsa seçili İKN'nin sözleşme tarihleri aylık seçenekleri belirler. Tüm ihalelerde aynı takvim ayının kapsanan tarih sınırları birleştirilir. Seçim yalnız uyumluysa korunur; geçersiz eski seçim temizlenir. Ana ekran → Hakediş Oluştur geçişi aynı ayı taşır.
3. İhale Bilgileri'ndeki İşin Adı kontrolü, etiketi ve bütün form yükleme/temizleme/değişiklik-izleme bağlantıları kaldırılır. Resmî belgelerin hat adından üretilen iş başlığı, kayıtlı ödeme metadata'sı ve eski dosya uyumluluğu form girdisi değildir; korunur.
4. Excel'den Yapıştır / Yapıştırmayı Geri Al düğmeleri ve özel Ctrl+V/Ctrl+Z planı/işleyicileri/testleri kaldırılır. Tek EXCEL’E AKTAR düğmesi mevcut seçili hat tablosunun görünür sütunlarını, geçerli güncel hücre değerlerini ve sütun sırasını .xlsx kaydeder. Sayılar decimal hassasiyetiyle sayısal hücre, metinler literal metin ve onaylar boolean hücre olarak yazılır. Önce hücre düzenlemesi tamamlanır; geçersiz/boş tablo kaydedilmez. Dosya seçimi iptalinde hiçbir dosya veya kaynak kayıt değişmez. Dışa aktarma kaynak hat kaydı, aylık taslak ve hakediş oluşturmaz.
5. Arşiv kaldırma talebi koşulludur. Kod incelemesinde geçmişin uygulama kayıt listesi, arşivin ise sözleşme/firma/hat/ödeme/önceki ödemelerin değişmez snapshot ve revizyon zinciri olduğu görüldü. Aynı işlev olmadıkları için arşiv kaldırılmaz.

Seçilen yaklaşım mevcut uygulamanın OpenXML ZIP yazıcısını genişletmektir; kullanıcı bilgisayarına yeni Excel/Node bağımlılığı eklenmez. CSV gerçek .xlsx ihtiyacını karşılamaz. Statik bir örnek workbook oluşturmak yerine uygulamanın anlık tablosu dışa aktarılır.

Doğrulama: kaynak başlık eşleşmeleri; İKN/program/ay aralığı ve ana ekrandan geçiş; form/pano kalıntılarının yokluğu; gerçek .xlsx paket içeriği/sayısal hassasiyet/metin güvenliği; görünür sütun ve satır sınırı; dışa aktarımın kayda yan etkisizliği; tam Windows Core/WPF/PDF regresyonu. Merge/release yapılmaz.
