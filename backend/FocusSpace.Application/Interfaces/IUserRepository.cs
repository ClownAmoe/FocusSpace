using FocusSpace.Domain.Entities;
using SystemTask = System.Threading.Tasks.Task;

namespace FocusSpace.Application.Interfaces;

public interface IUserRepository
{
    System.Threading.Tasks.Task<User?> GetByIdAsync(int userId);
    SystemTask UpdateAsync(User user);
}
