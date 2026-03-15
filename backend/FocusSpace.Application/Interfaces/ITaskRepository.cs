using DomainTask = FocusSpace.Domain.Entities.Task;

namespace FocusSpace.Application.Interfaces
{
    /// <summary>
    /// Repository contract for Task persistence operations.
    /// </summary>
    public interface ITaskRepository
    {
        Task<IEnumerable<DomainTask>> GetAllByUserIdAsync(int userId);
        Task<DomainTask?> GetByIdAsync(int id);
        Task<DomainTask> CreateAsync(DomainTask task);
        Task<DomainTask> UpdateAsync(DomainTask task);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
