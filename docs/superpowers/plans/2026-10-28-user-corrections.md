# Nine user corrections implementation plan

> **For agentic workers:** Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Resolve the nine issues in the user's annotated screenshot, retaining its numbering.
**Architecture:** Keep the Excel-verified calculator intact. Correct dashboard projection, fixed-document fields/layout, four-document export naming and selection presentation at their sources.
**Tech Stack:** .NET 8, WPF, PdfSharp, xUnit/PdfPig.
**Spec:** User's 2026-10-28 screenshot and approved numbered list in this conversation.

## Global constraints
- Do not change persisted payment inputs or Excel formulas.
- Price-difference inputs currently mean VAT-exclusive amounts; show the entered transport difference without guide amounts, deducting discounts or adding VAT (user item 7).
- Preserve output separation by program, tender, contractor, route/batch and month; do not expose tax IDs.
- Work in the existing clean feature checkout; user authorized autonomous execution. Do not publish or merge.

## Review focus
- Zero/negative differences and nonzero transport/guide discounts.
- Long contractor/route names, including Turkish letters and a single long token.
- A partial final contractual month and historic earlier payment output.
- Same contractor/tender with different routes, batches and months.
- Collapsed ComboBox rendering, not only its dropdown.

### Task 1: Dashboard and period selection (2, 7, 9)
- [x] Add failing STA dashboard test proving raw entered difference remains independent of discounts and engine totals.
- [x] Add collapsed selection presentation test with a real period option.
- [x] Use input differences for dashboard; supply the ComboBox item template selector to its ContentPresenter.
- [x] Run focused WPF tests.

### Task 2: Official form data and readable layout (1, 3, 4, 5, 6)
- [x] Replace old identifier-in-date tests with date-only assertions; retain IKN in a separate explicit field.
- [x] Add period completion and long name/layout regression tests for both programs.
- [x] Print date only in contract date fields; use acceptance period end for completion; enlarge service and contractor slots without overlapping following rows.
- [x] Verify all four PDFs, text bounds and page size for both programs.

### Task 3: Four-PDF export (8)
- [x] Add safe path tests with long contractor/route/batch names and collision separation.
- [x] Build short deterministic output locations and IKN-contractor filenames; stage in a short temporary directory.
- [x] Wire the four-document action to these paths and retain rollback/evidence behavior.
- [x] Run core/WPF suites, inspect generated PDF renders and review diff.

## Execution notes
- Root cause: dashboard uses CombinedPriceDifferenceExcludingVat (discount-adjusted), rather than the saved inputs.
- Root cause: FormatContractContext combines identifiers with contract date; completion uses ContractEndDate.
- Root cause: fixed 22/25 DIP text slots cannot hold realistic long names; PdfVectorWriter correctly rejects overflow.
- Root cause: custom ComboBox presenter does not propagate the selection template selector used by DisplayMemberPath.
- Export repeats long identity fragments across deeply nested directories and filenames; staging adds another long path segment.
- Review ruling: the user explicitly names the transport field in item 7; the dashboard must exclude guide amounts as well as discounts. Regression: 250 transport / 75 guide / 100 transport discount / 40 guide discount must display 250, not 325 or 185.
- Initial full regression: 278 core + 329 WPF tests passed. Final transport-only projection and PDF metadata integration are verified again below.


## Verification
- Failing reproductions: 9 failures / 10 cases before fixes; the later transport-only case failed with expected 250 vs actual 325 before correction.
- Final Release run: 278 core + 329 WPF = 607 passed, zero failures.
- Both programs: four real A4 vector PDFs generated via bounded export paths; metadata saved alongside the four PDFs.
- Eight sample PDF renders reviewed; final report service name and contractor fit and align without clipping.
- Diff reviewed locally; no subagent review tool is available. No push or merge performed.

- Local self-contained win-x64 package: dist/TEHYS-duzeltmeler-2026-10-28.zip; archive contents verified (application, usage, licenses only).
