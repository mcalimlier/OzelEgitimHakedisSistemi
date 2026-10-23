using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class RouteAssignmentValidationServiceTests
{
    [Fact]
    public void SameTenderAndRouteNumber_CannotBeAssignedToTwoContractors()
    {
        var existing = new RouteData(
            "01", "Merkez hattı", 100m, 0m, 180,
            "2026/123456", "11111111111", "FIRMA-11111111111");
        var proposed = new RouteData(
            "01", "Merkez hattı", 100m, 0m, 180,
            "2026/123456", "22222222222", "FIRMA-22222222222");

        var conflict = RouteAssignmentValidationService.FindOwnerConflict(new[] { existing }, proposed);

        Assert.Same(existing, conflict);
    }

    [Fact]
    public void SameRouteNumberUnderDifferentTender_CanBelongToDifferentContractors()
    {
        var existing = new RouteData(
            "01", "Merkez hattı", 100m, 0m, 180,
            "2026/111111", "11111111111", "FIRMA-11111111111");
        var proposed = new RouteData(
            "01", "Merkez hattı", 100m, 0m, 180,
            "2026/222222", "22222222222", "FIRMA-22222222222");

        Assert.Null(RouteAssignmentValidationService.FindOwnerConflict(new[] { existing }, proposed));
    }

    [Fact]
    public void SameTenderAndRouteNumberAcrossDifferentPrograms_IsNotOwnerConflict()
    {
        var existing = new RouteData(
            "01", "Özel Eğitim hattı", 100m, 0m, 180,
            "2026/123456", "11111111111", "FIRMA-11111111111",
            TransportationProgramType.SpecialEducation);
        var proposed = new RouteData(
            "01", "Temel Eğitim hattı", 100m, 0m, 180,
            "2026/123456", "22222222222", "FIRMA-22222222222",
            TransportationProgramType.BasicOrSecondary);

        Assert.Null(RouteAssignmentValidationService.FindOwnerConflict(new[] { existing }, proposed));
    }

    [Fact]
    public void SameTenderAndRouteNumber_ForSameContractor_IsNotOwnerConflict()
    {
        var existing = new RouteData(
            "01", "Merkez hattı", 100m, 0m, 180,
            "2026/123456", "11111111111", "FIRMA-11111111111");
        var proposed = existing with { Name = "Merkez hattı revize" };

        Assert.Null(RouteAssignmentValidationService.FindOwnerConflict(new[] { existing }, proposed));
    }

    [Fact]
    public void TenderAndRouteMatching_IsTrimmedAndCaseInsensitive()
    {
        var existing = new RouteData(
            " Hat-01 ", "Merkez hattı", 100m, 0m, 180,
            " ihale-1 ", "11111111111", "FIRMA-11111111111");
        var proposed = new RouteData(
            "hat-01", "Yeni ad", 100m, 0m, 180,
            "IHALE-1", "22222222222", "FIRMA-22222222222");

        Assert.Same(existing, RouteAssignmentValidationService.FindOwnerConflict(new[] { existing }, proposed));
    }
}
