using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CarRental.BLL.Services; // Добавить
using CarRental.Domain.Entities;

namespace CarRental.UI.Views.Windows
{
    public partial class CarDetailsWindow : Window
    {
        private readonly Car _car;
        private readonly CarService _service = new();
        private readonly MaintenanceService _maintService = new();
        private readonly InsuranceService _insService = new();

        private string _userRole;

        private DateTime _currentMonth;
        private DateTime? _selectedStart;
        private DateTime? _selectedEnd;

        // Список событий календаря (из БД)
        private List<CalendarItem> _schedule = new();


        // Конструктор теперь принимает опциональные даты (из фильтра)
        public CarDetailsWindow(Car car, DateTime? filterStart = null, DateTime? filterEnd = null)
        {
            InitializeComponent();
            _car = car;

            DataContext = _car;

            _userRole = AuthService.CurrentUser?.RoleName ?? "Гость";
            // Если есть фильтр - начинаем с месяца фильтра, иначе с текущего
            _currentMonth = filterStart ?? DateTime.Today;

            // Применяем фильтр как выбранный диапазон
            if (filterStart != null)
            {
                _selectedStart = filterStart;
                _selectedEnd = filterEnd; // Если End null, будет выбран 1 день
            }

            SetupAccessControl();
            LoadCarInfo();
            if (TabInfo.Visibility == Visibility.Visible)
            {
                LoadSchedule();
                DrawCalendar();
                UpdateCalculation();
            }

            LoadMaintenanceHistory();
            LoadInsurance();
        }

