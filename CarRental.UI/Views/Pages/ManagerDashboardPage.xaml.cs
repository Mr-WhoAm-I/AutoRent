using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.DTO;
using CarRental.UI.Views.Windows;

namespace CarRental.UI.Views.Pages
{
    public partial class ManagerDashboardPage : Page
    {
        private readonly ManagerService _service = new();

        public ManagerDashboardPage()
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
                var data = _service.GetDashboardData();

                TxtTodayDate.Text = DateTime.Now.ToString("dd MMMM yyyy");
                TxtSubtitle.Text = $"Задач на сегодня: {data.IssuesCount + data.ReturnsCount}, Внимание: {data.OverdueCount}";

                // 1. ВЫДАЧИ
                HeaderIssues.Text = $"↗ ОЖИДАЕМЫЕ ВЫДАЧИ ({data.IssuesCount})";
                ListIssues.ItemsSource = data.IssuesToday;
                EmptyIssues.Visibility = data.IssuesCount == 0 ? Visibility.Visible : Visibility.Collapsed;

                // 2. ВОЗВРАТЫ
                HeaderReturns.Text = $"↙ ОЖИДАЕМЫЕ ВОЗВРАТЫ ({data.ReturnsCount})";
                ListReturns.ItemsSource = data.ReturnsToday;
                EmptyReturns.Visibility = data.ReturnsCount == 0 ? Visibility.Visible : Visibility.Collapsed;

                // 3. ПРОСРОЧЕННЫЕ
                HeaderOverdue.Text = $"Просроченные ({data.OverdueCount})";
                ListOverdue.ItemsSource = data.OverdueRentals;
                EmptyOverdue.Visibility = data.OverdueCount == 0 ? Visibility.Visible : Visibility.Collapsed;

                // 4. СЕРВИС
                HeaderService.Text = $"В сервисе ({data.ServiceCount})";
                ListService.ItemsSource = data.CarsInService;
                EmptyService.Visibility = data.ServiceCount == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка обновления сводки: " + ex.Message, "Ошибка", true);
            }
        }

        // Кнопка "Выдать авто" (Зеленая)
        private void BtnIssue_Click(object sender, RoutedEventArgs e)
        {
            var win = new RentalAddWindow();
            win.ShowDialog();
            if (win.IsSuccess) LoadData();
        }

        // Кнопка "Новый клиент"
        private void BtnNewClient_Click(object sender, RoutedEventArgs e)
        {
            var win = new ClientEditWindow();
            win.ShowDialog();
            // Здесь обновлять сводку не обязательно, т.к. новый клиент не создает аренду
        }
        private void BtnAutoFine_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Вызываем нашу процедуру с курсором через сервис (нужен FinanceService)
                var financeService = new FinanceService();
                int count = financeService.AutoChargeFines();

                if (count > 0)
                {
                    InfoDialog.Show($"Начислено новых штрафов: {count}", "Успех");
                    LoadData(); // Обновляем сводку, чтобы увидеть изменения
                }
                else
                {
                    InfoDialog.Show("Новых просрочек не найдено.", "Инфо");
                }
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка: " + ex.Message, "Ошибка", true);
            }
        }

        // Двойной клик по карточке -> Открыть детали аренды
        private void Item_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверка на ДВОЙНОЙ клик
            if (e.ClickCount == 2)
            {
                if ((sender as FrameworkElement)?.DataContext is RentalViewItem item)
                {
                    var win = new RentalDetailsWindow(item.Id);
                    win.ShowDialog();
                    LoadData(); // Обновляем данные после закрытия окна (вдруг статус изменился)
                }
            }
        }
    }
}