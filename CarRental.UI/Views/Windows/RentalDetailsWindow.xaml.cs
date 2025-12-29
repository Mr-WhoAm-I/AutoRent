using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using CarRental.BLL.Services;
using CarRental.Domain.DTO;
using CarRental.Domain.Entities;
using CarRental.UI.Views; // Для InfoDialog

namespace CarRental.UI.Views.Windows
{
    public partial class RentalDetailsWindow : Window
    {
        private readonly int _rentalId;
        private readonly FinanceService _financeService = new();
        private readonly RentalService _rentalService = new(); // Чтобы обновлять общие цифры

        public RentalDetailsWindow(int rentalId)
        {
            InitializeComponent();
            _rentalId = rentalId;
            LoadAllData();
        }

        private void LoadAllData()
        {
            // 1. Загружаем основную инфу через RentalService -> RentalViewItem
            // Нам нужно найти метод, который вернет RentalViewItem по ID.
            // Если его нет, можно переиспользовать список или добавить метод GetViewItemById.
            // Для простоты возьмем из списка всех аренд (или добавьте метод в RentalRepository)
            var rentalView = _rentalService.GetAllRentals().FirstOrDefault(r => r.Id == _rentalId);

            if (rentalView != null)
            {
                // Клиент
                TxtClientName.Text = rentalView.ClientFullName;
                TxtClientPhone.Text = rentalView.ClientPhone;

                // Авто
                TxtCarName.Text = rentalView.CarName;
                TxtCarPlate.Text = rentalView.CarPlate;
                if (!string.IsNullOrEmpty(rentalView.CarPhoto))
                    ImgCar.Source = new BitmapImage(new Uri(rentalView.CarPhoto));

                // Сотрудник
                TxtEmpName.Text = rentalView.EmployeeName;
                TxtEmpPos.Text = rentalView.EmployeePosition;

                // Даты
                TxtDateStart.Text = rentalView.DateStart.ToString("dd.MM.yyyy");
                TxtDateEnd.Text = rentalView.DisplayEndDate.ToString("dd.MM.yyyy");
                TxtStatus.Text = rentalView.Status;

                // Финансы (Из View)
                TxtTotalCost.Text = $"{rentalView.TotalCost:N2} BYN";
                TxtPaid.Text = $"{rentalView.Paid:N2} BYN";
                TxtDebt.Text = $"{rentalView.Debt:N2} BYN";
                TxtDebt.Foreground = rentalView.HasDebt ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Green;
            }

            // Загружаем комментарий отдельно (из сущности Rental, т.к. во View он может быть не включен или ReadOnly)
            var rentalEntity = _rentalService.GetRentalById(_rentalId);
            if (rentalEntity != null) TxtComment.Text = rentalEntity.Review;

            // 2. Загружаем таблицы
            FineGrid.ItemsSource = _financeService.GetFines(_rentalId);
            PaymentGrid.ItemsSource = _financeService.GetPayments(_rentalId);
        }

        // --- ШТРАФЫ ---
        private void AddFine_Click(object sender, RoutedEventArgs e)
        {
            var win = new FineWindow(_rentalId);
            win.ShowDialog();
            if (win.IsSuccess) LoadAllData(); // Перезагружаем всё, чтобы обновились суммы
        }

        private void EditFine_Click(object sender, RoutedEventArgs e)
        {
            if (FineGrid.SelectedItem is Fine fine)
            {
                var win = new FineWindow(_rentalId, fine);
                win.ShowDialog();
                if (win.IsSuccess) LoadAllData();
            }
        }

        private void DeleteFine_Click(object sender, RoutedEventArgs e)
        {
            if (FineGrid.SelectedItem is Fine fine && MessageBox.Show("Удалить штраф?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _financeService.DeleteFine(fine.Id);
                LoadAllData();
            }
        }

        // --- ПЛАТЕЖИ ---
        private void AddPayment_Click(object sender, RoutedEventArgs e)
        {
            var win = new PaymentWindow(_rentalId);
            win.ShowDialog();
            if (win.IsSuccess) LoadAllData();
        }

        private void EditPayment_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentGrid.SelectedItem is Payment p)
            {
                var win = new PaymentWindow(_rentalId, p);
                win.ShowDialog();
                if (win.IsSuccess) LoadAllData();
            }
        }

        private void DeletePayment_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentGrid.SelectedItem is Payment p && MessageBox.Show("Удалить платеж?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _financeService.DeletePayment(p.Id);
                LoadAllData();
            }
        }

        // --- КОММЕНТАРИЙ ---
        private void SaveComment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _rentalService.UpdateComment(_rentalId, TxtComment.Text);
                InfoDialog.Show("Комментарий сохранен", "Успех");
            }
            catch (Exception ex)
            {
                InfoDialog.Show(ex.Message, "Ошибка");
            }
        }
        private void EditRental_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем сущность для редактирования
                var rental = _rentalService.GetRentalById(_rentalId);
                if (rental != null)
                {
                    // Открываем окно редактирования (RentalAddWindow)
                    var win = new RentalAddWindow(rental);
                    win.ShowDialog();

                    // Если сохранили изменения - перезагружаем данные карточки
                    if (win.IsSuccess)
                    {
                        LoadAllData();
                    }
                }
            }
            catch (Exception ex)
            {
                InfoDialog.Show(ex.Message, "Ошибка", true);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) DragMove(); }
    }
}