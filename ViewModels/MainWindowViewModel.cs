using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CarManagement.Data;
using CarManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace CarManagement.ViewModels;

// Central ViewModel that manages all three tabs: Cars, Rental History, and Customers
public class MainWindowViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private readonly CarRentalContext _db;

    public ObservableCollection<Car> Cars { get; }
    public ObservableCollection<RentalHistory> RentalHistories { get; }
    public ObservableCollection<Customer> Customers { get; }

    private Car? _selectedCar;
    public Car? SelectedCar
    {
        get => _selectedCar;
        set
        {
            if (_selectedCar == value) return;
            _selectedCar = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCar)));
            // Refresh button states since they depend on which car is selected
            EditCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            RentCommand.RaiseCanExecuteChanged();
            ReturnCommand.RaiseCanExecuteChanged();
        }
    }

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            if (_selectedCustomer == value) return;
            _selectedCustomer = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedCustomer)));
            EditCustomerCommand.RaiseCanExecuteChanged();
            DeleteCustomerCommand.RaiseCanExecuteChanged();
        }
    }

    public ICommand AddCommand { get; }
    public RelayCommand EditCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand RentCommand { get; }
    public RelayCommand ReturnCommand { get; }
    public ICommand ViewRentalsCommand { get; }
    public ICommand AddCustomerCommand { get; }
    public RelayCommand EditCustomerCommand { get; }
    public RelayCommand DeleteCustomerCommand { get; }

    public MainWindowViewModel()
    {
        _db = new CarRentalContext();

        Cars = new ObservableCollection<Car>();
        RentalHistories = new ObservableCollection<RentalHistory>();
        Customers = new ObservableCollection<Customer>();

        AddCommand = new RelayCommand(async _ => await AddCar());
        EditCommand = new RelayCommand(async _ => await EditCar(), _ => SelectedCar != null);
        DeleteCommand = new RelayCommand(async _ => await DeleteCar(), _ => SelectedCar != null);
        RentCommand = new RelayCommand(async _ => await RentCar(), _ => SelectedCar is { IsAvailable: true });
        ReturnCommand = new RelayCommand(async _ => await ReturnCar(), _ => SelectedCar is { IsAvailable: false });
        ViewRentalsCommand = new RelayCommand(async _ => await LoadRentalHistoriesAsync());
        AddCustomerCommand = new RelayCommand(async _ => await AddCustomer());
        EditCustomerCommand = new RelayCommand(async _ => await EditCustomer(), _ => SelectedCustomer != null);
        DeleteCustomerCommand = new RelayCommand(async _ => await DeleteCustomer(), _ => SelectedCustomer != null);
    }

    // Called once on window load to set up the database and populate all grids
    public async Task InitializeAsync()
    {
        await _db.Database.EnsureCreatedAsync();
        await DatabaseSeeder.SeedAsync(_db);
        await LoadCarsAsync();
        await LoadRentalHistoriesAsync();
        await LoadCustomersAsync();
    }

    private async Task LoadCarsAsync()
    {
        var list = await _db.Cars.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Cars.Clear();
            foreach (var c in list)
                Cars.Add(c);
        });
    }

    private async Task LoadRentalHistoriesAsync()
    {
        var list = await _db.RentalHistories
            .Include(r => r.Car)
            .AsNoTracking()
            .OrderByDescending(r => r.RentalDate)
            .ToListAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            RentalHistories.Clear();
            foreach (var r in list)
                RentalHistories.Add(r);
        });
    }

    private async Task LoadCustomersAsync()
    {
        var list = await _db.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Customers.Clear();
            foreach (var c in list)
                Customers.Add(c);
        });
    }

    private async Task AddCar()
    {
        var vm = new CarDialogViewModel(new Car { Year = DateTime.Now.Year, RentalPricePerDay = 50.00m });
        var dialog = new Views.CarDialog { DataContext = vm };
        var window = GetMainWindow();
        if (window == null) return;

        var result = await dialog.ShowDialog<Car?>(window);
        if (result == null) return;

        await _db.Cars.AddAsync(result);
        await _db.SaveChangesAsync();
        await Dispatcher.UIThread.InvokeAsync(() => Cars.Add(result));
    }

    private async Task EditCar()
    {
        if (SelectedCar == null) return;

        // Clone the car so we don't modify the original until the user confirms
        var clone = new Car
        {
            Id = SelectedCar.Id,
            Brand = SelectedCar.Brand,
            Model = SelectedCar.Model,
            Year = SelectedCar.Year,
            RentalPricePerDay = SelectedCar.RentalPricePerDay,
            IsAvailable = SelectedCar.IsAvailable
        };

        var vm = new CarDialogViewModel(clone);
        var dialog = new Views.CarDialog { DataContext = vm };
        var window = GetMainWindow();
        if (window == null) return;

        var result = await dialog.ShowDialog<Car?>(window);
        if (result == null) return;

        // Detach any tracked entity with the same ID to avoid conflicts
        var existing = _db.ChangeTracker.Entries<Car>().FirstOrDefault(e => e.Entity.Id == result.Id);
        if (existing != null)
            existing.State = EntityState.Detached;

        _db.Cars.Update(result);
        await _db.SaveChangesAsync();
        await LoadCarsAsync();
    }

    private async Task DeleteCar()
    {
        if (SelectedCar == null) return;

        // Find the tracked entity to avoid conflicts with untracked copies from AsNoTracking queries
        var carToDelete = await _db.Cars.FindAsync(SelectedCar.Id);
        if (carToDelete == null) return;

        _db.Cars.Remove(carToDelete);
        await _db.SaveChangesAsync();
        await Dispatcher.UIThread.InvokeAsync(() => Cars.Remove(SelectedCar));
    }

    private async Task RentCar()
    {
        if (SelectedCar == null || !SelectedCar.IsAvailable) return;

        var customers = await _db.Customers.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        if (customers.Count == 0) return;

        var rental = new RentalHistory();
        var vm = new RentalDialogViewModel(SelectedCar, rental, customers);
        var dialog = new Views.RentalDialog { DataContext = vm };
        var window = GetMainWindow();
        if (window == null) return;

        var result = await dialog.ShowDialog<RentalHistory?>(window);
        if (result == null) return;

        _db.ChangeTracker.Clear();
        await _db.RentalHistories.AddAsync(result);

        // Mark the car as no longer available
        SelectedCar.IsAvailable = false;
        _db.Cars.Update(SelectedCar);
        await _db.SaveChangesAsync();

        await LoadCarsAsync();
        await LoadRentalHistoriesAsync();

        RentCommand.RaiseCanExecuteChanged();
        ReturnCommand.RaiseCanExecuteChanged();
    }

    private async Task ReturnCar()
    {
        if (SelectedCar == null || SelectedCar.IsAvailable) return;

        _db.ChangeTracker.Clear();

        var activeRental = await _db.RentalHistories
            .FirstOrDefaultAsync(r => r.CarId == SelectedCar.Id && !r.IsReturned);

        if (activeRental == null) return;

        activeRental.ReturnDate = DateTime.Now;
        activeRental.IsReturned = true;

        var carToUpdate = await _db.Cars.FindAsync(SelectedCar.Id);
        if (carToUpdate != null)
            carToUpdate.IsAvailable = true;

        await _db.SaveChangesAsync();

        await LoadCarsAsync();
        await LoadRentalHistoriesAsync();

        RentCommand.RaiseCanExecuteChanged();
        ReturnCommand.RaiseCanExecuteChanged();
    }

    private async Task AddCustomer()
    {
        var vm = new CustomerDialogViewModel(new Customer());
        var dialog = new Views.CustomerDialog { DataContext = vm };
        var window = GetMainWindow();
        if (window == null) return;

        var result = await dialog.ShowDialog<Customer?>(window);
        if (result == null || string.IsNullOrWhiteSpace(result.Name)) return;

        await _db.Customers.AddAsync(result);
        await _db.SaveChangesAsync();
        await LoadCustomersAsync();
    }

    private async Task EditCustomer()
    {
        if (SelectedCustomer == null) return;

        var clone = new Customer
        {
            Id = SelectedCustomer.Id,
            Name = SelectedCustomer.Name,
            Phone = SelectedCustomer.Phone,
            Email = SelectedCustomer.Email,
            Address = SelectedCustomer.Address,
            Notes = SelectedCustomer.Notes
        };

        var vm = new CustomerDialogViewModel(clone);
        var dialog = new Views.CustomerDialog { DataContext = vm };
        var window = GetMainWindow();
        if (window == null) return;

        var result = await dialog.ShowDialog<Customer?>(window);
        if (result == null || string.IsNullOrWhiteSpace(result.Name)) return;

        var existing = _db.ChangeTracker.Entries<Customer>().FirstOrDefault(e => e.Entity.Id == result.Id);
        if (existing != null)
            existing.State = EntityState.Detached;

        _db.Customers.Update(result);
        await _db.SaveChangesAsync();
        await LoadCustomersAsync();
    }

    private async Task DeleteCustomer()
    {
        if (SelectedCustomer == null) return;

        var customerToDelete = await _db.Customers.FindAsync(SelectedCustomer.Id);
        if (customerToDelete == null) return;

        _db.Customers.Remove(customerToDelete);
        await _db.SaveChangesAsync();
        await Dispatcher.UIThread.InvokeAsync(() => Customers.Remove(SelectedCustomer));
    }

    private static Avalonia.Controls.Window? GetMainWindow()
    {
        return (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
    }
}
