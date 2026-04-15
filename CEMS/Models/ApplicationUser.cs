using Microsoft.AspNetCore.Identity;

namespace CEMS.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string FullName
    {
        get
        {
            var first = FirstName?.Trim() ?? string.Empty;
            var last = LastName?.Trim() ?? string.Empty;
            var fullName = $"{first} {last}".Trim();

            return string.IsNullOrWhiteSpace(fullName) ? Email ?? UserName ?? "User" : fullName;
        }
    }
}