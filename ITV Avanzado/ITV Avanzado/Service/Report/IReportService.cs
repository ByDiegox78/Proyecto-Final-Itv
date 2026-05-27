using CSharpFunctionalExtensions;
using GestionItv.Models;
using ITV_Avanzado.Error.Common;

namespace ITV_Avanzado.Service.Report;

public interface IReportService {
    Informe GenerarInformeCita(IEnumerable<Cita> citas);

    Result<string, DomainError> GenerarInformeCitaHtml(IEnumerable<Cita> citas, bool mostrarEliminados = false);

    Result<bool, DomainError> GuardarInformeHtml(string html, string fileName);
    
    Result<bool, DomainError> GuardarInforme(string html, string fileName);
    
    Result<bool, DomainError> GuardarInformePdf(string html, string fileName);
}