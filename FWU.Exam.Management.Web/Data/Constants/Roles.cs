public class Role
{
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string FacultyAdmin = nameof(FacultyAdmin);
    public const string CollegeAdmin = nameof(CollegeAdmin);
    public const string Student = nameof(Student);

    public static readonly string[] AllRoles = [SuperAdmin, FacultyAdmin, CollegeAdmin, Student];
}