using System;
using System.Windows;
using System.Windows.Controls;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;

namespace CarRental.UI.Views.Windows
{
    public partial class PaymentWindow : Window
    {
        private Payment _pay;
        private readonly FinanceService _service = new();
        public bool IsSuccess { get; private set; }

        public PaymentWindow(int rentalId, Payment payment = null)
        {
            InitializeComponent();
            _pay = payment ?? new Payment { RentalId = rentalId, Date = DateTime.Now };

            DatePick.SelectedDate = _pay.Date;
            TxtSum.Text = _pay.Amount > 0 ? _pay.Amount.ToString() : "";
            ComboType.Text = _pay.Type;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtSum.Text, out decimal sum)) return;
            _pay.Date = DatePick.SelectedDate ?? DateTime.Now;
            _pay.Amount = sum;
            _pay.Type = ComboType.Text;

            _service.SavePayment(_pay);
            IsSuccess = true;
            Close();
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); }
    }
}