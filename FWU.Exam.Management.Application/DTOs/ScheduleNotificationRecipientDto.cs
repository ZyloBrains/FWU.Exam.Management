namespace FWU.Exam.Management.Application.DTOs;

public class ScheduleNotificationRecipientDto
{
    public int StudentRegistrationId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class NotificationSendResult
{
    public int Attempted { get; set; }
    public int EmailSent { get; set; }
    public int SmsSent { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = [];
}
