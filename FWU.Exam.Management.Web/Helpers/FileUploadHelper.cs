namespace FWU.Exam.Management.Web.Helpers;

public interface IFileUploadHelper
{
    Task<string?> UploadAsync(IFormFile? file, string subfolder = "images");
}

public class FileUploadHelper(IWebHostEnvironment environment) : IFileUploadHelper
{
    private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly Dictionary<string, byte[][]> FileSignatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        [".jpeg"] = [new byte[] { 0xFF, 0xD8, 0xFF }],
        [".png"] = [new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }],
        [".gif"] = [new byte[] { 0x47, 0x49, 0x46, 0x38 }],
        [".webp"] = [new byte[] { 0x52, 0x49, 0x46, 0x46 }],
        [".pdf"] = [new byte[] { 0x25, 0x50, 0x44, 0x46 }],
    };

    public async Task<string?> UploadAsync(IFormFile? file, string subfolder = "images")
    {
        if (file == null || file.Length == 0)
            return null;

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

        await using var stream = file.OpenReadStream();

        var headerBytes = new byte[8];
        await stream.ReadExactlyAsync(headerBytes, 0, Math.Min(headerBytes.Length, (int)stream.Length));
        stream.Position = 0;

        if (FileSignatures.TryGetValue(extension, out var signatures))
        {
            var match = signatures.Any(sig => headerBytes.Take(sig.Length).SequenceEqual(sig));
            if (!match)
                throw new InvalidOperationException($"The file content does not match the expected format for '{extension}' files.");
        }

        var uploadPath = Path.Combine(environment.WebRootPath, subfolder);
        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using var fs = new FileStream(filePath, FileMode.Create);
        await stream.CopyToAsync(fs);

        return $"/{subfolder}/{fileName}";
    }
}
