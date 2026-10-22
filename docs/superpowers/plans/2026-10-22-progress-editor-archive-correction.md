# Hakediş Düzenleyicisi ve Arşiv Düzeltme Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Arşiv snapshot'ını güvenle düzelten ve başarılı yeni hesaplamadan sonra giriş taslağını temizleyen hakediş paneli oluşturmak.

**Architecture:** Arşiv düzeltme bağlamı `ProgressPaymentArchive` snapshot'ından kurulacak; canlı koleksiyonlar yalnızca yeni hesaplama için kullanılacak. `MainWindow` arşivde tek kilitli hat bağlamı kullanırken tek/çoklu yeni hesaplama ortak bir başarı-sonrası taslak sıfırlama geçişi kullanacak.

**Tech Stack:** .NET 8, WPF/XAML, xUnit, GitHub Actions PowerShell.

**Spec:** `docs/superpowers/specs/2026-10-22-progress-editor-archive-correction-design.md`

## Global Constraints

- Arşiv düzeltmesi canlı sözleşme, yüklenici veya hat kaydı olmasa da snapshot ile açılmalıdır.
- Arşiv düzeltmesinde yalnızca snapshot hattı seçili ve kilitli kalmalıdır.
- Başarılı yeni hesaplama seçimleri ve taslağı temizlemeli, son hesap kimliğini korumalıdır.
- Kalıcı kayıt başarısızsa giriş taslağı temizlenmemelidir.

## Review Focus

- Canlı kaydı olmayan arşiv snapshot'ı: Task 1 testinde tek hat/doğru ay görünür.
- Eski çoklu seçim: Task 2 testinde arşiv bağlamına sızmaz.
- Başarılı hesap sonrası tekrar hesaplama: Task 3 testinde yeni seçim zorunludur.
- Çoklu-hat başarı sonrası taslak: Task 3 testinde önbellek temizlenir.
- Disk kaydetme hatası: Task 3'te taslağın korunduğu sınanır.

---

### Task 1: Snapshot tabanlı arşiv bağlamı

**Files:**
- Create: `src/OzelEgitimHakedis.Wpf/ArchiveProgressEditorContext.cs`
- Create: `tests/OzelEgitimHakedis.Wpf.Tests/ArchiveProgressEditorContextTests.cs`

**Interfaces:**
- Produces: `ArchiveProgressEditorContext.Create(ProgressPaymentArchive archive)`.
- Returns: snapshot sözleşme, yüklenici, tek `RouteData` ve ödeme dönemi.

- [ ] **Step 1: Canlı kaydı olmayan snapshot için başarısız test yaz**

```csharp
var context = ArchiveProgressEditorContext.Create(archive);
Assert.Equal(archive.Route.RouteNumber, context.Route.RouteNumber);
Assert.Equal(archive.ProgressPayment.PeriodDate, context.PeriodDate);
```

- [ ] **Step 2: Testi kırmızı aşamada çalıştır**

Run: `dotnet test tests/OzelEgitimHakedis.Wpf.Tests/OzelEgitimHakedis.Wpf.Tests.csproj --filter FullyQualifiedName~ArchiveProgressEditorContextTests`

Expected: `ArchiveProgressEditorContext` eksik olduğu için FAIL.

- [ ] **Step 3: Snapshot'tan başka veri aramayan `Create` yöntemini uygula**

- [ ] **Step 4: Hedef testi yeşil çalıştır ve commit et**

```bash
git add src/OzelEgitimHakedis.Wpf/ArchiveProgressEditorContext.cs tests/OzelEgitimHakedis.Wpf.Tests/ArchiveProgressEditorContextTests.cs
git commit -m "Add archive progress editor context"
```

### Task 2: Arşiv düzeltme geçişi

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs: BeginProgressHistoryEdit, ClearArchiveCorrectionState, CancelProgressEditButton_Click`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs`

**Interfaces:**
- Consumes: `ArchiveProgressEditorContext.Create`.
- Produces: `LoadArchiveProgressEditorContext(ArchiveProgressEditorContext context)`.

- [ ] **Step 1: Başarısız UI sözleşme testi yaz**

Test, yükleme yordamının snapshot hattını tek öğeli `ItemsSource` ile seçtiğini ve `SelectedItems.Clear()` ile eski çoklu seçimi sildiğini doğrular.

- [ ] **Step 2: Hedef WPF testini kırmızı çalıştır**

