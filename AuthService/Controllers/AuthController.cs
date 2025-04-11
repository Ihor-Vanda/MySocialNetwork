using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AuthService.Models;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        protected readonly IConfiguration _configuration;
        private readonly IBus _bus;

        public AuthController(UserManager<User> userManager,
                                SignInManager<User> signInManager,
                                IConfiguration configuration,
                                IBus bus)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _bus = bus;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User
            {
                Id = model.Id,
                UserName = model.Login,
                Email = model.Email,
                BirthDate = model.BirthDate
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _bus.Publish(new UserCreatedEvent
                {
                    Id = user.Id,
                    Email = user.Email,
                    Login = user.UserName,
                    BirthDate = user.BirthDate,
                });

                return CreatedAtAction(nameof(Register), new { id = model.Id }, model);
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration["Jwt:SecretKey"] ?? throw new Exception("Jwt secret key is not configured");
            var key = Encoding.ASCII.GetBytes(secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? throw new Exception("Invalid user Email")),
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["Jwt:ExpiryInHours"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new { Token = tokenString });
        }
    }
}