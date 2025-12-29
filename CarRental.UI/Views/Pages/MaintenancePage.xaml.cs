using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views.Windows;

namespace CarRental.UI.Views.Pages
{
    public partial class MaintenancePage : Page
    {
        private readonly MaintenanceService _service = new();
        private readonly CarService _carService = new();

        private List<Maintenance> _activeTasks = new();
        private List<Maintenance> _historyTasks = new();

        public MaintenancePage()
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
                _activeTasks = _service.GetActive();
                _historyTasks = _service.GetHistory();

                ApplyFilters();
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки: " + ex.Message, "Ошибка", true);
            }
        }

        private void ApplyFilters()
        {
            if (ActiveGrid == null || HistoryGrid == null || ChkMyTasks == null || SearchBox == null) return;

            string search = SearchBox.Text?.Trim().ToLower() ?? "";
            bool myTasksOnly = ChkMyTasks.IsChecked == true;
            int currentUserId = AuthService.CurrentUser?.Id ?? 0;

            // 1. Фильтр активных
            var activeQuery = _activeTasks.AsEnumerable();

            if (myTasksOnly)
                activeQuery = activeQuery.Where(m => m.EmployeeId == currentUserId);

            if (!string.IsNullOrEmpty(search))
            {
                activeQuery = activeQuery.Where(m =>
                    m.CarName.ToLower().Contains(search) ||
                    m.PlateNumber.ToLower().Contains(search) ||
                    m.ServiceType.ToLower().Contains(search) ||
                    m.MechanicName.ToLower().Contains(search)); // <-- ДОБАВЛЕНО
            }

            ActiveGrid.ItemsSource = activeQuery.ToList();

            // 2. Фильтр истории
            var historyQuery = _historyTasks.AsEnumerable();

            if (!string.IsNullOrEmpty(search))
            {
                historyQuery = historyQuery.Where(m =>
                    m.CarName.ToLower().Contains(search) ||
                    m.PlateNumber.ToLower().Contains(search) ||
                    m.ServiceType.ToLower().Contains(search) ||
                    m.MechanicName.ToLower().Contains(search)); // <-- ДОБАВЛЕНО
            }

            HistoryGrid.ItemsSource = historyQuery.ToList();
        }

        // События фильтров
        private void Search_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFilters();
        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // === ДЕЙСТВИЯ ===

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            InfoDialog.Show("Чтобы создать наряд, перейдите в раздел 'Автомобили', выберите машину и нажмите 'Запись на сервис'.", "Подсказка");
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ActiveGrid.SelectedItem is Maintenance item) OpenEditWindow(item);
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveGrid.SelectedItem is Maintenance item) OpenEditWindow(item);
        }

        private void Finish_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Maintenance item) OpenEditWindow(item);
        }

        private void OpenEditWindow(Maintenance item)
        {
            var win = new MaintenanceWindow(item.CarId, item);
            win.ShowDialog();
            if (win.IsSuccess) LoadData();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ActiveGrid.SelectedItem is Maintenance item)
            {
                if (MessageBox.Show("Удалить запись о ремонте?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    _service.Delete(item.Id);
                    LoadData();
                }
            }
        }
    }
}