        private void SetupAccessControl()
        {
            // 1. Механик: Видит ТОЛЬКО вкладку ТО
            if (_userRole == "Механик")
            {
                TabInfo.Visibility = Visibility.Collapsed;      // Скрыть календарь
                TabInsurance.Visibility = Visibility.Collapsed; // Скрыть страховку

                // Переключаем на вкладку ТО
                TabMaintenance.IsSelected = true;
            }

            // 2. Кнопки "Добавить/Редактировать"
            // Менеджер НЕ может редактировать ТО и Страховки
            bool canEdit = _userRole == "Администратор" || _userRole == "Механик";
            bool isAdmin = _userRole == "Администратор";

            // Кнопка "+ Добавить запись ТО"
            BtnAddMaint.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            // Кнопка "+ Добавить полис" (Только Админ)
            BtnAddInsurance.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LoadCarInfo()
        {
            TxtTitle.Text = $"{_car.BrandName} {_car.Model}";
            TxtClass.Text = _car.ClassName;
            TxtPlate.Text = _car.PlateNumber;
            TxtYear.Text = _car.Year.ToString();
            TxtMileage.Text = $"{_car.Mileage:N0} км";

            TxtBody.Text = _car.BodyTypeName;
            TxtFuel.Text = _car.FuelName;
            TxtTrans.Text = _car.TransmissionName;
            TxtPricePerDay.Text = $"{_car.PricePerDay:N0} BYN";

            if (!string.IsNullOrEmpty(_car.ImagePath))
            {
                try { ImgCar.Source = new BitmapImage(new Uri(_car.ImagePath)); } catch { }
            }
        }
        private void LoadSchedule()
        {
            try
            {
                // Получаем реальные данные из БД
                _schedule = _service.GetCarSchedule(_car.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки расписания: " + ex.Message);
            }
        }

        private void DrawCalendar()
        {
            CalendarGrid.Children.Clear();
            TxtCalendarMonth.Text = _currentMonth.ToString("MMMM yyyy");

            DateTime firstDayOfMonth = new DateTime(_currentMonth.Year, _currentMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentMonth.Year, _currentMonth.Month);
            int startOffset = (int)firstDayOfMonth.DayOfWeek - 1;
            if (startOffset < 0) startOffset = 6;

            for (int i = 0; i < startOffset; i++) CalendarGrid.Children.Add(new Border());

            for (int day = 1; day <= daysInMonth; day++)
            {
                DateTime date = new DateTime(_currentMonth.Year, _currentMonth.Month, day);

                Border dayCell = new Border
                {
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(2),
                    Height = 40,
                    Tag = date
                };
                ToolTipService.SetInitialShowDelay(dayCell, 0);

                TextBlock txt = new TextBlock
                {
                    Text = day.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Medium
                };

                // 1. Поиск событий базы данных
                var dbEvent = _schedule.FirstOrDefault(s => date >= s.Start.Date && date <= s.End.Date);

                // 2. Проверка выбора пользователя
                bool isUserSelection = IsDateSelected(date);

                // --- ОПРЕДЕЛЕНИЕ СТИЛЯ ---
                if (dbEvent != null)
                {
                    // === ДЕНЬ ЗАНЯТ (База) ===
                    dayCell.Cursor = Cursors.Arrow;
                    dayCell.ToolTip = dbEvent.TooltipText;

                    // Базовые цвета
                    Color baseColor = Colors.Gray;
                    if (dbEvent.Type == "Rental") baseColor = (Color)ColorConverter.ConvertFromString("#4F46E5"); // Индиго
                    else if (dbEvent.Type == "Booking") baseColor = (Color)ColorConverter.ConvertFromString("#9C27B0"); // Фиолетовый
                    else if (dbEvent.Type == "Maintenance") baseColor = (Color)ColorConverter.ConvertFromString("#EF4444"); // Красный

                    // Проверяем, край это или середина
                    bool isStart = date == dbEvent.Start.Date;
                    bool isEnd = date == dbEvent.End.Date;

                    if (isStart || isEnd)
                    {
                        // Края: Сплошной цвет, Белый текст
                        dayCell.Background = new SolidColorBrush(baseColor);
                        txt.Foreground = Brushes.White;
                    }
                    else
                    {
                        // Середина: Полупрозрачный фон, Цветной текст
                        dayCell.Background = new SolidColorBrush(Color.FromArgb(50, baseColor.R, baseColor.G, baseColor.B)); // ~20%
                        txt.Foreground = new SolidColorBrush(baseColor);
                    }

                    // Для ремонта всегда зачеркиваем
                    if (dbEvent.Type == "Maintenance")
                    {
                        txt.TextDecorations = TextDecorations.Strikethrough;
                    }
                }
                else if (isUserSelection)
                {
                    // === ВЫБРАНО ПОЛЬЗОВАТЕЛЕМ ===
                    dayCell.Cursor = Cursors.Hand;
                    dayCell.MouseLeftButtonUp += Day_Click; // Разрешаем клик (для перевыбора)

                    Color green = (Color)Application.Current.Resources["VibrantShadowColor"]; // Или берем GreenAccentBrush
                    // Для надежности возьмем жестко, т.к. ресурс может быть Brush
                    green = (Color)ColorConverter.ConvertFromString("#00C853");

                    // Проверяем края выбора
                    bool isSelStart = _selectedStart != null && date == _selectedStart.Value;
                    bool isSelEnd = _selectedEnd != null && date == _selectedEnd.Value;
                    // Если выбран 1 день, он и старт и конец

                    if (isSelStart || isSelEnd)
                    {
                        dayCell.Background = new SolidColorBrush(green);
                        txt.Foreground = Brushes.White;
                        txt.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        // Середина выбора
                        dayCell.Background = new SolidColorBrush(Color.FromArgb(50, green.R, green.G, green.B));
                        txt.Foreground = new SolidColorBrush(green);
                        txt.FontWeight = FontWeights.Bold;
                    }
                }
                else
                {
                    // === СВОБОДНО ИЛИ ПРОШЛОЕ ===
                    if (date < DateTime.Today)
                    {
                        dayCell.Background = Brushes.Transparent;
                        txt.Foreground = Brushes.LightGray;
                        dayCell.Cursor = Cursors.Arrow;
                    }
                    else
                    {
                        // ВАЖНО: Background="Transparent" нужен для клика!
                        dayCell.Background = Brushes.Transparent;
                        txt.Foreground = Brushes.Black;
                        dayCell.Cursor = Cursors.Hand;

                        // Ховер
                        dayCell.MouseEnter += (s, e) => { (s as Border).Background = (Brush)Application.Current.Resources["GreenHoverBrush"]; };
                        dayCell.MouseLeave += (s, e) => { (s as Border).Background = Brushes.Transparent; };

                        dayCell.MouseLeftButtonUp += Day_Click;
                    }
                }

                dayCell.Child = txt;
                CalendarGrid.Children.Add(dayCell);
            }
        }

        // Обновленный метод клика (чтобы сбрасывал выбор при клике на свободное место)
        private void Day_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is DateTime date)
            {
                // Если мы кликаем, когда уже выбран диапазон -> сбрасываем и начинаем новый выбор с этой даты
                if (_selectedStart != null && _selectedEnd != null)
                {
                    _selectedStart = date;
                    _selectedEnd = null;
                }
                else if (_selectedStart == null)
                {
                    _selectedStart = date;
                }
                else
                {
                    // Второй клик (выбор конца)
                    if (date < _selectedStart)
                    {
                        _selectedEnd = _selectedStart;
                        _selectedStart = date;
                    }
                    else
                    {
                        _selectedEnd = date;
                    }

                    // Проверка на "дырки" (занятость внутри)
                    if (HasBusyDatesBetween(_selectedStart.Value, _selectedEnd.Value))
                    {
                        InfoDialog.Show("Выбранный период пересекается с занятыми днями!", "Ошибка", true);
                        _selectedStart = date; // Сбрасываем, оставляем только текущий клик
                        _selectedEnd = null;
                    }
                }

                DrawCalendar();
                UpdateCalculation();
            }
        }

