# OEHS Hakediş Arşiv ve Düzeltme Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Son yüklenen XLSM'yi tek hesaplama otoritesi kabul eden, hakedişleri doğrulayan, revision'lı arşivleyen, arşivden çağırıp düzelten, kaynak kayıt değişikliklerini ilişkili hakedişlere yansıtan ve mevcut 4 belgeli A4 PDF çıktısını güvenli adlandıran OEHS akışını tamamlamak.

**Architecture:** Hesaplama, kimlik/ilişki doğrulama, dönem kontrolü ve arşiv/revision işlemleri Core katmanında küçük, test edilebilir servisler olarak tutulacak. WPF `MainWindow` mevcut ekranları koruyacak; yalnızca yeni `ARŞİVLE`, `DÜZELT` ve `ARŞİVLER` akışını bu servisleri çağıracak şekilde bağlanacak. Mevcut `OehsApplicationDataStore` canlı veri deposu olarak kalacak; Desktop OEHS klasörü arşiv/PDF için kullanılacak.

**Tech Stack:** .NET 8, C# records, WPF, xUnit 2.9.0, System.Text.Json, PDFsharp-WPF ile doğrudan A4 PDF üretimi. PDF çıktısı Windows yazıcı kuyruğuna bağımlı olmayacak.

**Spec:** `docs/superpowers/specs/2026-09-30-hakedis-arsiv-ve-duzeltme-design.md`

## Global Constraints

- Finansal/hesapsal otorite yalnızca son yüklenen `Özel Eğitim Hakediş Ödeme Programı (1)(1).xlsm` dosyasıdır.
- İsimler ilişki anahtarı olarak kullanılmayacaktır.
- Yüklenici anahtarı normalize edilmiş VKN/TCKN'dir.
- Hakediş benzersizliği `İhale/Dosya No + Hat No + Yüklenici VKN/TCKN + Hakediş No + Dönem Tarihi` kombinasyonudur.
- Kaynak XLSM formüllerinde gösterilmeyen ara `ROUND(...,2)` işlemleri eklenmeyecektir.
- Normal bağımsız arşiv kayıtları `v01`, `v02` ... olarak korunur. `ARŞİVLER > DÜZELT` ile seçilen bitmiş iş aynı archive JSON dosyasında ve aynı revision numarasıyla güncellenir; bu düzeltme akışı yeni revision üretmez.
- Desktop arşiv kökü `%USERPROFILE%\\Desktop\\OzelEgitimHakedisSistemi\\Arsiv\\Hakedisler` olacaktır.
- PDF dört belgeyi tek A4 PDF akışında koruyacaktır: Hakediş Raporu, Hizmet İşleri Hakediş Raporu, Hizmet İşleri Kabul Tutanağı, Hakediş İcmali.
- Word çıktısı bu akışa eklenmeyecektir.
- Canlı ana veri deposu `LocalApplicationData\\OEHS\\oehs-data.json` yaklaşımında kalacaktır.
- Her production code değişikliğinden önce ilgili başarısız test yazılacak ve kırmızı sonuç görülecektir.
- Final kabul komutları `dotnet restore`, `dotnet build .\\OzelEgitimHakedisSistemi.sln --configuration Debug`, `dotnet test .\\OzelEgitimHakedisSistemi.sln --configuration Debug --no-build` olacaktır.

## Review Focus

1. XLSM'deki ara hassasiyetin yanlışlıkla 2 ondalıkta yuvarlanması; örnek route-1 zinciri tam Excel değerleriyle pinlenecek (Task 1).
2. Aynı yüklenicinin farklı ihale/dosya numaralarında aynı hat numarasını kullanması; ilişki yalnız isimle karıştırılmayacak (Task 2).
3. VKN/TCKN boşluk, nokta, tire gibi gösterim farkları; duplicate/ilişki kontrolü normalize edilmiş kimliği kullanacak (Task 2).
4. Eski veya bozuk arşiv JSON'unun çağrılması; uygulama çökmeden doğrulama hatası verecek (Task 3).
5. Bir kaynak kaydın değiştirilmesiyle iki hakedişin aynı kimlik/dönem kombinasyonuna dönüşmesi; işlem kaydetmeden önce çakışmayı engelleyecek (Task 4).

