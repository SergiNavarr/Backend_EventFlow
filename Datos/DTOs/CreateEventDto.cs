using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class CreateEventDto
    {
        [Required(ErrorMessage = "El título es obligatorio")]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        [Required(ErrorMessage = "La ubicación o URL es obligatoria")]
        public string Location { get; set; }

        public bool IsOnline { get; set; } = false;

        public string? CoverImageUrl { get; set; }

        public int? MaxAttendees { get; set; }

        public int? CommunityId { get; set; }
    }
}
