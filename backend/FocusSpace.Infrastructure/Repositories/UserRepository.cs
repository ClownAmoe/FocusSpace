using FocusSpace.Application.Interfaces;
using FocusSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FocusSpace.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async System.Threading.Tasks.Task<User?> GetByIdAsync(int userId)
        => await _userManager.FindByIdAsync(userId.ToString());

    public async System.Threading.Tasks.Task UpdateAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(
                $"Failed to update user {user.Id}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
    }
}
