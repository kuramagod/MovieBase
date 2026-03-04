using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MovieBase
{
    public partial class AdminEditWindow : Window
    {
        private object _item;
        private bool _isNew;
        private Dictionary<PropertyInfo, TextBox> _controls = new Dictionary<PropertyInfo, TextBox>();

        public AdminEditWindow(object item, bool isNew)
        {
            InitializeComponent();
            _item = item;
            _isNew = isNew;
            TitleText.Text = isNew ? "Добавление новой записи" : "Редактирование записи";

            GenerateFields();
        }

        private void GenerateFields()
        {
            // Получаем все публичные свойства объекта
            var properties = _item.GetType().GetProperties();

            foreach (var prop in properties)
            {
                // Пропускаем навигационные свойства Entity Framework
                if (prop.PropertyType.IsGenericType ||
                    (prop.PropertyType.Namespace != null && prop.PropertyType.Namespace.StartsWith("MovieBase.database")) ||
                    prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    continue;

                // Пропускаем ID для новых записей (автоинкремент)
                if (prop.Name.ToLower().EndsWith("id") && _isNew)
                    continue;

                // Создаем заголовок поля
                string displayName = GetDisplayName(prop.Name);
                var header = new TextBlock
                {
                    Text = displayName,
                    Style = (Style)FindResource("FieldHeaderStyle")
                };
                FieldsContainer.Children.Add(header);

                // Создаем поле ввода
                var textBox = new TextBox();

                // Применяем стиль в зависимости от доступности
                if (prop.Name.ToLower().EndsWith("id") && !_isNew)
                {
                    textBox.Style = (Style)FindResource("DisabledTextBox");
                }
                else
                {
                    textBox.Style = (Style)FindResource("ModernTextBox");
                }

                // Устанавливаем значение
                textBox.Text = prop.GetValue(_item)?.ToString() ?? "";

                FieldsContainer.Children.Add(textBox);
                _controls.Add(prop, textBox);
            }
        }

        private string GetDisplayName(string propertyName)
        {
            // Преобразуем название свойства в читаемый вид
            // Например: "Movieid" -> "ID фильма", "Title" -> "Название"
            switch (propertyName.ToLower())
            {
                case "movieid": return "ID фильма";
                case "userid": return "ID пользователя";
                case "reviewid": return "ID рецензии";
                case "genreid": return "ID жанра";
                case "contryid": return "ID страны";
                case "roleid": return "ID роли";
                case "favoriteid": return "ID избранного";
                case "title": return "Название";
                case "username": return "Имя пользователя";
                case "password": return "Пароль";
                case "email": return "Email";
                case "description": return "Описание";
                case "year": return "Год";
                case "rating": return "Рейтинг";
                case "name": return "Название";
                case "date": return "Дата";
                case "text": return "Текст";
                case "cover": return "Обложка";
                default: return propertyName;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var context = MovieBaseContext.GetContext();

                // Записываем данные из TextBox'ов обратно в объект
                foreach (var entry in _controls)
                {
                    var prop = entry.Key;
                    var textBox = entry.Value;

                    if (!textBox.IsEnabled) continue; // Пропускаем заблокированные поля

                    try
                    {
                        // Преобразование типов (из строки в тип свойства)
                        object value;

                        // Обработка специальных типов
                        if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                        {
                            if (DateTime.TryParse(textBox.Text, out DateTime dateValue))
                                value = dateValue;
                            else
                                value = prop.GetValue(_item); // оставляем старое значение
                        }
                        else if (prop.PropertyType == typeof(byte[]))
                        {
                            // Для byte[] пропускаем (обрабатывается отдельно)
                            continue;
                        }
                        else
                        {
                            value = Convert.ChangeType(textBox.Text, prop.PropertyType);
                        }

                        prop.SetValue(_item, value);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка в поле '{GetDisplayName(prop.Name)}': {ex.Message}",
                                      "Ошибка формата",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                        return;
                    }
                }

                if (_isNew)
                    context.Add(_item);
                else
                    context.Update(_item);

                context.SaveChanges();
                this.DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при сохранении: " + ex.Message,
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }
    }
}