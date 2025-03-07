using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PostService.Repo;

namespace PostService.Tests
{
    public static class InMemoryPostDbContextFactory
    {
        public static PostDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<PostDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new PostDbContext(options);
        }
    }
}