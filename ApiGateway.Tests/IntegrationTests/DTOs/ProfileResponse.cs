using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Tests.IntegrationTests.DTOs
{
    public class ProfileResponse
    {
        public required Guid Id { get; set; }
        public required string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public required DateTime BirthDate { get; set; }
        public string Bio { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}