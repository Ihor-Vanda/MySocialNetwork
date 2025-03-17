using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiGateway.Tests.IntegrationTests.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }
}