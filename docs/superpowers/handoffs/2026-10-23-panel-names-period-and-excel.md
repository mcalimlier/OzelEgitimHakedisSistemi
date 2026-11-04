# Exact captions, Home periods and current-table Excel export

User request: use exact source field names; replace Home's free date with Hakediş Dönemi; remove the İhale Bilgileri work-name input; remove custom Excel paste/undo and export the displayed table as XLSX; remove Archive only if it duplicates History.

## Changes

- Canonical captions now match the source controls: Rehber Personel Günlük Fiyat; Taşıma Günlük Fiyatı; Döneme Esas Taşıma Hakediş Gün Sayısı (Döneme Esas THGS); Rehber İş Günü (bu ay); Ele Geçen.
- Home uses the same ProgressPaymentMonthService and ProgressPaymentMonthOption as payment entry. Program/contract changes rebuild clipped monthly options and clear incompatible choices. No automatic period is selected. All-tender mode combines eligible months.
- WorkNameTextBox, its label, form dependencies and empty layout row are removed. Route-derived official document titles and legacy/snapshot metadata remain necessary and compatible.
- HybridClipboardPlan, custom clipboard handlers, paste/undo controls and obsolete tests are deleted. EXCEL’E AKTAR writes the current visible columns in display order, with current edited row values. Numeric cells retain invariant precision; boolean cells are typed; text remains literal. The file is replaced atomically after package creation.
- Export commits active cell edits into the draft but never saves source routes/monthly drafts, creates payments or resets dirty state. Save-dialog cancellation produces no file.
- Archive is retained because the user's removal condition is false: live payment history and immutable, versioned snapshots have different responsibilities. Snapshots preserve contract, contractor, route, current/previous payments and official document regeneration inputs.

## Review and verification

- RED commit 3680240: Core 265 passed; five new WPF regressions failed as expected (missing exporter, old controls/captions and missing Home periods).
- Independent read-only review found stale paste instructions and SpreadsheetML literal escape handling; both fixed. Active uncommitted cell export coverage added in response to the review. Follow-up review found no remaining important issue.
- Prior intermediate CI passed solution/WPF compilation, static UI/archive checks and all existing regressions; active-cell harness needed a real focused WPF window to exercise BeginEdit.
- Final code commit 07b9b24bd3c1899566c3a1c75520d075de10d9fc: Windows workflow 39938803445 completed successfully. Core 265/265, WPF 162/162; solution/WPF builds, static UI/layout/archive checks and 24 A4 PDF smoke checks passed. WPF suite also passed again during PDF generation.
- Hybrid render evidence generated at 1024×600, 1366×768 and 1920×1080 at 100/125/150%; 1366×768 capture visually checked for wrapped THGS header and reachable save action.
- Artifact uploads include UI renders and PDF review files. Existing workflow publishes EXE transiently but only uploads binaries on eligible packaging runs; no release was created.

Feature branch: feature/official-payment-document-templates. PR #3 remains draft. No merge or release requested/performed.
