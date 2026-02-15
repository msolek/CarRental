using System.ComponentModel;
using System.Windows.Input;
using CarManagement.Models;

namespace CarManagement.ViewModels;

// ViewModel for renting a car - lets the user pick a customer and confirm the rental
public class RentalDialogViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Car Car { get; }
    public RentalHistory Rental { get; }
    public List<Customer> Customers { get; }

    public string CarInfo => $"{Car.Brand} {Car.Model} ({Car.Year})";

    public string CustomerName
    {
        get => Rental.CustomerName;
        set
        {
            if (Rental.CustomerName != value)
            {
                Rental.CustomerName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CustomerName)));
            }
        }
    }

    public string? CustomerPhone
    {
        get => Rental.CustomerPhone;
        set
        {
            if (Rental.CustomerPhone != value)
            {
                Rental.CustomerPhone = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CustomerPhone)));
            }
        }
    }

    public string? CustomerEmail
    {
        get => Rental.CustomerEmail;
        set
        {
            if (Rental.CustomerEmail != value)
            {
                Rental.CustomerEmail = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CustomerEmail)));
            }
        }
    }

    // When the user picks a customer from the dropdown, auto-fill their contact details
    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            _selectedCustomer = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCustomer)));

            if (_selectedCustomer != null)
            {
                CustomerName = _selectedCustomer.Name;
                CustomerPhone = _selectedCustomer.Phone;
                CustomerEmail = _selectedCustomer.Email;
            }
        }
    }

    public ICommand OkCommand { get; }
    public ICommand CancelCommand { get; }

    public RentalHistory? Result { get; private set; }

    public event Action? RequestClose;

    public RentalDialogViewModel(Car car, RentalHistory rental, List<Customer> customers)
    {
        Car = car;
        Rental = rental;
        Customers = customers;
        Rental.CarId = car.Id;
        Rental.RentalDate = DateTime.Now;

        if (Customers.Count > 0)
            SelectedCustomer = Customers[0];

        OkCommand = new RelayCommand(_ => { Result = Rental; RequestClose?.Invoke(); });
        CancelCommand = new RelayCommand(_ => { Result = null; RequestClose?.Invoke(); });
    }
}
