using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views;

namespace CarRental.UI.Views.Windows
{
    public partial class BookingAddWindow : Window
    {
        private readonly CarService _carService = new();
        private readonly ClientService _clientService = new();
        private readonly BookingService _bookingService = new();

        private Booking _currentBooking;
        public bool IsSuccess { get; private set; }

        public BookingAddWindow()
        {
            InitializeComponent();
            _currentBooking = new Booking();
            LoadInitialData();
        }

        public BookingAddWindow(Booking booking)
        {
            InitializeComponent();
            _currentBooking = booking;
            TxtTitle.Text = $"Бронь №{booking.Id}";
            BtnSave.Content = "Сохранить";
            LoadInitialData();
            FillData();
        }

        private void LoadInitialData()
        {
            try
            {
                ComboClient.ItemsSource = _clientService.GetClients();

                if (_currentBooking.Id == 0)
                {
                    DateStart.SelectedDate = DateTime.Today.AddDays(1);
                    DateEnd.SelectedDate = DateTime.Today.AddDays(2);
                }
            }
            catch (Exception ex) { InfoDialog.Show(ex.Message, "Ошибка загрузки", true); }
        }

        private void FillData()
        {
            DateStart.SelectedDateChanged -= Date_Changed;
            DateEnd.SelectedDateChanged -= Date_Changed;

            DateStart.SelectedDate = _currentBooking.StartDate;
            DateEnd.SelectedDate = _currentBooking.EndDate;
            TxtComment.Text = _currentBooking.Comment;

            // Выбор клиента
            foreach (Client c in ComboClient.Items)
                if (c.Id == _currentBooking.ClientId) ComboClient.SelectedItem = c;

            // Загрузка и выбор авто
            LoadAvailableCars(_currentBooking.StartDate, _currentBooking.EndDate);

            // Если машина занята текущей бронью, подгружаем её
            var cars = ComboCar.ItemsSource as System.Collections.Generic.List<Car>;
            if (cars != null && !cars.Any(c => c.Id == _currentBooking.CarId))
            {
                var currentCar = _carService.GetCars().FirstOrDefault(c => c.Id == _currentBooking.CarId);
                if (currentCar != null)
                {
                    cars.Insert(0, currentCar);
                    ComboCar.ItemsSource = null;
                    ComboCar.ItemsSource = cars;
                }
            }

            foreach (Car c in ComboCar.Items)
                if (c.Id == _currentBooking.CarId) ComboCar.SelectedItem = c;

            DateStart.SelectedDateChanged += Date_Changed;
            DateEnd.SelectedDateChanged += Date_Changed;
        }

        private void Date_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DateStart.SelectedDate == null || DateEnd.SelectedDate == null) return;
            LoadAvailableCars(DateStart.SelectedDate.Value, DateEnd.SelectedDate.Value);
        }

        private void LoadAvailableCars(DateTime start, DateTime end)
        {
            try
            {
                var cars = _carService.GetAvailableCars(start, end);
                ComboCar.ItemsSource = cars;
                ComboCar.IsEnabled = true;
                ComboCar.Tag = cars.Count > 0 ? "Выберите автомобиль" : "Нет свободных авто";
            }
            catch (Exception ex) { InfoDialog.Show(ex.Message, "Ошибка поиска авто", true); }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (DateStart.SelectedDate == null || DateEnd.SelectedDate == null) { InfoDialog.Show("Выберите даты."); return; }
            if (ComboCar.SelectedItem is not Car car) { InfoDialog.Show("Выберите автомобиль."); return; }
            if (ComboClient.SelectedItem is not Client client) { InfoDialog.Show("Выберите клиента."); return; }

            try
            {
                _currentBooking.ClientId = client.Id;
                _currentBooking.CarId = car.Id;
                _currentBooking.StartDate = DateStart.SelectedDate.Value;
                _currentBooking.EndDate = DateEnd.SelectedDate.Value;
                _currentBooking.Comment = TxtComment.Text.Trim();

                if (_currentBooking.Id == 0)
                    _bookingService.CreateBooking(_currentBooking);
                else
                    _bookingService.UpdateBooking(_currentBooking);

                IsSuccess = true;
                Close();
            }
            catch (Exception ex) { InfoDialog.Show(ex.Message, "Ошибка", true); }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}