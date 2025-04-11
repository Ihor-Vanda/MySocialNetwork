using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Models
{
    public class User : IdentityUser<Guid>
    {
        public DateTime BirthDate { get; set; }
    }
}