---

### Task 1: XLSM Uyumlu Hakediş Hesaplama Motoru

**Files:**
- Modify: `src/OzelEgitimHakedis.Core/Services/ProgressPaymentCalculator.cs`
- Modify: `tests/OzelEgitimHakedis.Tests/ProgressPaymentCalculatorTests.cs`
- Potentially Modify: `src/OzelEgitimHakedis.Core/Models/ProgressPaymentCalculation.cs` only if a result property is needed to represent the XLSM chain without changing existing public semantics.

**Interfaces:**
- Consumes: `ProgressPaymentInput`
- Produces: `ProgressPaymentCalculation` with no artificial intermediate rounding.

- [ ] **Step 1: Write the failing tests.**
  - Create `FinalXlsmRoute1_ShouldMatchExactCalculationChain` and assert the final XLSM route-1 chain values.
  - Create `IntermediatePrecision_ShouldNotRoundAtTwoDecimals` with an input where early 2-decimal rounding changes the final result.
  - Create `VatExempt_ShouldNotCalculateVatOrWithholding`.
  - Create `NegativePriceDifference_ShouldFlowThroughLikeExcel`.

- [ ] **Step 2: Run `IntermediatePrecision_ShouldNotRoundAtTwoDecimals` and verify it fails.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter "FullyQualifiedName~OzelEgitimHakedis.Tests.ProgressPaymentCalculatorTests.IntermediatePrecision_ShouldNotRoundAtTwoDecimals"`
  - Expected: FAIL against the current calculator because an intermediate value is rounded too early.

- [ ] **Step 3: Implement the exact XLSM calculation chain.**
  - Implement:
    - `K=(G*H)+I-J`
    - `L=IF(E="Hayır",K*10/100,0)`
    - `M=L/2`
    - `N=K+M`
    - `O=K+L`
    - `T=(P*Q)+R-S`
    - `U=IF(E="Hayır",T*20/100,0)`
    - `V=IF(E="Hayır",U*9/10,0)`
    - `W=T+V`
    - `X=T+U`
    - `AC=(I*6/100)+(R*6/100)`
    - `AD=(K*9.48/1000)+(T*9.48/1000)`
    - `AE=M+V+Y+Z+AA+AB+AC+AD`
    - `AF=O+X`
    - `AG=AF-AE`
  - Do not add intermediate `Round(value,2)` calls unless the source XLSM formula itself rounds that exact point.
  - Retain current input validation for negative days/prices/debts, while allowing negative price difference when the XLSM allows it.

- [ ] **Step 4: Run the calculator tests and verify they pass.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter FullyQualifiedName~ProgressPaymentCalculatorTests`
  - Expected: all `ProgressPaymentCalculatorTests` PASS.

- [ ] **Step 5: Commit.**
  - Commit message: `fix: align calculator precision with final xlsm`

---

### Task 2: Kimlik, İlişki, Duplicate ve Dönem Doğrulama Servisleri

**Files:**
- Create: `src/OzelEgitimHakedis.Core/Services/OehsIdentityService.cs`
- Create: `src/OzelEgitimHakedis.Core/Services/ProgressPaymentValidationService.cs`
- Modify: `src/OzelEgitimHakedis.Core/Models/OehsApplicationData.cs` only where immutable key fields must be exposed consistently.
- Modify: `tests/OzelEgitimHakedis.Tests/ProgressPaymentCalculatorTests.cs` or create focused `tests/OzelEgitimHakedis.Tests/ProgressPaymentValidationTests.cs`.

**Interfaces:**
- Produces:
  - `public static string NormalizeIdentity(string value)`
  - `public static string BuildRouteKey(string tenderNumber, string routeNumber, string contractorTaxOrIdentityNumber)`
  - `public static string BuildProgressPaymentKey(string tenderNumber, string routeNumber, string contractorTaxOrIdentityNumber, int progressPaymentNumber, DateOnly periodDate)`
  - `public static IReadOnlyList<string> ValidateCreation(OehsApplicationData data, ContractDraftData contract, RouteData route, ProgressPaymentHistoryData candidate, bool editingExisting)`
  - `public static int SuggestNextProgressPaymentNumber(IEnumerable<ProgressPaymentHistoryData> history, string tenderNumber, string routeNumber, string contractorIdentity)`

