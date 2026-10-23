> **GÜNCEL XLSM ÖNCELİĞİ:** Bu metin önceki analiz tarihindeki kaynaklara göre hazırlanmıştır. Son yüklenen Excel dosyalarıyla çelişen formül, hücre adresi, tutar veya test beklentisi güncel kural değildir. Geçerli kaynak kimliği ve formüller için [SON_XLSM_KAYNAK_PARITESI_2026-10-20.md](SON_XLSM_KAYNAK_PARITESI_2026-10-20.md) esas alınmalıdır. Bu tarihsel metin yalnızca geçmiş inceleme izi olarak korunur.

---

# OEHS — Excel v2 Taraması ve Uygulama Eşleştirmesi

Tarih: 29.09.2026

## 1. VBA taraması

Orijinal `Özel Eğitim Hakediş Ödeme Programı.xlsm` dosyasında `xl/vbaProject.bin` bulundu ve `Module1` içindeki VBA kaynak kodu çıkarılarak incelendi.

Tespit edilen ana makrolar:

- `HakedişToplamlariniAktar`
- `FiyatFarkıVerileriniAktar`
- `RehberPersonelFiyatFarkıVerileriniAktar`
- `KesintiBilgileriniAktarAktar`
- `TumFiyatFarkiAktarimlari`
- `HesapKontrol`
- `SayfaYazdir`
- `DonemSiraNo`

Önemli aktarım ilişkileri:

- Hakediş toplamı: `Hesaplama!K + Hesaplama!T` -> İcmal Verileri aylık hakediş alanı.
- Taşıma fiyat farkı: `Hesaplama!I` -> İcmal Verileri taşıma fiyat farkı aylığı.
- Rehber personel fiyat farkı: `Hesaplama!R` -> İcmal Verileri rehber personel fiyat farkı aylığı.
- Kesinti toplamı: `Hesaplama!AE` -> İcmal Verileri kesinti aylığı.
- Fiyat farkı toplu aktarımı, taşıma ve rehber personel fiyat farkını ayrı ayrı çalıştırır.

## 2. Hesaplama! satır modeli

Taşıma:

- İş günü: `G`
- Günlük fiyat: `H`
- Taşıma fiyat farkı KDV hariç: `I`
- Taşıma iskonto: `J`
- Hakediş tutarı: `K = G*H + I - J`
- KDV: `L = K*10%`
- KDV tevkifatı: `M = L/2`
- Tevkifat sonrası: `N = K + M`
- Brüt: `O = K + L`

Rehber personel:

- İş günü: `P`
- Günlük fiyat: `Q`
- Rehber personel fiyat farkı KDV hariç: `R`
- Rehber iskonto: `S`
- Hakediş tutarı: `T = P*Q + R - S`
- KDV: `U = T*20%`
- KDV tevkifatı: `V = U*9/10`
- Tevkifat sonrası: `W = T + V`
- Brüt: `X = T + U`

Birlikte kullanılan sonuçlar:

- Ek kesin teminat: `AC = I*6% + R*6%`
- Damga vergisi: `AD = K*9,48/1000 + T*9,48/1000`
- Kesinti toplamı: `AE = M+V+Y+Z+AA+AB+AC+AD`
- Toplam brüt: `AF = O+X`
- Net: `AG = AF-AE`

## 3. KDV ve tevkifat

KDV uygulanıyorsa:

- Taşıma KDV: %10
- Rehber personel KDV: %20
- Taşıma tevkifat: KDV'nin 5/10'u
- Rehber personel tevkifat: KDV'nin 9/10'u

OEHS WPF ekranında KDV seçimi `KDV HESAPLA / DAHİL ET` olarak tutulmaktadır.

## 4. Ek kesin teminat

Excel'de ek kesin teminat yalnızca taşıma fiyat farkına bağlı değildir.

`(Taşıma fiyat farkı + Rehber personel fiyat farkı) x %6`

OEHS hesaplama motoru bu iki fiyat farkını ayrı alanlardan alıp birlikte hesaplar.

## 5. İcmal ilişkisi

İcmal Verileri'ndeki hizmet alımı tutarı ve iki ayrı fiyat farkı alanı, Hakediş İcmali formüllerine birlikte aktarılır.

Bu nedenle OEHS'te:

- Taşıma fiyat farkı
- Rehber personel fiyat farkı
- birleşik fiyat farkı
- ek kesin teminat

birbirinden ayrıştırılmış olarak tutulmalıdır.

## 6. Tespit edilen Excel rapor formülü tutarsızlığı

Orijinal workbook'un mevcut örnek verisinde Hesaplama!AG ile Hakediş Raporu!D28 arasında fiyat farkı bulunduğunda fark oluşuyor.

Sebep: Hakediş Raporu tarafında D11 hesabı, H2 içinde zaten bulunan fiyat farkını tekrar D7 üzerinden ekliyor.

Bu nedenle bu konu bilinçli olarak ayrıca işaretlenmiştir. PDF klonlama aşamasında OEHS'in:
1. Excel rapor formülünü birebir taklit etmesi veya
2. Hesaplama!AG sonucunu esas alması

arasında karar verilmeden sessizce değiştirilmemelidir.

## 7. WPF'e aktarılan yeni alanlar

- Rehber iş günü
- Rehber günlük fiyat
- Rehber iskonto
- Rehber fiyat farkı KDV hariç
- Taşıma/rehebr birleşik fiyat farkı
- Taşıma ve rehber ayrı KDV
- Taşıma ve rehber ayrı tevkifat
- Birleşik ek kesin teminat
- İhale tarihi
- Sözleşme tarihi
- Kayıtlı sözleşme/yüklenici/hat bilgilerinin rapor snapshot'ı

## 8. PDF klonlama hedefleri

Referans belgelerde özellikle:

- Hakediş Raporu kesinti başlığının düşey yerleşimi,
- A-I ana tablo satır düzeni,
- H ve I toplam satırlarının gri zeminli olması,
- Hizmet İşleri Hakediş Raporu'nun 2 kolonlu bilgi tablosu,
- Süre Uzatım Kararlarının ayrı tablo başlığı,
- Kabul Tutanağı'nın üst bilgi tablosu ve imza boşlukları,
- Hakediş İcmali'nin sağ üst sayfa/hakediş/hat kutusu

korunacaktır.
