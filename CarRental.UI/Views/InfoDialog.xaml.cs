using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CarRental.UI.Views
{
    public partial class InfoDialog : Window
    {
        // Приватный конструктор, чтобы окно создавалось только через метод Show
        private InfoDialog(string message, string title, bool isError)
        {
            InitializeComponent();

            MessageText.Text = message;
            TitleText.Text = title;

            if (isError)
            {
                IconPath.Data = (Geometry)Application.Current.Resources["CrossIconGeom"];

                IconPath.Fill = Brushes.White;
                IconPath.Stroke = null;
                IconPath.StrokeThickness = 0;

                TitleText.Text = string.IsNullOrEmpty(title) ? "Ошибка!" : title;
                ActionButton.Content = "Закрыть";
            }
            else
            {
                IconPath.Data = (Geometry)Application.Current.Resources["CheckIconGeom"];

                IconPath.Fill = null;
                IconPath.Stroke = Brushes.White;
                IconPath.StrokeThickness = 2.5;
                IconPath.StrokeStartLineCap = PenLineCap.Round;
                IconPath.StrokeEndLineCap = PenLineCap.Round;
                IconPath.StrokeLineJoin = PenLineJoin.Round;

                TitleText.Text = string.IsNullOrEmpty(title) ? "Успех!" : title;
                ActionButton.Content = "ОК";
            }
        }

        // Статический метод для вызова (как MessageBox.Show)
        public static void Show(string message, string title = "", bool isError = false)
        {
            var dialog = new InfoDialog(message, title, isError);
            dialog.Owner = Application.Current.MainWindow; // Чтобы окно было поверх главного
            dialog.ShowDialog(); // Модальный режим (блокирует остальные окна пока не закроешь)
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}