- [ ] **Step 1: Write failing tests.**
  - `NormalizeIdentity_ShouldCollapseFormattingDifferences`
  - `RouteKey_ShouldIncludeTenderNumberAndNormalizedIdentity`
  - `DuplicateProgressPaymentKey_ShouldRejectNewRecordButAllowCurrentEdit`
  - `SuggestNextProgressPaymentNumber_ShouldUseSameTenderRouteIdentity`
  - `PeriodBeforePrevious_ShouldBeRejected`
  - `PeriodOutsideContract_ShouldBeRejected`

- [ ] **Step 2: Run `NormalizeIdentity_ShouldCollapseFormattingDifferences` and verify it fails.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter "FullyQualifiedName~OzelEgitimHakedis.Tests.ProgressPaymentValidationTests.NormalizeIdentity_ShouldCollapseFormattingDifferences"`
  - Expected: FAIL because the new service does not yet exist.

- [ ] **Step 3: Implement `OehsIdentityService` and `ProgressPaymentValidationService`.**
  - Identity normalization must be deterministic and culture-independent.
  - Validation must compare tender number + route number + normalized identity.
  - Date ordering must use `PeriodDate`, falling back to `CreatedAt` only for legacy entries without period.
  - Validation must return user-facing Turkish messages rather than exposing implementation exceptions.

- [ ] **Step 4: Run the focused validation tests and verify they pass.**
  - Run the same command.
  - Expected: PASS.

- [ ] **Step 5: Commit.**
  - Commit message: `feat: add stable oehs identity and hakedis validation`

---

### Task 3: Self-Contained Revision'lı Arşiv Modeli, Store ve Snapshot Restore

**Files:**
- Create: `src/OzelEgitimHakedis.Core/Models/ProgressPaymentArchive.cs`
- Create: `src/OzelEgitimHakedis.Core/Services/ProgressPaymentArchiveStore.cs`
- Create: `src/OzelEgitimHakedis.Core/Services/ProgressPaymentArchiveRestoreService.cs`
- Create: `tests/OzelEgitimHakedis.Tests/ProgressPaymentArchiveStoreTests.cs`
- Create: `tests/OzelEgitimHakedis.Tests/ProgressPaymentArchiveRestoreServiceTests.cs`
- Modify: `src/OzelEgitimHakedis.Core/Models/OehsApplicationData.cs` only if a shared immutable archive DTO must be referenced.

**Interfaces:**
- Produces:
  - `public sealed record ProgressPaymentArchive(...)`
  - `public sealed class ProgressPaymentArchiveStore`
  - `public IReadOnlyList<FileInfo> List()`
  - `public ProgressPaymentArchive Load(string filePath)`
  - `public string Save(ProgressPaymentArchive archive)`
  - `public static string BuildFileStem(ProgressPaymentArchive archive)`

- [ ] **Step 1: Write failing tests.**
  - `FirstSave_ShouldCreateV01WithoutOverwriting`
  - `SecondSave_ShouldCreateV02AndKeepV01`
  - `Archive_ShouldContainSelfContainedSnapshotFields`
  - `BuildFileStem_ShouldSanitizeWindowsInvalidCharacters`
  - `Load_ShouldRoundTripValidArchive`
  - `Load_ShouldRejectMalformedJsonWithControlledException`
  - `List_ShouldIgnoreUnrelatedFilesAndOrderByRevisionKey`

- [ ] **Step 2: Run `FirstSave_ShouldCreateV01WithoutOverwriting` and verify it fails.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter "FullyQualifiedName~OzelEgitimHakedis.Tests.ProgressPaymentArchiveStoreTests.FirstSave_ShouldCreateV01WithoutOverwriting"`
  - Expected: FAIL because the archive store does not yet exist.

- [ ] **Step 3: Implement the archive model/store.**
  - Root path default: `Environment.SpecialFolder.DesktopDirectory/OzelEgitimHakedisSistemi/Arsiv/Hakedisler`.
  - Use `System.Text.Json` with indented output and explicit schema/revision fields.
  - Use a temp file and atomic move for writes.
  - Determine next revision from existing matching stems; never overwrite an existing revision.
  - Archive object must contain snapshots, not references that require the live data store to resolve.

