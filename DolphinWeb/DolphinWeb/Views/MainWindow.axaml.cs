using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace DolphinWeb.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        if (ActualThemeVariant == ThemeVariant.Dark)
            ExperimentalAcrylicBorder.Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintOpacity = 1,
                TintColor = Colors.Black,
                MaterialOpacity = 0.65
            };
        else
            ExperimentalAcrylicBorder.Material = new ExperimentalAcrylicMaterial
            {
                BackgroundSource = AcrylicBackgroundSource.Digger,
                TintOpacity = 1,
                TintColor = Colors.WhiteSmoke,
                MaterialOpacity = 0.65
            };
    }
}