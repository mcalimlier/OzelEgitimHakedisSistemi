namespace OzelEgitimHakedis.Core.Models;

public sealed record Route(
    string RouteNumber,
    string Name,
    Contract Contract,
    decimal DailyPrice,
    decimal GuideWorkDays = 0m,
    decimal GuideDailyPrice = 0m,
    decimal TransportWorkDays = 0m);
