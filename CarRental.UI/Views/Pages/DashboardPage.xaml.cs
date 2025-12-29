using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using CarRental.BLL.Services;
using CarRental.Domain.DTO;
using CarRental.UI.Views;

namespace CarRental.UI.Views.Pages
{
    public partial class DashboardPage : Page
    {
        private readonly DashboardService _service = new();

        // Свойства для привязки ЛИНЕЙНОГО графика (CartesianChart)
        public ChartValues<double> IncomeValues { get; set; }
        public ChartValues<double> ExpenseValues { get; set; }
        public string[] ChartLabels { get; set; }

        public DashboardPage()
        {
            InitializeComponent();
            DataContext = this; // Важно для Binding
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var stats = _service.GetDashboardStats();

                // 1. Заполняем карточки
                TxtIncome.Text = $"{stats.MonthlyIncome:N2} BYN";
                TxtExpense.Text = $"{stats.MonthlyExpenses:N2} BYN";
                TxtActiveRentals.Text = stats.ActiveRentalsCount.ToString();
                TxtTotalCars.Text = stats.TotalCars.ToString();
                TxtDate.Text = $"Данные за {DateTime.Now:MMMM yyyy}";

                // 2. Линейный график (Доходы/Расходы)
                int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                var labels = new List<string>();
                var incomeVals = new ChartValues<double>();
                var expenseVals = new ChartValues<double>();

                for (int i = 1; i <= daysInMonth; i++)
                {
                    labels.Add(i.ToString());

                    var inc = stats.IncomeChart.FirstOrDefault(x => x.Label == i.ToString());
                    incomeVals.Add(inc?.Value ?? 0);

                    var exp = stats.ExpenseChart.FirstOrDefault(x => x.Label == i.ToString());
                    expenseVals.Add(exp?.Value ?? 0);
                }

                ChartLabels = labels.ToArray();
                IncomeValues = incomeVals;
                ExpenseValues = expenseVals;

                // Передергиваем контекст для обновления графика
                DataContext = null;
                DataContext = this;

                // 3. Круговая диаграмма (Статусы) - Заполняем через Code Behind по имени
                StatusPieChart.Series = new SeriesCollection();

                foreach (var status in stats.CarStatusChart)
                {
                    StatusPieChart.Series.Add(new PieSeries
                    {
                        Title = status.Label,
                        Values = new ChartValues<double> { status.Value },
                        DataLabels = true,
                        LabelPoint = point => $"{point.Y} ({point.Participation:P0})"
                    });
                }
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки сводки: " + ex.Message, "Ошибка", true);
            }
        }
    }
}