using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dto;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {        
        private readonly IAuthRepository _repo;

        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repo,
                              IConfiguration configuration,
                              IMapper mapper)
        {
            this._repo = repo;
            this._configuration = configuration;
            this._mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserDto userDTO)
        {
            userDTO.Username = userDTO.Username.ToLower();

            if (await this._repo.UserExists(userDTO.Username))
                return BadRequest("Username already exists");

            var user = new User
            {
                UserName = userDTO.Username
            };

            var createdUser = this._repo.Register(user, userDTO.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var userFromRepo = await this._repo.Login(loginDto.Username.ToLower(), loginDto.Password);

            if (userFromRepo == null)
                return Unauthorized();
            
            //Build a JWT token to return for the user (client)
            var claims = new []
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.UserName)
            };

            //Get the key from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.
                GetBytes(this._configuration.GetSection("AppSettings:Token").Value));

            //Signin credentials
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var user = this._mapper.Map<UserForListDto>(userFromRepo);

            return Ok
            (
                new
                {
                    token = tokenHandler.WriteToken(token),
                    user
                }
            );
        }
    }
}