using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UserProfileService.Models
{
    public record UserCreatedEvent
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Login { get; init; } = string.Empty;
        public DateTime BirthDate { get; init; }
    }
}