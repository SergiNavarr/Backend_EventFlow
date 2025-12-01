using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datos.Models
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        // Borrado Lógico: Si es false, el dato "no existe" para el usuario
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
