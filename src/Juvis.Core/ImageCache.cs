using System.Security.Cryptography;
using System.Text;
namespace Juvis.Core;

public sealed class ImageCache(string directory, HttpClient http)
{
    private readonly SemaphoreSlim gate = new(1);
    public const long Limit = 100 * 1024 * 1024;
    public long Bytes => Directory.Exists(directory) ? new DirectoryInfo(directory).EnumerateFiles("*.img").Sum(f => f.Length) : 0;
    // Match API requests: transport startup, reads and disposal all belong on a worker.
    public Task<string?> Get(string url, CancellationToken ct) => Task.Run(() => GetCore(url, ct), ct);
    async Task<string?> GetCore(string url, CancellationToken ct)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https" || uri.IsLoopback) return null;
        await gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url))) + ".img");
            if (File.Exists(path)) { File.SetLastWriteTimeUtc(path, DateTime.UtcNow); return path; }
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            using var response = await http.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, timeout.Token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            if (!(response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") ?? false)) return null;
            var bytes = await ApiClient.ReadBounded(response.Content, 5 * 1024 * 1024, timeout.Token).ConfigureAwait(false);
            await File.WriteAllBytesAsync(path + ".tmp", bytes, ct).ConfigureAwait(false);
            File.Move(path + ".tmp", path, true);
            var files = new DirectoryInfo(directory).EnumerateFiles("*.img").OrderBy(f => f.LastWriteTimeUtc).ToList();
            long size = files.Sum(f => f.Length);
            foreach (var f in files) { if (size <= Limit) break; size -= f.Length; f.Delete(); }
            return path;
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or OperationCanceledException) { return null; }
        finally { gate.Release(); }
    }
    public async Task Clear()
    {
        await gate.WaitAsync();
        try { if (Directory.Exists(directory)) foreach (var f in Directory.EnumerateFiles(directory)) File.Delete(f); }
        finally { gate.Release(); }
    }
}
