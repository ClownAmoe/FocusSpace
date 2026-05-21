using FocusSpace.Domain.Entities;

namespace FocusSpace.Application.Interfaces;

public interface IPlanetRepository
{
    Task<IEnumerable<Planet>> GetAllOrderedAsync();
}
