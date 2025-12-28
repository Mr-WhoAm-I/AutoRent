using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views.Windows; // Для окон

namespace CarRental.UI.Views.Pages
{
    public partial class ClientsPage : Page
    {
        private readonly ClientService _service = new();
        private List<Client> _allClients = new();

        public ClientsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _allClients = _service.GetClients();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки: " + ex.Message, "Ошибка", true);
            }
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string searchText = SearchBox.Text?.Trim().ToLower() ?? "";

            if (string.IsNullOrEmpty(searchText))
            {
                ClientsGrid.ItemsSource = _allClients;
            }
            else
            {
                var filtered = _allClients.Where(c =>
                    c.FullName.ToLower().Contains(searchText) ||
                    c.Phone.ToLower().Contains(searchText) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchText))
                ).ToList();

                ClientsGrid.ItemsSource = filtered;
            }
        }

        // === ВОССТАНОВЛЕННАЯ ЛОГИКА ===

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Открываем форму создания (ClientEditWindow)
            var win = new ClientEditWindow();
            win.ShowDialog();

            if (win.IsSuccess) LoadData();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // Быстрое редактирование из меню (открываем сразу форму ввода)
            if (ClientsGrid.SelectedItem is Client client)
            {
                var win = new ClientEditWindow(client);
                win.ShowDialog();
                if (win.IsSuccess) LoadData();
            }
        }

        // Двойной клик открывает КАРТОЧКУ (ClientWindow)
        private void ClientsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenDetails();
        }
        private void OpenCard_Click(object sender, RoutedEventArgs e)
        {
            OpenDetails();
        }

        private void OpenDetails()
        {
            if (ClientsGrid.SelectedItem is Client client)
            {
                var win = new ClientWindow(client);
                win.ShowDialog();
                // Обновляем список, вдруг в карточке что-то поменяли
                LoadData();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem is Client client)
            {
                if (MessageBox.Show($"Вы уверены, что хотите отправить клиента {client.FullName} в архив?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _service.Archive(client.Id);
                        LoadData();
                    }
                    catch (Exception ex)
                    {
                        InfoDialog.Show(ex.Message, "Ошибка", true);
                    }
                }
            }
        }
    }
}