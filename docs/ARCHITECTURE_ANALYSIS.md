# OEHS Mimari Analiz — Başlangıç

## Referans: DogruTemin

DogruTemin'in GitHub üzerindeki mevcut yapısı incelendi.

### Tespitler
- C#
- .NET 8
- WPF
- Core / WPF / Tests ayrımı
- Core içinde Models ve Services
- Ayrı hesaplama motoru
- Veri saklama servisi
- Merkezi WPF kaynak ve stilleri
- Hata yakalama ve loglama yaklaşımı
- Test projesi

## OEHS için karar

Başlangıç mimarisi:

```text
OzelEgitimHakedis.Core
        ↑
OzelEgitimHakedis.Wpf

OzelEgitimHakedis.Tests
        ↑
OzelEgitimHakedis.Core
```

Business logic WPF katmanına konulmayacaktır.

## Önemli kural

DogruTemin referans projedir; OEHS bağımsız bir uygulamadır.

Excel formülleri birebir kodlanmayacaktır. Önce Excel'deki iş kuralları çıkarılacak, sonra domain modelleri ve hesaplama motoru tasarlanacaktır.
