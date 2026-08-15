using MarwadiGheeSweetsWeb.Models;

namespace MarwadiGheeSweetsWeb.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<AppUser>> GetAllAsync();
    Task<AppUser?> GetByUsernameAsync(string username);
    Task AddAsync(AppUser user);
    Task DeleteAsync(string username);
    Task SaveAsync(List<AppUser> users);
}
