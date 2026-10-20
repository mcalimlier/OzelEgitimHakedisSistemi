using System.Globalization;

namespace OzelEgitimHakedis.Core.Services;

public sealed record ProgressPaymentMonthOption(DateOnly StartDate, DateOnly EndDate)
{
    public DateOnly PeriodDate => EndDate;

    public string DisplayText
    {
        get
        {
            var tr = CultureInfo.GetCultureInfo("tr-TR");
            var monthYear = StartDate.ToString("MMMM yyyy", tr);
            return $"{tr.TextInfo.ToTitleCase(monthYear)} ({StartDate:dd.MM.yyyy} – {EndDate:dd.MM.yyyy})";
        }
    }
}

/// <summary>Builds month-only payment periods clipped to the contract dates.</summary>
public static class ProgressPaymentMonthService
{
    public static IReadOnlyList<ProgressPaymentMonthOption> BuildOptions(DateOnly contractStartDate, DateOnly contractEndDate)
    {
        if (contractEndDate < contractStartDate)
            throw new ArgumentOutOfRangeException(nameof(contractEndDate), "Sözleşme bitişi başlangıç tarihinden önce olamaz.");

        var result = new List<ProgressPaymentMonthOption>();
        var month = new DateOnly(contractStartDate.Year, contractStartDate.Month, 1);
        var lastMonth = new DateOnly(contractEndDate.Year, contractEndDate.Month, 1);

        while (month <= lastMonth)
        {
            var monthEnd = month.AddMonths(1).AddDays(-1);
            var start = contractStartDate > month ? contractStartDate : month;
            var end = contractEndDate < monthEnd ? contractEndDate : monthEnd;
            result.Add(new ProgressPaymentMonthOption(start, end));
            month = month.AddMonths(1);
        }

        return result;
    }

    public static ProgressPaymentMonthOption? FindForPeriod(IEnumerable<ProgressPaymentMonthOption> options, DateOnly periodDate)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.FirstOrDefault(x => x.EndDate.Year == periodDate.Year && x.EndDate.Month == periodDate.Month);
    }
}
