using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace CarRental.UI.Views.Windows
{
    public partial class MaintenanceWindow : Window
    {
        private readonly MaintenanceService _service = new();
        private readonly EmployeeService _empService = new();
        private Maintenance _current;
        private int _carId;

        public bool IsSuccess { get; private set; }

        public MaintenanceWindow(int carId, Maintenance maintenance = null)
        {
            InitializeComponent();
            _carId = carId;
            _current = maintenance ?? new Maintenance { CarId = carId, DateStart = DateTime.Today };

            LoadMechanics();
            FillData();

            ComboMechanic.AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent,
                      new System.Windows.Controls.TextChangedEventHandler(ComboMechanic_TextChanged));
        }

        private void LoadMechanics()
        {
            ComboMechanic.ItemsSource = _empService.GetMechanics();
        }
        private void ComboMechanic_TextChanged(object sender, TextChangedEventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb.ItemsSource == null) return;

            var itemsViewOriginal = System.Windows.Data.CollectionViewSource.GetDefaultView(cmb.ItemsSource);

            itemsViewOriginal.Filter = ((o) =>
            {
                if (string.IsNullOrEmpty(cmb.Text)) return true;

                if (o is Employee emp)
                {
                    return emp.FullName.Contains(cmb.Text, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            });

            // Открываем список только если мы в фокусе и что-то ищем
            if (cmb.IsKeyboardFocusWithin && !string.IsNullOrEmpty(cmb.Text))
            {
                cmb.IsDropDownOpen = true;
            }
        }

        private void FillData()
        {
            if (_current.Id != 0) TxtTitle.Text = "Редактирование ТО";

            ComboMechanic.SelectedValue = _current.EmployeeId;
            TxtType.Text = _current.ServiceType;
            TxtDesc.Text = _current.Description;
            TxtCost.Text = _current.Cost?.ToString("0.##");

            DateStart.SelectedDate = _current.DateStart;
            DateEnd.SelectedDate = _current.DateEnd;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (ComboMechanic.SelectedValue == null) { InfoDialog.Show("Выберите механика", "Ошибка", true); return; }
            if (string.IsNullOrWhiteSpace(TxtType.Text)) { InfoDialog.Show("Укажите тип работ", "Ошибка", true); return; }
            if (DateStart.SelectedDate == null) { InfoDialog.Show("Укажите дату начала", "Ошибка", true); return; }

            _current.EmployeeId = (int)ComboMechanic.SelectedValue;
            _current.ServiceType = TxtType.Text;
            _current.Description = TxtDesc.Text;
            _current.DateStart = DateStart.SelectedDate.Value;
            _current.DateEnd = DateEnd.SelectedDate;

            if (decimal.TryParse(TxtCost.Text, out decimal cost)) _current.Cost = cost;
            else _current.Cost = null;

            try
            {
                _service.Save(_current);
                IsSuccess = true;
                Close();
            }
            catch (Exception ex)
            {
                InfoDialog.Show(ex.Message, "Ошибка сохранения", true);
            }
        }

        private void NumberValidation(object sender, TextCompositionEventArgs e) => e.Handled = new Regex("[^0-9,]+").IsMatch(e.Text);
        private void Close_Click(object sender, RoutedEventArgs e) => Close();
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
    }
}