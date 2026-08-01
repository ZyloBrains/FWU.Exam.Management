using FWU.Exam.Management.Domain.Entities.Colleges;
using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class CollegeSeeder
{
    public static async Task SeedCollegesAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Colleges.IgnoreQueryFilters().AnyAsync())
            return;

        var localLevels = await context.LocalLevels.ToListAsync();

        var colleges = new List<College>();

        foreach (var d in CollegeData.All)
        {
            int? addressId = null;
            if (d.DistrictId > 0 && !string.IsNullOrEmpty(d.MunicipalityVdc))
            {
                var municipalityName = d.MunicipalityVdc.Split(',')[0].Trim();
                var localLevel = localLevels.FirstOrDefault(ll =>
                    ll.DistrictId == d.DistrictId &&
                    ll.LocalLevelName != null &&
                    ll.LocalLevelName.Equals(municipalityName, StringComparison.OrdinalIgnoreCase));

                if (localLevel != null)
                {
                    var address = new Address
                    {
                        LocalLevelId = localLevel.Id,
                        WardNumber = d.WardNo,
                        HouseNumber = d.HouseNo,
                        FullAddress = d.MunicipalityVdc,
                        IsActive = true,
                    };
                    context.Addresses.Add(address);
                    await context.SaveChangesAsync();
                    addressId = address.Id;
                }
            }

            colleges.Add(new College
            {
                Code = d.Code,
                Name = d.Name,
                ShortName = d.ShortName,
                Website = d.Website,
                Email = string.IsNullOrWhiteSpace(d.Email) ? $"{d.Code}@fwu.edu.np" : d.Email,
                Phone1 = d.Phone1,
                Phone2 = d.Phone2,
                Fax = d.Fax,
                PrincipalName = string.IsNullOrWhiteSpace(d.PrincipalName) ? "TBD" : d.PrincipalName,
                PrincipalContactNumber = string.IsNullOrWhiteSpace(d.PrincipalContactNo) ? "TBD" : d.PrincipalContactNo,
                Remarks = d.Remarks,
                IsActive = d.IsActive,
                IsExamCenterOnly = d.IsCentreOnly,
                AddressId = addressId,
                TenantColleges = [new TenantCollege()],
            });
        }

        context.Colleges.AddRange(colleges);
        await context.SaveChangesAsync();
    }
}

