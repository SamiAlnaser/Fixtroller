using Fixtroller.DAL.Data.DTOs.ChangeRoleDTOs.Requests;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.UsersDTOs.Requset
{
    public sealed class AdminCreateUserRequestDTO
    {

        [Required]
        public string FullNameAr { get; set; } = string.Empty;
        [Required]
        public string? FullNameEn { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Department { get; set; }

        [Required,Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MinLength(6)]
        [RegularExpression(
                        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{6,}$",
                        ErrorMessage = "PasswordNotComplex")]
        public string Password { get; set; } = string.Empty;

        [Required]
        public RoleType Role { get; set; }
    }
}
