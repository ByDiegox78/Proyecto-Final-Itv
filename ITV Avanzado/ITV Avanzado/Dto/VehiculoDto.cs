using System.Xml.Serialization;

namespace ITV_Avanzado.Dto;

[XmlRoot("Vehiculo")]
[XmlType("VehiculoDto")]
public record VehiculoDto(
    [property: XmlElement("Id")] int Id,
    [property: XmlElement("Matricula")] string Matricula,
    [property: XmlElement("Marca")] string Marca,
    [property: XmlElement("Cilindrada")] int Cilindrada,
    [property: XmlElement("TipoMotor")] string TipoMotor,
    [property: XmlElement("DniPropietario")]
    string DniPropietario,
    [property: XmlElement("IsDelete")] bool IsDelete,
    [property: XmlElement("CreatedAt")] string CreatedAt,
    [property: XmlElement("UpdatedAt")] string UpdatedAt
) {
    public VehiculoDto() : this(
        0,          
        "",        
        "",
        0,          
        "",        
        "",         
        false,      
        "",         
        ""          
    )
    { }
}