- [ ] **Step 4: Run the archive tests and verify they pass.**
  - Run the same command.
  - Expected: PASS.

- [ ] **Step 5: Implement snapshot restore.**
  - Restore missing tender, contractor, route and payment records from the archive snapshot using stable keys.
  - Repeated restore must not create duplicates.
  - Restore must select the archived tender as the active contract context.
- [ ] **Step 6: Run archive store and restore tests and verify they pass.**
  - Run the focused archive filters.
  - Expected: PASS.
- [ ] **Step 7: Commit.**
  - Commit message: `feat: add self-contained hakedis archive revisions`

---

### Task 4: Kaynak Kayıt Değişikliklerinin Hakedişlere Yayılması

**Files:**
- Create: `src/OzelEgitimHakedis.Core/Services/ProgressPaymentPropagationService.cs`
- Create: `tests/OzelEgitimHakedis.Tests/ProgressPaymentPropagationTests.cs`
- Modify: `src/OzelEgitimHakedis.Core/Models/OehsApplicationData.cs` only if propagation requires an immutable replacement helper.

**Interfaces:**
- Produces:
  - `public static IReadOnlyList<ProgressPaymentHistoryData> RecalculateForContractChange(IReadOnlyList<ProgressPaymentHistoryData> history, IReadOnlyList<RouteData> routes, ContractDraftData oldContract, ContractDraftData newContract)`
  - `public static IReadOnlyList<ProgressPaymentHistoryData> RecalculateForContractorChange(IReadOnlyList<ProgressPaymentHistoryData> history, Contractor oldContractor, Contractor newContractor)`
  - `public static IReadOnlyList<ProgressPaymentHistoryData> RecalculateForRouteChange(IReadOnlyList<ProgressPaymentHistoryData> history, RouteData oldRoute, RouteData newRoute, Contractor contractor)`
  - Each method preserves user-entered input values and rebuilds source-derived snapshot fields/calculations.
  - No method mutates a history record in place.

- [ ] **Step 1: Write failing tests.**
  - `RouteDailyPriceChange_ShouldRecalculateWithoutChangingUserInputs`
  - `ContractorRename_ShouldKeepStableIdentityAndUpdateDisplayName`
  - `VatExemptionChange_ShouldRecalculateVatAndWithholding`
  - `ContractMetadataChange_ShouldRefreshOutputIdentity`
  - `SourceChangeCreatingDuplicateKey_ShouldBeRejected`

- [ ] **Step 2: Run `RouteDailyPriceChange_ShouldRecalculateWithoutChangingUserInputs` and verify it fails.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter "FullyQualifiedName~OzelEgitimHakedis.Tests.ProgressPaymentPropagationTests.RouteDailyPriceChange_ShouldRecalculateWithoutChangingUserInputs"`
  - Expected: FAIL because the propagation service does not yet exist.

- [ ] **Step 3: Implement propagation.**
  - Identify affected histories only by stable relationship keys.
  - Preserve all user input fields in `ProgressPaymentInput`.
  - Build a new input from current tender/contractor/route source values, recalculate through Task 1's calculator, and replace only derived data.
  - Re-validate duplicate keys using Task 2 before returning a new collection.

- [ ] **Step 4: Run the propagation tests and verify they pass.**
  - Run the same command.
  - Expected: PASS.

- [ ] **Step 5: Commit.**
  - Commit message: `feat: propagate source record changes to hakedis history`

---

### Task 5: WPF Hakediş Giriş Akışı, ARŞİVLE ve DÜZELT

**Files:**
- Create: `src/OzelEgitimHakedis.Core/Services/ProgressCorrectionTargetService.cs`
- Create: `tests/OzelEgitimHakedis.Tests/ProgressCorrectionTargetServiceTests.cs`
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml`
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs`
- Modify: `src/OzelEgitimHakedis.Wpf/App.xaml` only if shared button/style resources are needed.

**Interfaces:**
- Core produces:
  - `public enum ProgressCorrectionTarget { Tender, Contractor, Route, ProgressPayment, Cancel }`
  - `public sealed record ProgressCorrectionTargetSelection(ProgressCorrectionTarget Target, string? TenderNumber, string? ContractorTaxOrIdentityNumber, string? RouteNumber, Guid? ProgressPaymentId)`
  - `public static ProgressCorrectionTargetSelection Select(ProgressCorrectionTarget target, ProgressPaymentHistoryData item)`
- WPF handlers/flows:
  - `ArchiveCurrentProgressButton_Click`
  - `CorrectCurrentProgressButton_Click`
  - `EditCorrectionTargetButton_Click`
  - `LoadArchiveButton_Click`
  - Existing handlers `CalculateProgressButton_Click`, `EditHistoryButton_Click`, `OpenHistoryFolderButton_Click` will call Core services instead of duplicating key/validation logic.

- [ ] **Step 1: Write failing Core integration-style tests for UI-dependent business decisions.**
  - Add tests for action-target mapping:
    - İHALE BİLGİLERİ -> selected tender
    - YÜKLENİCİ FİRMA -> selected contractor by normalized VKN/TCKN
    - HAT BİLGİLERİ -> selected tender + route + contractor
    - HAKEDİŞ GİRDİLERİ -> exact hakedis identity
    - VAZGEÇ -> no data mutation
  - Add a test proving next suggested hakedis number is shown as max+1.

- [ ] **Step 2: Run `SelectRoute_ShouldReturnTenderRouteAndContractorIdentity` and verify it fails.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter "FullyQualifiedName~OzelEgitimHakedis.Tests.ProgressCorrectionTargetServiceTests.SelectRoute_ShouldReturnTenderRouteAndContractorIdentity"`
  - Expected: FAIL because `ProgressCorrectionTargetService` does not yet exist.

