using Avalonia.Controls;
using CarManagement.ViewModels;

namespace CarManagement.Views;

public partial class CarDialog : Window
{
    public CarDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    // Wire up the close handler whenever a new ViewModel is assigned
    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is CarDialogViewModel vm)
            vm.RequestClose += () => Close(vm.Result);
    }
}
