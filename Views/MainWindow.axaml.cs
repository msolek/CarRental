using Avalonia.Controls;
using Avalonia.Interactivity;
using CarManagement.ViewModels;

namespace CarManagement.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    // Loads initial data (cars, customers, rental history) once the window is ready
    private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            await vm.InitializeAsync();
    }
}
