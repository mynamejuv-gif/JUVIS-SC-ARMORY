namespace Juvis.Core;

public sealed record SyncFailure(string Source, string Message);
public sealed record SyncReport(List<string> Updated, List<SyncFailure> Failed, bool Cancelled);

public static class SyncCoordinator
{
    public static IReadOnlyList<string> Sources { get; } = Array.AsReadOnly(new[]
        { "UEX items", "Wiki weapons & ammunition", "Commodities", "Blueprints", "Vehicles", "Wiki components", StarterGuide.Source });

    public static async Task<SyncReport> Run(
        Func<string, IProgress<string>, CancellationToken, Task> sync,
        IProgress<string> progress, CancellationToken ct)
    {
        var updated = new List<string>();
        var failed = new List<SyncFailure>();
        foreach (var source in Sources)
        {
            if (ct.IsCancellationRequested) return new(updated, failed, true);
            var prefix = $"{updated.Count + failed.Count + 1}/{Sources.Count} · {source}";
            progress.Report(prefix + "…");
            try
            {
                await sync(source, new ForwardProgress(s => progress.Report(prefix + " · " + s)), ct);
                updated.Add(source);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            { return new(updated, failed, true); }
            catch (Exception ex) when (ex is not OutOfMemoryException)
            {
                failed.Add(new(source, ex is OperationCanceledException ? "Request timed out." : ex.Message));
            }
        }
        return new(updated, failed, ct.IsCancellationRequested);
    }

    sealed class ForwardProgress(Action<string> report) : IProgress<string>
    { public void Report(string value) => report(value); }
}
