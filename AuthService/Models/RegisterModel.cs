using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Models
{
    public class RegisterModel
    {
        public Guid Id { get; } = Guid.NewGuid();

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string Name { get; set; }

        public required DateTime BirthDate { get; set; }
    }
}