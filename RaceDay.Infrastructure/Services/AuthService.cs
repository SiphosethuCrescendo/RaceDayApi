using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Core.DTOs;
using RaceDay.Core.Entities;
using RaceDay.Core.Interfaces;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(RaceDayDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (dto.Role != "Organiser" && dto.Role != "Participant")
            {
                throw new Exception("Role must be Organiser or Participant");
            }

            User existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);


            if (existingUser != null)
            {
                throw new Exception("Email is already registered");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            User newUser = new User();
            newUser.Email = dto.Email;
            newUser.PasswordHash = hashedPassword;
            newUser.FullName = dto.FullName;
            newUser.PhoneNumber = dto.PhoneNumber;
            newUser.Role = dto.Role;
            newUser.CreatedAt = DateTime.Now;
            newUser.IsActive = true;

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            if (dto.Role == "Organiser")
            {
                Organiser organiser = new Organiser();
                organiser.OrganiserID = newUser.UserID;
                organiser.CompanyName = dto.FullName;
                organiser.CompanyPhone = "";
                organiser.ApprovedEvents = 0;
                _context.Organisers.Add(organiser);
            }
            else
            {
                Participant participant = new Participant();
                participant.ParticipantID = newUser.UserID;
                _context.Participants.Add(participant);
            }

            await _context.SaveChangesAsync();

            string token = CreateToken(newUser);

            UserDto userInfo = new UserDto();
            userInfo.UserID = newUser.UserID;
            userInfo.Email = newUser.Email;
            userInfo.FullName = newUser.FullName;
            userInfo.PhoneNumber = newUser.PhoneNumber;
            userInfo.Role = newUser.Role;

            AuthResponseDto response = new AuthResponseDto();
            response.Token = token;
            response.User = userInfo;

            return response;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            User user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);


            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }

            bool passwordCorrect = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (passwordCorrect == false)
            {
                throw new Exception("Invalid email or password");
            }

            if (user.IsActive == false)
            {
                throw new Exception("This account is not active");
            }

            string token = CreateToken(user);

            UserDto userInfo = new UserDto();
            userInfo.UserID = user.UserID;
            userInfo.Email = user.Email;
            userInfo.FullName = user.FullName;
            userInfo.PhoneNumber = user.PhoneNumber;
            userInfo.Role = user.Role;

            AuthResponseDto response = new AuthResponseDto();
            response.Token = token;
            response.User = userInfo;

            return response;
        }

        private string CreateToken(User user)
        {
            // Safely read the config values
            string key = _config["Jwt:Key"] ?? "";
            string issuer = _config["Jwt:Issuer"] ?? "";
            string audience = _config["Jwt:Audience"] ?? "";

            // Make sure the key is long enough
            if (key.Length < 32)
            {
                throw new Exception("JWT Key must be at least 32 characters long");
            }

            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role)
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return tokenString;
        }
    }
}