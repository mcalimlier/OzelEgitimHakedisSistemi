# OEHS geçici arkadaş deneme derlemesi — 2026-10-19

- Amaç: mevcut `feature/official-payment-document-templates` dalından yeni Windows x64 deneme paketini CI üzerinden üretmek.
- Kaynak kod ve hesaplama davranışlarında değişiklik yapılmadı; bu commit yalnızca deneme derlemesini tetiklemek için kayıt dosyası ekler.
- Beklenen çıktı: `OEHS-Arkadas-Deneme-Paketi` (OEHS.exe, OEHS_DENEME_PAKETI.flag, KULLANIM.txt) ve Windows x64 derleme artefaktı.
- Doğrulama: GitHub Actions sonucu ve artefaktın gerçekten oluştuğu ayrıca kontrol edilmeden başarılı sayılmayacak.
- Ana dala birleştirme veya yayınlama yapılmadı.
