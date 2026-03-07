namespace FocusSpace.Domain.Entities
{
    public class Planet
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public string? Description { get; set; }
        public TimeSpan? DistanceFromPrevious { get; set; }
        public string? ImageUrl { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}