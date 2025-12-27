using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities; // для класса Car

namespace CarRental.UI.Views.Pages
{
    public partial class CarsPage : Page
    {
        private readonly CarService _service = new();

        public CarsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCars();
        }

        private void LoadCars()
        {
            var cars = _service.GetCars();
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
            // Получаем нажатую машину
            if ((sender as FrameworkElement)?.DataContext is Car selectedCar)
            {
                // Открываем модальное окно деталей
                var detailsWindow = new CarDetailsWindow(selectedCar);
                detailsWindow.Owner = Application.Current.MainWindow;
                detailsWindow.ShowDialog();
            }
        }
    }
}