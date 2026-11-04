# Record periods, payment actions and phone entry — continuation

Branch: feature/official-payment-document-templates. PR #3 remains open/draft. No merge or release performed.

## User request and implementation

- History and Archive use contract-clipped ProgressPaymentMonthOption choices, labelled Hakediş Dönemi, instead of arbitrary dates. Filtering still matches the payment's year/month, not archive creation date. Scope follows active program and İKN. Empty selection shows all periods; an incompatible selected month clears on scope change.
- Periods include live contracts, saved payment metadata and archived snapshot contract dates, so deleting a live contract does not hide its history. Reversed legacy dates are skipped while the payment month remains available for filtering/correction. No stored dates are repaired or invented.
- EditCurrentProgressButton, its handler and state dependencies are removed. History's edit handler still calls BeginHistoryOrArchiveCorrection and preserves archive/live correction workflows.
- New payment action reads HAKEDİŞ HESAPLA/KAYDET in XAML and runtime/reset paths; editing reads DÜZENLE. Existing successful calculations persist history, with rollback on save failure. Existing single/batch save regression tests remain green in initial verification.
- Phone layout has a separate fixed 0 and a ten-digit national-number textbox. New input starts 5, accepts ASCII digits, rejects a leading zero and invalid text, and validates nonempty input has ten digits before any contractor save. Optional empty input remains allowed.
- Existing domestic, +90 and 0090 records load prefix-free. Normal/archive save uses one domestic zero. Dirty detection and live/archive comparison normalize both values, avoiding presentation-only phone changes.
- Clipboard validation runs before WPF MaxLength truncation; an overlong paste is rejected as a whole rather than saved as a different number.

## Verification ledger

- Initial RED 911d0ac / run39940540710: four new cases failed as expected, existing162 WPF and265 Core passed.
- Initial implementation d6d8749 / run39940972919: success;265 Core,166 WPF, full builds/static UI checks and24 A4 PDFs passed.
- Independent read-only review identified overlong-paste truncation, raw archive phone comparison and invalid legacy date-range refresh. Added regressions in e09a596 / run39941292211: confirmed truncated5321234567 instead of rejected11-digit paste, missing normalized comparison, and thrown ArgumentOutOfRangeException for both history and archived legacy dates. Actual save test exposed a harness-only Items/ItemsSource setup issue, corrected by matching production selector setup.
- Fix04db2ff adds pre-paste validation, normalized archive comparison, guarded date ranges and correct real-save test setup. Final CI39941658459 completed successfully on code04db2ffd65bc530fe2fa7ea54fa181f16b828c37:265/265 Core,171/171 WPF, full solution/WPF builds, static UI/layout/archive checks and24 A4 PDF smoke checks. WPF passed again during PDF generation. Windows EXE publish and package preparation succeeded; no release was created.
- Final review fixes are covered RED→GREEN by actual clipboard paste, normalized comparison and both malformed history/archive regressions. Actual contractor-save test verifies partial rejection, canonical05321234567 persistence and cleared input. No deferred review findings remain.

## Relevant files

- MainWindow.RecordPeriods.cs: shared scoped period options and legacy fallbacks.
- MainWindow.RecordFilters.cs: filter reads, refresh guards and reset.
- MainWindow.ContractorPhone.cs: input/paste validation, load presentation, canonical comparisons.
- MainWindow.xaml/.cs and MainWindow.Comparison.cs: layout, action cleanup and load/save/correction wiring.
- HakedisCreateHybridUiContractTests.cs: UI, runtime mode, period, malformed/deleted archive, real clipboard and actual contractor persistence tests.
