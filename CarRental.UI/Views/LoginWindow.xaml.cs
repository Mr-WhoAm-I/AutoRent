using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;

namespace CarRental.UI.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;
        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthService();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                InfoDialog.Show("Пожалуйста, введите логин и пароль.", "Ошибка ввода", true);
                return;
            }

            // Вызываем реальную проверку
            bool isAuth = _authService.Login(login, password);

            if (isAuth)
            {
                // Успех! Открываем главное окно
                MainWindow main = new();
                main.Show();
                Close(); // Закрываем окно входа
            }
            else
            {
                InfoDialog.Show("Неверный логин или пароль. Попробуйте снова.", "Ошибка доступа", true);
            }
        }

        // Логика для скрытия/показа подсказки (Placeholder) в PasswordBox
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
            {
                // Находим TextBlock-подсказку внутри шаблона PasswordBox
                if (pb.Template.FindName("PlaceholderText", pb) is TextBlock placeholder)
                {
                    // Если пароль пустой, показываем подсказку, иначе скрываем
                    placeholder.Visibility = string.IsNullOrEmpty(pb.Password) ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }
    }
}