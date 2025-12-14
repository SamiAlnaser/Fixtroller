using Fixtroller.DAL.Entities.MaintenanceRequestEntity;
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
    public class MaintenanceRequestUpdateDTO : IValidatableObject
    {
        [StringLength(150)]
        public string? Title { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        public string? Latitude { get; set; }
        public string? Longitude { get; set; }

        public Priority? Priority { get; set; }

        [Range(1, int.MaxValue)]
        public int? ProblemTypeId { get; set; }

        public List<IFormFile>? NewImages { get; set; }

        public List<int>? RemoveImageIds { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var hasLat = !string.IsNullOrWhiteSpace(Latitude);
            var hasLng = !string.IsNullOrWhiteSpace(Longitude);

            if (hasLat ^ hasLng)
            {
                yield return new ValidationResult(
                    "Latitude and Longitude must be provided together.",
                    new[] { nameof(Latitude), nameof(Longitude) });
                yield break;
            }

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