file static class CollegeData
{
    public static readonly (string Code, string Name, string? ShortName, string? Website, string? Email,
        string? Phone1, string? Phone2, string? Fax, string? PrincipalName, string? PrincipalContactNo,
        string? Remarks, bool IsActive, bool IsCentreOnly, int CollegeTypeId, int DistrictId,
        string? MunicipalityVdc, int? WardNo, string? HouseNo)[] All =
    [
        ("SCH001", "UNIVERSITY CENTRAL CAMPUS", "1", null, "principal@fwu.edu.np", null, "099-526243", null, null, null, null, true, false, 1, 47, "MAHENDRANAGAR, KANCHANPUR", null, null),
        ("SCH002", "TIKAPUR MULTIPLE CAMPUS", "2", null, null, null, "9851148826", null, null, null, null, true, false, 1, 75, "TIKAPUR, KAILALI", null, null),
        ("SCH003", "DURGALAXMI MULTIPLE CAMPUS", "3", "http://durgalaxmi.fwu.edu.np/", "durgalaxmi@fwu.edu.np", null, null, null, null, null, null, true, false, 1, 75, "ATTARIYA, KAILALI", null, null),
        ("SCH010", "BADIMALIKA CAMPUS", "10", "http://badimalika.fwu.edu.np", "badimalika@fwu.edu.np", "9848437208", null, null, "Nar Bahadur Katuwal", "9848693755", null, true, false, 1, 71, "NAUBIS, BAJURA", null, null),
        ("SCH004", "TRIVENI MULTIPLE CAMPUS", "4", null, null, null, "9848831014", null, null, null, null, true, false, 1, 48, "JOGBUDHA, DADELDHURA", null, null),
        ("SCH005", "GHANTESHWAR MULTIPLE CAMPUS", "5", "http://ghanteshwar.fwu.edu.np/", "ghanteshwar@fwu.edu.np", null, null, null, "Sharad Kumar Adhikari", null, null, true, false, 1, 74, "JORAYAL, DOTI", null, null),
        ("SCH006", "SITARAM MULTIPLE CAMPUS", "6", null, null, null, "9848418879", null, null, null, null, true, false, 1, 74, "JIJODAMANDAU, DOTI", null, null),
        ("SCH007", "JANATA MULTIPLE CAMPUS", "7", null, null, null, "9843520790", null, null, null, null, true, false, 1, 73, "BAYALPATA, ACHHAM", null, null),
        ("SCH008", "JAYAPRITHVI MULTIPLE CAMPUS", "8", null, null, null, "9749001385", null, null, null, null, true, false, 1, 72, "BHOPUR, BAJHANG", null, null),
        ("SCH009", "BAJURA CAMPUS", "9", "http://bajura.fwu.edu.np", "bajura@fwu.edu.np", "9848480273", null, null, "Rup Bahadur Raule", "9848694478", null, true, false, 1, 71, "MARTADI, BAJURA", null, null),
        ("SCH011", "MANILEK MULTIPLE CAMPUS", "11", null, "manilek@fwu.edu.np", "9848782677", null, null, null, null, null, true, false, 1, 49, "MELAULI, BAITADI", null, null),
        ("SCH012", "PATAN MULTIPLE CAMPUS", "12", null, null, null, "9848804353", null, null, null, null, true, false, 1, 49, "PATAN, BAITADI", null, null),
        ("SCH013", "JAGANNATH MULTIPLE CAMPUS", "13", "http://patan.fwu.edu.np/", "patan@fwu.edu.np", "9848776825", null, null, "Binod Bhandari", "9848800939", null, true, false, 1, 49, "GOTHALAPANI, BAITADI", null, null),
        ("SCH014", "GOKULESHWAR MULTIPLE CAMPUS", "14", "http://gokuleshwar.fwu.edu.np/", "gokuleshwar@fwu.edu.np", "9848877877", null, null, "Parmal Singh Mahara", "9848837708", null, true, false, 1, 50, "GOKULESHWAR, DARCHULA", null, null),
        ("SCH015", "DARCHULA MULTIPLE CAMPUS", "15", "http://darchula.fwu.edu.np", "darchula@fwu.edu.np", "93420478", null, null, "Narendra Raj Awasthi", "9848727565", null, true, false, 1, 50, "KHALANGA, DARCHULA", null, null),
        ("SCH016", "KAILALI MULTIPLE CAMPUS", "16", null, null, null, "091-521223", null, null, null, null, true, false, 1, 75, "DHANGADHI, KAILALI", null, null),
        ("SCH017", "FACULTY OF HEALTH SCIENCES", "17", null, "dean.science@fwu.edu.np", null, null, null, null, null, null, true, false, 1, 48, "AMARGADHI, DADELDHURA", null, null),
        ("SCH101", "AADIM COLLEGE OF MANAGEMENT AND IT", "101", null, null, null, null, null, null, null, null, true, false, 2, 1, "Kathmandu Metropolitan City", null, null),
        ("SCH102", "APEX MANAGEMENT COLLEGE", "102", null, null, null, null, null, null, null, null, true, false, 2, 58, "Birgunj", null, null),
        ("SCH103", "ASIAN INSTITUTE OF TECHNOLOGY AND MANAGEMENT", "103", null, null, null, null, null, null, null, null, true, false, 2, 1, "Kathmandu Metropolitan City", null, null),
        ("SCH104", "BHERI COLLEGE OF ENGINEERING AND MANAGEMENT", "104", null, null, null, null, null, null, null, null, true, false, 2, 9, null, null, null),
        ("SCH105", "BIRGUNJ DECIMAL PUBLIC COLLEGE", "105", null, null, null, null, null, null, null, null, true, false, 2, 58, null, null, null),
        ("SCH106", "CAMBRIDGE BUSINESS SCHOOL", "106", null, null, null, null, null, null, null, null, true, false, 2, 11, null, null, null),
        ("SCH107", "DAV INTERNATIONAL COLLEGE PRIVATE LIMITED", "107", null, null, null, null, null, null, null, null, true, false, 2, 3, null, null, null),
        ("SCH108", "DREAMS COLLEGE", "108", null, null, null, null, null, null, null, null, true, false, 2, 59, null, null, null),
        ("SCH109", "FORBES COLLEGE", "109", null, null, null, null, null, null, null, null, true, false, 2, 35, null, null, null),
        ("SCH110", "GANDAKI ACADEMY OF INTERDISCIPLINARY STUDIES", "110", null, null, null, null, null, null, null, null, true, false, 2, 40, null, null, null),
        ("SCH111", "GERUWA MULTIPLE CAMPUS", "111", null, null, null, null, null, null, null, null, true, false, 2, 58, null, null, null),
        ("SCH112", "GYAN JYOTI MANAGEMENT COLLEGE", "112", null, null, null, null, null, null, null, null, true, false, 2, 64, null, null, null),
        ("SCH113", "GYANSUDHA TECHNICAL COLLEGE", "113", null, null, null, null, null, null, null, null, true, false, 2, 35, null, null, null),
        ("SCH114", "HIMALAYA COLLEGE", "114", null, null, null, null, null, null, null, null, true, false, 2, 27, null, null, null),
        ("SCH115", "INSTITUTE OF BUSINESS MANAGEMENT COLLEGE", "115", null, null, null, null, null, null, null, null, true, false, 2, 16, null, null, null),
        ("SCH116", "KATHMANDU WORLD SCHOOL OF ENGINEERING AND MANAGEMENT", "116", null, null, null, null, null, null, null, null, true, false, 2, 26, null, null, null),
        ("SCH117", "KEYSTONE MANAGEMENT COLLEGE", "117", null, null, null, null, null, null, null, null, true, false, 2, 34, null, null, null),
        ("SCH118", "MAHARISHI COLLEGE", "118", null, null, null, null, null, null, null, null, true, false, 2, 27, null, null, null),
        ("SCH119", "MEGA COLLEGE INTERNATIONAL", "119", null, null, null, null, null, null, null, null, true, false, 2, 25, null, null, null),
        ("SCH120", "MITHILA INSTITUTE OF TECHNOLOGY", "120", null, null, null, null, null, null, null, null, true, false, 2, 17, null, null, null),
        ("SCH121", "BARDIBAAS NAVA KSHITIZ COLLEGE", "121", null, null, null, null, null, null, null, null, true, false, 2, 18, null, null, null),
        ("SCH122", "PRANITA COLLEGE OF MANAGEMENT", "122", null, null, null, null, null, null, null, null, true, false, 2, 16, null, null, null),
        ("SCH123", "RAGHAVENDRA GAURAV MULTIPLE CAMPUS", "123", null, null, null, null, null, null, null, null, true, false, 2, 50, null, null, null),
        ("SCH124", "RELIABLE COLLEGE", "124", null, null, null, null, null, null, null, null, true, false, 2, 34, null, null, null),
        ("SCH125", "SAGUN COLLEGE OF MANAGEMENT", "125", null, null, null, null, null, null, null, null, true, false, 2, 34, null, null, null),
        ("SCH126", "SIDDHAPAILA TECHNICAL COLLEGE", "126", null, null, null, null, null, null, null, null, true, false, 2, 59, null, null, null),
        ("SCH127", "SHIKSHYALAYA COLLEGE", "127", null, null, null, null, null, null, null, null, true, false, 2, 26, null, null, null),
        ("SCH128", "TECHNICAL RESEARCH FOR TRAINING INSTITUTE", "128", null, null, null, null, null, null, null, null, true, false, 2, 34, null, null, null),
        ("SCH129", "WESTERN ADVANCE COLLEGE OF ENGINEERING AND MANAGEMENT", "129", null, null, null, null, null, null, null, null, true, false, 2, 48, null, null, null),
        ("SCH130", "WESTERN COLLEGE AND RESEARCH CENTER", "130", null, null, null, null, null, null, null, null, true, false, 2, 59, null, null, null),
        ("SCH131", "WESTERN COLLEGE OF BUSINESS STUDIES", "131", null, null, null, null, null, null, null, null, true, false, 2, 9, "Kohalpur", null, null),
        ("SCH132", "BARHADEU CAMPUS", "132", null, null, null, null, null, null, null, null, true, false, 2, 72, null, null, null),
        ("SCH133", "RAMAROSHAN MULTIPLE CAMPUS", "133", null, null, null, null, null, null, null, null, true, false, 2, 73, null, null, null),
        ("SCH134", "RUPAL MULTIPLE CAMPUS", "134", null, null, null, null, null, null, null, null, true, false, 2, 48, null, null, null),
        ("SCH135", "SAMBHUNATH MULTIPLE CAMPUS", "135", null, null, null, null, null, null, null, null, true, false, 2, 71, null, null, null),
        ("SCH136", "TURMAKHAND MULTIPLE CAMPUS", "136", null, null, null, null, null, null, null, null, true, false, 2, 73, null, null, null),
    ];
}
