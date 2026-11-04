# VBA, nihai PDF ve gerçek DPI kabulü — 25 Ekim 2026

Genel kabul **24/25 kapı = %96**. Gerçek Windows %150 DPI kontrolü bu cihazda yapılamadığından yayın kabulü açık kalır. Yüzdeler önceki rapordaki beşer kapının kapanma oranıdır; tüm olası girdiler için hatasızlık garantisi değildir.

| İşlevsel çekirdek | Kapı | Kabul | Bu çalışmadaki kanıt |
|---|---:|---:|---|
| Temel Eğitim hesaplama | 5/5 | %100 | Gerçek VBA sonrası hesap hücreleri ve rapor zinciri |
| Özel Eğitim hesaplama | 5/5 | %100 | Gerçek VBA sonrası hesap hücreleri ve rapor zinciri |
| Hakediş rapor zinciri | 5/5 | %100 | 14 dönem; kaynak sınır hataları açıkça ayrılmış geçmiş toplamı; resmî alanlar |
| İcmal ve aktarım | 5/5 | %100 | 49 aktarım; 14 E21; düzeltilmiş kaynakta aylık satırlar ve toplamlar |
| Dört PDF ve WPF | 4/5 | %80 | Sekiz belge çifti; 74 sayısal alan; A4 ve görsel inceleme. Gerçek %100/%125 geçti; cihazda %150 seçeneği yok. |

Tek fiziksel `OzelEgitimHakedis.Core` kütüphanesi vardır. Tablodaki beş satır işlevsel kabul alanlarıdır.

## Sonuç ve kapsam

- Release çözüm testi: **442/442** — Core 265, WPF 177, atlanan 0.
- Microsoft Excel/VBA: **49/49 gerçek hücre aktarımı**. Hedef önce `-999991` ile işaretlendi; eski önbellek sonuçları başarı sayılmadı.
- Her programda Ekim, Kasım, Aralık 2026; Ocak, Mayıs, Haziran ve Temmuz 2027: toplam **14 dönem**. Hesap, rapor ve E21 vektörleri saklandı.
- Aktarım sonrası uygulama paritesi: **285 sayısal hücre karşılaştırması (VBA dönem vektörleri ilk hattın satır 4 verilerini kullanır; sekiz hattın tamamı önceki kaynak/WPF kayıt testinde kapsanır)** ve kaynakta D10 bulunmayan iki Temmuz için bağımsız geçmiş-toplamı kontrolü.
- İki programın dört belge türü: **8 Excel + 8 OEHS PDF**, her biri tek sayfa A4. Rapor/icmalin **74 sayısal alanı** hem testlerde hem son PDF metinlerinde eşleşti.
- Sekiz belge çifti yan yana render edilip incelendi. Tutarlar, dönemler, sözleşme bilgileri, imzacılar, Türkçe karakterler ve kesilme/çakışma kontrol edildi. Şablonların yazı boyutları ve bazı çizgi/boşluk tercihleri farklıdır; piksel özdeşliği ileri sürülmez.
- Kaynak Özel Eğitim raporundaki sabit **24.06.2026 imza tarihi**, üretilen referans tarihle eşit sayılmadı. OEHS doğru referans tarihi **25.10.2026** basar. Bu bilinen kaynak farkı manifestte ayrı listelenir.

Bu çalışma üretim finansal formüllerini değiştirmedi. Test araçları, regresyon vektörleri ve kanıtlar eklendi. Normal uygulama kayıtlarına test verisi yazılmadı; asıl XLSM dosyaları kaydedilmedi. Kaynakların SHA-256 değerleri önceki raporla aynı kaldı.

## Gerçek VBA yöntemi

`Verify-SourceVbaTransferAcceptance.ps1` yalnız SHA-256 kimliği incelenmiş iki referans dosyayı kabul eder. Her biri ayrı kopyada açılır. Excel olayları kapalı, makro politikası `ByUI` bırakılır; güvenlik ayarı düşürülmez.

