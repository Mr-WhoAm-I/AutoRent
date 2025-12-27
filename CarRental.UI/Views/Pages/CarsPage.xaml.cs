using System.Collections.Generic;
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
            var cars = _carService.GetCars();
            CarsGridControl.ItemsSource = cars;
            CarsListControl.ItemsSource = cars;
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