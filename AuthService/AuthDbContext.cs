using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService
{
    public class AuthDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

    }
}