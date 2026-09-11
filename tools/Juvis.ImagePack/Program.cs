using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using Juvis.Core;

if (args.Length != 2) throw new ArgumentException("Usage: Juvis.ImagePack <BundledData.zip> <output BundledData directory>");
var destination = Path.GetFullPath(args[1]);
if (Directory.Exists(destination) && Directory.EnumerateFileSystemEntries(destination).Any())
    throw new IOException("Use a new empty output directory; existing image packs are not overwritten.");
using var archive = ZipFile.OpenRead(args[0]);
var allowed = new HashSet<string>([".png", ".jpg", ".jpeg", ".webp"], StringComparer.OrdinalIgnoreCase);
var entries = archive.Entries.Where(e => allowed.Contains(Path.GetExtension(e.FullName))).ToList();
if (entries.Count > 20000 || entries.Sum(e => e.Length) > 2L * 1024 * 1024 * 1024) throw new InvalidDataException("Image pack exceeds limits.");
var index = new SortedDictionary<string, string>();
var hashes = new SortedDictionary<string, string>();
foreach (var entry in entries)
{
    var leaf = entry.FullName.StartsWith("BundledData/Images/", StringComparison.Ordinal) ? entry.FullName["BundledData/Images/".Length..] : "";
    var id = BundledImageIndex.KeyForFile(leaf);
    if (id is null || entry.Length is <= 0 or > 50 * 1024 * 1024)
        throw new InvalidDataException("Unexpected image path or size: " + entry.FullName);
    if (!index.TryAdd(id, leaf)) throw new InvalidDataException("Multiple images for item key: " + id);
}
_ = new BundledImageIndex(index.ToDictionary());
Directory.CreateDirectory(Path.Combine(destination, "Images"));
foreach (var entry in entries)
{
    var id = BundledImageIndex.KeyForFile(entry.FullName["BundledData/Images/".Length..])!;
    var file = Path.Combine(destination, "Images", index[id]);
    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
    await using (var input = entry.Open())
    await using (var output = new FileStream(file, FileMode.CreateNew)) await input.CopyToAsync(output);
    await using var verify = File.OpenRead(file);
    hashes[index[id]] = Convert.ToHexString(await SHA256.HashDataAsync(verify)).ToLowerInvariant();
}
await File.WriteAllTextAsync(Path.Combine(destination, "image-index.json"), JsonSerializer.Serialize(index, LocalStore.Json));
await File.WriteAllTextAsync(Path.Combine(destination, "image-sha256.json"), JsonSerializer.Serialize(hashes, LocalStore.Json));
var report = new { Images = index.Count, Bytes = entries.Sum(e => e.Length), MissingImageRecords = archive.Entries.Count(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)) - index.Count,
    Note = "Original image bytes preserved. Desktop none/checkedAt records are not image assets. Item UUIDs and namespaced UEX commodity IDs resolve without remote URLs." };
await File.WriteAllTextAsync(Path.Combine(destination, "image-pack-report.json"), JsonSerializer.Serialize(report, LocalStore.Json));
Console.WriteLine(JsonSerializer.Serialize(report, LocalStore.Json));
