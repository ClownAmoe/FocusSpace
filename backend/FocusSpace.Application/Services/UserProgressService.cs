using FocusSpace.Application.DTOs;
using FocusSpace.Application.Interfaces;

namespace FocusSpace.Application.Services;

public class UserProgressService : IUserProgressService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlanetRepository _planetRepository;

    // Minutes of focus needed to reach each planet (keyed by OrderNumber).
    public static readonly IReadOnlyDictionary<int, long> ThresholdByOrder = new Dictionary<int, long>()
    {
        { 1, 0 },
        { 2, 120 },
        { 3, 300 },
        { 4, 600 },
        { 5, 1200 },
        { 6, 2100 },
        { 7, 3300 },
        { 8, 5000 }
    };

    public UserProgressService(IUserRepository userRepository, IPlanetRepository planetRepository)
    {
        _userRepository = userRepository;
        _planetRepository = planetRepository;
    }

    public async Task<PlanetAdvancementDto> AddFocusMinutesAndCheckPlanetAsync(int userId, int minutesCompleted)
    {
        if (minutesCompleted <= 0)
            throw new ArgumentException("Minutes completed must be positive.", nameof(minutesCompleted));

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found");

        user.TotalFocusMinutes += minutesCompleted;

        var planets = (await _planetRepository.GetAllOrderedAsync()).ToList();

        var earnedPlanet = planets
            .Where(p => ThresholdByOrder.TryGetValue(p.OrderNumber, out var required)
                        && user.TotalFocusMinutes >= required)
            .OrderByDescending(p => p.OrderNumber)
            .FirstOrDefault();

        bool advanced = earnedPlanet is not null && earnedPlanet.Id != user.CurrentPlanetId;
        if (advanced)
            user.CurrentPlanetId = earnedPlanet!.Id;

        await _userRepository.UpdateAsync(user);

        var nextPlanet = planets.FirstOrDefault(p =>
            ThresholdByOrder.TryGetValue(p.OrderNumber, out var req)
            && req > user.TotalFocusMinutes);

        long? minutesToNext = nextPlanet is not null
            && ThresholdByOrder.TryGetValue(nextPlanet.OrderNumber, out var nextReq)
            ? nextReq - user.TotalFocusMinutes
            : null;

        return new PlanetAdvancementDto
        {
            Advanced = advanced,
            CurrentPlanetId = user.CurrentPlanetId,
            CurrentPlanetName = earnedPlanet?.Name ?? string.Empty,
            TotalFocusMinutes = user.TotalFocusMinutes,
            MinutesToNextPlanet = minutesToNext
        };
    }
}
