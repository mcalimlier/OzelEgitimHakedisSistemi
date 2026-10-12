# WPF Arayüz Taşma ve DPI Düzeltme Kılavuzu

**Kapsam:** OEHS'nin WPF arayüzü ve DoğruTemin'den taşınan görsel dil. Bu çalışma ayrı OEHS projesini hedefler; mevcut DogruTemin uygulamasının kaynak kodunu değiştirmez.

## 1. Mevcut kod incelemesi

Mevcut yapı bazı doğru temellere zaten sahip: `Grid` tabanlı ana kabuk, ekran çalışma alanına göre pencere boyutlandırma, içerik alanlarında `ScrollViewer`, ortak stiller ve Segoe UI.

İncelemede görülen somut riskler:

- `MainWindow.xaml` penceresi 1280 × 720 WPF birimi minimumuna sabitlenmişti. WPF'de ölçüler çoğunlukla fiziksel piksel değil, **device-independent pixel (DIP)** birimidir; bu alt sınır yüksek ölçeklendirmede kullanılabilir alana sığmayabilir.
- `MainWindow_Loaded` içindeki `NormalizeButtonWidths`, sabit genişlikli düğmeleri içerik ölçümüne göre daha da büyütebiliyordu. Düğmenin ebeveyn hücresinde yer olup olmadığını denetlemediğinden sütun/kenar taşmasına yol açabilir.
- ÇIKTILAR ekranında önizleme, yazdırma ve PDF kaydetme düğmeleri aynı satırda; yazdırma ve kaydetme için sabit sütun genişlikleri ayrılmıştı. Dar kullanılabilir genişlikte “BELGE ÖNİZLEME” düğmesinin alanı gereğinden az kalabiliyordu.
- Global metin kutusu ve etiket stillerindeki büyük punto değerleri dar kartlarda dikey yoğunluğu artırıyor. Her sayfa için gerçek ekran kontrolü hâlâ gereklidir.

## 2. Uygulanan ilk aşama

Bu dalda aşağıdaki düzeltmeler yapılıyor:

1. Pencerenin minimum ölçüsü 1024 × 600 DIP'e indiriliyor. Bu, 1366 × 768 fiziksel ekranın %100 ölçeklendirmesinde pencerenin çalışma alanına sığmasını; daha yüksek çözünürlüklerde ise daha düşük mantıksal alanla çalışılmasını kolaylaştırır.
2. Sabit genişlikleri otomatik genişleten genel düğme yordamı kaldırılıyor. Düğme ölçüsü artık ebeveyn Grid'i aşacak şekilde kod tarafından zorla büyütülmüyor.
3. ÇIKTILAR ekranında eylemler iki satıra ayrılıyor: önizleme ve yazdırma ilk satırda, PDF kaydetme ikinci satırda tam genişlikte. Uzun düğme metinleri sarılabilir.
4. Windows uygulama bildirimi Per-Monitor DPI Awareness V2 olarak tanımlanıyor. Bu ayar tek başına responsive yerleşimin yerini tutmaz; gerçek %100/%125/%150 testleri ayrıca yapılmalıdır.
5. Türkçe dil bilgisi pencere köküne ekleniyor; mevcut Segoe UI görsel dili korunuyor.

### Görsel sıkılaştırma turları

- İlk düzenlemede sol navigasyon 62 DIP'ten 52 DIP'e, üst bar 82 DIP'ten 72 DIP'e ve menü genişliği 290 DIP'ten 260 DIP'e indirildi.
- Son ekran görüntüsü geri bildirimi üzerine ortak metin stilleri, sekme/panel başlıkları ve giriş alanları yaklaşık bir kademe daha küçültüldü. Sol navigasyon 46 DIP'e, üst bar 64 DIP'e, ana menü sütunu 250 DIP'e alındı.
- Kart dolguları, satır aralıkları ve form kontrol yükseklikleri tekrar sıkılaştırıldı; menü simgeleri de küçültüldü. Segoe UI ve renk paleti korundu. Resmî PDF şablonları ve hesaplama kuralları değiştirilmedi.
- Son ekran görüntüsünde hâlâ iri görünen ana sayfa ve panel içerikleri için, `MainWindow.xaml` içindeki açık `FontSize` değerleri yaklaşık %12 azaltıldı (11.5 DIP altına indirmeden); sayfa genelindeki `Margin` / `Padding` aralıkları da bir kademe daraltıldı. Sol menü 230 DIP, üst bar 60 DIP oldu; ana özet alanındaki sabit sütun ve eylem düğmesi genişlikleri de azaltıldı.
- Son gelen ekran görüntüsü üzerine yazı ve aralıklar tekrar ölçülü biçimde küçültüldü; ortak başlık/alan/kart stilleri bir kademe daha daraltıldı. Sol menü 215 DIP'e çekildi; uzun `YÜKLENİCİ FİRMALAR` etiketi için alan korunuyor. Üst çubuktaki hazırlık halkası 48 × 48 DIP yapılarak çizim alanının içinde kalacak şekilde düzeltildi. CI bu iki yerleşim kararını da koruma denetimine aldı.

