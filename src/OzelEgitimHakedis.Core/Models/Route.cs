namespace OzelEgitimHakedis.Core.Models;

public sealed record Route(
    string RouteNumber,
    string Name,
    Contract Contract,
    decimal DailyPrice);
