using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Repo;

namespace UserProfileService.Tests
{
    public static class InMemoryDbContextFactory
    {
        public static UserProfileDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<UserProfileDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new UserProfileDbContext(options);
        }
    }
}