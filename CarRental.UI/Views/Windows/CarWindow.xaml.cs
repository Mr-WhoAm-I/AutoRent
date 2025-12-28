using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using Microsoft.Win32;

namespace CarRental.UI.Views.Windows
{
    public partial class CarWindow : Window
    {
        private readonly CarService _carService = new();
        private readonly ReferenceService _refService = new();
        private Car _currentCar;
        public bool IsSuccess { get; private set; } = false;

        // Флаг, чтобы не зациклить TextChanged
        private bool _isFormatting = false;

        public CarWindow(Car car = null)
        {
            InitializeComponent();
            _currentCar = car;
            LoadReferences();

            if (_currentCar != null)
            {
                TitleText.Text = "Редактирование";
                ComboStatus.Visibility = Visibility.Visible;

                FillData();
                CheckFloatingLabels();
            }
        }

        private void CheckFloatingLabels()
        {
            // Принудительно дергаем текстбоксы, чтобы стиль "понял", что текст есть
            // (Это костыль для самописного стиля, если он не обновляется при инициализации)
            // Но обычно байндинга достаточно. Если заголовки наезжают - напишите, поправим.
        }

        private void LoadReferences()
        {
            ComboBrand.ItemsSource = _refService.GetBrands();
            ComboClass.ItemsSource = _refService.GetClasses();
            ComboBody.ItemsSource = _refService.GetBodyTypes();
            ComboTrans.ItemsSource = _refService.GetTransmissions();
            ComboFuel.ItemsSource = _refService.GetFuelTypes();
            ComboStatus.ItemsSource = _refService.GetStatuses();
        }

        private void FillData()
        {
            ComboBrand.SelectedValue = _currentCar.BrandId;
            TxtModel.Text = _currentCar.Model;
            ComboClass.SelectedValue = _currentCar.ClassId;
            ComboBody.SelectedValue = _currentCar.BodyTypeId;
            ComboTrans.SelectedValue = _currentCar.TransmissionId;
            ComboFuel.SelectedValue = _currentCar.FuelId;
            ComboStatus.SelectedValue = _currentCar.StatusId;

            TxtPlate.Text = _currentCar.PlateNumber;
            TxtYear.Text = _currentCar.Year.ToString();
            TxtMileage.Text = _currentCar.Mileage.ToString();
            TxtPrice.Text = _currentCar.PricePerDay.ToString("0.##");

            if (!string.IsNullOrEmpty(_currentCar.ImagePath))
            {
                TxtPhotoPath.Text = _currentCar.ImagePath;
                try { ImgPreview.Source = new BitmapImage(new Uri(_currentCar.ImagePath)); } catch { }
            }
        }

        // === ЖЕСТКАЯ МАСКА НОМЕРА: 1234 AB - 7 ===
        private void TxtPlate_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return;
            _isFormatting = true;

            TextBox tb = sender as TextBox;
            string text = tb.Text.ToUpper();

            // Очищаем от всего, кроме букв и цифр
            string raw = new string(text.Where(c => char.IsLetterOrDigit(c)).ToArray());
            string formatted = "";

            if (raw.Length > 0)
            {
                // 1. Цифры (до 4 шт)
                int digitsCount = 0;
                foreach (char c in raw)
                {
                    if (formatted.Length < 4)
                    {
                        if (char.IsDigit(c)) formatted += c;
                    }
                    else break; // Переходим к буквам
                    digitsCount++;
                }

                // 2. Буквы (2 шт)
                // Начинаем искать буквы в raw после цифр
                int lettersCount = 0;
                if (formatted.Length == 4 && raw.Length > 4)
                {
                    formatted += " "; // Авто-пробел
                    for (int i = 4; i < raw.Length; i++)
                    {
                        char c = raw[i];
                        if (char.IsLetter(c))
                        {
                            formatted += c;
                            lettersCount++;
                            if (lettersCount == 2) break;
                        }
                    }
                }

                // 3. Регион (1 цифра)
                // Ищем последнюю цифру после букв
                if (lettersCount == 2 && raw.Length > 4 + lettersCount)
                {
                    formatted += " - "; // Авто-разделитель

                    // Ищем цифру региона в оставшейся части строки
                    // (raw index: 4 цифры + 2 буквы = 6. Начинаем с 6)
                    for (int i = 6; i < raw.Length; i++)
                    {
                        char c = raw[i];
                        // Регион 1-8 (0 и 9 запрещены)
                        if (char.IsDigit(c) && c != '0' && c != '9')
                        {
                            formatted += c;
                            break; // Только 1 цифра
                        }
                    }
                }
            }

            tb.Text = formatted;
            tb.SelectionStart = formatted.Length; // Курсор в конец
            _isFormatting = false;
        }

        private void BrowsePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dlg.ShowDialog() == true)
            {
                TxtPhotoPath.Text = dlg.FileName;
                ImgPreview.Source = new BitmapImage(new Uri(dlg.FileName));
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (ComboBrand.SelectedValue == null || string.IsNullOrWhiteSpace(TxtModel.Text))
            {
                InfoDialog.Show("Укажите марку и модель.", "Внимание", true);
                return;
            }

            // Проверка номера (полная маска)
            if (TxtPlate.Text.Length < 11)
            {
                InfoDialog.Show("Гос. номер введен не полностью.\nФормат: 1234 AB - 7", "Ошибка", true);
                return;
            }

            // Сборка объекта
            if (_currentCar == null) _currentCar = new Car();

            _currentCar.BrandId = (int)ComboBrand.SelectedValue;
            _currentCar.Model = TxtModel.Text;
            _currentCar.ClassId = (int?)ComboClass.SelectedValue ?? 0;
            _currentCar.BodyTypeId = (int?)ComboBody.SelectedValue ?? 0;
            _currentCar.TransmissionId = (int?)ComboTrans.SelectedValue ?? 0;
            _currentCar.FuelId = (int?)ComboFuel.SelectedValue ?? 0;

            if (_currentCar.Id == 0)
            {
                // Если новая машина - статус всегда "Свободен" (1) или дефолтный
                _currentCar.StatusId = 1;
            }
            else
            {
                // Если редактирование - берем из выпадающего списка
                if (ComboStatus.SelectedValue != null)
                    _currentCar.StatusId = (int)ComboStatus.SelectedValue;
            }

            _currentCar.PlateNumber = TxtPlate.Text;
            int.TryParse(TxtYear.Text, out int year); _currentCar.Year = year;
            int.TryParse(TxtMileage.Text, out int mil); _currentCar.Mileage = mil;
            decimal.TryParse(TxtPrice.Text, out decimal price); _currentCar.PricePerDay = price;
            _currentCar.ImagePath = TxtPhotoPath.Text;

            try
            {
                if (_currentCar.Id == 0) _carService.AddCar(_currentCar);
                else _carService.UpdateCar(_currentCar);

                IsSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                InfoDialog.Show(ex.Message, "Ошибка БД", true);
            }
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e) =>
            e.Handled = new Regex("[^0-9,]+").IsMatch(e.Text);

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}