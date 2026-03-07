using FocusSpace.Domain.Enums;

namespace FocusSpace.Domain.Entities
{
    public class Session
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? TaskId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan PlannedDuration { get; set; }
        public TimeSpan? ActualDuration { get; set; }
        public SessionStatus Status { get; set; } = SessionStatus.Ongoing;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User User { get; set; } = null!;
        public Task? Task { get; set; }
    }
}
