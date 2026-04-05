using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using FocusSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FocusSpace.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _db;

    public SessionRepository(AppDbContext db)
    {
        _db = db;
    }

    public async System.Threading.Tasks.Task AddAsync(Session session)
        => await _db.Sessions.AddAsync(session);

    public async Task<Session?> GetByIdAsync(int id)
        => await _db.Sessions.FindAsync(id);

    public async Task<IEnumerable<Session>> GetByUserIdAsync(int userId)
        => await _db.Sessions
            .Where(s => s.UserId == userId)
            .Include(s => s.Task)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();

    public async System.Threading.Tasks.Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}