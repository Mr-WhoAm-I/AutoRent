using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CarRental.UI.Views
{
    public partial class CarDetailsWindow : Window
    {
        private readonly Car _car;
        private readonly CarService _service = new();
        private readonly MaintenanceService _maintService = new(); // Для вкладки ТО

        public CarDetailsWindow(Car car)
        {
            InitializeComponent();
            _car = car;
            LoadCarInfo();
            BuildCalendar();
            LoadMaintenanceHistory();
        }

        private void LoadCarInfo()
        {
            CarTitle.Text = $"{_car.BrandName} {_car.Model}";
            CarPlate.Text = _car.PlateNumber;
            CarClass.Text = _car.ClassName;
            TxtYear.Text = _car.Year.ToString();
            TxtMileage.Text = $"{_car.Mileage:N0} км";
            TxtPrice.Text = $"{_car.PricePerDay:N0} ₽";
            TxtStatus.Text = _car.StatusName;

            // TODO: Загрузка картинки
            // if (!string.IsNullOrEmpty(_car.ImagePath)) ...
        }

        private void LoadMaintenanceHistory()
        {
            // Здесь нужен метод получения истории ПО ID АВТОМОБИЛЯ
            // Пока используем общую историю как заглушку, но в идеале: _maintService.GetHistoryByCarId(_car.Id)
            MaintenanceGrid.ItemsSource = _maintService.GetHistory().Where(m => m.CarId == _car.Id).ToList();
        }

        // === САМОЕ ВАЖНОЕ: ГЕНЕРАТОР КАЛЕНДАРЯ ===
        private void BuildCalendar()
        {
            CalendarGrid.Children.Clear();

            // 1. Параметры (Текущий месяц)
            int year = 2025; // Для демо (в базе даты 2024-2025)
            int month = 12;  // Декабрь
            DateTime firstDay = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);

            // Сдвиг начала недели (Пн=1, ... Вс=7) -> DayOfWeek (Вс=0, Пн=1)
            int startOffset = ((int)firstDay.DayOfWeek == 0) ? 6 : (int)firstDay.DayOfWeek - 1;

            // 2. Получаем занятость
            var schedule = _service.GetCarSchedule(_car.Id);

            // 3. Рисуем пустые ячейки (сдвиг)
            for (int i = 0; i < startOffset; i++)
            {
                CalendarGrid.Children.Add(new Border());
            }

            // 4. Рисуем дни
            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime currentDate = new DateTime(year, month, day);

                // Проверяем статус дня
                bool isOccupied = schedule.Any(s => currentDate >= s.Start.Date && currentDate <= s.End.Date);

                // Создаем UI элемента дня
                Border dayCell = new Border
                {
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(2),
                    Height = 35,
                    Width = 35,
                    Cursor = Cursors.Hand
                };

                TextBlock dayText = new TextBlock
                {
                    Text = day.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 12
                };

                if (isOccupied)
                {
                    dayCell.Background = new SolidColorBrush(Color.FromRgb(238, 242, 255)); // Светло-голубой
                    dayText.Foreground = new SolidColorBrush(Color.FromRgb(79, 70, 229)); // Индиго
                    dayText.FontWeight = FontWeights.Bold;
                }
                else
                {
                    dayCell.Background = Brushes.Transparent;
                    dayText.Foreground = Brushes.Black;

                    // Ховер эффект (события)
                    dayCell.MouseEnter += (s, e) => { (s as Border).Background = Brushes.WhiteSmoke; };
                    dayCell.MouseLeave += (s, e) => { (s as Border).Background = Brushes.Transparent; };
                }

                dayCell.Child = dayText;
                CalendarGrid.Children.Add(dayCell);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}