using System;
using System.Collections.Generic;
using System.Linq; // Важно для Where
using System.Windows;
using System.Windows.Controls;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views.Windows;

namespace CarRental.UI.Views.Pages
{
    public partial class EmployeesPage : Page
    {
        private readonly EmployeeService _service = new();

        // Полный список для поиска в памяти (чтобы не дергать БД на каждый чих)
        private List<Employee> _allEmployees = new();

        public EmployeesPage()
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
                // 1. Загружаем всех из БД
                _allEmployees = _service.GetAll();

                // 2. Применяем фильтр (если в строке поиска что-то осталось)
                ApplyFilter();
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки: " + ex.Message, "Ошибка", true);
            }
        }

        // Событие ввода текста в SearchBar
        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string searchText = SearchBox.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(searchText))
            {
                // Если поиск пуст - показываем всех
                EmployeesGrid.ItemsSource = _allEmployees;
            }
            else
            {
                // Фильтруем по Фамилии, Имени или Логину
                var filtered = _allEmployees.Where(e =>
                    e.Surname.ToLower().Contains(searchText) ||
                    e.Name.ToLower().Contains(searchText) ||
                    e.Login.ToLower().Contains(searchText)
                ).ToList();

                EmployeesGrid.ItemsSource = filtered;
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var win = new EmployeeWindow();
            win.ShowDialog();
            if (win.IsSuccess) LoadData();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesGrid.SelectedItem is Employee emp)
            {
                var win = new EmployeeWindow(emp);
                win.ShowDialog();
                if (win.IsSuccess) LoadData();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesGrid.SelectedItem is Employee emp)
            {
                if (MessageBox.Show($"Вы уверены, что хотите отправить сотрудника {emp.Surname} в архив?\nОн потеряет доступ к системе.",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _service.Archive(emp.Id);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        InfoDialog.Show(ex.Message, "Ошибка", true);
                    }
                }
            }
        }
    }
}