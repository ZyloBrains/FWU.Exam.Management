using System.ComponentModel.DataAnnotations;

namespace fwu_examination_management_system.Data.Models
{
    public class Municipality
    {
        public int Id { get; set; }

        [Required]
        public int DistrictId { get; set; }

        [Required, MaxLength(255)]
        public string? MunicipalityName { get; set; }

        [Required]
        public MunicipalityType MunicipalityType { get; set; }  // <-- Enum

        public bool IsActive { get; set; }

        public virtual District? District { get; set; }
    }
    public enum MunicipalityType
    {
        Municipality,          // Standard municipality
        RuralMunicipality,     // Gaunpalika
        SubMetropolitan,       // Upamahanagarpalika
        Metropolitan           // Mahanagarpalika
    }
}
