using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITV_Avanzado.Entity;
/// <summary>
///     Entidad de base de datos para citas.
/// </summary>
[Table("Citas")]
public class CitaEntity {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] [MaxLength(7)] public string Matricula { get; set; } = string.Empty;

    [Required] [MinLength(2)] public string Marca { get; set; } = string.Empty;
    
    [Required] public int Cilindrada { get; set; }
    
    [Required] public int Motor { get; set; }
    
    [Required] [MaxLength(9)] public string Dni { get; set; } = string.Empty;
    
    [Required] public DateTime FechaMatriculacion { get; set; } = DateTime.UtcNow;

    [Required] public DateTime FechaInspeccion { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

}