using FWU.Exam.Management.Application.DTOs;

namespace FWU.Exam.Management.Application.Interfaces;

public interface IPublishResultsService
{
    Task<PublishResultsPreviewDto?> GetPreviewAsync(int examScheduleId, int collegeId);
    Task<PublishResultsResultDto> PublishResultsAsync(int examScheduleId, int collegeId, string publishedBy);
}
