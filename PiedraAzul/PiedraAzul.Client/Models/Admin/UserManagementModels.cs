namespace PiedraAzul.Client.Models.Admin;

public class UserCreateModel
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    /// <summary>"Patient" | "Admin" | "Doctor"</summary>
    public string Role { get; set; } = "Patient";
    /// <summary>Solo cuando Role = "Doctor"</summary>
    public string? DoctorType { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UserEditModel
{
    public string UserId { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "Patient";
    public string? DoctorType { get; set; }
}

public class UserFilterModel
{
    public string? SearchText { get; set; }
    public string? RoleFilter { get; set; }
    public string? StatusFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
