using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserProfileService.Models;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Repo;

namespace UserProfileService.Consumers
{
    public class UserCreatedConsumer : IConsumer<UserCreatedEvent>
    {
        private readonly ILogger<UserCreatedConsumer> _logger;
        private readonly UserProfileDbContext _context;

        public UserCreatedConsumer(ILogger<UserCreatedConsumer> logger, UserProfileDbContext context)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context), "Db Context is null"); ;
        }

        public async Task Consume(ConsumeContext<UserCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation($"Received UserCreatedEvent: UserId={message.Id}, Login={message.Login}");

            var profile = new UserProfile
            {
                Id = message.Id,
                Email = message.Email,
                Login = message.Login,
                BirthDate = message.BirthDate
            };

            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            await Task.CompletedTask;
        }

    }
}