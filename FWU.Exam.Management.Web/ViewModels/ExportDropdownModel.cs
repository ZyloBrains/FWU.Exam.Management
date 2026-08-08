using System.Collections.Generic;

namespace FWU.Exam.Management.Web.ViewModels;

public class ExportDropdownModel
{
    public Dictionary<string, string> RouteValues { get; set; } = new();
    public string ButtonClass { get; set; } = "bg-indigo-600 hover:bg-indigo-700";
    public string ScopeLabel { get; set; } = " (Current Page)";
    public Dictionary<string, string>? JavascriptCallbacks { get; set; }
}
