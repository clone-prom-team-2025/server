using App.Core.Models.User;

namespace App.Core.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}