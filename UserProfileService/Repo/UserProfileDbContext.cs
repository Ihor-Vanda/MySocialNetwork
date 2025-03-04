using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Models;

namespace UserProfileService.Repo
{
    public class UserProfileDbContext : DbContext
    {
        public UserProfileDbContext(DbContextOptions<UserProfileDbContext> options) : base(options) { }

        public DbSet<UserProfile> UserProfiles { get; set; } = default!;
    }
}