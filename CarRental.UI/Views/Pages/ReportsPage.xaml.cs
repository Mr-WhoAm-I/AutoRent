using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using CarRental.BLL.Services;
using CarRental.Domain.DTO;
using CarRental.UI.Views;

namespace CarRental.UI.Views.Pages
{
    public partial class ReportsPage : Page
    {
        private readonly ReportService _reportService = new();
        private readonly ReferenceService _refService = new();
        private readonly ExportService _exportService = new();

        private string _currentType = "";

        // Поля данных
        private List<ClientReportItem> _clientsData;
        private List<CarPerformanceItem> _carsData;
        private List<PaymentReportItem> _incomeData; // Новый тип DTO

        public ReportsPage()
        {
            InitializeComponent();
            DateStart.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateEnd.SelectedDate = DateTime.Now;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Заполняем CheckComboBox данными
                ComboClass.ItemsSource = _refService.GetClasses();
            }
            catch { }
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboType.SelectedItem is ComboBoxItem item)
            {
                _currentType = item.Tag.ToString();

                PanelDates.Visibility = (_currentType == "Income") ? Visibility.Visible : Visibility.Collapsed;
                ComboClass.Visibility = (_currentType == "Cars") ? Visibility.Visible : Visibility.Collapsed;

                GridClients.Visibility = Visibility.Collapsed;
                GridCars.Visibility = Visibility.Collapsed;
                GridIncome.Visibility = Visibility.Collapsed;
                PanelTotal.Visibility = Visibility.Collapsed;
                TxtHint.Visibility = Visibility.Visible;
            }
        }

        private void Generate_Click(object sender, RoutedEventArgs e)
        {
            TxtHint.Visibility = Visibility.Collapsed;
            try
            {
                switch (_currentType)
                {
                    case "Clients":
                        _clientsData = _reportService.GetClientsReport();
                        GridClients.ItemsSource = _clientsData;
                        GridClients.Visibility = Visibility.Visible;
                        break;

                    case "Cars":
                        // Получаем выбранные элементы из CheckComboBox
                        List<int> selectedIds = new();
                        foreach (Domain.Entities.CarClass item in ComboClass.SelectedItems)
                        {
                            selectedIds.Add(item.Id);
                        }

                        _carsData = _reportService.GetCarPerformance(selectedIds);
                        GridCars.ItemsSource = _carsData;
                        GridCars.Visibility = Visibility.Visible;
                        break;

                    case "Income":
                        if (DateStart.SelectedDate == null || DateEnd.SelectedDate == null) return;

                        // Получаем детальный список
                        _incomeData = _reportService.GetPaymentDetails(DateStart.SelectedDate.Value, DateEnd.SelectedDate.Value);
                        GridIncome.ItemsSource = _incomeData;
                        GridIncome.Visibility = Visibility.Visible;

                        // Считаем общий итог для предпросмотра
                        TxtTotalSum.Text = $"{_incomeData.Sum(x => x.Amount):N2} BYN";
                        PanelTotal.Visibility = Visibility.Visible;
                        break;
                }
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка: " + ex.Message, "Ошибка", true);
            }
        }

        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentType)) return;
            SaveFileDialog dlg = new SaveFileDialog { Filter = "Excel|*.xlsx", FileName = $"Report_{_currentType}_{DateTime.Now:ddMM}.xlsx" };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    switch (_currentType)
                    {
                        case "Clients":
                            // Словарь для перевода заголовков
                            var mapClient = new Dictionary<string, string> {
                                {"FullName", "ФИО Клиента"}, {"Phone", "Телефон"},
                                {"Email", "Почта"}, {"RentalsCount", "Всего аренд"}
                            };
                            _exportService.ExportToExcel(_clientsData, dlg.FileName, "Справочник клиентов", mapClient);
                            break;

                        case "Cars":
                            var mapCar = new Dictionary<string, string> {
                                {"CarName", "Автомобиль"}, {"PlateNumber", "Гос. номер"}, {"ClassName", "Класс"},
                                {"Revenue", "Доходы"}, {"Expenses", "Расходы"}, {"Profit", "Прибыль"},
                                {"ProfitIndex", "Индекс"}, {"Status", "Статус"}
                            };
                            _exportService.ExportToExcel(_carsData, dlg.FileName, "Эффективность парка", mapCar);
                            break;

                        case "Income":
                            // Специальный метод экспорта с группировкой
                            _exportService.ExportFinanceToExcel(_incomeData, dlg.FileName, $"Финансовый отчет ({DateStart.SelectedDate:dd.MM} - {DateEnd.SelectedDate:dd.MM})");
                            break;
                    }
                    InfoDialog.Show("Сохранено!", "Успех");
                }
                catch (Exception ex) { InfoDialog.Show(ex.Message, "Ошибка", true); }
            }
        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentType)) return;
            SaveFileDialog dlg = new SaveFileDialog { Filter = "PDF|*.pdf", FileName = $"Report_{_currentType}_{DateTime.Now:ddMM}.pdf" };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    switch (_currentType)
                    {
                        case "Clients":
                            _exportService.ExportToPdf(_clientsData, dlg.FileName, "Справочник клиентов",
                                new[] { "ФИО", "Телефон", "Почта", "Аренд" },
                                x => new[] { x.FullName, x.Phone, x.Email, x.RentalsCount.ToString() });
                            break;

                        case "Cars":
                            // Добавили Индекс в PDF
                            _exportService.ExportToPdf(_carsData, dlg.FileName, "Эффективность парка",
                                new[] { "Авто", "Номер", "Класс", "Доход", "Расход", "Индекс", "Статус" },
                                x => new[] { x.CarName, x.PlateNumber, x.ClassName, x.Revenue.ToString("N2"), x.Expenses.ToString("N2"), x.ProfitIndex.ToString("N2"), x.Status });
                            break;

                        case "Income":
                            // Специальный метод экспорта PDF с группировкой
                            _exportService.ExportFinanceToPdf(_incomeData, dlg.FileName, "Финансовый отчет");
                            break;
                    }
                    InfoDialog.Show("Сохранено!", "Успех");
                }
                catch (Exception ex) { InfoDialog.Show(ex.Message, "Ошибка", true); }
            }
        }
    }
} 