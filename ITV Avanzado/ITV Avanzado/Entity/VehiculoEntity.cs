using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITV_Avanzado.Entity;

[Table("Vehiculos")]
public class VehiculoEntity {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] [MaxLength(7)] public string Matricula { get; set; } = string.Empty;

    [Required] [MinLength(2)] public string Marca { get; set; } = string.Empty;
    
    [Required] public int Cilindrada { get; set; }
    
    [Required] public int Motor { get; set; }
    
    [Required] [MaxLength(8)] public string Dni { get; set; } = string.Empty;
    
    [Column(TypeName = "datetime2")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "datetime2")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

}