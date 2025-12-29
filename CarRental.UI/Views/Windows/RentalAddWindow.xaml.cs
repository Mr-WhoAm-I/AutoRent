using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views; // Для InfoDialog

namespace CarRental.UI.Views.Windows
{
    public partial class RentalAddWindow : Window
    {
        private readonly CarService _carService = new();
        private readonly ClientService _clientService = new();
        private readonly EmployeeService _employeeService = new();
        private readonly RentalService _rentalService = new();

        private Rental _currentRental; // Если null - создание, иначе редактирование
        public bool IsSuccess { get; private set; }

        // Конструктор для СОЗДАНИЯ
        public RentalAddWindow()
        {
            InitializeComponent();
            _currentRental = new Rental();
            LoadInitialData();
        }

        public RentalAddWindow(Rental rental)
        {
            InitializeComponent();
            _currentRental = rental;

            // Если ID == 0, значит это создание новой аренды (передали заготовку из карточки)
            if (_currentRental.Id == 0)
            {
                TxtTitle.Text = "Новая аренда";
                BtnSave.Content = "Оформить";
                PanelFinishRental.Visibility = Visibility.Collapsed; // Скрываем кнопку завершения
            }
            else
            {
                TxtTitle.Text = $"Аренда №{rental.Id}";
                BtnSave.Content = "Сохранить";

                // Показываем кнопку завершения, ТОЛЬКО если это существующая аренда и она еще активна
                if (_currentRental.ActualEndDate == null)
                {
                    PanelFinishRental.Visibility = Visibility.Visible;
                }
                else
                {
                    PanelFinishRental.Visibility = Visibility.Collapsed;
                }
            }

            LoadInitialData();
            FillData();
        }

        private void LoadInitialData()
        {
            try
            {
                // Загружаем списки клиентов и сотрудников
                ComboClient.ItemsSource = _clientService.GetClients();
                ComboEmployee.ItemsSource = _employeeService.GetManagersAndAdmins();

                // Устанавливаем текущую дату
                DateStart.SelectedDate = DateTime.Today;
                DateEnd.SelectedDate = DateTime.Today.AddDays(1);
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка", true);
            }
        }

        private void FillData()
        {
            // 1. Устанавливаем даты
            // Отключаем событие изменения дат, чтобы не сбросилась машина
            DateStart.SelectedDateChanged -= Date_Changed;
            DateEnd.SelectedDateChanged -= Date_Changed;

            DateStart.SelectedDate = _currentRental.StartDate;
            DateEnd.SelectedDate = _currentRental.PlannedEndDate;

            DateStart.SelectedDateChanged += Date_Changed;
            DateEnd.SelectedDateChanged += Date_Changed;

            // 2. Выбираем Клиента и Сотрудника
            // (Ищем в списках по ID)
            foreach (Client c in ComboClient.Items)
                if (c.Id == _currentRental.ClientId) ComboClient.SelectedItem = c;

            foreach (Employee e in ComboEmployee.Items)
                if (e.Id == _currentRental.EmployeeId) ComboEmployee.SelectedItem = e;

            // 3. Машина
            // В режиме редактирования мы должны добавить ТЕКУЩУЮ машину в список, 
            // даже если она занята (нами же), иначе валидация GetAvailableCars её не вернет.
            // Но для простоты пока загрузим доступные + подгрузим нашу
            LoadAvailableCars(_currentRental.StartDate, _currentRental.PlannedEndDate);

            // Если нашей машины нет в списке (потому что она занята этой арендой), 
            // нужно её подгрузить отдельно и добавить в комбобокс
            var cars = ComboCar.ItemsSource as System.Collections.Generic.List<Car>;
            if (cars != null && !cars.Any(c => c.Id == _currentRental.CarId))
            {
                // Ищем машину в базе, так как GetAvailable её отфильтровал
                // В данном случае можно просто взять её из базы
                var currentCar = _carService.GetCars().FirstOrDefault(c => c.Id == _currentRental.CarId);
                if (currentCar != null)
                {
                    cars.Insert(0, currentCar);
                    ComboCar.ItemsSource = null; // Передергиваем
                    ComboCar.ItemsSource = cars;
                }
            }

            // Выбираем машину
            foreach (Car c in ComboCar.Items)
                if (c.Id == _currentRental.CarId) ComboCar.SelectedItem = c;

            ComboCar.IsEnabled = true;
            RecalculatePrice();
        }

        // --- ЛОГИКА ВЫБОРА АВТО ---

        private void Date_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (DateStart.SelectedDate == null || DateEnd.SelectedDate == null) return;

            DateTime start = DateStart.SelectedDate.Value;
            DateTime end = DateEnd.SelectedDate.Value;