- [ ] **Step 3: Implement the WPF flow.**
  - On selected tender, filter routes by tender and contractor identity; never match by contractor name alone.
  - Auto-suggest next hakedis number on route selection.
  - Validate before calculation using Task 2.
  - Preserve existing readable/widened button layout.
  - After successful calculation show `ARŞİVLE` and `DÜZELT`.
  - `DÜZELT` opens exactly the five choices from the spec.
  - Choosing a source record navigates and preloads the related record.
  - Choosing `HAKEDİŞ GİRDİLERİ` loads the exact history entry for editing.
  - `ARŞİVLE` builds a self-contained archive snapshot and saves a new revision.
  - Archive load fills the existing progress editor with all user inputs and source snapshot fields, while re-resolving the live relationship key before persistence.
  - Existing history `KLASÖR` action remains and points to the existing document folder.

- [ ] **Step 4: Run the solution build.**
  - Run: `dotnet build .\\OzelEgitimHakedisSistemi.sln --configuration Debug`
  - Expected: BUILD succeeded.

- [ ] **Step 5: Commit.**
  - Commit message: `feat: add hakedis archive and correction workflow`

---

### Task 6: ARŞİVLER Paneli, Revision Çağırma ve Düzeltme

**Files:**
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml`
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs`
- Create: `src/OzelEgitimHakedis.Core/Services/ArchiveDisplayService.cs` only if WPF list mapping becomes non-trivial.
- Create: `tests/OzelEgitimHakedis.Tests/ArchiveDisplayServiceTests.cs` if that service is created.

**Interfaces:**
- Produces WPF archive row data with:
  - Ihale/Dosya No
  - Hat No / Hat Adı
  - Hakediş No
  - Dönem
  - Revision
  - Yüklenici
  - Net Ödeme
  - action command target.

- [ ] **Step 1: Write failing tests for archive list mapping.**
  - `ArchiveList_ShouldShowV01AndV02AsSeparateRows`
  - `LatestRevision_ShouldBeIdentifiedWithoutDeletingOlderRevisions`
  - `LoadRevision_ShouldRestoreExactUserInputs`
  - `MalformedArchive_ShouldBecomeControlledInvalidEntry`

- [ ] **Step 2: Run `ArchiveList_ShouldShowV01AndV02AsSeparateRows` and verify it fails.**
  - Run the archive-list test command after adding the test method.
  - Expected: FAIL until the list mapping exists.