Asıl `Module1` yordamları Excel'in VBA motorunda çalıştırılır. Toplu koşumda eklenen `QA_DialogAdapter`, yalnız bilinen aktarım onaylarını ve tamamlanma mesajlarını kaydeder/yanıtlar. Bilinmeyen mesaj iptal edilir ve koşum başarısız olur. Bu, VBA'nın yeniden yazılmış bir C# benzetimi değildir. Canlı `Module1` kod metninin önceki/sonraki SHA-256 değerleri aynıdır. Bu hash'ler satır sonu ve VBE gösterimi nedeniyle statik OLE çıkarımının hash'iyle karıştırılmamalıdır.

Kaynak kopyalarındaki düzeltmeler:

| Kaynak sorunu | Kopyadaki düzeltme / kabul yaklaşımı |
|---|---|
| Özel Eğitim AP3'te yinelenen Aralık başlığı | Önceden kabul edilen Kasım bağlantısı: `=Anasayfa!I8` |
| Özel Eğitim O4 ilk MATCH Mayıs'ta, ikinci MATCH Haziran'da bitiyor | İlk aralık Haziran'a uzatıldı. Hata önce yeni parite testinde görüldü; düzeltilmiş Excel yeniden çalıştırıldı. |
| Temel Eğitim Temmuz başlıkları yılı içermiyor; makro tam dönem arıyor | Yalnız kopyada N3/Z3/AL3 `Temmuz 2027`. Orijinal başlıkla gerçek VBA'nın hedefe yazmadan reddettiği ayrıca yeniden üretildi. |
| Kaynak iki programda Temmuz raporu D10 aralığı dışında | Kaynak boş hücre eşleşmiş sayı sayılmadı. OEHS'nin önceki okul-yılı kayıtlarını koruyan toplamı, ham gün/fiyat/iskonto/fiyat farkı bileşenlerinin bağımsız toplamıyla kontrol edildi. E21'in mevcut Temmuz sıfır politikası değiştirilmedi. |
| İcmalde fiyat farkının C ve D'de tekrar sayılması | Önceden kabul edilen C6:C16 taban ayrımı yalnız kaynak kopyasında uygulandı. |
| Hizmet raporunda teslim tarihi bitiş tarihine bağlı | F15, sözleşmenin gerçek başlangıç tarihine bağlandı. Uygulama zaten başlangıç tarihini kullanıyor. |
| Özel rapor başlığı sabit Nisan tarihi | Kopyada seçili Kasım dönemiyle düzeltildi. Sabit eski imza tarihi ayrıca kaynak farkı olarak kaldı. |

Orijinal Temmuz başlıklarıyla olumsuz koşum **beklenen şekilde başarısızdır**: makro `Temmuz 2027 başlığı bulunamadı` mesajını verir ve N4 `-999991` olarak kalır. Bu hata başarılı aktarım sayısına eklenmedi. Düzeltilmiş koşum 49/49 geçti.

## Gerçek DPI ve tek açık kapı

`NativeDpiAcceptanceTests.cs`, gösterilmiş gerçek bir HWND'de PerMonitorV2 farkındalığıyla `GetDpiForWindow` ve WPF DPI değerini karşılaştırır. İş parçacığı farkındalığı değiştirilir, **Windows ekran ölçeği değiştirilmiş gibi yapılmaz**. Önceki iş parçacığı ayarı sonunda geri yüklenir.

Ölçülen gerçek değerler: **Windows/WPF 96 DPI (%100)** ve **Windows/WPF 120 DPI (%125)**. Her iki ölçekte üç kart girdisinin üst konumu **140,2 DIP**, yüksekliği **34 DIP**. Altı ayrı hat satırı gerçek pencerede oluşturuldu ve son satıra erişildi. Kanıt görüntüleri, bu gerçek DPI'deki WPF görsel ağacı render'larıdır; masaüstü ekran yakalaması oldukları iddia edilmez.

%125, kullanıcı Windows ölçeğini değiştirdikten sonra `OEHS_EXPECTED_NATIVE_DPI=120` ile gerçekten test edildi ve geçti. Kullanıcı bu ekranda **%150 seçeneğinin bulunmadığını** bildirdi. Bu nedenle 144 DPI ölçümü yapılamadı; 150 varsayımıyla bitmap büyütülüp gerçek testmiş gibi gösterilmedi.

