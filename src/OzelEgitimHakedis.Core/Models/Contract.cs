namespace OzelEgitimHakedis.Core.Models;

public sealed record Contract(
    Contractor Contractor,
    decimal ContractAmount,
    DateOnly TenderDate,
    DateOnly ContractDate,
    DateOnly StartDate,
    DateOnly EndDate);
