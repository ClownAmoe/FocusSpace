using FocusSpace.Application.DTOs;

namespace FocusSpace.Application.Interfaces;

public interface IUserProgressService
{
    Task<PlanetAdvancementDto> AddFocusMinutesAndCheckPlanetAsync(int userId, int minutesCompleted);
}
