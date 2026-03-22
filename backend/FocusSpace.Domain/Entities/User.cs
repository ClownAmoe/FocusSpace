using FocusSpace.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FocusSpace.Domain.Entities
{
    /// <summary>
    /// Application user — extends IdentityUser&lt;int&gt; with domain-specific fields.
    /// </summary>
    public class User : IdentityUser<int>
    {
        public UserRole Role { get; set; } = UserRole.User;

        /// <summary>Whether an admin has manually blocked this account.</summary>
        public bool IsBlocked { get; set; } = false;

        /// <summary>Whether the account is awaiting admin approval after registration.</summary>
        public bool IsApproved { get; set; } = false;

        public int CurrentPlanetId { get; set; } = 1;
        public long TotalFocusMinutes { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Planet CurrentPlanet { get; set; } = null!;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}