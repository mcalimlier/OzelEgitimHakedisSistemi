# Excel Masaüstü Kabul Kontrol Listesi

Tarih: 18.10.2026  
Amaç: Kaynak XLSM ile OEHS arasındaki finansal davranışı tahminle değil, aynı veriyle gerçek Excel/VBA çıktısı üzerinden karşılaştırmak.

## Kullanım ve güvenlik

- Orijinal iki XLSM dosyasını değiştirmeyin. Her program için ayrı bir test kopyası oluşturun.
- Makroları yalnızca yetkili/güvenilir çalışma ortamında etkinleştirin.
- Her testte dosya adı, SHA-256, Excel sürümü, hesaplama modu, seçili dönem, hat anahtarı ve test zamanı kaydedilsin.
- Her senaryodan önce tam yeniden hesaplama yapın; makro öncesi ve sonrası değerleri ayrı kaydedin.
- Kaynak Excel ve OEHS aynı hakediş girdilerini kullanmalı. Ekran görüntüsü tek başına yeterli değil; hücre adresi ve sayısal değer de yazılmalı.
- Kaynak Excel PDF'leri yoksa dört PDF kabul kapısı kapatılmış sayılmaz.

## A. Temel Eğitim / Ortaöğretim

### Senaryo B1 — E21 önceki ay aktarımı
- [ ] Anasayfa!E4 seçili dönemini Kasım 2026 yap.
- [ ] Hat anahtarını Anasayfa!K16 üzerinden doğrula; K17 boşken E21'in hangi anahtarı kullandığını not et.
- [ ] İcmal Verileri!AB3:AL3 aylık başlıklarını, AB4:AL4 ilgili hat kesintilerini ve AM4 toplamını kaydet.
- [ ] Makro öncesi Hakediş İcmali!E21 değerini kaydet.
- [ ] İlgili aylık aktarım makrosunu çalıştır; makro sonrası E21 ve AB:AM hücrelerini kaydet.
- [ ] OEHS'nin Ekim 2026 kesintisini Kasım icmaline taşıma sonucuyla karşılaştır. Beklenti varsayımı ile literal kaynak E21 davranışını ayrı raporla.

### Senaryo B2 — Teminat/damga ve net tutar
- [ ] Aynı örnek hat için Hesaplama Bilgileri!I4, K4, T4, U4, V4, W4 değerlerini kaydet.
- [ ] T5:W6 aralığını da kaydet; yalnız tek satırdan genelleme yapma.
- [ ] Hakediş Raporu!D6, D7, D8, D10, D11, D28 değerlerini makro öncesi/sonrası kaydet.
- [ ] D28 ile W değerini ayrı ayrı karşılaştır; farkı otomatik olarak hata veya resmî tutar kabul etme.
- [ ] T'nin elle girilen sabit mi, kalan örnek veri mi, yoksa eksik formül mü olduğunu workbook/VBA davranışıyla doğrula.

## B. Özel Eğitim

### Senaryo S1 — İcmal ay başlıkları
- [ ] Anasayfa!E5 dönemini sırayla Kasım 2026, Aralık 2026, Ocak 2027, Haziran 2027 ve Temmuz 2027 yap.
- [ ] Anasayfa!K17 hat anahtarını ve ilgili hat numarasını kaydet.
- [ ] Her dönem için İcmal Verileri!AN3:AX3 başlıklarını ve AN4:AX4 değerlerini makro öncesi/sonrası kaydet.
- [ ] Hakediş İcmali!E21 değerini her dönem için kaydet.
- [ ] Kasım atlanan/Aralık tekrarlanan başlık kusurunun gerçek Excel/VBA aktarımında da oluşup oluşmadığını belirle.

### Senaryo S2 — Rapor D28 ile hesaplama neti
- [ ] Aynı hakediş için Hesaplama!AG4:AG6 değerlerini kaydet.
- [ ] Hakediş Raporu!D6, D7, D8, D10, D11, D28 değerlerini kaydet.
- [ ] Satır 4, 5 ve 6 için D28-AG farkını ayrı hesapla ve raporla.
- [ ] Makroları çalıştırmadan ve çalıştırdıktan sonraki sonuçları ayrı tut.

## C. Dört PDF — her iki programda aynı hakediş

Her program ve test hakedişi için:
- [ ] Hakediş Raporu PDF
- [ ] Hizmet İşleri Hakediş Raporu PDF
- [ ] Hizmet İşleri Kabul Tutanağı PDF
- [ ] Hakediş İcmali PDF

Dört PDF birlikte kontrol edilmeli:
- [ ] Tutarlar: kaynak hücreler ve OEHS hesap sonuçlarıyla uyum
- [ ] Dönem / hizmet başlangıç-bitiş tarihleri
- [ ] Yüklenici ve sözleşme/ihale bilgileri
- [ ] Hat numarası ve güzergâh adı
- [ ] İmza adları ve imza alanları
- [ ] Sayfa boyutu, yönü, kesilme/taşma ve sayfa sayısı
- [ ] İcmal ay sırası Eylül–Temmuz; programlar arası kural birleştirilmemeli

## Sonuç kayıt şablonu

Her senaryo için aşağıdakiler yazılmalı:
- Program / test senaryosu:
- Kaynak XLSM dosyası ve SHA-256:
- Excel sürümü ve tarih/saat:
- Seçili dönem / hat:
- Makro öncesi hücre değerleri:
- Makro sonrası hücre değerleri:
- OEHS karşılıkları:
- Farklar ve farkın yönü:
- Dört PDF kontrolü (var/yok ve bulgular):
- Sonuç: geçti / kaldı / karar bekliyor
- Karar gerekiyorsa seçenekler ve karar sahibi:

**Kabul kuralı:** Gerçek Excel/VBA çıktısı olmadan bu kontrol listesi tamamlanmış sayılmaz. LibreOffice yeniden hesaplaması ve sentetik PDF smoke çıktıları bu kanıtın yerine geçmez.
