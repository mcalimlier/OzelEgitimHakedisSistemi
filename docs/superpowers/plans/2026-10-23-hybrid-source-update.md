# Hybrid source update implementation plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans. Steps use checkbox syntax.

**Goal:** Remove annual guide days and persist confirmed editable route/payment drafts.
**Architecture:** Remove the route-only annual field. Preserve period guide days. Track row baselines, persist route-backed fields and period-keyed drafts atomically through existing program store, refresh selector records without replacing user drafts.
**Tech Stack:** C# .NET 8, WPF, System.Text.Json, xUnit, Windows CI.
**Spec:** docs/superpowers/specs/2026-10-23-hybrid-source-update-design.md

## Global Constraints

- No merge/release. Use current feature branch. No personal test data.
- Preserve separate program rules, monthly guide days and immutable official history.
- Confirmation is in the product, not an extra development approval.

## Review Focus

- Cancel and write failure retain edits and keep button enabled.
- Changing route selection never overwrites edited daily prices.
- Same route numbers in different programs/firms never mix drafts.
- Removed annual guide days never overwrite recorded monthly guide days.
- Monthly drafts survive restart and remain scoped to their period.

### Task 1: Remove annual guide day field

Files: route models, propagation service, MainWindow.xaml/.cs, affected constructor fixtures.
- [x] Write legacy JSON serialization and monthly-guide-preservation regression tests.
- [x] Run Windows CI and verify failures before implementing.
- [x] Remove route annual field/control/read/write/validation/summary paths; preserve monthly payment input. Adjust positional constructor callers.
- [x] Run Core/WPF build and tests; no annual field in active source.

### Task 2: Confirmed grid source update

Files: MainWindow.HybridRow.cs, MainWindow.HybridEntry.cs, new MainWindow.HybridSourceUpdate.cs; OehsApplicationData and program-scoped store; WPF interaction tests.
- [x] Add edit/cancel/confirm/persistence/reselection/rollback behavior tests and see expected failures.
- [x] Track row baseline and expose HasChanges. Editable price/name columns; button directly below grid.
- [x] Persist changed selected route values and period draft inputs in one save. Cancel/error preserve draft and original data. Restore saved monthly drafts by exact identity/period.
- [x] Refresh all live source views with retained row edits; read route override when editing source route.
- [x] Verify full Windows suite and PDF smoke; review change; save continuation evidence and commit.

## Completion evidence

Windows CI 39935756432 for 5567b5380f0e179f52d3fe8edd3b63539afa8e31: Core 265/265, WPF 167/167, 24 A4 PDF smoke PASS. Independent review findings were fixed and covered by passing regressions. Branch/PR preserved; no merge/release. Full evidence: `docs/superpowers/handoffs/2026-10-23-rehber-yillik-kaldirma-onayli-tablo-kaydi.md`.
