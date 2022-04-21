using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using T_FANCY_Back.Models;

namespace T_FANCY_Back.Services
{
    public class JwtService : IJwtService
    {
        private readonly TfancyContext _context;
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> userManager;
        public JwtService(TfancyContext context, IConfiguration configuration, UserManager<User> userManager)
        {
            this.userManager = userManager;

            _context = context;
            _configuration = configuration;
        }

        //Generate new token and refreshtoken
        public async Task<AuthenticationResponse> GetTokensAsync(string ipAddess, User user)
        {

            var roles = await userManager.GetRolesAsync(user);
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateToken(user.Email,roles);
            return await SaveTokenDetails(ipAddess, user, accessToken, refreshToken);

        }

        //Save new token in database
        private async Task<AuthenticationResponse> SaveTokenDetails(string ipAddress, User user, string tokenString, string refreshToken)
        {
            var userRefreshToken = new UserRefreshToken
            {
                CreatedDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddDays(7),
                IpAddress = ipAddress,
                IsInvalidated = false,
                RefreshToken = refreshToken,
                Token = tokenString,
                user = user
            };
            await _context.userRefreshToken.AddAsync(userRefreshToken);
            await _context.SaveChangesAsync();
            return new AuthenticationResponse { token= tokenString, refreshtoken= refreshToken,IsSuccess = true
            };

        }

        //Generate refreshtoken
        private string GenerateRefreshToken()
        {
            var byteArray = new byte[64];
            using (var cryptoProvider= new RNGCryptoServiceProvider())
               {
                cryptoProvider.GetBytes(byteArray);
                return Convert.ToBase64String(byteArray);
            } 
        }
        //Generate token
          private string GenerateToken(String Email,IList<string> roles)
            {
            var jwtkey = _configuration.GetValue<string>("JwtSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(jwtkey);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new[]
                {
                  new Claim(ClaimTypes.Email, Email),         
                  new Claim(JwtRegisteredClaimNames.Email,Email)
                };

            var claimsWithRoles = roles.Select(role => new Claim(ClaimTypes.Role, role));
            var allClaims = claims.Concat(claimsWithRoles);


            var descriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(allClaims),
                Expires = DateTime.UtcNow.AddSeconds(90),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
               SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(descriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        //validate token
        public async Task<bool> IsTokenValid(string accessToken, string ipAddress)
        {
            var isValid= _context.userRefreshToken.FirstOrDefault(x => x.Token == accessToken && x.IpAddress == ipAddress) != null;
            return await Task.FromResult(isValid);
                 }
    }
}
    

