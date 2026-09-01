namespace FWU.Exam.Management.Web.Helpers;

public interface IFileUploadHelper
{
    Task<string?> UploadAsync(IFormFile? file, string subfolder = "images", long? maxFileSizeBytes = null, IEnumerable<string>? allowedExtensions = null);
}

public class FileUploadHelper(IWebHostEnvironment environment) : IFileUploadHelper
{
    public static readonly HashSet<string> DefaultAllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".pdf"];
    public static readonly HashSet<string> ImageOnlyExtensions = [".jpg", ".jpeg", ".png"];
    public static readonly HashSet<string> DocumentAllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".gif", ".webp", ".doc", ".docx"];

    public const long MaxFileSizeBytes = 5 * 1024 * 1024;
    public const long MaxPhotoSizeBytes = 500 * 1024;
    public const long MaxSignatureSizeBytes = 100 * 1024;
    public const long MaxDocumentSizeBytes = 2 * 1024 * 1024;

    public async Task<string?> UploadAsync(IFormFile? file, string subfolder = "images", long? maxFileSizeBytes = null, IEnumerable<string>? allowedExtensions = null)
    {
        if (file == null || file.Length == 0)
            return null;

        var limit = maxFileSizeBytes ?? MaxFileSizeBytes;
        var extensions = allowedExtensions ?? DefaultAllowedExtensions;

        if (file.Length > limit)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {FormatSize(limit)}.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!extensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", extensions)}");

        var uploadPath = Path.Combine(environment.WebRootPath, subfolder);

        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{subfolder}/{fileName}";
    }

    private static string FormatSize(long bytes) => bytes % (1024 * 1024) == 0
        ? $"{bytes / (1024 * 1024)} MB"
        : $"{bytes / 1024} KB";
}
