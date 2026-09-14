using backend.Models;

namespace backend.Services
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
}
