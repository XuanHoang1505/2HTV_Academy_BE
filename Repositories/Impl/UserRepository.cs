using App.Data;
using App.Providers;
using App.Repositories.Interfaces;
using App.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.EF;

namespace App.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly JwtTokenProvider _jwtTokenProvider;
        private readonly ISendMailService _emailService;
        private readonly AppDBContext _context;
        private readonly string[] _searchableFields = { "UserName", "FullName", "Email", "Phone", "Role" };

        public UserRepository(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            JwtTokenProvider jwtTokenProvider,
            ISendMailService emailService,
            AppDBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _jwtTokenProvider = jwtTokenProvider;
            _emailService = emailService;
            _context = context;
        }

        public async Task<IEnumerable<ApplicationUser>> AllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            return users;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> IsEmailExistsForUpdateAsync(string email, string userId)
        {
            return await _userManager.Users.AnyAsync(u => u.Email == email && u.Id != userId);
        }
        public async Task<bool> DeleteUserAsync(ApplicationUser user)
        {
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user;
        }

        public async Task<ApplicationUser> GetUserByUsernameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user;
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password)
        {

            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                return user;
            }
            return null;
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
        
        public async Task<string> GetUserRoleAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        public async Task<ApplicationUser?> GetUserByRefreshTokenAsync(string refreshToken)
        {
            // var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.UtcNow);
            // return user;
            throw new NotImplementedException();
        }

        public async Task UpdateRefreshTokenAsync(string userId, string refreshToken, DateTime expiry)
        {
            var user = await GetUserByIdAsync(userId);
            if (user != null)
            {
                // user.RefreshToken = refreshToken;
                // user.RefreshTokenExpiryTime = expiry;
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task<bool> LockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            user.IsLocked = true;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<bool> UnLockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            user.IsLocked = false;
            var result = await _userManager.UpdateAsync(user);

            return result.Succeeded;
        }

        public async Task<IPagedList<ApplicationUser>> GetAllUsersPagedAsync(int page, int limit, Dictionary<string, string>? filters = null)
        {
            var query = _userManager.Users.AsQueryable();
            query = ApplyUserFilters(query, filters);
            return await query.ToPagedListAsync(page, limit);
        }

        private IQueryable<ApplicationUser> ApplyUserFilters(
            IQueryable<ApplicationUser> query,
            Dictionary<string, string>? filters)
        {
            if (filters == null || !filters.Any())
                return query;

            if (filters.ContainsKey("search") && !string.IsNullOrEmpty(filters["search"]))
            {
                var searchTerm = filters["search"];
                query = query.Where(u =>
                    EF.Functions.Like(u.UserName, $"%{searchTerm}%") ||
                    EF.Functions.Like(u.Email, $"%{searchTerm}%") ||
                    EF.Functions.Like(u.FullName, $"%{searchTerm}%") ||
                    (u.PhoneNumber != null && EF.Functions.Like(u.PhoneNumber, $"%{searchTerm}%"))
                );

            }

            foreach (var (key, value) in filters)
            {
                if (key.Equals("search", StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrEmpty(value))
                    continue;

                if (!_searchableFields.Contains(key, StringComparer.OrdinalIgnoreCase))
                    continue;

                var lowerKey = key.ToLower();

                switch (lowerKey)
                {
                    case "username":
                        query = query.Where(u => EF.Functions.Like(u.UserName, $"%{value}%"));
                        break;

                    case "fullname":
                        query = query.Where(u => EF.Functions.Like(u.FullName, $"%{value}%"));
                        break;

                    case "email":
                        query = query.Where(u => EF.Functions.Like(u.Email, $"%{value}%"));
                        break;

                    case "phone":
                        query = query.Where(u => u.PhoneNumber != null &&
                                               EF.Functions.Like(u.PhoneNumber, $"%{value}%"));
                        break;

                    case "role":
                        var userIdsWithRole = _context.UserRoles
                            .Where(ur => _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == value))
                            .Select(ur => ur.UserId);

                        query = query.Where(u => userIdsWithRole.Contains(u.Id));
                        break;
                }
            }

            return query;
        }
    }
}