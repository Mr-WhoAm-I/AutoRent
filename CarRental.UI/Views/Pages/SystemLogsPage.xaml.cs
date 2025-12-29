using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using CarRental.DAL.Repositories;
using CarRental.UI.Views;

namespace CarRental.UI.Views.Pages
{
    public partial class SystemLogsPage : Page
    {
        private readonly LogRepository _repo = new();

        public SystemLogsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Загружаем список таблиц, которые мы логируем
            ComboTables.ItemsSource = _repo.GetLogTables();
            if (ComboTables.Items.Count > 0)
                ComboTables.SelectedIndex = 0;
        }

        private void ComboTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadLogs();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void LoadLogs()
        {
            if (ComboTables.SelectedItem == null) return;
            string tableName = ComboTables.SelectedItem.ToString();

            try
            {
                DataTable dt = _repo.GetLogs(tableName);
                LogsGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка чтения логов: " + ex.Message, "Ошибка", true);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (ComboTables.SelectedItem == null) return;
            string tableName = ComboTables.SelectedItem.ToString();

            if (MessageBox.Show($"Вы уверены, что хотите очистить историю изменений для таблицы {tableName}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _repo.ClearLogs(tableName);
                    LoadLogs();
                    InfoDialog.Show("Журнал очищен.", "Успех");
                }
                catch (Exception ex)
                {
                    InfoDialog.Show(ex.Message, "Ошибка", true);
                }
            }
        }

        private void LogsGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.PropertyName;

            // Скрываем служебные ID и технические поля, если не нужны
            if (header == "idLog") e.Cancel = true;

            // Можно переименовать для красоты
            if (header == "typeLog") e.Column.Header = "Тип";
            if (header == "dateLog") e.Column.Header = "Дата";
            if (header == "userLog") e.Column.Header = "Пользователь";
            if (header == "hostLog") e.Column.Header = "Компьютер";
        }

        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (ComboTables.SelectedItem == null) return;
            string tableName = ComboTables.SelectedItem.ToString();

            // Получаем строку, на которой нажали кнопку
            // DataContext строки в DataGrid с DataTable - это DataRowView
            if ((sender as Button)?.DataContext is DataRowView row)
            {
                int logId = (int)row["idLog"]; // Берем ID лога из скрытого поля (или из данных)
                string type = row["typeLog"].ToString();
                string date = row["dateLog"].ToString();

                string msg = $"Вы хотите восстановить состояние таблицы '{tableName}' к версии от {date}?\n\n";
                if (type == "D") msg += "Внимание: Это вернет удаленную запись.";
                else msg += "Внимание: Текущие данные этой записи будут перезаписаны данными из лога.";

                if (MessageBox.Show(msg, "Откат изменений", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repo.RestoreVersion(tableName, logId);
                        InfoDialog.Show("Данные успешно восстановлены!", "Успех");

                        // Обновляем лог (там появится новая запись о том, что мы только что сделали Insert/Update)
                        LoadLogs();
                    }
                    catch (Exception ex)
                    {
                        InfoDialog.Show("Ошибка восстановления: " + ex.Message + "\nВозможно, запись ссылается на несуществующие данные.", "Ошибка", true);
                    }
                }
            }
        }
    }
}