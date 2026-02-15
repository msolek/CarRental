using Avalonia.Controls;
using CarManagement.ViewModels;

namespace CarManagement.Views;

public partial class CustomerDialog : Window
{
    public CustomerDialog()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is CustomerDialogViewModel vm)
            vm.RequestClose += () => Close(vm.Result);
    }
}