Run: `dotnet test tests/OzelEgitimHakedis.Wpf.Tests/OzelEgitimHakedis.Wpf.Tests.csproj --filter FullyQualifiedName~HakedisCreateHybridUiContractTests`

Expected: `LoadArchiveProgressEditorContext` eksik olduğu için FAIL.

- [ ] **Step 3: Arşiv bağlamı yükleme ve çıkış temizliğini uygula**

Ay seçeneklerini snapshot sözleşmesinden üret; yalnızca snapshot hattını seç; çoklu-hat önbelleğini temizle; kayıt veya iptalde geçici bağlamı kaldır.

- [ ] **Step 4: WPF testlerini yeşil çalıştır ve commit et**

```bash
git add src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs
git commit -m "Stabilize archive payment correction context"
```

### Task 3: Başarı sonrası yeni taslağı sıfırlama

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs: CalculateProgressButton_Click, SaveMultiRouteProgressPayment, ClearProgressEntryFields`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs`

**Interfaces:**
- Produces: `ResetNewProgressDraftAfterCalculation(ProgressPaymentHistoryData lastCalculated)`.

- [ ] **Step 1: Başarısız taslak-sıfırlama sözleşme testi yaz**

Test; tek/çoklu başarı yollarının yeni yordamı çağırdığını, seçimi/ayı/hatları/önbelleği temizlediğini ve `_lastCalculatedHistoryId` değerini koruduğunu doğrular.

- [ ] **Step 2: Hedef testi kırmızı çalıştır**

Run: `dotnet test tests/OzelEgitimHakedis.Wpf.Tests/OzelEgitimHakedis.Wpf.Tests.csproj --filter FullyQualifiedName~HakedisCreateHybridUiContractTests`

Expected: sonuçlandırma yordamı eksik olduğu için FAIL.

- [ ] **Step 3: Tek ve çoklu-hat başarı yollarına sonucu uygula**

Yalnızca kalıcı kayıt başarılı olduktan sonra taslağı temizle; arşiv düzeltme ve kalıcı kayıt-hatası yollarında çağırma.

- [ ] **Step 4: Tam test takımını çalıştır ve commit et**

Run: `dotnet test OzelEgitimHakedisSistemi.sln`

```bash
git add src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs
git commit -m "Clear completed progress payment drafts"
```

### Task 4: Panel bağlam ve işlem yerleşimi

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml: ProgressPanel`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs`

**Interfaces:**
- Produces: `ProgressContextPanel`, `ProgressRouteSelectionPanel` ve `ProgressActionPanel`.

- [ ] **Step 1: Başarısız XAML sıralama testi yaz**

Test, bağlam şeridinin hat seçiminden önce; işlem çubuğunun tüm giriş/özet kartlarından sonra olduğunu ve arşiv düzeltme durumunun görünür olduğunu doğrular.

- [ ] **Step 2: Hedef testi kırmızı çalıştır**

Run: `dotnet test tests/OzelEgitimHakedis.Wpf.Tests/OzelEgitimHakedis.Wpf.Tests.csproj --filter FullyQualifiedName~HakedisCreateHybridUiContractTests`

Expected: adlandırılmış panel eksik olduğu için FAIL.

- [ ] **Step 3: Dört bölümlü panel yerleşimini uygula**

Bağlam şeridi, hat tablosu, giriş/özet kartları ve işlem çubuğunu sıralı kur; arşiv modundaki bağlamı amber renkli/okunur yap.

- [ ] **Step 4: WPF testlerini yeşil çalıştır ve commit et**

```bash
git add src/OzelEgitimHakedis.Wpf/MainWindow.xaml tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs
git commit -m "Clarify progress payment panel workflow"
```

### Task 5: Uçtan uca doğrulama

**Files:**
- Modify: `.github/workflows/oehs-windows-build.yml` yalnızca yeni panel adları CI denetimi gerektirirse.

- [ ] **Step 1: CI statik kontrollerini yerelde çalıştır**

Expected: eski, kırılgan XAML eşleşmesi olmadan PASS.

- [ ] **Step 2: Release test çözümünü çalıştır**

Run: `dotnet test OzelEgitimHakedisSistemi.sln --configuration Release`

Expected: PASS.

- [ ] **Step 3: Diff denetimi ve gerekli CI commit'i yap**

Run: `git diff --check && git status --short`

