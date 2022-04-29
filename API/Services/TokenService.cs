using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using API.Entities;
using API.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;

namespace API.Services
{
    // Summary:
    // Implements TokenService interface for the creation and distribution of json web tokens.
    public class TokenService : ITokenService
    {
        // Implements encryption that utilizes the same key to sign JWT token and verify signature.
        private readonly SymmetricSecurityKey _key;
        // Creates new encryption key.
        public TokenService(IConfiguration config)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
        }
        // Summary:
        // Method for creation of JWT tokens
        public string CreateToken(AppUser user)
        {
            // Creates new claim for JWT token storing username under NameId.
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.UserName)
            };
            // Creates new credentials with encryption key and security algorithm.
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
            // Describes token with subject(included claims), expiration, and signing credentials.
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            // Creates token handler.
            var tokenHandler = new JwtSecurityTokenHandler();
            // Generate token.
            var token = tokenHandler.CreateToken(tokenDescriptor);
            // Return generated token.
            return tokenHandler.WriteToken(token);
        }
    }
}