Windows Ayarlar penceresi başlangıçta otomasyon aracına içerik döndürmedi; alternatif araç zaman aşımına uğradı. Ölçek geçişlerini kullanıcı yaptı. Global DPI/güvenlik kayıt defteri ayarı, sahte DPI mesajı veya oturum kapatma kullanılmadı. Kullanıcı testi bitirirken %100'e geri döndü; son koşumda gerçek Windows/WPF değeri tekrar 96 DPI ölçüldü ve test geçti.

Açık kalan 144 DPI için %150 sunan başka bir ekran/cihazda aynı test `OEHS_EXPECTED_NATIVE_DPI=144` ile çalıştırılmalıdır. Destek kapsamı sessizce daraltılmadı: mevcut %100/%125 kabulü tamam, önceki %150 hedefi açık. Ölçek seçeneklerinin ekran yapılandırmasına bağlı olabildiğine ilişkin [Microsoft DPI açıklaması](https://learn.microsoft.com/en-us/windows-hardware/manufacture/desktop/dpi-related-apis-and-registry-settings?view=windows-11).

```powershell
$env:OEHS_EXPECTED_NATIVE_DPI='144'
$env:OEHS_NATIVE_DPI_OUTPUT=(Join-Path (Get-Location) '.superpowers/dpi-acceptance')
dotnet test tests/OzelEgitimHakedis.Wpf.Tests -c Release --no-build --filter FullyQualifiedName~PaymentPanel_ShouldAlignOnNativeWindowAtActualMonitorDpi
```

## Yeniden üretme ve kanıtlar

```powershell
./scripts/Verify-SourceVbaTransferAcceptance.ps1 -OutputRoot .superpowers/vba-acceptance
./scripts/Export-SourceExcelAcceptancePdfs.ps1 -OutputRoot .superpowers/pdf-acceptance
$env:OEHS_FINAL_PDF_DIR=(Join-Path (Get-Location) '.superpowers/pdf-acceptance')
dotnet test tests/OzelEgitimHakedis.Wpf.Tests -c Release --filter FullyQualifiedName~FinalExcelFields
dotnet test -c Release --no-restore
```

VBA olumsuz kaynak denemesi için `-KeepOriginalJulyHeaders` verilir; sıfır olmayan çıkış beklenir. PDF karşılaştırma senaryosu Kasım 2026 ve 25 Ekim 2026 referans tarihidir; başka bir dönemin genel dışa aktarma aracı değildir. Kaynak Excel'de İKN bulunmadığından bu eski kaynak karşılaştırmasında o alan boş bırakılmıştır; uygulamanın İKN yazdırma/saklama kontrolleri ayrı mevcut regresyonlarda yer alır.

Kalıcı kanıtlar:

- [49 VBA hedef hücresi](evidence/2026-10-25-vba-transfers.json)
- [VBA modül bütünlüğü](evidence/2026-10-25-vba-module-integrity.json)
- [Kaynak kopyası hücre değişiklikleri](evidence/2026-10-25-source-copy-corrections.json)
- [Orijinal Temmuz reddi](evidence/2026-10-25-source-july-rejection.json)
- [Sekiz PDF çifti ve SHA-256 manifesti](evidence/2026-10-25-final-pdf-comparison.json)
- [Başlangıca dönüş: gerçek 96 DPI](evidence/2026-10-25-restored-native-dpi-96.json)
- [Gerçek 96 DPI ölçümü](evidence/2026-10-25-native-dpi-96.json) ve [gerçek 120 DPI ölçümü](evidence/2026-10-25-native-dpi-120.json)
- Test vektörleri: `excel-final-document-fields-2026-10-25.json`, `excel-vba-periods-2026-10-25.json`.

Bu bilgisayardaki 16 nihai deneme PDF'si ve sekiz yan yana karşılaştırma `.superpowers/icmal-2026-10-25/final-pdfs/` altındadır. Normal verilerden ayrıdır; PDF/PNG dosyaları depoya eklenmez. Test sonuçları iki test projesinin `TestResults/release-acceptance.trx` dosyalarındadır.

Öz inceleme yapıldı; bu oturumda bağımsız inceleme ajanı aracı bulunmadı. Merge, push veya yayın yapılmadı. Yayını kapatan son koşul gerçek %150 DPI kanıtıdır.
