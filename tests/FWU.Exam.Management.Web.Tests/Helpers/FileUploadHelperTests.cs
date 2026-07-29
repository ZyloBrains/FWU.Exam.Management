using FluentAssertions;
using FWU.Exam.Management.Web.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;

namespace FWU.Exam.Management.Web.Tests.Helpers;

public class FileUploadHelperTests
{
    private readonly IWebHostEnvironment _env;
    private readonly FileUploadHelper _sut;
    private readonly string _webRootPath;

    public FileUploadHelperTests()
    {
        _webRootPath = Path.Combine(Path.GetTempPath(), "FWU_Test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_webRootPath);

        _env = Substitute.For<IWebHostEnvironment>();
        _env.WebRootPath.Returns(_webRootPath);
        _sut = new FileUploadHelper(_env);
    }

    [Fact]
    public async Task UploadAsync_WithNullFile_ReturnsNull()
    {
        var result = await _sut.UploadAsync(null);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadAsync_WithEmptyFile_ReturnsNull()
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(0);

        var result = await _sut.UploadAsync(file);
        result.Should().BeNull();
    }

    [Fact]
    public async Task UploadAsync_WithValidImage_ReturnsPath()
    {
        var content = new byte[1024];
        var ms = new MemoryStream(content);
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(1024);
        file.FileName.Returns("photo.jpg");
        file.CopyToAsync(Arg.Any<Stream>()).ReturnsForAnyArgs(callInfo =>
        {
            var target = callInfo.Arg<Stream>();
            return ms.CopyToAsync(target);
        });

        var result = await _sut.UploadAsync(file);

        result.Should().NotBeNull();
        result.Should().StartWith("/images/");
        result.Should().EndWith(".jpg");
    }

    [Fact]
    public async Task UploadAsync_WithDisallowedExtension_Throws()
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(1024);
        file.FileName.Returns("document.exe");

        var act = () => _sut.UploadAsync(file);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*'.exe'*not allowed*");
    }

    [Fact]
    public async Task UploadAsync_WithOversizedFile_Throws()
    {
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(10 * 1024 * 1024);
        file.FileName.Returns("large.pdf");

        var act = () => _sut.UploadAsync(file);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds the maximum*");
    }

    [Fact]
    public async Task UploadAsync_WithCustomSubfolder_UsesSubfolder()
    {
        var content = new byte[512];
        var ms = new MemoryStream(content);
        var file = Substitute.For<IFormFile>();
        file.Length.Returns(512);
        file.FileName.Returns("doc.pdf");
        file.CopyToAsync(Arg.Any<Stream>()).ReturnsForAnyArgs(callInfo =>
        {
            var target = callInfo.Arg<Stream>();
            return ms.CopyToAsync(target);
        });

        var result = await _sut.UploadAsync(file, "documents");

        result.Should().StartWith("/documents/");
        result.Should().EndWith(".pdf");
    }

    public void Dispose()
    {
        try { Directory.Delete(_webRootPath, true); } catch { }
    }
}
