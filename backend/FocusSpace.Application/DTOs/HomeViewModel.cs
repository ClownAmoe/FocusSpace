using FocusSpace.Domain.Entities;

namespace FocusSpace.Application.DTOs
{
    public class HomeViewModel
    {
        public List<Planet> Planets { get; set; } = [];
        public int CurrentPlanetOrderNumber { get; set; } = 1;
        public long TotalFocusMinutes { get; set; }
        public long? MinutesToNextPlanet { get; set; }
        public IReadOnlyDictionary<int, long> PlanetThresholds { get; set; } = new Dictionary<int, long>();
    }
}