            if (start > end)
            {
                // Если начали выбирать и дата старта больше конца - не ругаемся сразу, ждем
                return;
            }

            LoadAvailableCars(start, end);
            RecalculatePrice();
        }

        private void LoadAvailableCars(DateTime start, DateTime end)
        {
            try
            {
                // 1. Запоминаем текущий выбор (ID машины), чтобы он не слетел при обновлении списка
                int? selectedCarId = (ComboCar.SelectedItem as Car)?.Id;

                // 2. Получаем список, ИСКЛЮЧАЯ текущую аренду из проверки занятости
                // Если _currentRental.Id == 0 (новая), то фильтр просто никого не исключит, что ок.
                var cars = _carService.GetAvailableCars(start, end, _currentRental.Id);

                ComboCar.ItemsSource = cars;
                ComboCar.IsEnabled = true;

                if (cars.Count == 0)
                {
                    ComboCar.Tag = "Нет свободных машин на эти даты";
                }
                else
                {
                    ComboCar.Tag = "Выберите автомобиль";
                }

                // 3. Восстанавливаем выбор, если эта машина все еще доступна (или это наша текущая)
                if (selectedCarId.HasValue)
                {
                    // Пытаемся найти эту машину в новом списке
                    var carToSelect = cars.FirstOrDefault(c => c.Id == selectedCarId.Value);
                    if (carToSelect != null)
                    {
                        ComboCar.SelectedItem = carToSelect;
                    }
                }
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка поиска авто: " + ex.Message, "Ошибка", true);
            }
        }

        // --- ПЕРЕСЧЕТ ЦЕНЫ ---

        private void ComboCar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RecalculatePrice();
        }

        private void RecalculatePrice()
        {
            if (ComboCar.SelectedItem is Car car && DateStart.SelectedDate.HasValue && DateEnd.SelectedDate.HasValue)
            {
                try
                {
                    decimal total = _rentalService.CalculateCost(
                        DateStart.SelectedDate.Value,
                        DateEnd.SelectedDate.Value,
                        car.PricePerDay
                    );

                    var days = (DateEnd.SelectedDate.Value - DateStart.SelectedDate.Value).Days;
                    if (days < 1) days = 1;

                    TxtTotal.Text = $"{total:N2} BYN";
                    TxtDays.Text = $"{days} сут.";
                    TxtPricePerDay.Text = $"{car.PricePerDay:N0} BYN/сут.";
                }
                catch (Exception ex)
                {
                    TxtTotal.Text = "Ошибка БД";
                }
            }
            else
            {
                TxtTotal.Text = "0.00 BYN";
                TxtDays.Text = "0 сут.";
                TxtPricePerDay.Text = "";
            }
        }

        // --- СОХРАНЕНИЕ ---

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // 1. Валидация
            if (DateStart.SelectedDate == null || DateEnd.SelectedDate == null) { InfoDialog.Show("Выберите даты аренды."); return; }
            if (ComboCar.SelectedItem is not Car car) { InfoDialog.Show("Выберите автомобиль."); return; }
            if (ComboClient.SelectedItem is not Client client) { InfoDialog.Show("Выберите клиента."); return; }
            if (ComboEmployee.SelectedItem is not Employee emp) { InfoDialog.Show("Выберите сотрудника."); return; }

            if (DateStart.SelectedDate > DateEnd.SelectedDate) { InfoDialog.Show("Дата начала не может быть позже окончания."); return; }

            try
            {
                // 2. Заполнение объекта данными из полей
                _currentRental.ClientId = client.Id;
                _currentRental.CarId = car.Id;
                _currentRental.EmployeeId = emp.Id;

                // ВАЖНО: При редактировании мы перезаписываем старые значения новыми
                _currentRental.StartDate = DateStart.SelectedDate.Value;
                _currentRental.PlannedEndDate = DateEnd.SelectedDate.Value;
                _currentRental.PriceAtRentalMoment = car.PricePerDay; // Можно оставить старую цену или обновить, по бизнес-логике

                // 3. ПРОВЕРКА: Создание или Обновление?
                if (_currentRental.Id == 0)
                {
                    // Новая аренда
                    _rentalService.CreateRental(_currentRental);
                    InfoDialog.Show("Аренда успешно оформлена!", "Успех");
                }
                else
                {
                    // Редактирование существующей
                    _rentalService.UpdateRental(_currentRental);
                    InfoDialog.Show("Параметры аренды обновлены!", "Успех");
                }

                IsSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка сохранения: " + ex.Message, "Ошибка", true);
            }
        }

        private void FinishRental_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Завершить аренду сейчас?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _rentalService.FinishRental(_currentRental.Id);
                    IsSuccess = true;
                    Close();
                }
                catch (Exception ex)
                {
                    InfoDialog.Show(ex.Message, "Ошибка", true);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}