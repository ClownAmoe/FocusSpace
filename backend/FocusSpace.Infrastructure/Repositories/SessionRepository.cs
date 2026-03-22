using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using FocusSpace.Infrastructure.Data;

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

    public async System.Threading.Tasks.Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}