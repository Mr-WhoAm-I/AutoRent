using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarRental.BLL.Services;
using CarRental.Domain.Entities;
using CarRental.UI.Views;

namespace CarRental.UI.Views.Pages
{
    public partial class DirectoriesPage : Page
    {
        private readonly ReferenceService _service = new();
        private List<int> _deletedIds = new(); // Список ID на удаление
        private string _currentTable = "";

        public DirectoriesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (MenuListBox.SelectedIndex == -1) MenuListBox.SelectedIndex = 0;
        }

        private void MenuListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MenuListBox.SelectedItem is ListBoxItem item)
            {
                _currentTable = item.Tag.ToString();
                LoadTable();
            }
        }

        private void LoadTable()
        {
            try
            {
                _deletedIds.Clear(); // Очищаем список удаленных при смене таблицы

                // Проверяем, нужна ли колонка Описания
                bool hasDesc = _service.HasDescription(_currentTable);
                ColDesc.Visibility = hasDesc ? Visibility.Visible : Visibility.Collapsed;

                // Загружаем данные
                var items = _service.GetItems(_currentTable);
                RefGrid.ItemsSource = items;
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка загрузки: " + ex.Message, "Ошибка", true);
            }
        }

        // Сохранение изменений
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Завершаем редактирование в DataGrid
            RefGrid.CommitEdit();
            RefGrid.CommitEdit();

            try
            {
                // Берем элементы из таблицы
                var items = RefGrid.Items.OfType<ReferenceItem>().ToList();

                // Валидация
                if (items.Any(x => string.IsNullOrWhiteSpace(x.Name)))
                {
                    InfoDialog.Show("Название не может быть пустым.", "Ошибка");
                    return;
                }

                // Отправляем в сервис
                _service.SaveChanges(_currentTable, items, _deletedIds);

                InfoDialog.Show("Данные успешно сохранены!", "Успех");
                LoadTable(); // Перезагружаем, чтобы получить новые ID из базы
            }
            catch (Exception ex)
            {
                InfoDialog.Show("Ошибка сохранения: " + ex.Message, "Ошибка", true);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            LoadTable(); // Сброс изменений
        }

        // Логика удаления по клавише Delete
        private void RefGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                // Получаем выделенные элементы
                var selectedItems = RefGrid.SelectedItems.Cast<ReferenceItem>().ToList();

                foreach (var item in selectedItems)
                {
                    // Если у элемента ID > 0, значит он есть в базе, нужно удалить
                    if (item.Id > 0)
                    {
                        _deletedIds.Add(item.Id);
                    }
                }
                // DataGrid сам удалит их визуально, нам нужно только запомнить ID
            }
        }
    }
}