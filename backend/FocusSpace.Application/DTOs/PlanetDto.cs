namespace FocusSpace.Application.DTOs
{
    public class PlanetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public string? Description { get; set; }
        public TimeSpan? DistanceFromPrevious { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdatePlanetDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int OrderNumber { get; set; }
        public string? Description { get; set; }
        public TimeSpan? DistanceFromPrevious { get; set; }
        public string? ImageUrl { get; set; }
    }
}
