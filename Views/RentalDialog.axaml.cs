using Avalonia.Controls;
using Avalonia.Interactivity;
using CarManagement.ViewModels;

namespace CarManagement.Views;

public partial class RentalDialog : Window
{
    public RentalDialog()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContextChanged += OnDataContextChanged;
    }

    // Pre-select today's date in the picker once the dialog is ready
    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is RentalDialogViewModel vm && RentalDatePicker != null)
            RentalDatePicker.SelectedDate = new DateTimeOffset(vm.Rental.RentalDate);
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is RentalDialogViewModel vm)
            vm.RequestClose += () => Close(vm.Result);
    }

    // Sync the date picker value back to the rental model
    private void RentalDatePicker_SelectedDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
    {
        if (DataContext is RentalDialogViewModel vm && e.NewDate.HasValue)
            vm.Rental.RentalDate = e.NewDate.Value.DateTime;
    }
}
