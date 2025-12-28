using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.Domain.DTO;

namespace CarRental.UI.Views.Windows
{
    // Класс для динамической легенды
    public class LegendItem
    {
        public string Name { get; set; } = string.Empty;
        public Brush ColorBrush { get; set; }
        public bool IsHatched { get; set; } // Для брони
    }

    public partial class ClientWindow : Window
    {
        private readonly Client _client;
        private readonly ClientService _clientService = new();
        private readonly CarService _carService = new();

        private List<ClientHistoryItem> _fullHistory; // Вся история
        private int _selectedYear; // Текущий выбранный год


        public ClientWindow(Client client)
        {
            InitializeComponent();
            _client = client;

            LoadClientInfo();
            LoadHistory(); // Загружает данные и инициализирует годы
        }

        private void LoadClientInfo()
        {
            string initials = (!string.IsNullOrEmpty(_client.Surname) ? _client.Surname[0].ToString() : "") +
                              (!string.IsNullOrEmpty(_client.Name) ? _client.Name[0].ToString() : "");
            TxtInitials.Text = initials.ToUpper();

            TxtFullName.Text = _client.FullName;
            TxtPhone.Text = _client.Phone;
            TxtEmail.Text = _client.Email ?? "нет email";
            TxtAge.Text = _client.AgeString;
            TxtExp.Text = $"Стаж: {_client.ExperienceString}";
        }

        private void LoadHistory()
        {
            _fullHistory = _clientService.GetClientHistory(_client.Id);

            // Загружаем список лет для выбора
            // Находим минимальный год или берем текущий
            int minYear = _fullHistory.Any() ? _fullHistory.Min(x => x.StartDate.Year) : DateTime.Now.Year;
            int currentYear = DateTime.Now.Year;

            // Если есть брони в будущем (на след. год), берем и их
            int maxYear = _fullHistory.Any() ? Math.Max(currentYear, _fullHistory.Max(x => x.EndDate.Year)) : currentYear;

            var years = new List<int>();
            for (int y = maxYear; y >= minYear; y--)
            {
                years.Add(y);
            }

            ListYears.ItemsSource = years;
            ListYears.SelectedItem = currentYear; // Выбираем текущий год (сработает событие SelectionChanged)
        }

