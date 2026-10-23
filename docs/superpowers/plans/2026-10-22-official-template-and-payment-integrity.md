# Resmî Şablon Uyumu ve Hakediş Bütünlüğü Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hakediş hesap/PDF doğruluğunu, resmî Excel terminolojisini ve dört bölümlü responsive hakediş çalışma alanını güvenceye almak.

**Architecture:** Para girişleri gösterimden bağımsız tam `decimal` hassasiyetini koruyacak. Resmî belge üretimi her parasal ayı icmale taşıyacak ve PDF metnini doğrudan doğrulayacak. Program türü, XAML etiketlerini ve belge adlarını belirleyen küçük bir şablon profiline bağlanacak; ana pencere bu profili UI bağlamında kullanacak.

**Tech Stack:** .NET 8, WPF/XAML, xUnit, PdfSharpCore tabanlı vektör PDF yazıcısı, GitHub Actions PowerShell.

**Spec:** `docs/superpowers/specs/2026-10-22-official-template-and-payment-integrity-design.md`

## Global Constraints

- Günlük fiyatın hesap hassasiyeti seçme/düzenleme akışında iki ondalığa indirgenemez.
- Pozitif fiyat farkı, hizmet tabanı sıfır olsa da icmal/PDF toplamında görünür.
- Temel ve Özel Eğitim resmî etiket/sayfa profilleri birbirinden ayrılır; serbest `İşin Adı` girişi zorunlu değildir.
- Başlangıç ve bitiş tarihi zorunludur; boş değer bugüne dönüştürülemez.
- Panel sırası bağlam → hatlar → bileşen/özet → işlemler olarak kalır.
- Kullanıcının sağladığı XLSM/RAR dosyaları değiştirilmez, VBA çalıştırılmaz.

## Review Focus

- `19539.1608` günlük fiyatının düzenleme sonrası PDF netinde kuruş farkı yaratmaması.
- Sıfır hizmet ve pozitif fiyat farkı ayının icmalde görünmesi.
- 1024×600’de menü, geçmiş satırı ve işlem düğmelerinin erişilebilir kalması.
- Temel/Özel profil değişiminde ekran etiketlerinin doğru resmî başlığa dönmesi.
- Uzun yüklenici/güzergâh metninin PDF’de sessizce kaybolmaması.

---

### Task 1: Hassas günlük fiyat ve fiyat-farkı icmal bütünlüğü

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs`
- Modify: `src/OzelEgitimHakedis.Wpf/OehsOfficialDocumentFactory.cs`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/OfficialDocumentOutputTests.cs`
- Modify: `tests/OzelEgitimHakedis.Tests/CurrentBasicWorkbookDiscrepancyTests.cs`

**Interfaces:**
- Preserves: existing `ProgressPaymentInput` decimal fields and calculation service contracts.
- Produces: full-precision editor transfer and an icmal row predicate that includes material price differences.

- [ ] **Step 1: Write failing precision and zero-service-price-difference regressions**

Use `19539.1608m` to assert the selection/recalculation path preserves report net; create a KDV-muaf 0-day, 100 TL price-difference document and assert its month/total exists in icmal.

- [ ] **Step 2: Run focused tests RED**

Run: `dotnet test tests/OzelEgitimHakedis.Wpf.Tests/OzelEgitimHakedis.Wpf.Tests.csproj --filter FullyQualifiedName~OfficialDocumentOutputTests` and the affected core test filter.

Expected: current rounding / `service > 0m` behavior fails the new assertions.

- [ ] **Step 3: Separate display formatting from calculation input and broaden icmal inclusion**

Replace editor population sites that currently use `ToString("N2")` for calculation fields with an invariant full-precision display helper appropriate to `tr-TR`; do not change result-card currency formatting. In `OehsOfficialDocumentFactory`, include an otherwise valid report month when its positive/negative reportable service, guide service, or price-difference amount is nonzero.

- [ ] **Step 4: Run focused GREEN and full solution tests; commit**

```bash
dotnet test OzelEgitimHakedisSistemi.sln --configuration Release
git add src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs src/OzelEgitimHakedis.Wpf/OehsOfficialDocumentFactory.cs tests
git commit -m "Preserve payment precision in official totals"
```

### Task 2: PDF field completeness and overflow proof

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/OehsOfficialDocumentFactory.cs`
- Modify: `src/OzelEgitimHakedis.Wpf/PdfVectorWriter.cs`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/OfficialDocumentOutputTests.cs`

**Interfaces:**
- Preserves: `OehsOfficialDocumentFactory.BuildSeparate` and `PdfVectorWriter.Save` public contracts.
- Produces: PDF-visible contract identifiers and explicit overflow behavior.

- [ ] **Step 1: Write failing PDF text regressions**

Generate a document with test IKN/contract number and long contractor/route text; extract generated PDF text using the project’s existing PDF test mechanism and assert identifier presence plus either complete text or a deterministic overflow exception.

- [ ] **Step 2: Run PDF-focused RED test**

Run: `dotnet test tests/OzelEgitimHakedis.Wpf.Tests/OzelEgitimHakedis.Wpf.Tests.csproj --filter FullyQualifiedName~OfficialDocumentOutputTests`.

- [ ] **Step 3: Add identifiers and safe wrapping/failure semantics**

Place both identifiers in the official contract context row. Make `PdfVectorWriter` calculate the remaining line count before stopping; throw a descriptive document-layout exception when text cannot fit rather than discarding it.

