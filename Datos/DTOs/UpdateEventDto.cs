using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.DTOs
{
    public class UpdateEventDto
    {
        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }

        [Required]
        public string Location { get; set; }

        public bool IsOnline { get; set; }

        public string? CoverImageUrl { get; set; }

        public int? MaxAttendees { get; set; }
    }
}
