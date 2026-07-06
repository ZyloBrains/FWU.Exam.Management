public class Role
{
    public const string SuperAdmin = nameof(SuperAdmin);
    public const string FacultyAdmin = nameof(FacultyAdmin);
    public const string CollegeAdmin = nameof(CollegeAdmin);
    public const string DepartmentAdmin = nameof(DepartmentAdmin);
    public const string Teacher = nameof(Teacher);
    public const string Student = nameof(Student);

    public static readonly string[] AllRoles = [SuperAdmin, FacultyAdmin, CollegeAdmin, DepartmentAdmin, Teacher, Student];
}