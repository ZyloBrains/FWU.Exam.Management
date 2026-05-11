namespace FWU.Exam.Management.Web.Helpers;

public interface IFileUploadHelper
{
    Task<string?> UploadImageAsync(IFormFile? file, string subfolder = "images");
    Task<string?> UploadDocumentAsync(IFormFile? file, string subfolder = "documents");
}

public class FileUploadHelper(IWebHostEnvironment environment) : IFileUploadHelper
{
    private static readonly HashSet<string> ImageExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];
    private static readonly HashSet<string> DocumentExtensions = [".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png"];

    public async Task<string?> UploadImageAsync(IFormFile? file, string subfolder = "images")
    {
        return await UploadAsync(file, subfolder, ImageExtensions);
    }

    public async Task<string?> UploadDocumentAsync(IFormFile? file, string subfolder = "documents")
    {
        return await UploadAsync(file, subfolder, DocumentExtensions);
    }

    private async Task<string?> UploadAsync(IFormFile? file, string subfolder, HashSet<string> allowedExtensions)
    {
        if (file == null || file.Length == 0)
            return null;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");

        var uploadPath = Path.Combine(environment.WebRootPath, subfolder);

        Directory.CreateDirectory(uploadPath);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/{subfolder}/{fileName}";
    }
}
