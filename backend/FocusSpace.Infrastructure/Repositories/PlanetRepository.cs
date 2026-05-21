using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using FocusSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FocusSpace.Infrastructure.Repositories;

public class PlanetRepository : IPlanetRepository
{
    private readonly AppDbContext _db;

    public PlanetRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Planet>> GetAllOrderedAsync()
        => await _db.Planets.OrderBy(p => p.OrderNumber).ToListAsync();
}
