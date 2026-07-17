namespace FWU.Exam.Management.Application.Interfaces;

public interface IBackupRestoreService
{
    Task<string> BackupDatabaseAsync(string? backupName = null);
    Task<List<BackupInfo>> GetBackupsAsync();
    Task<string> RestoreDatabaseAsync(string backupFileName);
    Task<string> GetDatabaseNameAsync();
    string GetBackupDirectory();
}

public class BackupInfo
{
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public long SizeBytes { get; set; }
    public DateTime CreatedDate { get; set; }
    public string SizeDisplay { get; set; } = "";
}