- [ ] **Step 4: Run PDF tests GREEN and commit**

```bash
git add src/OzelEgitimHakedis.Wpf/OehsOfficialDocumentFactory.cs src/OzelEgitimHakedis.Wpf/PdfVectorWriter.cs tests/OzelEgitimHakedis.Wpf.Tests/OfficialDocumentOutputTests.cs
git commit -m "Verify official PDF fields and overflow"
```

### Task 3: Resmî Excel terminoloji profili ve türetilmiş iş adı

**Files:**
- Create: `src/OzelEgitimHakedis.Wpf/OfficialTemplateProfile.cs`
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml`
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs`
- Create: `tests/OzelEgitimHakedis.Wpf.Tests/OfficialTemplateProfileTests.cs`

**Interfaces:**
- Produces: `OfficialTemplateProfile.For(TransportationProgramType)` with exact sheet and label properties.
- Consumes: selected transportation program type from the current form.

- [ ] **Step 1: Write failing profile and XAML contract tests**

Assert exact profile values: Temel `Hesaplama Bilgileri` / `Hizmet İşleri Hakediş Raporu`; Özel `Hesaplama` / `Hizmetişleri Hakediş Raporu`. Assert `İşin Adı` is a read-only derived presentation, not a required editable input, and price-difference/net labels match the selected profile.

- [ ] **Step 2: Run WPF tests RED**

Run: `dotnet test tests/OzelEgitimHakedis.Wpf.Tests/OzelEgitimHakedis.Wpf.Tests.csproj --filter "FullyQualifiedName~OfficialTemplateProfileTests|FullyQualifiedName~HakedisCreateHybridUiContractTests"`.

- [ ] **Step 3: Implement profile and bind label refresh**

Create an immutable profile record. Update UI labels from the selected program, change work-name display to the `Hattın Adı + " Hattı Öğrenci Taşıma İşi"` derivation, and remove its required-field validation while preserving legacy saved `WorkName` as a fallback only.

- [ ] **Step 4: Run WPF suite GREEN and commit**

```bash
git add src/OzelEgitimHakedis.Wpf/OfficialTemplateProfile.cs src/OzelEgitimHakedis.Wpf/MainWindow.xaml src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs tests/OzelEgitimHakedis.Wpf.Tests
git commit -m "Align payment labels with official templates"
```

### Task 4: Erişilebilir gezinme ve zorunlu sözleşme tarihleri

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml`
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs`

**Interfaces:**
- Preserves: `NavigationButton_Click` and contract save event contracts.
- Produces: `Summary` and `Acceptance` navigation targets plus date-required validation.

- [ ] **Step 1: Write failing navigation/date validation tests**

Assert dedicated navigation buttons target `Summary` and `Acceptance`, their panels become visible under those tags, and missing start/end dates are added to the save validation errors before a `ContractDraftData` is constructed.

- [ ] **Step 2: Run focused WPF tests RED**

- [ ] **Step 3: Correct tags/panel visibility and reject missing dates**

Change `NavAcceptance` to its own target, add a Summary navigation entry, make `AcceptancePanel` conditional on its tag, and replace `DateTime.Today` fallback with required-field failure.

- [ ] **Step 4: Run WPF suite GREEN and commit**

```bash
git add src/OzelEgitimHakedis.Wpf/MainWindow.xaml src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs
git commit -m "Make payment navigation and contract dates safe"
```

### Task 5: Dört bölümlü responsive çalışma alanı

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml`
- Modify: `src/OzelEgitimHakedis.Wpf/App.xaml`
- Modify: `tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs`

**Interfaces:**
- Preserves: named `ProgressContextPanel`, `ProgressRouteSelectionPanel`, `ProgressActionPanel` and all existing event handlers.
- Produces: a dark navy four-zone panel, scrollable narrow navigation/history, and wrapping action controls.

- [ ] **Step 1: Write failing responsive layout contract tests**

Assert sidebar navigation is contained in a vertical `ScrollViewer`, history has horizontal scrolling, route/contractor action groups use `WrapPanel` or equivalent, and the four named zones retain document order.

- [ ] **Step 2: Run focused WPF layout test RED**

- [ ] **Step 3: Implement narrow-window behavior without changing calculations**

Apply source-based visual hierarchy from the approved mockup: contextual amber archive strip, clear route selection, component/result cards, and action bar beneath. Add only layout containers/styles needed for accessibility at 1024×600; keep original control names/bindings.

- [ ] **Step 4: Run WPF suite GREEN and commit**

```bash
git add src/OzelEgitimHakedis.Wpf/MainWindow.xaml src/OzelEgitimHakedis.Wpf/App.xaml tests/OzelEgitimHakedis.Wpf.Tests/HakedisCreateHybridUiContractTests.cs
git commit -m "Make payment workspace responsive and accessible"
```

### Task 6: CI and release verification

**Files:**
- Modify: `.github/workflows/oehs-windows-build.yml` only if the new named layout/profile contracts require it.

- [ ] **Step 1: Run every local static workflow check**

- [ ] **Step 2: Run `dotnet test OzelEgitimHakedisSistemi.sln --configuration Release`**

- [ ] **Step 3: Run `git diff --check` and inspect `git status --short`**

- [ ] **Step 4: Commit a workflow adjustment only when necessary**

