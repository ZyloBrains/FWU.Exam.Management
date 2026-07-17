using Microsoft.Data.SqlClient;
using FWU.Exam.Management.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace FWU.Exam.Management.Infrastructure.Services;

public class BackupRestoreService(IConfiguration configuration) : IBackupRestoreService
{
    private string connectionString => configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    public string GetBackupDirectory()
    {
        var baseDir = AppContext.BaseDirectory;
        var backupDir = Path.Combine(baseDir, "Backups");
        Directory.CreateDirectory(backupDir);
        return backupDir;
    }

    public async Task<string> GetDatabaseNameAsync()
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return connection.Database;
    }

    public async Task<string> BackupDatabaseAsync(string? backupName = null)
    {
        var dbName = await GetDatabaseNameAsync();
        var backupDir = GetBackupDirectory();

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = string.IsNullOrWhiteSpace(backupName)
            ? $"{dbName}_{timestamp}.bak"
            : $"{backupName}_{timestamp}.bak";

        var filePath = Path.Combine(backupDir, fileName);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var sql = $"BACKUP DATABASE [{dbName}] TO DISK = N'{filePath}' WITH FORMAT, COMPRESSION, NAME = N'{dbName} - Full Backup', STATS = 10";

        await using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = 300;
        await command.ExecuteNonQueryAsync();

        return fileName;
    }

    public async Task<List<BackupInfo>> GetBackupsAsync()
    {
        var backupDir = GetBackupDirectory();
        var backups = new List<BackupInfo>();

        if (!Directory.Exists(backupDir))
            return backups;

        var files = Directory.GetFiles(backupDir, "*.bak");
        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            backups.Add(new BackupInfo
            {
                FileName = fileInfo.Name,
                FilePath = fileInfo.FullName,
                SizeBytes = fileInfo.Length,
                CreatedDate = fileInfo.CreationTime,
                SizeDisplay = FormatFileSize(fileInfo.Length)
            });
        }

        return backups.OrderByDescending(b => b.CreatedDate).ToList();
    }

    public async Task<string> RestoreDatabaseAsync(string backupFileName)
    {
        var backupDir = GetBackupDirectory();
        var filePath = Path.Combine(backupDir, backupFileName);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Backup file not found: {backupFileName}");

        var dbName = await GetDatabaseNameAsync();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Set database to single user mode to disconnect other users
        var setSingleUser = $"ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
        await using (var cmd = new SqlCommand(setSingleUser, connection))
        {
            cmd.CommandTimeout = 60;
            await cmd.ExecuteNonQueryAsync();
        }

        // Restore the database
        var restoreSql = $"RESTORE DATABASE [{dbName}] FROM DISK = N'{filePath}' WITH REPLACE, STATS = 10";
        await using (var cmd = new SqlCommand(restoreSql, connection))
        {
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync();
        }

        // Set database back to multi user mode
        var setMultiUser = $"ALTER DATABASE [{dbName}] SET MULTI_USER";
        await using (var cmd = new SqlCommand(setMultiUser, connection))
        {
            cmd.CommandTimeout = 60;
            await cmd.ExecuteNonQueryAsync();
        }

        return $"Database '{dbName}' restored successfully from {backupFileName}";
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB"];
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
