using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public class Comment : BaseEntity
    {
        [Required]
        public string Content { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int AuthorId { get; set; }
        public User Author { get; set; } = null!;
    }
}
