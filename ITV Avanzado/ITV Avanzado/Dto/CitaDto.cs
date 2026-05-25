using System.Xml.Serialization;

namespace ITV_Avanzado.Dto;
/// <summary>
///     Objeto de Transferencia de Datos para citas.
///     Se utiliza para y deserialización de datos de citas
/// </summary>
[XmlRoot("Cita")]
[XmlType("CitaDto")]
public record CitaDto(
    [property: XmlElement("Id")] int Id,
    [property: XmlElement("Matricula")] string Matricula,
    [property: XmlElement("Marca")] string Marca,
    [property: XmlElement("Cilindrada")] int Cilindrada,
    [property: XmlElement("TipoMotor")] string TipoMotor,
    [property: XmlElement("DniPropietario")] string DniPropietario,
    [property: XmlElement("FechaMatriculacion")] string FechaMatriculacion,
    [property: XmlElement("FechaInspeccion")] string FechaInspeccion,
    [property: XmlElement("IsDelete")] bool IsDelete,
    [property: XmlElement("CreatedAt")] string CreatedAt,
    [property: XmlElement("UpdatedAt")] string UpdatedAt
) {
    public CitaDto() : this(
        0,          
        "",        
        "",
        0,          
        "",        
        "", 
        "",
        "",
        false,      
        "",         
        ""          
    )
    { }
}
