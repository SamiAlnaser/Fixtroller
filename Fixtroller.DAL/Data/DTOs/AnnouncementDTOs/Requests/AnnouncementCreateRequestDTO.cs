using Fixtroller.DAL.Entities.Announcements;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.AnnouncementDTOs.Requests
{
    public class AnnouncementCreateRequestDTO
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Url]
        public string? LinkUrl { get; set; }

        [Required]
        [Range(typeof(AnnouncementAudience), "1", "2",
             ErrorMessage = "Audience must be 1 or 2.")]
        public AnnouncementAudience Audience { get; set; }

        public List<IFormFile>? Images { get; set; }
    }
}
