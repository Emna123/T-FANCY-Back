using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using T_FANCY_Back.Models;

namespace T_FANCY_Back.Services
{
    public interface IJwtService
    {
        Task<AuthenticationResponse> GetTokensAsync(string ipAddess, User user);
       Task<bool> IsTokenValid(string accessToken, string ipAddress);

    }
}
