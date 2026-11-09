using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Core.DTOs;
using RaceDay.Infrastructure.Data;
using RaceDay.Infrastructure.Services;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthServiceTests
    {
        // ---- Success case: Register a Participant ----
        [Fact]
        public async Task Register_Participant_Succeeds()
        {
            // Arrange
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            RegisterDto dto = new RegisterDto();
            dto.Email = "test@test.com";
            dto.Password = "Password123!";
            dto.FullName = "Test User";
            dto.PhoneNumber = "0821234567";
            dto.Role = "Participant";

            // Act
            AuthResponseDto result = await service.RegisterAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Token);
            Assert.Equal("Participant", result.User.Role);
            Assert.Equal("test@test.com", result.User.Email);

            int userCount = await context.Users.CountAsync();
            Assert.Equal(1, userCount);
        }

        // ---- Success case: Register an Organiser ----
        [Fact]
        public async Task Register_Organiser_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            RegisterDto dto = new RegisterDto();
            dto.Email = "organiser@test.com";
            dto.Password = "Password123!";
            dto.FullName = "Test Organiser";
            dto.PhoneNumber = "0821234567";
            dto.Role = "Organiser";

            AuthResponseDto result = await service.RegisterAsync(dto);

            Assert.NotNull(result.Token);
            Assert.Equal("Organiser", result.User.Role);

            int organiserCount = await context.Organisers.CountAsync();
            Assert.Equal(1, organiserCount);
        }

        // ---- Failure case: Duplicate email ----
        [Fact]
        public async Task Register_DuplicateEmail_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            RegisterDto dto = new RegisterDto();
            dto.Email = "dup@test.com";
            dto.Password = "Password123!";
            dto.FullName = "First User";
            dto.PhoneNumber = "0821234567";
            dto.Role = "Participant";

            await service.RegisterAsync(dto);

            // Try again with same email
            RegisterDto dto2 = new RegisterDto();
            dto2.Email = "dup@test.com";
            dto2.Password = "DifferentPassword!";
            dto2.FullName = "Second User";
            dto2.PhoneNumber = "0829999999";
            dto2.Role = "Participant";

            // Assert
            await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(dto2));
        }

        // ---- Failure case: Invalid role ----
        [Fact]
        public async Task Register_InvalidRole_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            RegisterDto dto = new RegisterDto();
            dto.Email = "bad@test.com";
            dto.Password = "Password123!";
            dto.FullName = "Bad Role User";
            dto.PhoneNumber = "0821234567";
            dto.Role = "Admin";

            await Assert.ThrowsAsync<Exception>(() => service.RegisterAsync(dto));
        }

        // ---- Success case: Login with correct password ----
        [Fact]
        public async Task Login_CorrectCredentials_Succeeds()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            RegisterDto dto = new RegisterDto();
            dto.Email = "login@test.com";
            dto.Password = "Password123!";
            dto.FullName = "Login User";
            dto.PhoneNumber = "0821234567";
            dto.Role = "Participant";

            await service.RegisterAsync(dto);

            LoginDto login = new LoginDto();
            login.Email = "login@test.com";
            login.Password = "Password123!";

            AuthResponseDto result = await service.LoginAsync(login);

            Assert.NotNull(result.Token);
            Assert.Equal("login@test.com", result.User.Email);
        }

        // ---- Failure case: Login with wrong password ----
        [Fact]
        public async Task Login_WrongPassword_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            RegisterDto dto = new RegisterDto();
            dto.Email = "wrongpass@test.com";
            dto.Password = "Password123!";
            dto.FullName = "Test";
            dto.PhoneNumber = "0821234567";
            dto.Role = "Participant";

            await service.RegisterAsync(dto);

            LoginDto login = new LoginDto();
            login.Email = "wrongpass@test.com";
            login.Password = "WrongPassword";

            await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(login));
        }

        // ---- Failure case: Login with unknown email ----
        [Fact]
        public async Task Login_UnknownEmail_ThrowsException()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            LoginDto login = new LoginDto();
            login.Email = "doesnotexist@test.com";
            login.Password = "Password123!";

            await Assert.ThrowsAsync<Exception>(() => service.LoginAsync(login));
        }

        // ---- Security: Password is hashed, not stored in plain text ----
        [Fact]
        public async Task Register_PasswordIsHashed_NotStoredPlain()
        {
            RaceDayDbContext context = TestHelper.GetInMemoryDbContext();
            IConfiguration config = TestHelper.GetConfiguration();
            AuthService service = new AuthService(context, config);

            RegisterDto dto = new RegisterDto();
            dto.Email = "hash@test.com";
            dto.Password = "PlainPassword";
            dto.FullName = "Hash Test";
            dto.PhoneNumber = "0821234567";
            dto.Role = "Participant";

            await service.RegisterAsync(dto);

            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "hash@test.com");
            Assert.NotNull(user);
            Assert.NotEqual("PlainPassword", user.PasswordHash);
            // BCrypt hashes start with $2
            Assert.StartsWith("$2", user.PasswordHash);
        }
    }
}