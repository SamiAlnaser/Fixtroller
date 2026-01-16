using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.Authentication.Requests
{
    public class RegisterRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(20)]
        public string UserName { get; set; }

        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string FullNameAr { get; set; }

        // 👇 الاسم الإنجليزي
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string FullNameEn { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string Location { get; set; }

    }
}
