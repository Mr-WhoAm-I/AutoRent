using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.DTO;
using CarRental.UI.Views;
using CarRental.UI.Views.Windows;

namespace CarRental.UI.Views.Pages
{
    public partial class RentalsPage : Page
    {
        private readonly RentalService _rentalService = new();
        private readonly BookingService _bookingService = new(); // НОВЫЙ СЕРВИС

        private readonly ClientService _clientService = new();
        private readonly CarService _carService = new();

        private List<RentalViewItem> _allRentals = new();
        private List<BookingViewItem> _allBookings = new(); // СПИСОК БРОНЕЙ

        public RentalsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _allRentals = _rentalService.GetAllRentals();
                _allBookings = _bookingService.GetAllBookings(); // ЗАГРУЗКА БРОНЕЙ

                ApplyFilters();
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", true);
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void Filter_Changed(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void ApplyFilters()
        {
            string search = SearchBox.Text?.Trim().ToLower() ?? "";

            // --- 1. ФИЛЬТРАЦИЯ АРЕНД ---
            var rentalsQuery = _allRentals.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                rentalsQuery = rentalsQuery.Where(r =>
                    r.ClientFullName.ToLower().Contains(search) ||
                    r.ClientPhone.ToLower().Contains(search) || // ПОИСК ПО ТЕЛЕФОНУ
                    r.CarName.ToLower().Contains(search) ||
                    r.CarPlate.ToLower().Contains(search) ||
                    r.Id.ToString().Contains(search)
                );
            }

            if (DateStartPicker.SelectedDate.HasValue)
                rentalsQuery = rentalsQuery.Where(r => r.DateStart >= DateStartPicker.SelectedDate.Value);

            if (DateEndPicker.SelectedDate.HasValue)
                rentalsQuery = rentalsQuery.Where(r => r.DateStart <= DateEndPicker.SelectedDate.Value);

            RentalsGrid.ItemsSource = rentalsQuery.ToList();


            // --- 2. ФИЛЬТРАЦИЯ БРОНИРОВАНИЙ ---
            var bookingsQuery = _allBookings.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                bookingsQuery = bookingsQuery.Where(b =>
                    b.ClientFullName.ToLower().Contains(search) ||
                    b.ClientPhone.ToLower().Contains(search) || // ПОИСК ПО ТЕЛЕФОНУ
                    b.CarName.ToLower().Contains(search) ||
                    b.CarPlate.ToLower().Contains(search)
                );
            }

            if (DateStartPicker.SelectedDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.DateStart >= DateStartPicker.SelectedDate.Value);

            if (DateEndPicker.SelectedDate.HasValue)
                bookingsQuery = bookingsQuery.Where(b => b.DateStart <= DateEndPicker.SelectedDate.Value);

            BookingsGrid.ItemsSource = bookingsQuery.ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, какая вкладка открыта
            if (MainTabs.SelectedIndex == 0) // Аренды
            {
                var win = new RentalAddWindow();
                win.ShowDialog();
                if (win.IsSuccess) LoadData();
            }
            else // Бронирования (Index == 1)
            {
                var win = new BookingAddWindow();
                win.ShowDialog();
                if (win.IsSuccess) LoadData();
            }
        }

        // === ПЕРЕХОДЫ (ССЫЛКИ) ===

        // Универсальные обработчики для обоих гридов
        private void OpenClient_Click(object sender, MouseButtonEventArgs e)
        {
            int? clientId = null;

            // Проверяем, откуда пришел клик (Аренда или Бронь)
            if ((sender as FrameworkElement)?.DataContext is RentalViewItem rItem) clientId = rItem.ClientId;
            else if ((sender as FrameworkElement)?.DataContext is BookingViewItem bItem) clientId = bItem.ClientId;

            if (clientId.HasValue) OpenClientCard(clientId.Value);
        }

        private void OpenCar_Click(object sender, MouseButtonEventArgs e)
        {
            int? carId = null;

            if ((sender as FrameworkElement)?.DataContext is RentalViewItem rItem) carId = rItem.CarId;
            else if ((sender as FrameworkElement)?.DataContext is BookingViewItem bItem) carId = bItem.CarId;

            if (carId.HasValue) OpenCarCard(carId.Value);
        }

        // Обработчики контекстного меню
        // Внимание: ContextMenu привязывается к строке, поэтому DataContext там правильный
        private void OpenClientContext_Click(object sender, RoutedEventArgs e)
        {
            // Пытаемся получить выделенный элемент из активного грида
            if (RentalsGrid.SelectedItem is RentalViewItem rItem) OpenClientCard(rItem.ClientId);
            else if (BookingsGrid.SelectedItem is BookingViewItem bItem) OpenClientCard(bItem.ClientId);
        }

        private void OpenCarContext_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsGrid.SelectedItem is RentalViewItem rItem) OpenCarCard(rItem.CarId);
            else if (BookingsGrid.SelectedItem is BookingViewItem bItem) OpenCarCard(bItem.CarId);
        }

        // Вспомогательные методы открытия
        private void OpenClientCard(int clientId)
        {
            var client = _clientService.GetClients().FirstOrDefault(c => c.Id == clientId);
            if (client != null) { var win = new ClientWindow(client); win.ShowDialog(); }
            else InfoDialog.Show("Клиент не найден.", "Ошибка", true);
        }

        private void OpenCarCard(int carId)
        {
            var car = _carService.GetCars().FirstOrDefault(c => c.Id == carId);
            if (car != null) { var win = new CarDetailsWindow(car); win.ShowDialog(); }
            else InfoDialog.Show("Автомобиль не найден.", "Ошибка", true);
        }

        private void OpenDetails_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsGrid.SelectedItem is RentalViewItem item)
            {
                var win = new RentalDetailsWindow(item.Id);
                win.ShowDialog();
                LoadData();
            }
        }

        private void EditRentalContext_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsGrid.SelectedItem is RentalViewItem item)
            {
                try
                {
                    var rental = _rentalService.GetRentalById(item.Id);
                    if (rental != null)
                    {
                        var win = new RentalAddWindow(rental);
                        win.ShowDialog();
                        if (win.IsSuccess) LoadData(); // Обновляем таблицу
                    }
                }
                catch (Exception ex)
                {
                    InfoDialog.Show(ex.Message, "Ошибка", true);
                }
            }
        }
        private void OpenBookingDetails_Click(object sender, RoutedEventArgs e)
        {
            if (BookingsGrid.SelectedItem is BookingViewItem item)
            {
                try
                {
                    var booking = _bookingService.GetBookingById(item.Id);
                    if (booking != null)
                    {
                        var win = new BookingAddWindow(booking);
                        win.ShowDialog();
                        if (win.IsSuccess) LoadData();
                    }
                }
                catch (Exception ex) { InfoDialog.Show(ex.Message, "Ошибка", true); }
            }
        }
    }
}