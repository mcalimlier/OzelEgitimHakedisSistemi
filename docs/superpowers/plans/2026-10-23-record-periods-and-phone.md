# Record periods and phone entry implementation plan

> **For agentic workers:** Use superpowers:executing-plans inline; one final independent read-only review.

**Goal:** Shared contract periods in History/Archive, unambiguous payment actions and prefix-free mobile input.
**Architecture:** Existing WPF feature branch, shared Core month service, focused phone/period partials.
**Tech Stack:** .NET 8, WPF, xUnit, Windows CI.
**Spec:** docs/superpowers/specs/2026-10-23-record-periods-and-phone-design.md

## Global Constraints

Keep snapshot revisions, historic correction, successful-calculation persistence and rollback. No merge/release. Fixed telephone prefix must not become editable or duplicate in stored data.

## Review Focus

- Program/İKN changes must clear invalid period selections without event recursion.
- Archives must use payment/contract periods, not archive creation dates, including archived contracts no longer live.
- Removing one edit entry point must preserve history and archive corrections.
- Loading or comparing existing phone values must not create spurious changes.
- Typing, paste, deletion and partial-number save must obey the same phone rule.

### Task 1: Red regressions

- [ ] Add real-handler tests for record periods and phone editing/storage; UI/action removal assertions and runtime button label.
- [ ] Commit tests and observe expected Windows failures.

### Task 2: Implement shared period filters and payment actions

- [ ] Replace record DatePickers, refresh scoped options under event guards, preserve compatible month choices and reset behavior.
- [ ] Remove EditCurrentProgressButton and handler/state references; align runtime/new-payment caption.

### Task 3: Phone input and verification

- [ ] Add fixed-prefix layout and input validation; normalize load/save/comparison paths without mutating records on load.
- [ ] Run full Windows suite and PDF smoke; final review; record evidence and continuation note.
