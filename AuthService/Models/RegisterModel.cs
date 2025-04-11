using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Models
{
    public class RegisterModel
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string Login { get; set; }

        public required DateTime BirthDate { get; set; }
    }
}