using System.Collections.Generic;

namespace FWU.Exam.Management.Application.DTOs;

public class GradingSchemeSelectListsDto
{
    public List<SelectOption> Programs { get; set; } = [];
    public List<SelectOption> AcademicYears { get; set; } = [];
}
