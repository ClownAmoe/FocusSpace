using FocusSpace.Application.DTOs;

namespace FocusSpace.Application.Interfaces
{
    /// <summary>
    /// Service contract for Task business logic.
    /// </summary>
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(int userId);
        Task<TaskDto?> GetTaskByIdAsync(int id);
        Task<TaskDto> CreateTaskAsync(CreateTaskDto dto);
        Task<TaskDto?> UpdateTaskAsync(UpdateTaskDto dto);
        Task<bool> DeleteTaskAsync(int id);
    }
}
