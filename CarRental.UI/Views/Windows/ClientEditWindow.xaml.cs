using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views; // InfoDialog

namespace CarRental.UI.Views.Windows
{
    public partial class ClientEditWindow : Window
    {
        private readonly ClientService _service = new();
        private Client _client;
        public bool IsSuccess { get; private set; }

        public ClientEditWindow(Client client = null)
        {
            InitializeComponent();
            _client = client ?? new Client();
            FillData();
        }

        private void FillData()
        {
            if (_client.Id != 0) TxtTitle.Text = "Редактирование клиента";

            TxtSurname.Text = _client.Surname;
            TxtName.Text = _client.Name;
            TxtPatronymic.Text = _client.Patronymic;
            TxtPhone.Text = _client.Phone;
            TxtEmail.Text = _client.Email;
            DateBirth.SelectedDate = _client.DateOfBirth;
            TxtExp.Text = _client.DrivingExperience?.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _client.Surname = TxtSurname.Text.Trim();
            _client.Name = TxtName.Text.Trim();
            _client.Patronymic = TxtPatronymic.Text.Trim();
            _client.Phone = TxtPhone.Text.Trim();
            _client.Email = TxtEmail.Text.Trim();
            _client.DateOfBirth = DateBirth.SelectedDate;

            if (int.TryParse(TxtExp.Text, out int exp)) _client.DrivingExperience = exp;
            else _client.DrivingExperience = null;

            try
            {
                _service.SaveClient(_client);
                IsSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                InfoDialog.Show(ex.Message, "Ошибка сохранения", true);
            }
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e) => e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}