using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;

namespace CarRental.UI.Views.Windows
{
    public partial class InsuranceWindow : Window
    {
        private readonly InsuranceService _service = new();
        private Insurance _current;
        private int _carId;
        public bool IsSuccess { get; private set; }

        public InsuranceWindow(int carId, Insurance insurance = null)
        {
            InitializeComponent();
            _carId = carId;
            _current = insurance ?? new Insurance { CarId = carId, StartDate = DateTime.Today, EndDate = DateTime.Today.AddYears(1) };

            FillData();
        }

        private void FillData()
        {
            if (_current.Id != 0)
            {
                TxtTitle.Text = "Редактирование полиса";
                TxtPolicy.IsReadOnly = true; // Запрет изменения номера!
                TxtPolicy.Foreground = System.Windows.Media.Brushes.Gray;
                TxtPolicy.ToolTip = "Номер полиса нельзя изменить";
            }

            TxtPolicy.Text = _current.PolicyNumber;
            TxtType.Text = _current.Type;
            TxtCost.Text = _current.Cost.ToString("0.##");
            DateStart.SelectedDate = _current.StartDate;
            DateEnd.SelectedDate = _current.EndDate;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtPolicy.Text)) { InfoDialog.Show("Введите номер полиса", "Ошибка", true); return; }
            if (DateStart.SelectedDate == null || DateEnd.SelectedDate == null) { InfoDialog.Show("Укажите даты", "Ошибка", true); return; }

            _current.PolicyNumber = TxtPolicy.Text;
            _current.Type = TxtType.Text;
            _current.StartDate = DateStart.SelectedDate.Value;
            _current.EndDate = DateEnd.SelectedDate.Value;

            if (decimal.TryParse(TxtCost.Text, out decimal cost)) _current.Cost = cost;

            try
            {
                _service.Save(_current);
                IsSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                InfoDialog.Show(ex.Message, "Ошибка", true);
            }
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e) => e.Handled = new Regex("[^0-9,]+").IsMatch(e.Text);
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}