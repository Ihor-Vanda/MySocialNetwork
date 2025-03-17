using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostService.DTOs
{
    public class PostUpdateDTO
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }

        public string? MediaUrl { get; set; }
    }
}