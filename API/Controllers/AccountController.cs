using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
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
            if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
            
            // Brings in all user properties from Dto.
            var user = _mapper.Map<AppUser>(registerDto);
            
            // Gets username.
            user.UserName = registerDto.Username.ToLower();
            // Creates user and saves changes in database.
            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded) return BadRequest(result.Errors);
            // Add user role into member role.
            var roleResult = await _userManager.AddToRoleAsync(user, "Member");
            // Check if role result is successful.
            if (!roleResult.Succeeded) return BadRequest(result.Errors);

            // Returns UserDto that includes UserName and generated JWT token.
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
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
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());
            // Validates user is not null.
            if (user == null) return Unauthorized("Invalid Username");
            // Sign in user.
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded) return Unauthorized();

            // Returns UserDto that includes UserName and generated JWT token.
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        // Method to check database for matching username.
        private async Task<bool> UserExists(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}