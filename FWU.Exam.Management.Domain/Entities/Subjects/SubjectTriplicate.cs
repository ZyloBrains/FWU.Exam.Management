using System.ComponentModel.DataAnnotations;

namespace FWU.Exam.Management.Domain.Entities.Subjects;

public class SubjectTriplicate
{
    public int Id { get; set; }

    public int Year { get; set; }

    [Required, MaxLength(20)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(15)]
    public string? School { get; set; }

    [MaxLength(15)]
    public string? Center { get; set; }

    [MaxLength(15)]
    public string? Symbol { get; set; }

    [MaxLength(1)]
    public string? Alphabet { get; set; }

    [MaxLength(50)]
    public string? Type { get; set; }

    public int Grade { get; set; }

    [MaxLength(100)]
    public string? Name { get; set; }

    [MaxLength(10)]
    public string? Sex { get; set; }

    [MaxLength(10)]
    public string? DateOfBirth { get; set; }

    [MaxLength(10)]
    public string? Subject1 { get; set; }

    [MaxLength(10)]
    public string? Theory1 { get; set; }

    [MaxLength(10)]
    public string? Practical1 { get; set; }

    [MaxLength(10)]
    public string? Subject2 { get; set; }

    [MaxLength(10)]
    public string? Theory2 { get; set; }

    [MaxLength(3)]
    public string? Practical2 { get; set; }

    [MaxLength(10)]
    public string? Subject3 { get; set; }

    [MaxLength(10)]
    public string? Theory3 { get; set; }

    [MaxLength(3)]
    public string? Practical3 { get; set; }

    [MaxLength(10)]
    public string? Subject4 { get; set; }

    [MaxLength(10)]
    public string? Theory4 { get; set; }

    [MaxLength(3)]
    public string? Practical4 { get; set; }

    [MaxLength(10)]
    public string? Subject5 { get; set; }

    [MaxLength(10)]
    public string? Theory5 { get; set; }

    [MaxLength(3)]
    public string? Practical5 { get; set; }

    [MaxLength(10)]
    public string? Subject6 { get; set; }

    [MaxLength(10)]
    public string? Theory6 { get; set; }

    [MaxLength(3)]
    public string? Practical6 { get; set; }

    [MaxLength(10)]
    public string? Subject7 { get; set; }

    [MaxLength(10)]
    public string? Theory7 { get; set; }

    [MaxLength(3)]
    public string? Practical7 { get; set; }

    [MaxLength(10)]
    public string? Subject8 { get; set; }

    [MaxLength(10)]
    public string? Theory8 { get; set; }

    [MaxLength(3)]
    public string? Practical8 { get; set; }

    [MaxLength(10)]
    public string? Subject9 { get; set; }

    [MaxLength(10)]
    public string? Theory9 { get; set; }

    [MaxLength(3)]
    public string? Practical9 { get; set; }

    [MaxLength(10)]
    public string? Subject10 { get; set; }

    [MaxLength(10)]
    public string? Theory10 { get; set; }

    [MaxLength(3)]
    public string? Practical10 { get; set; }

    [MaxLength(10)]
    public string? Subject11 { get; set; }

    [MaxLength(10)]
    public string? Theory11 { get; set; }

    [MaxLength(3)]
    public string? Practical11 { get; set; }
}
