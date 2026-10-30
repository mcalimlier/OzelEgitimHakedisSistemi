using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentArchiveBatchServiceTests
{
    private static readonly string[] AllArchiveIds = new[] { "A", "B", "C" };
    private static readonly string[] ExpectedDeletedFiles = new[] { "B.json", "A.json" };
    private static readonly string[] SelectedArchiveIds = new[] { "A", "B" };
    private static readonly string[] OrderedArchiveFiles = new[] { "A.json", "B.json" };
    private static readonly string[] ExpectedDeleteErrors = new[] { "A.json: delete failed" };
    private static readonly string[] ExpectedRollbackErrors = new[] { "A.json: unexpected rollback failure" };
    private static readonly string[] SingleArchiveFile = new[] { "A.json" };

    [Fact]
    public void Execute_ShouldRollbackEarlierFiles_WhenLaterArchiveWriteFails()
    {
        var written = new List<string>();
        var rolledBack = new List<string>();

        var result = ProgressPaymentArchiveBatchService.Execute(
            AllArchiveIds,
            item =>
            {
                if (item == "C")
                    return (false, (string?)null, (string?)"simulated disk failure");

                var path = item + ".json";
                written.Add(path);
                return (true, (string?)path, (string?)null);
            },
            path => rolledBack.Add(path));

        Assert.False(result.Success);
        Assert.Equal("simulated disk failure", result.Error);
        Assert.Equal(ExpectedDeletedFiles, rolledBack);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.RollbackFailures);
    }

    [Fact]
    public void Execute_ShouldReturnEveryPath_WhenAllWritesSucceed()
    {
        var rolledBack = new List<string>();

        var result = ProgressPaymentArchiveBatchService.Execute(
            SelectedArchiveIds,
            item => (true, item + ".json", (string?)null),
            path => rolledBack.Add(path));

        Assert.True(result.Success);
        Assert.Equal(OrderedArchiveFiles, result.WrittenPaths);
        Assert.Empty(result.RollbackFailures);
        Assert.Empty(rolledBack);
    }

    [Fact]
    public void Execute_ShouldReportRollbackFailures_WithoutHidingOriginalWriteFailure()
    {
        var result = ProgressPaymentArchiveBatchService.Execute(
            SelectedArchiveIds,
            item => item == "B"
                ? (false, (string?)null, (string?)"write failed")
                : (true, (string?)"A.json", (string?)null),
            _ => throw new IOException("delete failed"));

        Assert.False(result.Success);
        Assert.Equal("write failed", result.Error);
        Assert.Equal(ExpectedDeleteErrors, result.RollbackFailures);
    }

    [Fact]
    public void Execute_ShouldRollback_WhenWriterThrowsUnexpectedException()
    {
        var rolledBack = new List<string>();

        var result = ProgressPaymentArchiveBatchService.Execute(
            SelectedArchiveIds,
            item => item == "B"
                ? throw new FormatException("unexpected writer failure")
                : (true, (string?)"A.json", (string?)null),
            path => rolledBack.Add(path));

        Assert.False(result.Success);
        Assert.Equal("unexpected writer failure", result.Error);
        Assert.Equal(SingleArchiveFile, rolledBack);
        Assert.Empty(result.WrittenPaths);
    }

    [Fact]
    public void Execute_ShouldReportUnexpectedRollbackException()
    {
        var result = ProgressPaymentArchiveBatchService.Execute(
            SelectedArchiveIds,
            item => item == "B"
                ? (false, (string?)null, (string?)"write failed")
                : (true, (string?)"A.json", (string?)null),
            _ => throw new FormatException("unexpected rollback failure"));

        Assert.False(result.Success);
        Assert.Equal("write failed", result.Error);
        Assert.Equal(ExpectedRollbackErrors, result.RollbackFailures);
    }

    [Fact]
    public void Execute_ShouldRejectEmptyBatch()
    {
        var result = ProgressPaymentArchiveBatchService.Execute(
            Array.Empty<string>(),
            item => (true, (string?)item, (string?)null),
            _ => { });

        Assert.False(result.Success);
        Assert.Empty(result.WrittenPaths);
        Assert.Contains("bulunamadı", result.Error, StringComparison.Ordinal);
    }
}
