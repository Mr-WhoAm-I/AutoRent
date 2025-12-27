using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;

namespace CarRental.UI.Views.Pages
{
    public partial class CarsPage : Page
    {
        private readonly CarService _carService = new();
        private readonly ReferenceService _refService = new(); // Добавляем сервис справочников
        private List<Car> _allCars = [];

        // Свойства для привязки (Binding)
        public List<CarClass> Classes { get; set; }
        public List<CarBrand> Brands { get; set; }
        public List<BodyType> BodyTypes { get; set; }
        public List<FuelType> FuelTypes { get; set; }
        public List<TransmissionType> Transmissions { get; set; }


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
            Classes = _refService.GetClasses();
            Brands = _refService.GetBrands();
            BodyTypes = _refService.GetBodyTypes();
            FuelTypes = _refService.GetFuelTypes();
            Transmissions = _refService.GetTransmissions();
        }

        private void LoadCars()
        {
            _allCars = _carService.GetCars();

            // Настраиваем видимость карандашей для каждой машины
            bool isAdmin = AuthService.CurrentUser?.RoleName == "Администратор";
            foreach (var car in _allCars)
            {
                car.EditVisibility = isAdmin ? "Visible" : "Collapsed";
            }

            ApplyFiltersAndSort();
        }

        private void Sort_Changed(object sender, RoutedEventArgs e)
        {
            ApplyFiltersAndSort();
        }

        private void ApplyFiltersAndSort()
        {
            // === ГЛАВНАЯ ПРАВКА: ПРОВЕРКА НА NULL ===
            // Если элементы управления еще не загрузились, выходим, чтобы не было ошибки
            if (SortDirectionBtn == null || SortCombo == null || _allCars == null) return;

            // Копируем список для манипуляций
            IEnumerable<Car> query = _allCars;

            // --- СОРТИРОВКА ---
            int sortIndex = SortCombo.SelectedIndex;
            bool isDesc = SortDirectionBtn.IsChecked == true;

            switch (sortIndex)
            {
                case 0: // По цене
                    query = isDesc ? query.OrderByDescending(c => c.PricePerDay)
                                   : query.OrderBy(c => c.PricePerDay);
                    break;
                case 1: // По названию
                    query = isDesc ? query.OrderByDescending(c => c.BrandAndModel)
                                   : query.OrderBy(c => c.BrandAndModel);
                    break;
            }

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

        private void Car_Click(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Car selectedCar)
            {
                // Пока просто MessageBox или открытие окна, если оно уже готово
                MessageBox.Show($"Выбрана машина: {selectedCar.Model}");
            }
        }
    }
}