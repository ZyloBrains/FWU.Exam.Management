using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class DeletePreviewDto
{
    public string? ScheduleName { get; set; }
    public List<DeletePreviewItemDto> Items { get; set; } = [];
}

public class DeletePreviewItemDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}
