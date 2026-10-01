using $safeprojectname$.Domain.Entities.Seedworks;

namespace $safeprojectname$.Domain.Entities;

public class AppUserProfile : BaseEntity
{
    public Guid AppUserId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public DateOnly? BirthDate { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? Country { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Bio { get; private set; }
    public virtual AppUser AppUser { get; private set; } = null!;

    private AppUserProfile() { }

    public AppUserProfile(Guid appUserId, string firstName = "", string lastName = "")
    {
        AppUserId = appUserId;
        FirstName = string.IsNullOrWhiteSpace(firstName) ? appUserId.ToString()[..8] : firstName;
        LastName = string.IsNullOrWhiteSpace(lastName) ? "User" : lastName;
    }

    public void Update(string firstName, string lastName, DateOnly? birthDate, string? phoneNumber, string? address, string? city, string? country, string? avatarUrl, string? bio)
    {
        FirstName = firstName;
        LastName = lastName;
        BirthDate = birthDate;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        Country = country;
        if (avatarUrl != null) AvatarUrl = avatarUrl;
        Bio = bio;
    }

    public string FullName => $"{FirstName} {LastName}";
}
