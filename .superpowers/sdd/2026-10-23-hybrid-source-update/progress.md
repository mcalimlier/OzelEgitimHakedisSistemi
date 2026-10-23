# SDD ledger — plan: docs/superpowers/plans/2026-10-23-hybrid-source-update.md

Pre-flight: Tasks 1/2 share RouteData constructors and input guide fields; remove only route annual field, keep payment monthly GuideWorkDays.
Ruling: prior explicit autonomous execution preference applies; no extra development approval requested. Product confirmation remains mandatory.
Ruling: route identity (number/program/IKN/contract/firm) stays fixed; editable name and numeric values synchronize by exact identity. Changing shared contract daily price from one route would alter unrelated routes, so per-route override remains in route record.
Ruling: monthly-only values are persisted as period-specific drafts, not backwritten to old official payments or immutable archives.
Task 1: RED commit fb789cb; CI 39931903346: legacy annual serialization failed and monthly 18 days became 180. Implemented removal in b14135b; CI pending.
Task 2: tests added in fb789cb; initial Core failure stopped job before WPF. Waiting for WPF behavior evidence after Task 1 fix.
Task 1: fixture arities corrected in 90728ac; CI 39932717853 Core 264/264 PASS. WPF only two expected Task 2 failures (missing button; edited daily price 1234.5678 reverted to1000).
Task 2: implemented 6391f22; CI identified nullable-flow compile error from preserving per-route daily price. Fixed nullable contract access in test/evidence commit fb40782.
Final review: independent reviewer found first-month draft restoration omission, unrelated-contract-edit price overwrite, stale source-backed fields and unwrapped directory-write failure. Added behavioral regression tests in fb40782 plus negative discount parity test. One fix pass pending RED evidence.
