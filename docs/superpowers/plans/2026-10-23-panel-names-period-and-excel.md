# Panel names, period and Excel implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Execute inline with one final independent review.

**Goal:** Match source field names, use contract month options on Home, remove the contract work-name input and paste feature, export the current grid as XLSX.
**Architecture:** Existing WPF controls and shared month service; extend the existing OpenXML writer with typed table cells. Keep immutable archive storage because it differs from live history.
**Tech Stack:** C# .NET 8 / WPF / System.IO.Compression / XML / xUnit / Windows CI.
**Spec:** docs/superpowers/specs/2026-10-23-panel-names-period-and-excel-design.md

## Global Constraints

- Preserve exact source captions, program-specific calculations and official documents.
- No source record mutations on export. No merge/release.
- Remove only the contract-entry WorkName UI; retain route-derived official titles and compatibility metadata.
- Archive condition evaluated false: preserve snapshot/revision functionality.

## Review Focus

- Invalid selections must clear when program/IKN changes.
- Export must contain edited values and exact visible/reordered columns, not old source defaults.
- Export must not mark edits as saved or create a payment.
- Numeric XLSX cells retain precision; text such as =1+1 stays literal.
- Removed controls and custom paste hooks must not leave dead source references.

### Task 1: Captions, Home period, work-name input cleanup

Files: MainWindow.xaml/.cs; MainWindow.Operations.cs/HomeFinance.cs; WPF UI contract and Home tests.
- [x] Add contract-caption/period/input-removal tests and observe failures on Windows.
- [x] Replace Home DatePicker with shared month-option ComboBox; update filter/transfer paths and existing tests.
- [x] Correct captions; remove WorkName control and form references; keep official output titles.
- [x] Verify period scoping, explicit selection and report regressions.

### Task 2: Remove paste and export current grid

Files: delete HybridClipboardPlan.cs/MainWindow.HybridClipboard.cs and obsolete tests; extend ExcelDocumentExporter.cs; create MainWindow.HybridExcelExport.cs; add XLSX package and grid integration tests.
- [x] Add export/paste-removal regressions and see failures.
- [x] Remove paste/undo controls, custom key hooks and code/test remnants.
- [x] Add EXCEL’E AKTAR SaveFileDialog and typed visible-grid XLSX snapshot writer.
- [x] Verify generated package content, precision, column ordering and zero persistence side effects.
- [x] Run full Windows suite/PDF smoke, independent review, fix important findings and record continuation evidence.
