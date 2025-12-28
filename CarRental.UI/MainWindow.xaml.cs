using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.UI.Views.Pages; // Подключаем пространство имен страниц

namespace CarRental.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (AuthService.CurrentUser == null) return;

            var user = AuthService.CurrentUser;

            UserNameText.Text = $"{user.Surname} {user.Name}";
            UserRoleText.Text = user.RoleName;

            string initials = "";
            if (!string.IsNullOrEmpty(user.Surname)) initials += user.Surname[0];
            if (!string.IsNullOrEmpty(user.Name)) initials += user.Name[0];
            UserInitialsText.Text = initials.ToUpper();

            BtnManagerHome.Visibility = Visibility.Collapsed;
            BtnDashboard.Visibility = Visibility.Collapsed;

            switch (user.RoleName)
            {
                case "Администратор":
                    // 1. Показываем нижнюю кнопку "Сводка"
                    BtnDashboard.Visibility = Visibility.Visible;

                    // 2. Включаем остальной админский функционал
                    MenuSeparator.Visibility = Visibility.Visible;
                    BtnReports.Visibility = Visibility.Visible;
                    BtnDirectories.Visibility = Visibility.Visible;
                    BtnEmployees.Visibility = Visibility.Visible;
                    BtnArchive.Visibility = Visibility.Visible;
                    BtnLogs.Visibility = Visibility.Visible;

                    // Доступ к операционке
                    BtnRentals.Visibility = Visibility.Visible;
                    BtnClients.Visibility = Visibility.Visible;

                    // 3. Навигация на Сводку
                    BtnCars.IsChecked = false;
                    BtnDashboard.IsChecked = true;
                    MainFrame.Navigate(new DashboardPage());
                    return;

                case "Менеджер":
                    BtnManagerHome.Visibility = Visibility.Visible;
                    BtnRentals.Visibility = Visibility.Visible;
                    BtnClients.Visibility = Visibility.Visible;

                    BtnCars.IsChecked = false;
                    BtnManagerHome.IsChecked = true;
                    MainFrame.Navigate(new ManagerDashboardPage());
                    return;

                case "Механик":
                    BtnMaintenance.Visibility = Visibility.Visible;
                    BtnCars.IsChecked = false;
                    BtnMaintenance.IsChecked = true;
                    MainFrame.Navigate(new MaintenancePage());
                    return;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null) 
            {
                string pageTag = rb.Tag.ToString();

                // Навигация по страницам
                switch (pageTag)
                {
                    case "CarsPage":
                        MainFrame.Navigate(new CarsPage());
                        break;
                    case "ClientsPage":
                        MainFrame.Navigate(new ClientsPage());
                        break;
                    case "RentalsPage":
                        MainFrame.Navigate(new RentalsPage());
                        break;
                    case "MaintenancePage":
                        MainFrame.Navigate(new MaintenancePage());
                        break;
                    case "EmployeesPage":
                        MainFrame.Navigate(new EmployeesPage());
                        break;
                    case "ReportsPage":
                        MainFrame.Navigate(new ReportsPage());
                        break;
                    case "SystemLogsPage":
                        MainFrame.Navigate(new SystemLogsPage());
                        break;
                    case "ManagerDashboardPage":
                        MainFrame.Navigate(new ManagerDashboardPage());
                        break;
                    case "DashboardPage":
                        MainFrame.Navigate(new DashboardPage());
                        break;
                    case "DirectoriesPage":
                        MainFrame.Navigate(new DirectoriesPage());
                        break;
                    case "ArchivePage":
                        MainFrame.Navigate(new ArchivePage());
                        break;
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Views.LoginWindow login = new();
            login.Show();
            Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}