- [ ] **Step 3: Implement the `ARŞİVLER` panel.**
  - Add a left-navigation `ARŞİVLER` item and panel.
  - Provide `ÇAĞIR`, `DÜZELT`, `KLASÖRÜ AÇ`.
  - Keep list order by payment period and revision, not file creation timestamp.
  - ÇAĞIR normal yüklemedir ve canlı veri akışında ayrı çalışır. DÜZELT ise seçilen archive snapshot'ı üzerinde özel düzenleme modudur.
  - Normal ilk arşivleme v01 oluşturur. ARŞİVLER > DÜZELT ile seçilen bitmiş iş düzenlendiğinde aynı arşiv JSON dosyası ve aynı revision numarası doğrudan güncellenir; yeni revision oluşturulmaz.

- [ ] **Step 4: Run tests and build.**
  - Run full solution build and tests.
  - Expected: BUILD succeeded; full suite PASS.

- [ ] **Step 5: Commit.**
  - Commit message: `feat: add hakedis archives panel and revision loading`

---

### Task 7: Excel-Compatible Cumulative Reports, PDF Filename and General Regression Cleanup

**Files:**
- Create: `src/OzelEgitimHakedis.Core/Services/OehsFileNameService.cs`
- Modify: `src/OzelEgitimHakedis.Wpf/MainWindow.xaml.cs`
- Modify: `src/OzelEgitimHakedis.Wpf/ExcelDocumentExporter.cs` only where output identity/value sourcing must be corrected.
- Modify: `src/OzelEgitimHakedis.Core/Services/OehsApplicationDataStore.cs` only if archive/live-save atomic behavior integration requires it.
- Modify: `tests/OzelEgitimHakedis.Tests/ProgressPaymentCalculatorTests.cs`
- Create: `tests/OzelEgitimHakedis.Tests/ProgressPaymentReportTests.cs`
- Create: `tests/OzelEgitimHakedis.Tests/PdfFileNameTests.cs`
- Create: `docs/OEHS_30_09_2026_Genel_Kontrol.md`

**Interfaces:**
- Produces:
  - `public static string BuildProgressPdfFileName(string tenderNumber, string routeNumber, int progressPaymentNumber, DateOnly periodDate)` in `OehsFileNameService`.
  - report builder behavior using period-sorted histories.
  - final general-control document recording commands/results and any remaining minor findings.

- [ ] **Step 1: Write failing tests.**
  - `BuildProgressPdfFileName_ShouldUseExactDescriptivePattern`
  - `BuildProgressPdfFileName_ShouldSanitizeInvalidCharacters`
  - `ReportHistory_ShouldOrderByPeriodDateNotCreatedAt`
  - `CurrentPeriodPriceDifference_ShouldRemainInCurrentReport`
  - `FinalXlsmRoute1Report_ShouldMatchWorkbookReference`
  - `OfficialDocumentNames_ShouldContainTheFourRequiredDocuments`

