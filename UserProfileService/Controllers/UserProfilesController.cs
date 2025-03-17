using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserProfileService.Models;
using UserProfileService.Repo;

namespace UserProfileService.Controllers
{
    [ApiController]
    [Route("profiles")]
    public class UserProfilesController : ControllerBase
    {
        private readonly UserProfileDbContext _context;

        public UserProfilesController(UserProfileDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var profile = await _context.UserProfiles.FindAsync(id);
            if (profile == null) { return NotFound(); }

            return Ok(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserProfile profile)
        {
            if (profile == null) { return BadRequest(); }
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = profile.Id }, profile);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserProfileDTO updatedProfile)
        {
            var existingProfile = await _context.UserProfiles.FindAsync(id);
            if (existingProfile == null)
                return NotFound();

            existingProfile.FirstName = updatedProfile.FirstName;
            existingProfile.LastName = updatedProfile.LastName;
            existingProfile.BirthDate = updatedProfile.BirthDate;
            existingProfile.Bio = updatedProfile.Bio;
            existingProfile.ProfilePictureUrl = updatedProfile.ProfilePictureUrl;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var profile = await _context.UserProfiles.FindAsync(id);
            if (profile == null)
                return NotFound();

            _context.UserProfiles.Remove(profile);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}