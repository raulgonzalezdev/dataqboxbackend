using System.ComponentModel.DataAnnotations;

namespace dataqboxbackend.Models
{
    public class Usuario
    {
        [Required] [Key]
        public string Cod_Usuario { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public bool? Updates { get; set; }
        public bool? Addnews { get; set; }
        public bool? Deletes { get; set; }
        public bool? Creador { get; set; }
        public bool? Cambiar { get; set; }
        public bool? PrecioMinimo { get; set; }
        public bool? Credito { get; set; }
    }
}
