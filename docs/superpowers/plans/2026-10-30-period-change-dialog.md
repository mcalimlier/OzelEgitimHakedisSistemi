# Period change dialog implementation

Approved approach: keep one payment row per route; open a dialog to edit multiple service sections. Special education has independent transport and guide sections, each with days, daily price and signed VAT-exclusive price difference. Discounts/debts/taxes remain once per claim. Retain the previously completed Oct 30 changes.

1. Add optional immutable rate sections to payment inputs, validating aggregate days/final price/raw price difference and exact decimal sums; legacy inputs remain supported.
2. Use section sums consistently in calculator, official report, explanation and PDFs. Persist sections in drafts, history and archives; protect historical sections from master-route price propagation.
3. Add a cancel-safe WPF dialog with add/remove sections, totals, validation and single-rate reset. Add per-row actions and protect aggregate day/price/difference cells from direct edits.
4. Restore sections when reopening a period or correction. Suggest final saved section prices in subsequent months without copying work days, differences or section lists.
5. Test exact amounts, independent guide differences, invalid inputs, persistence, next-month behavior, single-row save, correction and four-page PDF output. Run full Release tests and prepare a local runnable package.

No weighted-price approximation, no duplicate payment rows, no automatic historical rewrites, no remote publication.

Verified 2026-10-30: full Release suite passed (281 core + 340 WPF = 621 tests). Both dialog layouts were rendered and visually checked. Coverage includes independent transport/guide price differences, exact section sums, JSON persistence, a single claim with a four-page PDF, Excel detail export, latest-revision daily-rate suggestions and legacy-draft precedence. Draft save timestamps order revisions within the same month; subsequent months start with zero days/differences and only the latest daily prices.

Local runnable package target: `dist/TEHYS-donem-degisikligi-2026-10-30.zip`. No GitHub push or remote release performed for these changes.
