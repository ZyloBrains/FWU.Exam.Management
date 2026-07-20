using FWU.Exam.Management.Domain.Entities.Location;
using FWU.Exam.Management.Domain.Enums;
using FWU.Exam.Management.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FWU.Exam.Management.Web.Data.Seeders;

public static class LocationSeeder
{
    public static async Task SeedLocationDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();

        if (await context.Provinces.AnyAsync())
            return;

        // 7 Provinces of Nepal
        var provinces = new[]
        {
            new Province { ProvinceName = "Koshi Province", ProvinceCode = "P1", IsActive = true },
            new Province { ProvinceName = "Madhesh Province", ProvinceCode = "P2", IsActive = true },
            new Province { ProvinceName = "Bagmati Province", ProvinceCode = "P3", IsActive = true },
            new Province { ProvinceName = "Gandaki Province", ProvinceCode = "P4", IsActive = true },
            new Province { ProvinceName = "Lumbini Province", ProvinceCode = "P5", IsActive = true },
            new Province { ProvinceName = "Karnali Province", ProvinceCode = "P6", IsActive = true },
            new Province { ProvinceName = "Sudurpashchim Province", ProvinceCode = "P7", IsActive = true }
        };
        await context.Provinces.AddRangeAsync(provinces);
        await context.SaveChangesAsync();

