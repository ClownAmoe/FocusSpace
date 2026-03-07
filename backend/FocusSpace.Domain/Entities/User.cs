using FocusSpace.Domain.Enums;

namespace FocusSpace.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public bool IsBlocked { get; set; } = false;

        public int CurrentPlanetId { get; set; } = 1;
        public long TotalFocusMinutes { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Planet CurrentPlanet { get; set; } = null!;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
    }
}