namespace OzelEgitimHakedis.Core.Models;

public sealed record Route(
    string RouteNumber,
    string Name,
    Contract Contract,
    decimal DailyPrice,
    decimal GuideDailyPrice = 0m,
    decimal TransportWorkDays = 0m);
