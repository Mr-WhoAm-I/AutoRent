using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views.Windows; // Для будущих окон
using CarRental.UI.Views;         // Для InfoDialog

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

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Открыть окно добавления клиента
            // var win = new ClientWindow();
            // win.ShowDialog();
            // if (win.IsSuccess) LoadData();
            InfoDialog.Show("Окно добавления клиента будет создано на следующем этапе.", "Информация");
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            var editWin = new ClientEditWindow(_client);
            editWin.ShowDialog();

            if (editWin.IsSuccess)
            {
                LoadClientInfo();
                InfoDialog.Show("Данные клиента обновлены!", "Успех");
            }
        }

        private void ClientsGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenDetails();
        }

        private void OpenDetails()
        {
            if (ClientsGrid.SelectedItem is Client client)
            {
                // TODO: Открыть карточку клиента
                var win = new ClientWindow(client);
                win.ShowDialog();
                LoadData();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Удалять могут и Админы, и Менеджеры (согласно ТЗ)
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