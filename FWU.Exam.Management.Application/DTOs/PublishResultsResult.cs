namespace FWU.Exam.Management.Application.DTOs;

public class PublishResultsResult
{
    public int Published { get; set; }
    public int Notified { get; set; }
    public int Failed { get; set; }
    public List<string> Errors { get; set; } = [];
}
