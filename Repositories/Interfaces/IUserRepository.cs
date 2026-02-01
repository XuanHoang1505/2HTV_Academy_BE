


using App.Data;
using X.PagedList;

namespace App.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> AllUsersAsync();
        Task<IPagedList<ApplicationUser>> GetAllUsersPagedAsync(int page, int limit, Dictionary<string, string>? filters = null);
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password);
        Task UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(ApplicationUser user);
        Task<bool> LockUserAsync(string userId);
        Task<bool> UnLockUserAsync(string userId);
        Task<bool> IsEmailExistsAsync(string email);
        Task<bool> IsEmailExistsForUpdateAsync(string email, string userId);
        Task<string> GetUserRoleAsync(ApplicationUser user);

        Task<ApplicationUser> GetUserByUsernameAsync(string userName);
        Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken);
        Task UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiry);
    }
}