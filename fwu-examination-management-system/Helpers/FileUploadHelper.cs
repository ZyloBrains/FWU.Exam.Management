namespace fwu_examination_management_system.Helpers
{
    public interface IFileUploadHelper
    {
        Task<string?> UploadAsync(IFormFile? file, string subfolder = "images");
    }

    public class FileUploadHelper(IWebHostEnvironment environment) : IFileUploadHelper
    {
        private static readonly HashSet<string> AllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg"];

        public async Task<string?> UploadAsync(IFormFile? file, string subfolder = "images")
        {
            if (file == null || file.Length == 0)
                return null;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException($"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}");

            var uploadPath = Path.Combine(environment.WebRootPath, subfolder);

            Directory.CreateDirectory(uploadPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{subfolder}/{fileName}";
        }
    }
}
