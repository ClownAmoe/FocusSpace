using FocusSpace.Application.Interfaces;
using FocusSpace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DomainTask>> GetAllByUserIdAsync(int userId) =>
            await _context.Tasks
                          .Where(t => t.UserId == userId)
                          .OrderByDescending(t => t.CreatedAt)
                          .ToListAsync();

        public async Task<DomainTask?> GetByIdAsync(int id) =>
            await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);

        public async Task<DomainTask> CreateAsync(DomainTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<DomainTask> UpdateAsync(DomainTask task)
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task DeleteAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task is not null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id) =>
            await _context.Tasks.AnyAsync(t => t.Id == id);
    }
}
