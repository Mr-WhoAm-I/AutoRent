using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input; // Важно для MouseButtonEventArgs
using CarRental.BLL.Services;
using CarRental.Domain.DTO;
using CarRental.UI.Views;
using CarRental.UI.Views.Windows; // Для открытия окон

namespace CarRental.UI.Views.Pages
{
    public partial class RentalsPage : Page
    {
        private readonly RentalService _rentalService = new();

        // Дополнительные сервисы для поиска объектов при клике
        private readonly ClientService _clientService = new();
        private readonly CarService _carService = new();

        private List<RentalViewItem> _allRentals = new();

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
            var query = _allRentals.AsEnumerable();

            string search = SearchBox.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r =>
                    r.ClientFullName.ToLower().Contains(search) ||
                    r.CarName.ToLower().Contains(search) ||
                    r.CarPlate.ToLower().Contains(search) ||
                    r.Id.ToString().Contains(search)
                );
            }

            if (DateStartPicker.SelectedDate.HasValue)
                query = query.Where(r => r.DateStart >= DateStartPicker.SelectedDate.Value);

            if (DateEndPicker.SelectedDate.HasValue)
                query = query.Where(r => r.DateStart <= DateEndPicker.SelectedDate.Value);

            RentalsGrid.ItemsSource = query.ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            InfoDialog.Show("Мастер оформления аренды будет реализован следующим шагом.", "В разработке");
        }

        // === ПЕРЕХОДЫ (ССЫЛКИ) ===

        // 1. Клик по клиенту -> Открыть ClientWindow
        private void OpenClient_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is RentalViewItem item)
            {
                OpenClientCard(item.ClientId);
            }
        }

        // 2. Клик по машине -> Открыть CarDetailsWindow
        private void OpenCar_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is RentalViewItem item)
            {
                OpenCarCard(item.CarId);
            }
        }

        // Вспомогательные методы открытия (чтобы использовать и в контекстном меню)
        private void OpenClientCard(int clientId)
        {
            var client = _clientService.GetClients().FirstOrDefault(c => c.Id == clientId);
            if (client != null)
            {
                var win = new ClientWindow(client);
                win.ShowDialog();
            }
            else
            {
                InfoDialog.Show("Клиент не найден (возможно, удален).", "Ошибка", true);
            }
        }

        private void OpenCarCard(int carId)
        {
            var car = _carService.GetCars().FirstOrDefault(c => c.Id == carId);
            if (car != null)
            {
                var win = new CarDetailsWindow(car);
                win.ShowDialog();
            }
            else
            {
                InfoDialog.Show("Автомобиль не найден.", "Ошибка", true);
            }
        }

        // === КОНТЕКСТНОЕ МЕНЮ ===

        private void OpenDetails_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsGrid.SelectedItem is RentalViewItem item)
            {
                InfoDialog.Show($"Аренда №{item.Id}. Окно просмотра в разработке.", "Инфо");
            }
        }

        private void OpenClientContext_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsGrid.SelectedItem is RentalViewItem item) OpenClientCard(item.ClientId);
        }

        private void OpenCarContext_Click(object sender, RoutedEventArgs e)
        {
            if (RentalsGrid.SelectedItem is RentalViewItem item) OpenCarCard(item.CarId);
        }
    }
}