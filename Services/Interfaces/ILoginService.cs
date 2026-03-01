using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.DTOs;

namespace Services.Interfaces
{
    public interface ILoginService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> ValidateTokenAsync(string token);
        Task LogoutAsync(string token);
    }
}
