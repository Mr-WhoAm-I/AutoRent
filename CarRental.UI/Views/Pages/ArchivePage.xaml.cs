using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views;
using CarRental.UI.Views.Windows; // Для открытия карточек

namespace CarRental.UI.Views.Pages
{
    public partial class ArchivePage : Page
    {
        private readonly ClientService _clientService = new();
        private readonly EmployeeService _employeeService = new();
        private readonly CarService _carService = new();
        private readonly InsuranceService _insuranceService = new();
        private readonly MaintenanceService _maintenanceService = new();

        public ArchivePage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MenuListBox.SelectedIndex == -1) MenuListBox.SelectedIndex = 0;
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem is ListBoxItem item)
            {
                string tag = item.Tag.ToString();
                LoadSection(tag);
            }
        }

        private void LoadSection(string tag)
        {
            // Скрываем все таблицы
            GridClients.Visibility = Visibility.Collapsed;
            GridEmployees.Visibility = Visibility.Collapsed;
            GridCars.Visibility = Visibility.Collapsed;
            GridInsurance.Visibility = Visibility.Collapsed;
            GridMaintenance.Visibility = Visibility.Collapsed;

            try
            {
                switch (tag)
                {
                    case "Clients":
                        GridClients.ItemsSource = _clientService.GetArchivedClients();
                        GridClients.Visibility = Visibility.Visible;
                        break;

                    case "Employees":
                        GridEmployees.ItemsSource = _employeeService.GetArchivedEmployees();
                        GridEmployees.Visibility = Visibility.Visible;
                        break;

                    case "Cars":
                        GridCars.ItemsSource = _carService.GetWrittenOffCars();
                        GridCars.Visibility = Visibility.Visible;
                        break;

                    case "Insurance":
                        // ИСПОЛЬЗУЕМ НОВЫЙ МЕТОД (Вместо фильтра по дате)
                        GridInsurance.ItemsSource = _insuranceService.GetArchivedInsurances();
                        GridInsurance.Visibility = Visibility.Visible;
                        break;

                    case "Maintenance":
                        // ИСПОЛЬЗУЕМ НОВЫЙ МЕТОД
                        GridMaintenance.ItemsSource = _maintenanceService.GetArchivedMaintenance();
                        GridMaintenance.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки архива: " + ex.Message, "Ошибка", true);
            }
        }

        // === ОБРАБОТЧИКИ ДВОЙНОГО КЛИКА (ОТКРЫТИЕ КАРТОЧЕК) ===

        private void GridClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridClients.SelectedItem is Client client)
            {
                var win = new ClientWindow(client);
                win.ShowDialog();
                // Обновляем список после закрытия (вдруг восстановили из архива?)
                LoadSection("Clients");
            }
        }

        private void GridEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridEmployees.SelectedItem is Employee emp)
            {
                var win = new EmployeeWindow(emp);
                win.ShowDialog();
                LoadSection("Employees");
            }
        }

        private void GridCars_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridCars.SelectedItem is Car car)
            {
                var win = new CarWindow(car);
                win.ShowDialog();
                LoadSection("Cars");
            }
        }
        private void GridInsurance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridInsurance.SelectedItem is Insurance item)
            {
                var win = new InsuranceWindow(item.CarId, item);
                win.ShowDialog();
                LoadSection("Insurance");
            }
        }

        private void GridMaintenance_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (GridMaintenance.SelectedItem is Maintenance item)
            {
                var win = new MaintenanceWindow(item.CarId, item);
                win.ShowDialog();
                LoadSection("Maintenance");
            }
        }
        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (MenuListBox.SelectedItem is not ListBoxItem item) return;
            string tag = item.Tag.ToString();

            try
            {
                bool restored = false;

                switch (tag)
                {
                    case "Clients":
                        if (GridClients.SelectedItem is Client c)
                        {
                            _clientService.RestoreClient(c.Id);
                            restored = true;
                        }
                        break;

                    case "Employees":
                        if (GridEmployees.SelectedItem is Employee emp)
                        {
                            _employeeService.RestoreEmployee(emp.Id);
                            restored = true;
                        }
                        break;

                    case "Cars":
                        if (GridCars.SelectedItem is Car car)
                        {
                            _carService.RestoreCar(car.Id);
                            restored = true;
                        }
                        break;

                    case "Insurance":
                        if (GridInsurance.SelectedItem is Insurance ins)
                        {
                            _insuranceService.RestoreInsurance(ins.Id);
                            restored = true;
                        }
                        break;

                    case "Maintenance":
                        if (GridMaintenance.SelectedItem is Maintenance m)
                        {
                            _maintenanceService.RestoreMaintenance(m.Id);
                            restored = true;
                        }
                        break;
                }

                if (restored)
                {
                    InfoDialog.Show("Запись успешно восстановлена!", "Успех");
                    LoadSection(tag); // Обновляем таблицу
                }
                else
                {
                    InfoDialog.Show("Выберите запись для восстановления.", "Внимание");
                }
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка: " + ex.Message, "Ошибка", true);
            }
        }
    }
}