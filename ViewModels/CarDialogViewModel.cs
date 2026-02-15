using System.Windows.Input;
using CarManagement.Models;

namespace CarManagement.ViewModels;

// ViewModel for the add/edit car dialog - holds the car being edited and dialog result
public class CarDialogViewModel
{
    public Car Car { get; }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public Car? Result { get; private set; }

    public event Action? RequestClose;

    public CarDialogViewModel(Car car)
    {
        Car = car;
        OkCommand = new RelayCommand(_ => { Result = Car; RequestClose?.Invoke(); });
        CancelCommand = new RelayCommand(_ => { Result = null; RequestClose?.Invoke(); });
    }
}
