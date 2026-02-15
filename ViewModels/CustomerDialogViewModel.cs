using System.Windows.Input;
using CarManagement.Models;

namespace CarManagement.ViewModels;

// ViewModel for the add/edit customer dialog
public class CustomerDialogViewModel
{
    public Customer Customer { get; }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public Customer? Result { get; private set; }

    public event Action? RequestClose;

    public CustomerDialogViewModel(Customer customer)
    {
        Customer = customer;
        OkCommand = new RelayCommand(_ => { Result = Customer; RequestClose?.Invoke(); });
        CancelCommand = new RelayCommand(_ => { Result = null; RequestClose?.Invoke(); });
    }
}