Amaç, özellikle 1366 × 768'de daha fazla panel içeriğini görünür yapmak ve görsel yoğunluğu azaltmaktır. Her ekranda gerçek Windows görüntüsüyle kabul kontrolü yine gereklidir.

## 3. Önerilen UI mimarisi

### Katmanlar

- **App.xaml:** renk token'ları, ortak font ailesi, buton, alan, kart ve başlık stilleri.
- **MainWindow.xaml:** ana pencere, üst bar, navigasyon ve ortak kabuk. İş ekranlarının tüm XAML'ini zamanla burada tutmak yerine her ekranı bağımsız `UserControl` olarak ayırın.
- **Views/**: `HomeView.xaml`, `ContractsView.xaml`, `ContractorsView.xaml`, `RoutesView.xaml`, `ProgressPaymentView.xaml`, `ReportsView.xaml`, `HistoryView.xaml`, `AcceptanceView.xaml`, `ArchivesView.xaml`.
- **ViewModels/**: ekran durumu ve komutlar. Birinci aşamada mevcut code-behind akışı çalışır bırakılır; ekranlar taşınırken iş kuralları UI olaylarının içine kopyalanmaz.
- **Core/Services:** hesaplama, kayıt, PDF ve iş kuralları. Ekran düzeni değişince bu servisler değişmemelidir.

Ekranları tek seferde yeniden yazmayın. Bir ekranı UserControl'e taşıyın, eski davranışlarla karşılaştırın ve testten sonra sonraki ekrana geçin.

## 4. Esnek Grid ve metin örüntüsü

Sabit içerik yüksekliğini mümkün olduğunda `Auto`, kalan alanı `*` ile tanımlayın. Uzun gövdeyi kaydırılabilir alanda tutun:

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="*" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>

    <TextBlock Grid.Row="0"
               Text="BÖLÜM BAŞLIĞI"
               TextWrapping="Wrap" />

    <ScrollViewer Grid.Row="1"
                  VerticalScrollBarVisibility="Auto"
                  HorizontalScrollBarVisibility="Disabled">
        <Grid Margin="0,8">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto" />
                <ColumnDefinition Width="*" />
            </Grid.ColumnDefinitions>
            <!-- Label: Auto sütunu; giriş: kalan * sütunu. -->
        </Grid>
    </ScrollViewer>

    <StackPanel Grid.Row="2"
                Orientation="Horizontal"
                HorizontalAlignment="Right">
        <Button Content="İPTAL" MinWidth="96" MinHeight="42" />
        <Button Content="Hesaplamaları Doğrula"
                MinWidth="180" MinHeight="42" Margin="8,0,0,0" />
    </StackPanel>
</Grid>
```

Yatay bir satırda birden fazla uzun eylem düğmesini yan yana sıkıştırmayın. Genişlik azaldıkça iki satıra geçin veya `WrapPanel` kullanın. Düğme metni için `TextBlock TextWrapping="Wrap"` kullanıldığında düğmenin yüksekliği de metnin iki satır olmasına izin vermelidir.

### Metin ve tablo kuralları

- Kullanıcıya dönük açıklamalar için `TextWrapping="Wrap"` kullanın; önemli metinlerde `TextTrimming="CharacterEllipsis"` kullanmayın.
- `TextBox` ve `ComboBox` giriş alanlarında gerçek veri değeri kırpılmamalı; gerekirse araç ipucu ile tam değer gösterilmeli.
- Uzun tabloları `Grid` içinde sabit sütunlara zorlamak yerine yıldız genişlikleriyle kurun. Gerekli olduğunda yatay `ScrollViewer` veya DataGrid'in kendi kaydırmasını kullanın.
- Para sütunlarını sağa hizalayın; açıklama sütunlarını sarın. Satır yüksekliğini sabit küçük bir değere kilitlemeyin.
- Düğmelerin genişliğini içerik adına kodla ebeveynin dışına taşırmayın. Genişlik yetersizse satır düzenini değiştirin veya metni sarın.

## 5. DPI ve ölçüleri doğru yorumlama

WPF'de `Width`, `Height`, `MinWidth`, `MinHeight`, `FontSize` değerleri genellikle DIP'tir. Yaklaşık dönüşüm: fiziksel piksel = DIP × ölçek katsayısı. Örneğin %150 ölçeklemede 1024 DIP yaklaşık 1536 fiziksel piksele karşılık gelir.

Bu sebeple **MinWidth="1366" yazmak**, 1366 fiziksel piksel genişliğindeki ekranda 1366 piksele sığmayı garanti etmez. Pencere alt sınırı ve içerik akışkanlığı birlikte tasarlanmalıdır.

- 1366 × 768 fiziksel ekran / %100: temel kabul çözünürlüğü.
- %125 ve %150: ideal olarak daha yüksek fiziksel çözünürlüklü ekranda test edin.
- 1366 × 768 fiziksel ekran / %150: kullanılabilir alan yaklaşık 911 × 512 DIP'e kadar düşebileceğinden mevcut çift panelli yoğun ana kabuğu aynı anda sığdırmak gerçekçi olmayabilir. Bu birleşim zorunluysa ayrıca dar ekran modu, daraltılabilir/ikon menü ve daha fazla dikey kaydırma tasarlanmalıdır.

Per-Monitor DPI bildirimi Windows'un DPI farkını uygulamaya bildirmesini sağlar; ancak sabit genişlikli sütunları kendiliğinden responsive yapmaz. Ölçüler, kaydırma ve testler hâlâ gereklidir. Kaynak: [Microsoft — WPF otomatik yerleşim](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/use-automatic-layout-overview), [Microsoft — DPI ve device-independent pixels](https://learn.microsoft.com/en-us/windows/win32/learnwin32/dpi-and-device-independent-pixels).

## 6. Tipografi standardı

Mevcut uygulamadaki ortak font ailesi Segoe UI olarak korunur. Yeni/taşınan ekranlarda başlangıç değerleri:

| Bileşen | Başlangıç punto değeri (WPF DIP) |
|---|---:|
| Sayfa başlığı | 22–25 |
| Kart/alan başlığı | 15–18 |
| Gövde / tablo içeriği | 13–14 |
| Alan etiketi | 12–13 |
| Eylem düğmesi | 12–13 |
| İkincil açıklama | 11.5–13 |

Bunlar başlangıç değerleridir; mevcut ekranlardaki açık `FontSize` değerleri ortak stili geçersiz kılabilir. Tüm ekranları bir defada küçültmek yerine, sayfa bazında okunabilirlik ve taşma denetimiyle yaklaştırın. Türkçe büyük/küçük harfler için metin dönüştürmesini kültür duyarlı yapın; başlıklarda `I/İ/ı/i` karakterlerini test edin.

## 7. Kabul testi listesi

- [ ] 1366 × 768, %100: pencere ve tüm içerik çalışma alanına sığıyor.
- [ ] 1920 × 1080, %100 ve %125: kartlar gereksiz boşluk veya taşma üretmiyor.
- [ ] 1920 × 1080, %150: pencere ekran dışına çıkmıyor; metin bulanıklaşmıyor.
- [ ] Farklı ölçekli iki monitör arasında pencere taşınıyor ve yeniden çizim düzgün.
- [ ] “Hesaplamaları Doğrula”, “DÜZENLEME İPTAL”, “PDF OLARAK KAYDET” gibi uzun Türkçe metinler tam görünüyor.
- [ ] Dar genişlikte düğmeler üst üste binmiyor; gerektiğinde alt satıra geçiyor.
- [ ] Alan hataları/açıklamalar tam görünüyor; kayıt listeleri dikey olarak kaydırılabiliyor.
- [ ] Farklı uzunlukta ihale, firma ve hat adları deneniyor.
- [ ] Test edilen gerçek Windows ekran görüntüleri gözden geçiriliyor; yalnızca build/test başarısı görsel doğrulama olarak kabul edilmiyor.

## 8. Değişiklik yaklaşımı

1. Küçük ve izole XAML değişikliği yapın.
2. `dotnet build .\OzelEgitimHakedisSistemi.sln --configuration Debug` ve `dotnet test .\OzelEgitimHakedisSistemi.sln --configuration Debug --no-build` çalıştırın.
3. Uygulamayı gerçek Windows ortamında kabul testi listesiyle kontrol edin.
4. Ekran görüntüsü ve gerekçesiyle doğrulamadan tüm uygulamaya küresel stil değişikliği uygulamayın.
5. Bir ekrandaki düzeltmeyi diğer ekranlara kontrollü şekilde adapte edin.


## 9. Visual Studio 18 çözüm dosyası kararlılığı

`OzelEgitimHakedisSistemi.sln` Visual Studio 18 üst bilgisiyle depoya kaydedildi. Dört proje ve her birinin Debug/Release yapılandırma eşlemeleri korunmuştur. `.sln` dosya biçimi `Format Version 12.00` olarak kalır; `# Visual Studio Version 18` ve `VisualStudioVersion` alanları dosyayı son kaydeden Visual Studio ailesi/sürümü hakkında bilgi taşır. Bu ayrım Microsoft'un [Solution (.sln) file açıklamasında](https://learn.microsoft.com/en-us/visualstudio/extensibility/internals/solution-dot-sln-file) belgelenmiştir.

CI artık çözüm üst bilgisinin VS18 olduğunu, `dotnet sln list` komutunun dört projeyi bulduğunu ve tüm çözümün derlenebildiğini doğrulayan adımlar içerir. Böylece gelecekte ana dalda VS17 üst bilgisine geri dönüş veya proje eşlemelerinin kaybolması otomatik kontrolde yakalanır. VS18 dosyası depodan alındıktan sonra yalnızca IDE sürüm/biçim değişikliği yüzünden yine Changes çıkarsa bu dosyayı commit etmeden önce diff karşılaştırılmalıdır.