        // Событие смены года
        private void ListYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListYears.SelectedItem is int year)
            {
                _selectedYear = year;
                TxtActivityTitle.Text = $"История активности ({year} год)";

                DrawActivityGraph(year);
                UpdateTable(year);
            }
        }

        private void UpdateTable(int year)
        {
            // В таблице показываем только записи, которые пересекаются с выбранным годом
            var yearHistory = _fullHistory.Where(h => h.StartDate.Year == year || h.EndDate.Year == year).ToList();
            HistoryGrid.ItemsSource = yearHistory;
        }

        private void DrawActivityGraph(int year)
        {
            ActivityGrid.Children.Clear();
            ActivityGrid.ColumnDefinitions.Clear();
            ActivityGrid.RowDefinitions.Clear();

            // 1. Создаем строки (0=Месяцы, 1-7=Дни)
            ActivityGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) }); // Месяцы
            for (int i = 0; i < 7; i++)
                ActivityGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) }); // Дни

            // Даты
            DateTime startDate = new DateTime(year, 1, 1);
            DateTime endDate = new DateTime(year, 12, 31);

            int dayOfWeekOffset = ((int)startDate.DayOfWeek + 6) % 7;

            int currentColumn = 0;
            int currentRow = dayOfWeekOffset + 1;

            ActivityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15) });

            int totalDays = 0;
            var classesUsedInYear = new HashSet<string>();
            bool hasBookingInYear = false;

            DrawMonthLabels(year);

            // === ОТРИСОВКА КВАДРАТИКОВ ===
            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                // ИЗМЕНЕНИЕ 1: Ищем ВСЕ активности на эту дату
                var activities = _fullHistory
                    .Where(h => date >= h.StartDate.Date && date <= h.EndDate.Date)
                    .ToList();

                // Создаем контейнер для дня
                Border boxContainer = new Border
                {
                    Width = 11,
                    Height = 11,
                    CornerRadius = new CornerRadius(2),
                    Margin = new Thickness(1),
                    Background = new SolidColorBrush(Color.FromRgb(235, 237, 240)) // Серый фон по умолчанию
                };

                if (activities.Any())
                {
                    totalDays++; // Считаем дни активности (даже если несколько машин, день считается один раз)

                    // ИЗМЕНЕНИЕ 2: Формируем общий Tooltip
                    string tooltipText = $"{date:dd MMMM}:\n";
                    foreach (var act in activities)
                    {
                        tooltipText += $"• {act.CarTitle} ({act.Status})\n";

                        // Сбор статистики для легенды
                        string carClass = act.CarDetails.Split('•')[0].Trim();
                        classesUsedInYear.Add(carClass);
                        if (act.Type == "Booking") hasBookingInYear = true;
                    }
                    boxContainer.ToolTip = tooltipText.Trim();

                    // ИЗМЕНЕНИЕ 3: Рисуем полоски
                    // Используем UniformGrid для автоматического разделения места
                    var uniformGrid = new System.Windows.Controls.Primitives.UniformGrid
                    {
                        Rows = 1, // Делим по вертикали (полоски стоят рядом)
                        Columns = activities.Count // Столько колонок, сколько машин
                    };

                    // Скругляем сам контейнер UniformGrid, чтобы полоски не вылезали за углы Border
                    // Для этого используем OpacityMask (немного сложнее) или просто Border внутри Border.
                    // Самый простой способ сохранить скругление при разделении цветов -
                    // использовать Clip у контейнера, но в WPF это громоздко.
                    // Проще: пусть будет квадратным внутри, но визуально это почти не заметно на 11px.

                    foreach (var act in activities)
                    {
                        string carClass = act.CarDetails.Split('•')[0].Trim();
                        Brush baseBrush = GetBrushForClass(carClass);

                        // Создаем "ломтик" для одной машины
                        Border slice = new Border
                        {
                            Background = baseBrush
                        };

                        // Если Бронь - добавляем штриховку поверх
                        if (act.Type == "Booking")
                        {
                            Grid overlay = new Grid();
                            // Фон уже задан у slice
                            overlay.Children.Add(new System.Windows.Shapes.Rectangle
                            {
                                Fill = (Brush)FindResource("HatchedBrush")
                            });
                            slice.Child = overlay;
                        }

                        uniformGrid.Children.Add(slice);
                    }

                    // Важно: Чтобы скругление работало, нужно обрезать содержимое. 
                    // Но для простоты просто положим сетку внутрь.
                    boxContainer.Child = uniformGrid;
                    boxContainer.Background = Brushes.Transparent; // Фон перекрывается полосками
                }
                else
                {
                    boxContainer.ToolTip = $"{date:dd MMMM}: Нет активности";
                }

                Grid.SetColumn(boxContainer, currentColumn);
                Grid.SetRow(boxContainer, currentRow);
                ActivityGrid.Children.Add(boxContainer);

                currentRow++;
                if (currentRow > 7)
                {
                    currentRow = 1;
                    currentColumn++;
                    ActivityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(15) });
                }
            }

            TxtActivityStats.Text = $"{totalDays} дн. аренды в {year}";
            GenerateLegend(classesUsedInYear, hasBookingInYear);
        }

        private void DrawMonthLabels(int year)
        {
            string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };

            for (int m = 1; m <= 12; m++)
            {
                DateTime firstDay = new DateTime(year, m, 1);
                // Вычисляем номер недели (колонку) для 1-го числа месяца
                int dayOfYear = firstDay.DayOfYear;
                // Смещение дней с начала года + смещение недели начала года
                int startOffset = ((int)new DateTime(year, 1, 1).DayOfWeek + 6) % 7;
                int column = (dayOfYear + startOffset - 1) / 7;

                TextBlock txt = new TextBlock
                {
                    Text = months[m - 1],
                    FontSize = 10,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(2, 0, 0, 0)
                };

                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, column);
                Grid.SetColumnSpan(txt, 2); // Чтобы влезло название
                ActivityGrid.Children.Add(txt);
            }
        }

        private void GenerateLegend(HashSet<string> classes, bool hasBooking)
        {
            var items = new List<LegendItem>();

            // Проходим по найденным классам и добавляем в легенду
            foreach (var cls in classes)
            {
                items.Add(new LegendItem
                {
                    Name = cls,
                    ColorBrush = GetBrushForClass(cls),
                    IsHatched = false
                });
            }

            // Добавляем бронь только если она была
            if (hasBooking)
            {
                items.Add(new LegendItem
                {
                    Name = "Бронь (штрих)",
                    ColorBrush = Brushes.Gray,
                    IsHatched = true
                });
            }

            LegendControl.ItemsSource = items;
        }

        private Brush GetBrushForClass(string className)
        {
            className = className.ToLower();
            if (className.Contains("эконом")) return (Brush)FindResource("ClassEconomyBrush");
            if (className.Contains("комфорт")) return (Brush)FindResource("ClassComfortBrush");
            if (className.Contains("бизнес")) return (Brush)FindResource("ClassBusinessBrush");
            if (className.Contains("внедорожник")) return (Brush)FindResource("ClassSUVBrush");
            return (Brush)FindResource("ClassOtherBrush");
        }

        private void CarInfo_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is ClientHistoryItem item)
            {
                var car = _carService.GetCars().FirstOrDefault(c => c.Id == item.CarId);
                if (car != null)
                {
                    var win = new CarDetailsWindow(car);
                    win.ShowDialog();
                }
            }
        }
        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            // Открываем окно редактирования, передавая текущего клиента
            var editWin = new ClientEditWindow(_client);
            editWin.ShowDialog();

            if (editWin.IsSuccess)
            {
                // Если сохранили - обновляем данные на экране карточки
                // _client уже обновлен (так как передан по ссылке), просто перерисовываем UI
                LoadClientInfo();
                InfoDialog.Show("Данные клиента обновлены!", "Успех");
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}