        // 77 Districts
        var districts = new[]
        {
            // Koshi Province (P1) - 14 districts
            new District { DistrictName = "Taplejung", DistrictCode = "TAP", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Panchthar", DistrictCode = "PAN", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Ilam", DistrictCode = "ILA", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Jhapa", DistrictCode = "JHA", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Morang", DistrictCode = "MOR", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Sunsari", DistrictCode = "SUN", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Dhankuta", DistrictCode = "DHA", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Terhathum", DistrictCode = "TER", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Bhojpur", DistrictCode = "BHO", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Sankhuwasabha", DistrictCode = "SAN", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Solukhumbu", DistrictCode = "SOL", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Okhaldhunga", DistrictCode = "OKH", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Khotang", DistrictCode = "KHO", ProvinceId = provinces[0].Id, IsActive = true },
            new District { DistrictName = "Udayapur", DistrictCode = "UDA", ProvinceId = provinces[0].Id, IsActive = true },
            // Madhesh Province (P2) - 8 districts
            new District { DistrictName = "Saptari", DistrictCode = "SAP", ProvinceId = provinces[1].Id, IsActive = true },
            new District { DistrictName = "Siraha", DistrictCode = "SIR", ProvinceId = provinces[1].Id, IsActive = true },
            new District { DistrictName = "Dhanusha", DistrictCode = "DHA2", ProvinceId = provinces[1].Id, IsActive = true },
            new District { DistrictName = "Mahottari", DistrictCode = "MAH", ProvinceId = provinces[1].Id, IsActive = true },
            new District { DistrictName = "Sarlahi", DistrictCode = "SAR", ProvinceId = provinces[1].Id, IsActive = true },
            new District { DistrictName = "Rautahat", DistrictCode = "RAU", ProvinceId = provinces[1].Id, IsActive = true },
            new District { DistrictName = "Bara", DistrictCode = "BAR", ProvinceId = provinces[1].Id, IsActive = true },
            new District { DistrictName = "Parsa", DistrictCode = "PAR", ProvinceId = provinces[1].Id, IsActive = true },
            // Bagmati Province (P3) - 13 districts
            new District { DistrictName = "Sindhuli", DistrictCode = "SIN", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Ramechhap", DistrictCode = "RAM", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Dolakha", DistrictCode = "DOL", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Bhaktapur", DistrictCode = "BHA", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Dhading", DistrictCode = "DHA3", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Kathmandu", DistrictCode = "KAT", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Lalitpur", DistrictCode = "LAL", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Nuwakot", DistrictCode = "NUW", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Rasuwa", DistrictCode = "RAS", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Sindhupalchok", DistrictCode = "SIP", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Kavrepalanchok", DistrictCode = "KAV", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Makwanpur", DistrictCode = "MAK", ProvinceId = provinces[2].Id, IsActive = true },
            new District { DistrictName = "Chitwan", DistrictCode = "CHI", ProvinceId = provinces[2].Id, IsActive = true },
            // Gandaki Province (P4) - 11 districts
            new District { DistrictName = "Gorkha", DistrictCode = "GOR", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Lamjung", DistrictCode = "LAM", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Tanahun", DistrictCode = "TAN", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Syangja", DistrictCode = "SYA", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Kaski", DistrictCode = "KAS", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Manang", DistrictCode = "MAN", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Mustang", DistrictCode = "MUS", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Myagdi", DistrictCode = "MYA", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Parbat", DistrictCode = "PAR2", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Baglung", DistrictCode = "BAG", ProvinceId = provinces[3].Id, IsActive = true },
            new District { DistrictName = "Nawalpur", DistrictCode = "NAW", ProvinceId = provinces[3].Id, IsActive = true },
            // Lumbini Province (P5) - 12 districts
            new District { DistrictName = "Gulmi", DistrictCode = "GUL", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Palpa", DistrictCode = "PAL", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Arghakhanchi", DistrictCode = "ARG", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Nawalparasi West", DistrictCode = "NAW2", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Rupandehi", DistrictCode = "RUP", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Kapilvastu", DistrictCode = "KAP", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Pyuthan", DistrictCode = "PYU", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Rolpa", DistrictCode = "ROL", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Dang", DistrictCode = "DAN", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Banke", DistrictCode = "BAN", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Bardiya", DistrictCode = "BAR2", ProvinceId = provinces[4].Id, IsActive = true },
            new District { DistrictName = "Eastern Rukum", DistrictCode = "ERU", ProvinceId = provinces[4].Id, IsActive = true },
            // Karnali Province (P6) - 10 districts
            new District { DistrictName = "Western Rukum", DistrictCode = "WRU", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Salyan", DistrictCode = "SAL", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Dolpa", DistrictCode = "DOL2", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Humla", DistrictCode = "HUM", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Jumla", DistrictCode = "JUM", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Kalikot", DistrictCode = "KAL", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Mugu", DistrictCode = "MUG", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Surkhet", DistrictCode = "SUR", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Dailekh", DistrictCode = "DAI", ProvinceId = provinces[5].Id, IsActive = true },
            new District { DistrictName = "Jajarkot", DistrictCode = "JAJ", ProvinceId = provinces[5].Id, IsActive = true },
            // Sudurpashchim Province (P7) - 9 districts
            new District { DistrictName = "Kailali", DistrictCode = "KAI", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Achham", DistrictCode = "ACH", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Doti", DistrictCode = "DOT", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Bajhang", DistrictCode = "BAJ", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Bajura", DistrictCode = "BAJ2", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Kanchanpur", DistrictCode = "KAN", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Dadeldhura", DistrictCode = "DAD", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Baitadi", DistrictCode = "BAI", ProvinceId = provinces[6].Id, IsActive = true },
            new District { DistrictName = "Darchula", DistrictCode = "DAR", ProvinceId = provinces[6].Id, IsActive = true }
        };
        await context.Districts.AddRangeAsync(districts);
        await context.SaveChangesAsync();

        // ==================== KOSHI PROVINCE LOCAL LEVELS ====================

        // 1. Taplejung - 9 local levels
        var d = districts.First(x => x.DistrictCode == "TAP");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Phungling Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mikwakhola Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phaktanglung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sidingba Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sirijangha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aathrai Tribeni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Meringden Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pathibhara Yangwarak Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Maiwakhola Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 2. Panchthar - 8 local levels
        d = districts.First(x => x.DistrictCode == "PAN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Phidim Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Falgunanda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kummayak Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hilihang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Miklajung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Yangnam Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phalebas Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tumnam Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 3. Ilam - 10 local levels
        d = districts.First(x => x.DistrictCode == "ILA");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Ilam Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chulachuli Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mangsebung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sandakpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Suryodaya Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rong Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Deumai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phakphokthum Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahamai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 4. Jhapa - 15 local levels
        d = districts.First(x => x.DistrictCode == "JHA");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Kakarbhitta Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhadrapur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Damak Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mechinagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Birtamod Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Arjundhara Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shivasatakshi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gauradaha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kankai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gauriganga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kamal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Itahari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Haldibari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jhapa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Barhadashi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 5. Morang - 17 local levels
        d = districts.First(x => x.DistrictCode == "MOR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Biratnagar Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Itahari Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Urlabari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Belbari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sundar Dulari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rangeli Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pathari Shanischare Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kanepokhari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kerabari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ratuwamai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Budhiganga Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gramthan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhanpalthan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunbarshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Miklajung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Letang Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Matigachha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 6. Sunsari - 12 local levels
        d = districts.First(x => x.DistrictCode == "SUN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Inaruwa Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dharan Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Itahari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Duhabi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ramdhuni Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Barah Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Harinagara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhokraha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Koshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baraju Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tinpaini Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 7. Dhankuta - 7 local levels
        d = districts.First(x => x.DistrictCode == "DHA");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dhankuta Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sangurigadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chaubise Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahalaxmi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhathar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khalsa Chhintang Sahidbhumi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pakhribas Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 8. Terhathum - 5 local levels
        d = districts.First(x => x.DistrictCode == "TER");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Myanglung Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phedap Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sidicharan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aathrai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhathar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 9. Bhojpur - 10 local levels
        d = districts.First(x => x.DistrictCode == "BHO");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Bhojpur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shadanand Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Arun Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Salinadi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pauwa Sikhar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ramprasad Rai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tyasur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hatuwagadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Champe Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhankuta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 10. Sankhuwasabha - 7 local levels
        d = districts.First(x => x.DistrictCode == "SAN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Khandbari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chichila Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Silichong Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Makalu Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Panchkhapan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chainpur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 11. Solukhumbu - 8 local levels
        d = districts.First(x => x.DistrictCode == "SOL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Solududhkunda Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sidhicharan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahakulung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sotang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gumel Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thulung Dudhkoshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Likhu Pike Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Necho Bedghari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 12. Okhaldhunga - 8 local levels
        d = districts.First(x => x.DistrictCode == "OKH");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Siddhicharan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chisankhugadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunkoshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Manebhanjyang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khijidemba Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Molung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Champadevi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pakhribas Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 13. Khotang - 10 local levels
        d = districts.First(x => x.DistrictCode == "KHO");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Diktel Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Halesi Tuwachung Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aiselukharka Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sakela Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khotang Daha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Janata Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baraha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dipali Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kepilasgadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lamidanda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 14. Udayapur - 11 local levels
        d = districts.First(x => x.DistrictCode == "UDA");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Triyuga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Katari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chaudandi Gadhi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Udayapurgadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tribeni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rautamai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Limpiyadhura Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ramprasad Rai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunkoshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Belaka Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sirise Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        await context.SaveChangesAsync();

        // ==================== MADHESH PROVINCE LOCAL LEVELS ====================

        // 15. Saptari - 18 local levels
        d = districts.First(x => x.DistrictCode == "SAP");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Rajbiraj Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Saptakoshi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Surunga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhanukhadham Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bodebarsain Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kanchanrup Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shambhunath Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tilathi Koiladi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Agnisair Krishna Savaran Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chinnamasta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Saptari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khadak Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Balan Bihul Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tirahut Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hariharpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahadewa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bishnupur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rupani Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 16. Siraha - 17 local levels
        d = districts.First(x => x.DistrictCode == "SIR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Siraha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lahan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhangadhimai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Golbazar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kalyanpur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mirchaiya Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sukhipur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bariyarpatti Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aurahi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bishnupur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Laxmipur Patari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Naraha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhagawanpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sakhuwanankarkatti Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dudhauli Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Choharwa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Navarajpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 17. Dhanusha - 21 local levels
        d = districts.First(x => x.DistrictCode == "DHA2");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Janakpurdham Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhanushadham Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhireswarnath Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ganeshman Charanath Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bideshwar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hansapursatosi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kamala Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mithila Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sahidnagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Laxminiya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mukhiyapatti Musaharwa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bighapani Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aurahi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhanauji Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hariharpurgoad Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Yadukaha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhagawatipur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gujara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Singyahi Madhapur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nagarain Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bateshwar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 18. Mahottari - 16 local levels
        d = districts.First(x => x.DistrictCode == "MAH");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Jaleshwar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bardibas Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gaushala Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Samserganj Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahottari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Laxminiya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sakhuwa Prasawni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ekdarabela Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pipra Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Maniyari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhakurmara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Saptakoshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aurahi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhanapatti Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ratnawati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Matihani Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 19. Sarlahi - 20 local levels
        d = districts.First(x => x.DistrictCode == "SAR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Malangwa Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lalbandi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Barahathawa Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Haripur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ishworpur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Balara Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kaudena Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Brahmapuri Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bishnu Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhankaul Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chakraghatta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Haripurwa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Basantapur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Godeta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bagmati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Parsa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sarlahi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nawarajpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Raniganj Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kabilasi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 20. Rautahat - 18 local levels
        d = districts.First(x => x.DistrictCode == "RAU");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Gaur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chandrapur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Garuda Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ishnath Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baudhimai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Paroha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Brumhania Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dewahi Gonahi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madhavnarayan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rajpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bariyarpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gujara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tilathi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Katahariya Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Yamunamai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dharmawati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Brindaban Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Samyukta udeypur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 21. Bara - 16 local levels
        d = districts.First(x => x.DistrictCode == "BAR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Kalaiya Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Simraungadh Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jitpur Simara Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nijgadh Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pacharauta Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahagadhimai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pheta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Parwanipur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Prasauni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Karaiyamai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Devtal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Adarshakotwal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bishrampur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Suwarna Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dewapur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 22. Parsa - 14 local levels
        d = districts.First(x => x.DistrictCode == "PAR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Birgunj Metropolitan City", LocalLevelType = LocalLevelType.Metropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pokhariya Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bahudarmai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhipaharmai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Paterwa Sugauli Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bindabasini Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhobini Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jagarnathpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sakhuwa Prasauni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baudhimai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thori Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kalikatar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jirabhawani Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pakaha Mainpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        await context.SaveChangesAsync();

        // ==================== BAGMATI PROVINCE LOCAL LEVELS ====================

        // 23. Sindhuli - 9 local levels
        d = districts.First(x => x.DistrictCode == "SIN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Kamalamai Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dudhauli Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sindhulimadi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hariharpurgadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Golanjor Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Marin Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ghyanglekh Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tinpatan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phikkal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 24. Ramechhap - 8 local levels
        d = districts.First(x => x.DistrictCode == "RAM");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Ramechhap Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Manthali Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khandadevi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Umakunda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gokulganga Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Likhu Tamakoshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Doramba Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunapati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 25. Dolakha - 10 local levels
        d = districts.First(x => x.DistrictCode == "DOL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Bhimeshwar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jiri Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tamakoshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sailung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gaurishankar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kalinchok Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bigu Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Melung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shailung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lamidanda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 26. Bhaktapur - 3 local levels
        d = districts.First(x => x.DistrictCode == "BHA");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Bhaktapur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Suryabinayak Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Changunarayan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 27. Dhading - 13 local levels
        d = districts.First(x => x.DistrictCode == "DHA3");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dhading Besi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nilkantha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gajuri Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Benighat Rorang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Galchhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ganga Jamuna Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Netrawati Dabjong Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khaniyabas Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jwalamukhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thakre Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tripurasundari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rubi Valley Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Siddhalek Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 28. Kathmandu - 8 local levels
        d = districts.First(x => x.DistrictCode == "KAT");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Kathmandu Metropolitan City", LocalLevelType = LocalLevelType.Metropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kageswari-Manohara Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gokarneshwara Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tarakeshwara Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Budhanilakantha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chandragiri Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tokha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nagarjun Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // ==================== GANDAKI PROVINCE LOCAL LEVELS ====================

        // 36. Gorkha - 11 local levels
        d = districts.First(x => x.DistrictCode == "GOR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Gorkha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Palungtar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sulikot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Siranchok Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ajirkot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhimsen Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gandaki Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sahid Lakhan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chumanubri Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dharche Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Masel Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 37. Lamjung - 8 local levels
        d = districts.First(x => x.DistrictCode == "LAM");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Besisahar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madhya Nepal Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rainas Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kwhlosothar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dudhpokhari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sundarbazar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Marsyangdi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ghalegaun Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 38. Tanahun - 10 local levels
        d = districts.First(x => x.DistrictCode == "TAN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Bharatpur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Damauli Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhanu Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ghansikuwa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Myagde Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Santimunicipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aanbukhaireni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Devghat Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bandipur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rishing Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 39. Syangja - 9 local levels
        d = districts.First(x => x.DistrictCode == "SYA");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Putalibazar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Galyang Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhirkot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Waling Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Biruwa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aarjunchaupari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kaligandaki Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phedikhola Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chapakot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 40. Kaski - 7 local levels
        d = districts.First(x => x.DistrictCode == "KAS");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Pokhara Metropolitan City", LocalLevelType = LocalLevelType.Metropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lekhnath Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Machhapuchchhre Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Annapurna Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gandaki Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Parbat Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 41. Manang - 3 local levels
        d = districts.First(x => x.DistrictCode == "MAN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Chame Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Narpa Bhumi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nasong Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 42. Mustang - 3 local levels
        d = districts.First(x => x.DistrictCode == "MUS");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Gharpajhong Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lo Manthang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baragaun Muktikshetra Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 43. Myagdi - 7 local levels
        d = districts.First(x => x.DistrictCode == "MYA");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Beni Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mangala Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Raghuganga Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Malika Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Annapurna Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhaulagiri Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Muna Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 44. Parbat - 6 local levels
        d = districts.First(x => x.DistrictCode == "PAR2");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Kusma Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phalebas Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jalpa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bihadi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Paiyun Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahashila Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 45. Baglung - 10 local levels
        d = districts.First(x => x.DistrictCode == "BAG");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Baglung Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Galkot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jaimini Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kathekhola Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Taman Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Salyan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tara Saligram Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nisikhola Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Badigad Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhorpatan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 46. Nawalpur - 11 local levels
        d = districts.First(x => x.DistrictCode == "NAW");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Kawasoti Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gaindakot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Devachuli Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bardaghat Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madhyabindu Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Binayi Tribeni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bulingtar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hupsekot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sarawal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pragatinagar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rampur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        await context.SaveChangesAsync();

        // 29. Lalitpur - 6 local levels
        d = districts.First(x => x.DistrictCode == "LAL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Lalitpur Metropolitan City", LocalLevelType = LocalLevelType.Metropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Godawari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahalaxmi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Konjyosom Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bagmati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahankal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 30. Nuwakot - 12 local levels
        d = districts.First(x => x.DistrictCode == "NUW");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Bidur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rasuwa Kalika Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kakani Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kispang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tadi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Suryagadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Panchakanya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dupcheshwar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shivapuri Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Likhu Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sangkosp Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Myagang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 31. Rasuwa - 5 local levels
        d = districts.First(x => x.DistrictCode == "RAS");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Rasuwa Gadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gosaikunda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Langtang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kalika Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Uttargaya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 32. Sindhupalchok - 16 local levels
        d = districts.First(x => x.DistrictCode == "SIP");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Chautara Sangachokgadhi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Melamchi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Panchpokhari Thangpal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunkoshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Balephi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Helambu Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bahrabise Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Indrawati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jugal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lisankhupakhar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sangachok Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Balefi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhumesthan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Choyatar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kubinde Rauta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhotang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 33. Kavrepalanchok - 13 local levels
        d = districts.First(x => x.DistrictCode == "KAV");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dhulikhel Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Banepa Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Panauti Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhaktapur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mandandeupur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Namobuddha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahabharat Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Roshi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Temal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khanikhola Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bethanchok Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chauri Deurali Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Panchkhal Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 34. Makwanpur - 10 local levels
        d = districts.First(x => x.DistrictCode == "MAK");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Hetauda Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ratnanagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhimtar Shivalaya Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thaha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bakaiya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bagmati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Makawanpurgadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Raksirang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Indrasarovar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Manahari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 35. Chitwan - 7 local levels
        d = districts.First(x => x.DistrictCode == "CHI");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Bharatpur Metropolitan City", LocalLevelType = LocalLevelType.Metropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ratnanagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khairahani Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kalika Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madhyabindu Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ichchhakamana Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rapti Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        await context.SaveChangesAsync();

        // ==================== LUMBINI PROVINCE LOCAL LEVELS ====================

        // 47. Gulmi - 12 local levels
        d = districts.First(x => x.DistrictCode == "GUL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Tamghas Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Musikot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Resunga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhatrakot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhurkot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gulmi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kaligandaki Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madane Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ruru Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Satyawati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sirpa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ismake Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 48. Palpa - 10 local levels
        d = districts.First(x => x.DistrictCode == "PAL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Tansen Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rampur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bagnaskali Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mitlung Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nisdi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Purbakhola Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tinau Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ribdikot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rambha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madirgunj Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 49. Arghakhanchi - 6 local levels
        d = districts.First(x => x.DistrictCode == "ARG");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Sandhikharka Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhumikasthan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sitganga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhatradev Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Malarani Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Panini Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 50. Nawalparasi West - 10 local levels
        d = districts.First(x => x.DistrictCode == "NAW2");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Ramgram Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunwal Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bardaghat Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Palhinandan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sarawal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pratappur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Susta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sanda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tribeni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rajahar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 51. Rupandehi - 16 local levels
        d = districts.First(x => x.DistrictCode == "RUP");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Butwal Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Siddharthanagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tilottama Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Devinagar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kanchan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sainamaina Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gaidahawa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mayadevi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rohini Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Omsatiya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lumbini Sanskritik Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kotahimai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sammarimai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Siyari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Marchawari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Suddhodhan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 52. Kapilvastu - 11 local levels
        d = districts.First(x => x.DistrictCode == "KAP");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Kapilvastu Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Banganga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shivaraj Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Maharajgunj Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Yashodhara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Suda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bijayanagar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mayadevi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Budhi Kedar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Krishnanagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gorusinge Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 53. Pyuthan - 9 local levels
        d = districts.First(x => x.DistrictCode == "PYU");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Pyuthan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sarumarani Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mallarani Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Airawati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mandavi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jhimruk Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baraun Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gaumukhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Naubahini Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 54. Rolpa - 10 local levels
        d = districts.First(x => x.DistrictCode == "ROL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Rolpa Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tribeni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Runtigadhi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunil Smriti Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gangadev Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lungri Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thawang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Paribartan Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rolpa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 55. Dang - 11 local levels
        d = districts.First(x => x.DistrictCode == "DAN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Ghorahi Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tulsipur Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lamahi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gaidakot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Babai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dang Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rapti Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shantinagar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aathrawati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rajpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hapur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 56. Banke - 8 local levels
        d = districts.First(x => x.DistrictCode == "BAN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Nepalgunj Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kohalpur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bansgadhi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Narainapur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Raptisonari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khajura Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Janaki Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baijapur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 57. Bardiya - 8 local levels
        d = districts.First(x => x.DistrictCode == "BAR2");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Gulariya Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Madhuwan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thakurbaba Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bansgari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Badhaiyatal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rajpur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Geruwa Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Barabardiya Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 58. Eastern Rukum - 4 local levels
        d = districts.First(x => x.DistrictCode == "ERU");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Rukumkot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shyari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Putha Uttarganga Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Musikot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        await context.SaveChangesAsync();

        // ==================== KARNALI PROVINCE LOCAL LEVELS ====================

        // 59. Western Rukum - 5 local levels
        d = districts.First(x => x.DistrictCode == "WRU");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = " Musikot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Triveni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chaurjahari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aathbiskot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhumekot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 60. Salyan - 10 local levels
        d = districts.First(x => x.DistrictCode == "SAL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Salyan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shakha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khapchok Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kumakh Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Marmat Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kaira Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tribeni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bangad Kupinde Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Siddha Kumakh Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhatrakot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 61. Dolpa - 7 local levels
        d = districts.First(x => x.DistrictCode == "DOL2");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dolpo Buddha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shey Phoksundo Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thuli Bheri Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tripurasundari Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mudkechula Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kaike Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chharka Tangsong Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 62. Humla - 9 local levels
        d = districts.First(x => x.DistrictCode == "HUM");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Simkot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Namkha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mugkali Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sarkegad Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chankheli Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Adanchuli Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tanjakot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kharpunath Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tsumkot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 63. Jumla - 9 local levels
        d = districts.First(x => x.DistrictCode == "JUM");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Chandannath Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kanaka Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Hima Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tila Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khuwaphad Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Patmara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Narharinath Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sinja Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Guthichaur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 64. Kalikot - 9 local levels
        d = districts.First(x => x.DistrictCode == "KAL");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Khandachakra Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Raskot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sanni Triveni Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Subha Kalika Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "NarahariNath Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Palma Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Phaktash Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tilma Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jumla Baisi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 65. Mugu - 6 local levels
        d = districts.First(x => x.DistrictCode == "MUG");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Gamgadhi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhayanath Rara Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sarmul Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sathi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhathar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kotankanda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 66. Surkhet - 8 local levels
        d = districts.First(x => x.DistrictCode == "SUR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Birendranagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gurbhakot Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lekbesi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bheriganga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kamal Bazaar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chaukune Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Barahatal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Simta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 67. Dailekh - 10 local levels
        d = districts.First(x => x.DistrictCode == "DAI");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dullu Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dailekh Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gurans Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhairabi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Narayan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aathbisha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dungeshwor Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chamundabindu Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhagawatimai Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thantikandh Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 68. Jajarkot - 8 local levels
        d = districts.First(x => x.DistrictCode == "JAJ");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Bheri Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chedagad Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Nalgad Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Junichande Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shibalaya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dunai Kath Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Khalanga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ghoreta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        await context.SaveChangesAsync();

        // ==================== SUDURPASHCHIM PROVINCE LOCAL LEVELS ====================

        // 69. Kailali - 14 local levels
        d = districts.First(x => x.DistrictCode == "KAI");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dhangadhi Sub-Metropolitan City", LocalLevelType = LocalLevelType.SubMetropolitan, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ghodaghodi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Tikapur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lamki Chuha Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gauriganga Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Joshipur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kailari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mohanyal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ghiring Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Janaki Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bardagoriya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhajani Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Krishnapur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Attariya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 70. Achham - 10 local levels
        d = districts.First(x => x.DistrictCode == "ACH");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Mangalsen Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sanfebagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Panchadewal Binayak Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chaurpati Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mellekh Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dhankari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shivalaya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kamalbazar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Turmakhand Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bannigadi Jayagad Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 71. Doti - 9 local levels
        d = districts.First(x => x.DistrictCode == "DOT");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dipayal Silgadhi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Aadarsha Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jorayal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "K.I. Singh Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sayal Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shikhar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bogatanikoala Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Purbichauki Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Badi Kedar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 72. Bajhang - 12 local levels
        d = districts.First(x => x.DistrictCode == "BAJ");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Jaya Prithvi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bungal Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhabis Pathibhera Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Thalara Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Durgathali Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kedarsyun Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Masta Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bayabish Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Surma Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Rithapata Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sunkuda Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Marita Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 73. Bajura - 9 local levels
        d = districts.First(x => x.DistrictCode == "BAJ2");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Badimalika Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Budhinanda Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Jukot Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bajura Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Himali Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Swami Kartik Khapar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gaumul Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kolti Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Chhatraganj Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 74. Kanchanpur - 10 local levels
        d = districts.First(x => x.DistrictCode == "KAN");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Mahendranagar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Beldandi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhimdatta Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Krishnapur Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Punarbas Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Mahakali Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Baijanath Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Laljhadi Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shuklaphanta Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Raipur Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 75. Dadeldhura - 7 local levels
        d = districts.First(x => x.DistrictCode == "DAD");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Amargadhi Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Alital Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ajayameru Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bhageswari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Ganyapadhura Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Parshuram Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Navadurga Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        // 76. Baitadi - 10 local levels
        d = districts.First(x => x.DistrictCode == "BAI");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Dasharathchand Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Patan Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Melauli Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shivanath Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Pancheshwar Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Dogada Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Surnaya Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Sigas Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Kedarnath Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Bitthad Bazar Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true }
        });

        // 77. Darchula - 10 local levels
        d = districts.First(x => x.DistrictCode == "DAR");
        await context.LocalLevels.AddRangeAsync(new[] {
            new LocalLevel { LocalLevelName = "Mahakali Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Duhun Municipality", LocalLevelType = LocalLevelType.Municipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Apidor Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Byash Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Darchula Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Gokule Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Lekam Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Malikarjun Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Naugad Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true },
            new LocalLevel { LocalLevelName = "Shaileshwari Rural Municipality", LocalLevelType = LocalLevelType.RuralMunicipality, DistrictId = d.Id, IsActive = true }
        });

        await context.SaveChangesAsync();
    }
}
