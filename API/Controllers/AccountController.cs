using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            _mapper = mapper;
            _tokenService = tokenService;
            _context = context;
        }
        //  Summary:
        //  Method for registering new user.
        //
        //  Parameters:
        //      registerDto:
        //      Pulls in string UserName and Password from RegisterDto 
        //      function to register user.  
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {   
            // Checks for existing user.
            if (await UserExists(registerDto.UserName)) return BadRequest("Username is taken");
            
            // Brings in all user properties from Dto.
            var user = _mapper.Map<AppUser>(registerDto);
            
            // Provides hashing algorithms for PasswordHash and PasswordSalt.
            using var hmac = new HMACSHA512();

            // Creates new application user.
            user.UserName = registerDto.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            user.PasswordSalt = hmac.Key;

            // Tracks new user in entity framework.
            _context.Users.Add(user);
            // Adds new user to database.
            await _context.SaveChangesAsync();

            // Returns UserDto that includes UserName and generated JWT token.
            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }

        //  Summary:
        //  Method to allow user to login.
        //
        //  Parameters:
        //      registerDto:
        //      Pulls in string UserName and Password from loginDto 
        //      function to validate username and password and then
        //      allow user to login.
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            // Checks for user in database.
            var user = await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);
            // Validates user is not null.
            if (user == null) return Unauthorized("Invalid Username");
            // Provides hashing algorithms for PasswordSalt.
            using var hmac = new HMACSHA512(user.PasswordSalt);
            // Creates computedHash for login password
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
            // compares computedHash to stored user PasswordHash to validate.
            for (int i = 0; i < computedHash.Length; i++)
            {
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            // Returns UserDto that includes UserName and generated JWT token.
            return new UserDto
            {
                UserName = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs
            };
        }
        // Method to check database for matching username.
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}