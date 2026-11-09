using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDay.Infrastructure.Data;

namespace RaceDay.Tests
{
    // Helper class to create a fresh in-memory database and config for each test
    public static class TestHelper
    {
        public static RaceDayDbContext GetInMemoryDbContext()
        {
            DbContextOptions<RaceDayDbContext> options =
                new DbContextOptionsBuilder<RaceDayDbContext>()
                    .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                    .Options;

            return new RaceDayDbContext(options);
        }

        public static IConfiguration GetConfiguration()
        {
            Dictionary<string, string?> settings = new Dictionary<string, string?>();
            settings.Add("Jwt:Key", "TestKeyThatIsAtLeast32CharactersLongForTesting!");
            settings.Add("Jwt:Issuer", "TestIssuer");
            settings.Add("Jwt:Audience", "TestAudience");

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .AddInMemoryCollection(settings);

            return builder.Build();
        }
    }
}