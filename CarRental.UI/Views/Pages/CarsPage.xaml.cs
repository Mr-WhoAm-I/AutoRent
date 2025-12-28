using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views.Windows;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarRental.UI.Views.Pages
{
    public class SelectableItem<T>
    {
        public T Item { get; set; }
        public bool IsSelected { get; set; }
        public SelectableItem(T item) => Item = item;
    }
    public partial class CarsPage : Page
    {
        private readonly CarService _carService = new();
        private readonly ReferenceService _refService = new(); // Добавляем сервис справочников
        private List<Car> _allCars = [];

        // Свойства для привязки (Binding)
        public List<SelectableItem<CarClass>> Classes { get; set; }
        public List<SelectableItem<CarBrand>> Brands { get; set; }
        public List<SelectableItem<BodyType>> BodyTypes { get; set; }
        public List<SelectableItem<FuelType>> FuelTypes { get; set; }
        public List<SelectableItem<TransmissionType>> Transmissions { get; set; }


        public CarsPage()
        {
            InitializeComponent();
            LoadFilters(); // Загружаем данные справочников
            DataContext = this; // Важно! Чтобы XAML видел наши списки
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCars(); 
            SetupAccessControl();
        }

        private void SetupAccessControl()
        {
            // Проверяем роль (предполагаем, что AuthService.CurrentUser заполнен при входе)
            bool isAdmin = AuthService.CurrentUser?.RoleName == "Администратор";

            // 1. Кнопка Добавить
            if (BtnAdd != null)
            {
                BtnAdd.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;
            }

            // 2. Колонка-разделитель перед кнопкой (опционально, чтобы убрать лишний отступ)
            // Но в Grid WPF если колонка Auto и элемент Collapsed, она схлопнется сама.
        }

        private void LoadFilters()
        {
            Classes = _refService.GetClasses().Select(x => new SelectableItem<CarClass>(x)).ToList();
            Brands = _refService.GetBrands().Select(x => new SelectableItem<CarBrand>(x)).ToList();
            BodyTypes = _refService.GetBodyTypes().Select(x => new SelectableItem<BodyType>(x)).ToList();
            FuelTypes = _refService.GetFuelTypes().Select(x => new SelectableItem<FuelType>(x)).ToList();
            Transmissions = _refService.GetTransmissions().Select(x => new SelectableItem<TransmissionType>(x)).ToList();
        }

        private void LoadCars()
        {
            _allCars = _carService.GetCars();

            // === 1. Настройка прав (карандаши) ===
            bool isAdmin = AuthService.CurrentUser?.RoleName == "Администратор";
            foreach (var car in _allCars) car.EditVisibility = isAdmin ? "Visible" : "Collapsed";

            // === 2. Настройка ЦЕН (Мин/Макс) из базы ===
            if (_allCars.Any())
            {
                double minPrice = (double)_allCars.Min(c => c.PricePerDay);
                double maxPrice = (double)_allCars.Max(c => c.PricePerDay);

                // Даем небольшой запас, чтобы ползунок был красивым
                PriceRange.Minimum = Math.Floor(minPrice / 10) * 10;
                PriceRange.Maximum = Math.Ceiling(maxPrice / 10) * 10;

                // Ставим ползунки на края
                PriceRange.ValueStart = PriceRange.Minimum;
                PriceRange.ValueEnd = PriceRange.Maximum;
            }

            ApplyFiltersAndSort();
        }

        private void Page_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Keyboard.ClearFocus();
        }

        private void Filter_Changed(object sender, RoutedEventArgs e) => ApplyFiltersAndSort();
        private void Sort_Changed(object sender, RoutedEventArgs e) => ApplyFiltersAndSort();
        private void ApplyFilters_Click(object sender, RoutedEventArgs e) => ApplyFiltersAndSort();

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем чекбоксы
            foreach (var i in Classes) i.IsSelected = false;
            foreach (var i in Brands) i.IsSelected = false;
            foreach (var i in BodyTypes) i.IsSelected = false;
            foreach (var i in FuelTypes) i.IsSelected = false;
            foreach (var i in Transmissions) i.IsSelected = false;

            // Обновляем UI чекбоксов (костыль для обновления привязок, если не INotifyPropertyChanged)
            // Проще всего переприсвоить DataContext или вызвать Apply, 
            // но так как SelectableItem простой, ListBox может не увидеть изменений сразу.
            // Для надежности можно перезагрузить ItemsSource, но пока просто сбросим значения и применим.

            // Сбрасываем цену
            PriceRange.ValueStart = PriceRange.Minimum;
            PriceRange.ValueEnd = PriceRange.Maximum;

            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            if (SortDirectionBtn == null || SortCombo == null || _allCars == null) return;

            var query = _allCars.AsEnumerable();

            // 1. ПОИСК (Бренд или Модель)
            string searchText = SearchBox.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(c => c.BrandName.ToLower().Contains(searchText) ||
                                         c.Model.ToLower().Contains(searchText));
            }

            // 2. ДАТЫ (Доступность)
            if (DateStartPicker.SelectedDate.HasValue || DateEndPicker.SelectedDate.HasValue)
            {
                DateTime start = DateStartPicker.SelectedDate ?? DateTime.Today;
                DateTime end = DateEndPicker.SelectedDate ?? start.AddDays(7); // Если нет "По", берем неделю

                // Получаем ID занятых машин
                var busyCarIds = _carService.GetOccupiedCarIds(start, end);

                // Оставляем только те, которых НЕТ в списке занятых
                query = query.Where(c => !busyCarIds.Contains(c.Id));
            }

            // 3. ЦЕНА
            query = query.Where(c => (double)c.PricePerDay >= PriceRange.ValueStart &&
                                     (double)c.PricePerDay <= PriceRange.ValueEnd);

            // 4. ЧЕКБОКСЫ (Если хоть один выбран в группе, фильтруем по нему)

            // Класс
            var selectedClasses = Classes.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();
            if (selectedClasses.Any()) query = query.Where(c => selectedClasses.Contains(c.ClassId));

            // Марка
            var selectedBrands = Brands.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();
            if (selectedBrands.Any()) query = query.Where(c => selectedBrands.Contains(c.BrandId));

            // Кузов
            var selectedBodies = BodyTypes.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();
            if (selectedBodies.Any()) query = query.Where(c => selectedBodies.Contains(c.BodyTypeId));

            // Топливо
            var selectedFuels = FuelTypes.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();
            if (selectedFuels.Any()) query = query.Where(c => selectedFuels.Contains(c.FuelId));

            // Коробка
            var selectedTrans = Transmissions.Where(x => x.IsSelected).Select(x => x.Item.Id).ToList();
            if (selectedTrans.Any()) query = query.Where(c => selectedTrans.Contains(c.TransmissionId));


            // 5. СОРТИРОВКА
            int sortIndex = SortCombo.SelectedIndex;
            bool isDesc = SortDirectionBtn.IsChecked == true;

            switch (sortIndex)
            {
                case 0: // Цена
                    query = isDesc ? query.OrderByDescending(c => c.PricePerDay) : query.OrderBy(c => c.PricePerDay);
                    break;
                case 1: // Название
                    query = isDesc ? query.OrderByDescending(c => c.BrandAndModel) : query.OrderBy(c => c.BrandAndModel);
                    break;
            }

            // 6. ФИНАЛ
            var resultList = query.ToList();
            CarsGridControl.ItemsSource = resultList;
            CarsListControl.ItemsSource = resultList;
        }

        private void ViewType_Changed(object sender, RoutedEventArgs e)
        {
            if (CarsGridControl == null || CarsListControl == null) return;

            if (ViewGridBtn.IsChecked == true)
            {
                CarsGridControl.Visibility = Visibility.Visible;
                CarsListControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                CarsGridControl.Visibility = Visibility.Collapsed;
                CarsListControl.Visibility = Visibility.Visible;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var win = new CarWindow(); // Создаем окно в режиме "Добавление"
            win.ShowDialog(); // Показываем модально

            if (win.IsSuccess)
            {
                LoadCars(); // Перезагружаем список, если сохранили
                InfoDialog.Show("Автомобиль успешно добавлен!", "Готово");
            }
        }

        private void Car_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Car selectedCar)
            {
                // Пока просто MessageBox или открытие окна, если оно уже готово
                MessageBox.Show($"Выбрана машина: {selectedCar.Model}");
            }
        }

        private void EditCar_Click(object sender, RoutedEventArgs e)
        {
            // Кнопка сама перехватывает клик, но на всякий случай останавливаем всплытие
            e.Handled = true;

            if ((sender as FrameworkElement).DataContext is Car selectedCar)
            {
                var win = new CarWindow(selectedCar);
                win.ShowDialog();

                if (win.IsSuccess)
                {
                    LoadCars();
                    InfoDialog.Show("Данные автомобиля обновлены!", "Успех");
                }
            }
        }
    }
}