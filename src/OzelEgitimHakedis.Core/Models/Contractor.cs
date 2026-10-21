namespace OzelEgitimHakedis.Core.Models;

public sealed record Contractor(
    string Name,
    string TaxOrIdentityNumber,
    string Address = "",
    string Phone = "",
    string Email = "",
    string BankAccountNumber = "",
    string ContactPerson = "",
    TransportationProgramType ProgramType = TransportationProgramType.SpecialEducation,
    bool VatExempt = false);