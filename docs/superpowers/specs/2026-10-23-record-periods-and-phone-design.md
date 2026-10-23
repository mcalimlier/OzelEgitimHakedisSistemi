# Record periods, payment actions and mobile phone entry

Continue the existing WPF feature branch autonomously. History and Archive replace free dates with the shared ProgressPaymentMonthOption/ProgressPaymentMonthService contract-clipped monthly periods. Scope periods by the active program and İKN; current contracts and historical/snapshot contract dates remain available. Keep no selection for all periods and clear incompatible selections. Preserve unreadable/undated status filtering and archive revisions.

Remove only EditCurrentProgressButton and its event/form-state dependencies. History's edit action already calls the same BeginHistoryOrArchiveCorrection entry point; preserve history/archive correction and cancellation. Use exact HAKEDİŞ HESAPLA/KAYDET text for new payments in XAML and all runtime modes; editing continues to say DÜZENLE. Successful calculations already persist history with rollback on disk failure.

Phone entry shows a separate fixed 0 immediately before a ten-digit textbox. User input is optional, otherwise ASCII digits beginning 5, maximum ten digits; saving a nonempty partial number fails. Invalid typing/paste/leading-zero edits are rejected. Existing domestic/+90/0090 values load without the prefix; persistence, archive correction and comparisons use canonical domestic 0 + national number to prevent duplicate zeros and false dirty comparisons. Existing source records are not migrated merely by opening the form.

Approach: reuse shared month service and existing correction/persistence flows, with a focused WPF partial for phone presentation/validation. A free DatePicker renamed as period would still allow arbitrary dates; altering stored phone models to prefix-free would unnecessarily change documents and compatibility.
