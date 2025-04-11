using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserProfileService.Models
{
    public class UserProfile
    {
        [Key]
        public required Guid Id { get; set; }
        public required string Login { get; set; }
        public required string Email { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public required DateTime BirthDate { get; set; }
        public string? Bio { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; } = string.Empty;
    }
}