        private void LoadMaintenanceHistory()
        {
            try
            {
                MaintenanceGrid.ItemsSource = _maintService.GetHistoryByCarId(_car.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории ТО: " + ex.Message);
            }
        }

        private void LoadInsurance()
        {
            try
            {
                InsuranceGrid.ItemsSource = _insService.GetHistory(_car.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки страховок: " + ex.Message);
            }
        }

        // Проверка: Дата попадает в наш выбор?
        private bool IsDateSelected(DateTime date)
        {
            if (_selectedStart == null) return false;
            if (_selectedEnd == null) return date == _selectedStart.Value;
            return date >= _selectedStart.Value && date <= _selectedEnd.Value;
        }

        // Проверка: Есть ли занятые дни внутри диапазона?
        private bool HasBusyDatesBetween(DateTime start, DateTime end)
        {
            return _schedule.Any(s => s.End.Date >= start && s.Start.Date <= end);
        }

        // ... Остальные методы (UpdateCalculation, LoadCarInfo, Nav buttons) ...
        // (Они остаются такими же, как в прошлом ответе, просто скопируйте их)
        private void UpdateCalculation()
        {
            if (_selectedStart == null)
            {
                TxtTotalDays.Text = "ВЫБРАНО: 0 СУТ";
                TxtDates.Text = "Выберите даты";
                TxtTotalPrice.Text = "0 BYN";
                BtnAction.IsEnabled = false;
                BtnAction.Content = "Выберите даты";
                return;
            }

            DateTime start = _selectedStart.Value;
            DateTime end = _selectedEnd ?? start;

            int days = (end - start).Days + 1;
            decimal total = days * _car.PricePerDay;

            TxtTotalDays.Text = $"ВЫБРАНО: {days} СУТ";
            TxtDates.Text = $"{start:dd.MM} — {end:dd.MM.yyyy}";
            TxtTotalPrice.Text = $"{total:N0} BYN";

            BtnAction.IsEnabled = true;
            BtnAction.Content = (start.Date == DateTime.Today) ? "ОФОРМИТЬ АРЕНДУ" : "ЗАБРОНИРОВАТЬ";
        }

        private void PrevMonth_Click(object sender, RoutedEventArgs e) { _currentMonth = _currentMonth.AddMonths(-1); DrawCalendar(); }
        private void NextMonth_Click(object sender, RoutedEventArgs e) { _currentMonth = _currentMonth.AddMonths(1); DrawCalendar(); }
        private void BtnAction_Click(object sender, RoutedEventArgs e) { MessageBox.Show("Переход к оформлению..."); }
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }

        private void AddMaintenance_Click(object sender, MouseButtonEventArgs e)
        {
            InfoDialog.Show("Здесь откроется окно добавления ТО.", "В разработке");
        }

        // Двойной клик по таблице ТО (Редактирование)
        private void MaintenanceGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Проверка прав
            if (_userRole == "Менеджер") return;

            if (MaintenanceGrid.SelectedItem is Maintenance selectedMaint)
            {
                InfoDialog.Show($"Редактирование записи ТО от {selectedMaint.DateStart:d}", "В разработке");
            }
        }

        // Клик по "+ Добавить полис"
        private void AddInsurance_Click(object sender, MouseButtonEventArgs e)
        {
            InfoDialog.Show("Здесь откроется окно добавления страховки.", "В разработке");
        }

        // Двойной клик по таблице Страховок
        private void InsuranceGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_userRole != "Администратор") return;

            if (InsuranceGrid.SelectedItem is Insurance selectedIns)
            {
                InfoDialog.Show($"Редактирование полиса №{selectedIns.PolicyNumber}", "В разработке");
            }
        }
    }
}