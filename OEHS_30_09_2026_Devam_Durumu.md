# OEHS — 30.09.2026 Devam Durumu

## Bugünkü devam noktası

Aktif branch: `feature/wpf-contract-entry-v0.5`

Önceki oturumdaki hedefler devam ettirildi.

### 1. Taşıma Günlük Fiyatı merkezileştirildi

Excel mantığına uygun olarak taşıma günlük fiyatının tek giriş yeri:

**İhale Bilgileri → Taşıma Günlük Fiyatı**

olarak belirlendi.

Hat ekleme ekranından taşıma günlük fiyatı girişi kaldırıldı.

Hakediş ekranındaki taşıma günlük fiyatı kullanıcı girişi olarak kaldırıldı; hesaplama için seçilen hat/sözleşme verisinden otomatik kullanılmaya devam ediyor.

`ContractDraftData` içine:

`TransportDailyPrice`

alanı eklendi.

Eski kayıtlarla uyumluluk için alan varsayılan olarak `0` bırakıldı.

### 2. Hakediş geçmişi arşiv erişimi

Hakediş Geçmişi ekranına:

- **EXCEL** — kayıtlı `OEHS_Hakediş_Belgeleri.xlsx` dosyasını açar.
- **KLASÖR** — ilgili hakediş arşiv klasörünü açar.

butonları eklendi.

Arşiv yapısı:

```
Masaüstü
└── OzelEgitimHakedisSistemi
    ├── Arsiv
    │   └── Hakedisler
    │       └── [hakediş].json
    └── Belgeler
        └── [hakediş klasörü]
            ├── OEHS_Hakediş_Belgeleri.xlsx
            └── OEHS_Hakediş_Bilgi.json
```

### 3. Hakediş geçmişine banka bilgisi

`ProgressPaymentHistoryData` içine opsiyonel:

`BankAccountNumber`

eklendi.

Yeni oluşturulan hakediş kayıtlarında sözleşmedeki banka hesabı korunuyor.

Excel arşivinde banka hesabı ve sözleşme metadata bilgileri gösteriliyor.

### 4. Excel çıktısı genişletildi

Dört Excel sayfasında artık yalnızca hesap sonuçları değil, mevcut hakediş kaydındaki sözleşme metadata bilgileri de veriliyor:

- İşin Adı
- Yüklenici
- Hat No
- Hat Adı
- İhale / Dosya No
- Sözleşme No
- Sözleşme Bedeli
- Banka Hesap No
- İhale Tarihi
- Sözleşme Tarihi
- İş Başlangıcı
- İş Bitişi
- Hakediş No
- Dönem
- Hakediş hesap kalemleri
- Kesintiler
- Brüt
- Net Ödenecek

Excel yazdırma kalıbı:

- A4
- Dikey
- Sol 1 cm
- Sağ 1 cm
- Üst 1 cm
- Alt 1 cm
- Sabit yazdırma alanı
- Tek çalışma kitabında 4 sayfa

## Korunan Excel hesaplama senaryosu

29 taşıma iş günü × 2.608,26 TL
+ 1.500,00 TL taşıma fiyat farkı

30 rehber iş günü × 1.362,49 TL

KDV istisnası yok.

Beklenen:

- Taşıma Hizmet: 75.639,54 TL
- Taşıma Hakediş: 77.139,54 TL
- Rehber Hizmet: 40.874,70 TL
- Rehber Hakediş: 40.874,70 TL
- Toplam: 118.014,24 TL
- KDV: 15.888,89 TL
- Tevkifat: 11.214,42 TL
- Fatura: 129.228,66 TL
- Fiyat Farkı Teminatı: 90,00 TL
- Damga Vergisi: 1.118,77 TL
- Toplam Kesinti: 12.423,20 TL
- Brüt: 133.903,13 TL
- **Net Ödenecek: 121.479,94 TL**

## Yarın / sonraki F5 doğrulama sırası

### 1. GitHub
```powershell
git pull
```

### 2. Temiz derleme
```powershell
dotnet clean .\OzelEgitimHakedisSistemi.sln
dotnet restore
dotnet build .\OzelEgitimHakedisSistemi.sln --configuration Debug
```

### 3. Test
```powershell
dotnet test .\OzelEgitimHakedisSistemi.sln --configuration Debug --no-build
```

### 4. Visual Studio
- Clean Solution
- Rebuild Solution
- F5

## F5 sonrası kontrol

1. Ana ekran tek kompakt özet paneline sığıyor mu?
2. İhale Bilgileri'nde Taşıma Günlük Fiyatı var mı?
3. Hat eklerken taşıma günlük fiyatı tekrar sorulmuyor mu?
4. Hakedişte fiyat otomatik geliyor mu?
5. Hakediş sonucu 121.479,94 TL net veriyor mu?
6. 4 belge tek önizlemede 4 ayrı A4 sayfa olarak görünüyor mu?
7. Tek yazdırma ile dört sayfa birlikte gidiyor mu?
8. Excel dosyası açılıyor mu?
9. Excel'de 4 sayfa var mı?
10. Excel yazdırma önizlemesi A4 ve 1 cm kenar boşluklarıyla doğru mu?
11. Metinler gereksiz küçülmüş mü?
12. Hakediş Geçmişi → EXCEL butonu arşiv dosyasını açıyor mu?
13. Hakediş Geçmişi → KLASÖR butonu ilgili klasörü açıyor mu?

## Bundan sonraki geliştirme

F5 doğrulamasından sonra:

1. Resmi Excel/kurum belge içeriklerini kaynak şablonlarla daha ayrıntılı eşleştirmek.
2. Excel hücrelerini mümkün olduğunca gerçek sayı/tarih hücreleri olarak üretmek.
3. Yazdırma başlıkları ve tekrar eden satırları güçlendirmek.
4. Hakediş geçmişi ve arşiv ekranını tamamlamak.
5. İcmal ekranını Excel'deki toplu hesap mantığıyla genişletmek.
6. Son UI temizliği ve kalan analyzer uyarılarını kapatmak.
7. Tam senaryo ile son regresyon testi.
8. PR #2'nin son durumunu kontrol edip main'e alma aşamasına geçmek.

> Not: Bu aşamada gerçek Windows Visual Studio/F5 çalıştırması henüz yapılmadı. Kod değişiklikleri hazırlandı; gerçek derleme ve çıktı doğrulaması bir sonraki F5 adımında yapılacak.
