
using SPMS.Models;

public class UserSession
{
    public Int64 UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserRole { get; set; }
}

public partial class UserProfileViewModel
{
    public long UserId { get; set; }

    public string Title { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Phone { get; set; }

    public string Address1 { get; set; } = null!;

    public string? Address2 { get; set; }

    public string? Town { get; set; }

    public string State { get; set; } = null!;

    public string Country { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string? Ipaddress { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool? IsActive { get; set; }
    public Int64 StateId { get; set; } = 0;
    public Int64 CountryId { get; set; } = 0;
}


