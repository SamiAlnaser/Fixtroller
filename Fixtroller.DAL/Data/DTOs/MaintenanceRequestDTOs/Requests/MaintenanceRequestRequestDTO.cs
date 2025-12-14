using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
using Fixtroller.DAL.Entities.ProblemTypeEntity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fixtroller.DAL.Data.DTOs.MaintenanceRequestDTOs.Requests
{
    public class MaintenanceRequestRequestDTO : IValidatableObject
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        public List<IFormFile>? Images { get; set; }


        [StringLength(300)]
        public string? Address { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        [Required]
        public Priority Priority { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int ProblemTypeId { get; set; }








        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasAddress = !string.IsNullOrWhiteSpace(Address);
            var hasLat = !string.IsNullOrWhiteSpace(Latitude);
            var hasLng = !string.IsNullOrWhiteSpace(Longitude);

            // لازم يا Address يا (Lat+Lng)
            if (!hasAddress && !(hasLat && hasLng))
            {
                yield return new ValidationResult(
                    "You must provide either Address or both Latitude and Longitude.",
                    new[] { nameof(Address), nameof(Latitude), nameof(Longitude) });
                yield break;
            }

            // ممنوع واحد لحاله
            if (hasLat ^ hasLng)
            {
                yield return new ValidationResult(
                    "Latitude and Longitude must be provided together.",
                    new[] { nameof(Latitude), nameof(Longitude) });
                yield break;
            }

            // لو موجودين: parse + range
            if (hasLat && hasLng)
            {
                if (!TryParseInvariant(Latitude!, out var lat) || lat < -90 || lat > 90)
                    yield return new ValidationResult("Latitude is invalid.", new[] { nameof(Latitude) });

                if (!TryParseInvariant(Longitude!, out var lng) || lng < -180 || lng > 180)
                    yield return new ValidationResult("Longitude is invalid.", new[] { nameof(Longitude) });
            }
        }

        private static bool TryParseInvariant(string s, out decimal value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            var normalized = s.Trim().Replace("،", ",").Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }


    }
}