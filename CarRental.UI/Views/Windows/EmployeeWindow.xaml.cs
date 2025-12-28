using System;
using System.Windows;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;

namespace CarRental.UI.Views.Windows
{
    public partial class EmployeeWindow : Window
    {
        private readonly EmployeeService _service = new();
        private readonly ReferenceService _refService = new();
        private Employee _current;

        public bool IsSuccess { get; private set; }

        public EmployeeWindow(Employee employee = null)
        {
            InitializeComponent();
            _current = employee ?? new Employee();

            LoadRoles();
            FillData();
        }

        private void LoadRoles()
        {
            ComboRole.ItemsSource = _refService.GetRoles();
        }

        private void FillData()
        {
            if (_current.Id != 0)
            {
                TxtTitle.Text = "Редактирование сотрудника";
                // Меняем подсказку, чтобы было понятно, что поле необязательно
                TxtPassword.Tag = "Новый пароль (оставьте пустым, чтобы не менять)";
            }

            TxtSurname.Text = _current.Surname;
            TxtName.Text = _current.Name;
            TxtPosition.Text = _current.Position;
            TxtLogin.Text = _current.Login;
            ComboRole.SelectedValue = _current.RoleId;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ComboRole.SelectedValue == null) { InfoDialog.Show("Выберите роль", "Ошибка", true); return; }

            _current.Surname = TxtSurname.Text;
            _current.Name = TxtName.Text;
            _current.Position = TxtPosition.Text;
            _current.Login = TxtLogin.Text;
            _current.RoleId = (int)ComboRole.SelectedValue;

            // Просто берем текст из поля
            string password = TxtPassword.Text;

            try
            {
                // Передаем в сервис только новый пароль (или пустую строку)
                _service.Save(_current, password);
                IsSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                InfoDialog.Show(ex.Message, "Ошибка сохранения", true);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}