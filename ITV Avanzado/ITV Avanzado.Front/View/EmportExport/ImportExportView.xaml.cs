using System.Windows;
using System.Windows.Controls;
using ITV_Avanzado.Front.ViewModels.ImportExport;
using Microsoft.Extensions.DependencyInjection;

namespace ITV_Avanzado.Front.View.EmportExport;

public partial class ImportExportView : Page {
    public ImportExportView() {
        InitializeComponent();
        DataContext = App.Services.GetRequiredService<ImportExportViewModel>();
    }
}