- [ ] **Step 2: Run `BuildProgressPdfFileName_ShouldUseExactDescriptivePattern` and verify it fails.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter "FullyQualifiedName~OzelEgitimHakedis.Tests.PdfFileNameTests.BuildProgressPdfFileName_ShouldUseExactDescriptivePattern"`
  - Expected: FAIL until `OehsFileNameService` exists.

- [ ] **Step 3: Implement report/PDF corrections.**
  - Keep the four-document A4 content layout while saving through the OEHS direct PDF exporter.
  - Replace old GUID-heavy filename use with `BuildProgressPdfFileName`.
  - Ensure every document receives the same tender, route, contractor, contract and hakedis identity source.
  - Ensure cumulative report calculations consume period-sorted history and do not accidentally use row order or edit timestamp.
  - Preserve the current PDF output behavior already working in the application.

- [ ] **Step 4: Run the report/PDF tests and verify they pass.**
  - Run: `dotnet test tests/OzelEgitimHakedis.Tests/OzelEgitimHakedis.Tests.csproj --configuration Debug --filter "FullyQualifiedName~OzelEgitimHakedis.Tests.ProgressPaymentReportTests|FullyQualifiedName~OzelEgitimHakedis.Tests.PdfFileNameTests"`
  - Expected: all selected report/PDF tests PASS.

- [ ] **Step 5: Run `dotnet restore`.**
  - Expected: restore succeeds.
- [ ] **Step 6: Run the full solution build.**
  - Run: `dotnet build .\\OzelEgitimHakedisSistemi.sln --configuration Debug`
  - Expected: BUILD succeeded.
- [ ] **Step 7: Run the full solution test suite.**
  - Run: `dotnet test .\\OzelEgitimHakedisSistemi.sln --configuration Debug --no-build`
  - Expected: all tests PASS.

- [ ] **Step 8: Perform manual general control.**
  - Open the WPF app and verify:
    - navigation and button text do not clip,
    - tender -> route -> contractor selection is correct,
    - suggested hakedis number is correct,
    - pre-calculation validation blocks invalid input,
    - result contains ARŞİVLE and DÜZELT,
    - DÜZELT presents the five requested choices,
    - archive creates v01 and later v02 without deleting v01,
    - ARŞİVLER lists/loads both versions,
    - source record edits recalculate affected histories,
    - duplicate key collision is prevented,
    - PDF is one A4 4-document output with the exact descriptive filename.
  - Compare a controlled route-1 scenario directly with the final XLSM.
  - Verify existing DogruTemin behavior is not touched/broken by this feature work.

- [ ] **Step 9: Write the general-control report.**
  - Record:
    - tested commands,
    - pass/fail results,
    - controlled XLSM comparisons,
    - manual UI observations,
    - PDF/archive observations,
    - fixes made during control,
    - remaining low-risk improvement suggestions.

- [ ] **Step 10: Commit.**
  - Commit message: `test: complete oehs final regression and general control`

---

## Execution Order

Tasks 1 → 2 → 3 → 4 → 5 → 6 → 7.

Task 1 establishes the financial contract. Task 2 establishes stable identity and validation. Task 3 supplies immutable archive revisions. Task 4 supplies recalculation propagation. Tasks 5 and 6 wire the WPF user experience to those Core contracts. Task 7 validates reports/PDF and performs the final whole-system regression.

## Out-of-Scope Checks

Do not add:
- a new database engine,
- runtime Excel/VBA dependency,
- a large MVVM rewrite,
- unrelated module refactors,
- Word export,
- destructive archive overwrite.

## Acceptance Criteria

The branch is accepted only when:
1. All final XLSM calculation regression tests pass.
2. Duplicate and period validation tests pass.
3. Archive v01/v02 round-trip tests pass.
4. Source-change propagation tests pass.
5. Full solution build succeeds.
6. Full solution test suite passes.
7. Manual UI general-control checks pass.
8. A controlled PDF output is verified as four A4 documents under the descriptive filename pattern.
9. A written general-control record exists in `docs/OEHS_30_09_2026_Genel_Kontrol.md`.
10. ARŞİVLER > DÜZELT seçilen archive snapshot'ı aynı dosya ve aynı revision numarasıyla günceller; normal v01/v02 gibi yeni revision üretimi yalnızca bağımsız tekrar arşivleme senaryosunda kullanılır.


## USER-REDEFINED ACCEPTANCE RULE — ARCHIVE CORRECTION

The user explicitly requires that a completed archived job be corrected in place. Therefore:
- DÜZELT -> HAT BİLGİLERİ permits editing Hat No, Hat Adı, iş günleri and rehber günlük fiyat.
- DÜZELT -> HAKEDİŞ GİRDİLERİ recalculates the archived calculation and writes back to the same selected archive file.
- DÜZELT -> İHALE BİLGİLERİ and YÜKLENİCİ FİRMA update the selected archive snapshot in place.
- The correction flow must never create a new active route as a side effect.
- The correction flow must never auto-create v02/v03 merely because a selected archive was corrected.


## FINAL USER DECISION — 01.10.2026

The archive correction workflow is intentionally destructive to the selected snapshot content but not to the revision number. This is the required business behavior:

- ARŞİVLER > DÜZELT edits the selected completed job itself.
- HAT BİLGİLERİ includes editable Hat No.
- HAKEDİŞ GİRDİLERİ recalculates and writes back to the same archive file.
- İHALE BİLGİLERİ and YÜKLENİCİ FİRMA update the same archive snapshot.
- No active RouteDraft is created as a side effect.
- No v02/v03 is created merely because the selected archive was corrected.
