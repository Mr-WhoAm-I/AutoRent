using System;
using System.Windows;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;

namespace CarRental.UI.Views.Windows
{
    public partial class FineWindow : Window
    {
        private Fine _fine;
        private readonly FinanceService _service = new();
        public bool IsSuccess { get; private set; }

        public FineWindow(int rentalId, Fine fine = null)
        {
            InitializeComponent();
            _fine = fine ?? new Fine { RentalId = rentalId, Date = DateTime.Now };

            DatePick.SelectedDate = _fine.Date;
            TxtSum.Text = _fine.Amount > 0 ? _fine.Amount.ToString() : "";
            TxtReason.Text = _fine.Reason;
            ChkPaid.IsChecked = _fine.IsPaid;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(TxtSum.Text, out decimal sum)) return;
            _fine.Date = DatePick.SelectedDate ?? DateTime.Now;
            _fine.Amount = sum;
            _fine.Reason = TxtReason.Text;
            _fine.IsPaid = ChkPaid.IsChecked == true;

            _service.SaveFine(_fine);
            IsSuccess = true;
            Close();
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); }
    }
}