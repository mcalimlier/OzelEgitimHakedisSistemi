# Period days and single payment PDF

User scope: apply requests 1 and 3 in both transportation programs. Request 2 (increase/decrease additional calculation rows) remains deferred; no implementation or suggestions in this change.

- Remove the route editor's monthly work-day control. Preserve legacy serialized route days for compatibility, but do not seed, reconcile or overwrite monthly payment inputs from them.
- New period rows start at zero workdays; saved period-specific hybrid drafts and existing payments preserve their own workdays. Route edits may update prices and names without changing payment days.
- Save one four-page PDF, using BuildCombined's order: payment report, service report, acceptance, summary. Name: IKN-contractor-Ödemeye Ait Evraklar.pdf, within existing bounded identity-separated directories.
- Update output labels, metadata and evidence validation. Retain existing four-file evidence for compatibility, and support a single combined file.
- Verify regressions with targeted tests, full Release suite, real combined PDF generation and visual review. Prepare a local runnable package. No automatic merge or remote release.

User follow-up: remove long route/work descriptions from IKN selections; show the transportation program below the IKN in Home and Payment Creation. Canonical visible labels are "Özel Eğitim Taşıma" and "Temel Eğitim/Ortaöğretim Taşıma". Preserve enum identifiers, data folders and user records. Request 2 remains deferred until the current work is complete.

## Verification
- Full Release suite: 278 core + 336 WPF = 614 passing tests; no failures.
- Combined PDFs for both programs contain exactly four A4 pages in the required order; output metadata and current-file evidence support a single combined PDF.
- Real WPF selector render tests show IKN and canonical transport type without the old long work description; route editor render confirms removal of the monthly day field.
- Existing route days are retained only as legacy data; new rows start at zero, period drafts retain their days, route changes do not alter payment days.
- Requests 2 remain explicitly deferred. No split/increase/decrease payment rows implemented.
