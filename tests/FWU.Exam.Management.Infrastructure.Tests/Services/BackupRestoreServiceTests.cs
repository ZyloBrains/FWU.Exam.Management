using FWU.Exam.Management.Application.Interfaces;
using FWU.Exam.Management.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace FWU.Exam.Management.Infrastructure.Tests.Services;

public class BackupRestoreServiceTests
{
    private static IConfiguration CreateConfiguration(string? connStr = null)
    {
        var config = Substitute.For<IConfiguration>();
        config.GetConnectionString("DefaultConnection")
            .Returns(connStr ?? "Server=.;Database=TestDb;Trusted_Connection=true;TrustServerCertificate=true;");
        return config;
    }

    [Fact]
    public void GetBackupDirectory_ShouldCreateAndReturnPath()
    {
        var config = CreateConfiguration();
        var service = new BackupRestoreService(config);

        var dir = service.GetBackupDirectory();

        dir.Should().NotBeNullOrEmpty();
        Directory.Exists(dir).Should().BeTrue();
        dir.Should().EndWith("Backups");
    }

    [Fact]
    public void GetBackupDirectory_ShouldReturnSamePathOnSecondCall()
    {
        var config = CreateConfiguration();
        var service = new BackupRestoreService(config);

        var dir1 = service.GetBackupDirectory();
        var dir2 = service.GetBackupDirectory();

        dir1.Should().Be(dir2);
    }

    [Fact]
    public async Task Constructor_ShouldNotThrow_ButPropertyAccessThrows_WhenConnectionStringMissing()
    {
        var config = Substitute.For<IConfiguration>();
        config.GetConnectionString("DefaultConnection").Returns((string?)null);

        var service = new BackupRestoreService(config);
        var act = async () => await service.GetDatabaseNameAsync();

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*Connection string*not found*");
    }
}
