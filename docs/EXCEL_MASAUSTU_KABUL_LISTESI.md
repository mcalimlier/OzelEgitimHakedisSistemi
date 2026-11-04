# Excel Masaüstü Kabul Kontrol Listesi — SON XLSM (20.10.2026)

**Bu listedeki kaynak:** [SON_XLSM_KAYNAK_PARITESI_2026-10-20.md](SON_XLSM_KAYNAK_PARITESI_2026-10-20.md). Eski listelerde geçen E4/K16/AB4 gibi adresler yeni dosyada doğrulanmadan kullanılmamalıdır. Bu liste son yüklenen dosyaları temel alır; geçmiş XLSM'lerle ilgili listeler güncel kabul için geçersizdir.

## 1. Güvenli test hazırlığı
- [ ] Son Temel ve Özel Eğitim XLSM dosyalarının ayrı kopyalarını oluştur; orijinalleri değiştirme.
- [ ] Dosya adı, boyut, SHA-256, Excel sürümü, hesaplama modu, tarih/saat ve test kullanıcısını kaydet.
- [ ] VBA kodunu ve makro adını incelemeden makro çalıştırma. Toplu temizleme/silme makrolarını çalıştırma.
- [ ] Excel'de tam yeniden hesaplama yap; makro öncesi hücre değerlerini ve formülleri kaydet.
- [ ] Makroyu yalnızca test kopyasında çalıştır; makro sonrası hücreleri, seçili dönemi ve seçili hattı tekrar kaydet.

## 2. Güncel kaynak kimliği
- [ ] Temel Eğitim: SHA-256 `37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296`.
- [ ] Özel Eğitim: SHA-256 `c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e`.
- [ ] Kaynak dosyanın hash'i farklıysa bu listedeki beklenen değerleri otomatik kabul etme; önce kaynak değişikliğini kayda al.

## 3. Temel Eğitim / Ortaöğretim — kaynak satırları 4–8
- [ ] `Hesaplama Bilgileri!K4:W8` aralığındaki K, L, M, O, T, U, V, W sonuçlarını kaydet.
- [ ] Formül zincirini doğrula: `K=(G*H)+I-J`; `T=I*6%`; `U=K*9.48/1000`; `V=P+Q+R+S+T+U+M`; `W=O-V`.
- [ ] Özellikle satır 4: K=319.991,9228; T=441,921; U=3.033,523428144; V=19.475,040568144; W=332.516,074511856.
- [ ] İcmal aktarımı sonrası ilgili satırdaki `P4/Q4/R4` ve `AD4` değerlerini kaydet; son kaynakta P/Q/R'nin I ile, AD'nin V ile eşleştiğini doğrula.
- [ ] İcmalde C8=K4 ve D8=R4+AD4, E8=C8+D8 olup olmadığını kaydet. Bu zincir I fiyat farkını C içinde ve D içinde tekrar sayıyor olabilir; farkı gizleme, Excel/VBA makro sonrası sonucu ayrı kaydet.

## 4. Özel Eğitim — kaynak satırları 4–6
- [ ] `Hesaplama!K4:AG6` içinde K, T, AC, AD, AE, AF, AG sonuçlarını kaydet.
- [ ] Formül zincirini doğrula: `K=(G*H)+I-J`; `T=(P*Q)+R-S`; `AC=I*6%+R*6%`; `AD=K*9.48/1000+T*9.48/1000`; `AE=M+V+Y+Z+AA+AB+AC+AD`; `AF=O+X`; `AG=AF-AE`.
- [ ] Özellikle satır 4: K=43.630,00; T=34.037,27; AC=661,80; AD=736,2857196; AE=27.328,7843196; AG=61.508,9396804.
- [ ] İcmal C8, D8 ve E8 hücrelerini kaydet. Son dosyada C8=K+T, D8=I+R, E8=C8+D8 örüntüsünü doğrula; fiyat farkı ikinci kez ekleniyorsa kayda al, otomatik olarak onaylı davranış sayma.

## 5. Dört PDF × iki program — 8/8 kabul
Her program için aynı test girdisi ve aynı dönem/hatla dört PDF üret:
- [ ] Hakediş Raporu
- [ ] Hizmet İşleri Hakediş Raporu
- [ ] Hizmet İşleri Kabul Tutanağı
- [ ] Hakediş İcmali

Her PDF için:
- [ ] Dönem, yüklenici, hat/iş adı, sözleşme tarihleri ve iş günü bilgileri.
- [ ] Brüt tutar, fiyat farkı, KDV, tevkifat, ek kesin teminat, damga vergisi, kesintiler ve net tutar.
- [ ] İmza/unvan alanları, taşma/kesilme, sayfa yönü ve A4 ölçüsü.
- [ ] Excel PDF'si ve OEHS PDF'si yan yana kontrol edildi; fark varsa hücre/formül/alan bazında not edildi.
- [ ] PDF dosya adı, boyutu ve SHA-256 kaydedildi.

## 6. Sonuç kaydı ve kapatma ölçütü
- [ ] Kaynak formül önbelleği, Excel tam hesaplama sonucu ve VBA sonrası sonuç ayrı sütunlarda saklandı.
- [ ] Excel formülü ile VBA aktarımı veya OEHS arasında fark varsa hiçbir tutar sessizce değiştirilmedi; kaynak hücre, beklenen/gerçek değer ve karar sahibi kaydedildi.
- [ ] 8/8 PDF için finansal ve görsel kabul kanıtı mevcut.
- [ ] Test sonucu, Excel sürümü ve test kopyası hash'i devam notuna eklendi.

**Kabul sınırı:** LibreOffice yeniden hesaplama testi veya C# PDF regresyonu, masaüstü Excel/VBA çalıştırması ve gerçek Excel PDF'lerinin görsel kabulü yerine geçmez. Bu kanıtlar yoksa kabul tamamlanmış sayılmaz.
