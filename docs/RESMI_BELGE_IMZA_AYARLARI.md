# Resmî belge imza ayarları

OEHS'nin dört resmî PDF şablonunda kullanılan düzenleyen, kontrol eden ve onaylayan adları/unvanları JSON ayar dosyasından okunur.

## Ayar dosyasının konumu

Uygulama ilk resmî belgeyi oluştururken dosya yoksa varsayılan değerlerle otomatik oluşturur:

```text
%LOCALAPPDATA%\OEHS\official-document-settings.json
```

Windows Gezgini'nin adres çubuğuna bu yolu yapıştırabilirsiniz. Dosyayı Not Defteri ile açıp JSON değerlerini değiştirin. Değişiklikler bir sonraki belge oluşturma/önizleme isteğinde yeniden okunur; uygulamayı yeniden başlatmak gerekmez.

## Alanlar

- `preparerName`: Düzenleyen adı
- `preparerRole`: Düzenleyen unvanı
- `controllerName`: Kontrol eden adı
- `controllerRole`: Kontrol eden unvanı
- `approverName`: Onaylayan adı
- `approverRole`: Onaylayan unvanı

Örnek dosya: [official-document-settings.example.json](official-document-settings.example.json)

## Güvenli kullanım

Dosyada geçerli JSON kullanılmalıdır. Boş veya yalnızca boşluk içeren alanlarda standart varsayılan değer kullanılır. JSON biçimi bozuksa OEHS açık bir hata verir; yanlış veya eksik imza bilgisiyle sessizce belge üretmez.

Bu ayar yalnızca imza isimlerini ve unvanlarını değiştirir; hakediş hesaplarını, kesintileri ya da hukuki belge metinlerinin mevzuata uygunluğunu doğrulamaz. Resmî belgeleri kullanımdan önce kurumunuzun onayladığı örneklerle karşılaştırın.
