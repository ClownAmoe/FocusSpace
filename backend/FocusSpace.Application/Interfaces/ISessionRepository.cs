using FocusSpace.Domain.Entities;

namespace FocusSpace.Application.Interfaces;

public interface ISessionRepository
{
    System.Threading.Tasks.Task AddAsync(Session session);
    Task<Session?> GetByIdAsync(int id);
    System.Threading.Tasks.Task SaveChangesAsync();
}