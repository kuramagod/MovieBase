using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using MovieBase.database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace MovieBase
{
    public partial class AdminEditWindow : Window
    {
        private object _item;
        private bool _isNew;
        private string _currentTable;
        private Dictionary<PropertyInfo, Control> _controls = new Dictionary<PropertyInfo, Control>();

        // Специальные контролы для Movie
        private ComboBox _countryComboBox;
        private ComboBox _genreComboBox;
        private TextBox _coverTextBox;
        private byte[] _selectedImageData;

        // Специальный контрол для Role
        private ComboBox _roleComboBox;

        public AdminEditWindow(object item, bool isNew)
        {
            InitializeComponent();
            _item = item;
            _isNew = isNew;
            _currentTable = item.GetType().Name;
            TitleText.Text = isNew ? "Добавление новой записи" : "Редактирование записи";

            GenerateFields();
        }

        private void GenerateFields()
        {
            // Получаем все публичные свойства объекта
            var properties = _item.GetType().GetProperties();

            // Для таблицы Movies используем специальную генерацию
            if (_item is Movie)
            {
                GenerateMovieFields(properties);
            }
            // Для таблицы Users используем специальную генерацию с комбобоксом роли
            else if (_item is User)
            {
                GenerateUserFields(properties);
            }
            else
            {
                GenerateGenericFields(properties);
            }
        }

        private void GenerateMovieFields(PropertyInfo[] properties)
        {
            var context = MovieBaseContext.GetContext();

            // Название
            AddFieldHeader("Название");
            var titleBox = CreateTextBox(properties.First(p => p.Name == "Title"));
            FieldsContainer.Children.Add(titleBox);
            _controls.Add(properties.First(p => p.Name == "Title"), titleBox);

            // Описание (многострочное)
            AddFieldHeader("Описание");
            var descBox = CreateMultilineTextBox(properties.First(p => p.Name == "Description"));
            FieldsContainer.Children.Add(descBox);
            _controls.Add(properties.First(p => p.Name == "Description"), descBox);

            // Год
            AddFieldHeader("Год");
            var yearBox = CreateTextBox(properties.First(p => p.Name == "Year"));
            FieldsContainer.Children.Add(yearBox);
            _controls.Add(properties.First(p => p.Name == "Year"), yearBox);

            // Рейтинг
            AddFieldHeader("Рейтинг");
            var ratingBox = CreateTextBox(properties.First(p => p.Name == "Rating"));
            FieldsContainer.Children.Add(ratingBox);
            _controls.Add(properties.First(p => p.Name == "Rating"), ratingBox);

            // Страна (ComboBox)
            AddFieldHeader("Страна");
            _countryComboBox = CreateCountryComboBox(context);

            // Устанавливаем выбранную страну если редактирование
            if (!_isNew)
            {
                var movie = (Movie)_item;
                if (movie.Contryid > 0) // Проверяем, что ID больше 0 (значит выбрана страна)
                {
                    _countryComboBox.SelectedValue = movie.Contryid;
                }
            }

            FieldsContainer.Children.Add(_countryComboBox);
            _controls.Add(properties.First(p => p.Name == "Contryid"), _countryComboBox);

            // Жанр (ComboBox)
            AddFieldHeader("Жанр");
            _genreComboBox = CreateGenreComboBox(context);

            // Устанавливаем выбранный жанр если редактирование
            if (!_isNew)
            {
                var movie = (Movie)_item;
                if (movie.Genreid > 0) // Проверяем, что ID больше 0 (значит выбран жанр)
                {
                    _genreComboBox.SelectedValue = movie.Genreid;
                }
            }

            FieldsContainer.Children.Add(_genreComboBox);
            _controls.Add(properties.First(p => p.Name == "Genreid"), _genreComboBox);

            // Обложка (загрузка файла)
            AddFieldHeader("Обложка");

            var coverPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var coverBox = new TextBox
            {
                Style = (Style)FindResource("ModernTextBox"),
                Width = 300,
                IsReadOnly = true,
                Text = _isNew ? "Файл не выбран" : "Обложка загружена"
            };

            var selectFileButton = new Button
            {
                Content = "Выбрать файл",
                Style = (Style)FindResource("FileButtonStyle"),
                Margin = new Thickness(10, 0, 0, 0)
            };
            selectFileButton.Click += SelectCoverFile_Click;

            coverPanel.Children.Add(coverBox);
            coverPanel.Children.Add(selectFileButton);

            FieldsContainer.Children.Add(coverPanel);

            // Сохраняем TextBox для Cover
            _coverTextBox = coverBox;
            var coverProperty = properties.First(p => p.Name == "Cover");
            _controls.Add(coverProperty, coverBox);

            // Сохраняем существующее изображение если есть
            if (!_isNew && ((Movie)_item).Cover != null)
            {
                _selectedImageData = ((Movie)_item).Cover;
            }
        }

        private void GenerateUserFields(PropertyInfo[] properties)
        {
            var context = MovieBaseContext.GetContext();

            // Имя пользователя
            AddFieldHeader("Имя пользователя");
            var usernameBox = CreateTextBox(properties.First(p => p.Name == "Username"));
            FieldsContainer.Children.Add(usernameBox);
            _controls.Add(properties.First(p => p.Name == "Username"), usernameBox);

            // Пароль
            AddFieldHeader("Пароль");
            var passwordBox = CreateTextBox(properties.First(p => p.Name == "Password"));
            FieldsContainer.Children.Add(passwordBox);
            _controls.Add(properties.First(p => p.Name == "Password"), passwordBox);

            // Email
            AddFieldHeader("Email");
            var emailBox = CreateTextBox(properties.First(p => p.Name == "Email"));
            FieldsContainer.Children.Add(emailBox);
            _controls.Add(properties.First(p => p.Name == "Email"), emailBox);

            // Роль (ComboBox)
            AddFieldHeader("Роль");
            _roleComboBox = CreateRoleComboBox(context);

            // Устанавливаем выбранную роль если редактирование
            if (!_isNew)
            {
                var user = (User)_item;
                if (user.Roleid > 0) // Проверяем, что ID больше 0 (значит выбрана роль)
                {
                    _roleComboBox.SelectedValue = user.Roleid;
                }
            }

            FieldsContainer.Children.Add(_roleComboBox);
            _controls.Add(properties.First(p => p.Name == "Roleid"), _roleComboBox);
        }

        private void GenerateGenericFields(PropertyInfo[] properties)
        {
            foreach (var prop in properties)
            {
                // Пропускаем навигационные свойства Entity Framework
                if (prop.PropertyType.IsGenericType ||
                    (prop.PropertyType.Namespace != null && prop.PropertyType.Namespace.StartsWith("MovieBase.database")) ||
                    prop.PropertyType.IsClass && prop.PropertyType != typeof(string) && prop.PropertyType != typeof(byte[]))
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
                Control control;

                if (prop.Name.ToLower().EndsWith("id") && !_isNew)
                {
                    var textBox = new TextBox();
                    textBox.Style = (Style)FindResource("DisabledTextBox");
                    textBox.Text = prop.GetValue(_item)?.ToString() ?? "";
                    control = textBox;
                }
                else
                {
                    var textBox = new TextBox();
                    textBox.Style = (Style)FindResource("ModernTextBox");
                    textBox.Text = prop.GetValue(_item)?.ToString() ?? "";
                    control = textBox;
                }

                FieldsContainer.Children.Add(control);
                _controls.Add(prop, control);
            }
        }

        private void AddFieldHeader(string text)
        {
            var header = new TextBlock
            {
                Text = text,
                Style = (Style)FindResource("FieldHeaderStyle")
            };
            FieldsContainer.Children.Add(header);
        }

        private TextBox CreateTextBox(PropertyInfo prop)
        {
            var textBox = new TextBox
            {
                Style = (Style)FindResource("ModernTextBox")
            };

            if (!_isNew)
            {
                textBox.Text = prop.GetValue(_item)?.ToString() ?? "";
            }

            return textBox;
        }

        private TextBox CreateMultilineTextBox(PropertyInfo prop)
        {
            var textBox = new TextBox
            {
                Style = (Style)FindResource("MultilineTextBox")
            };

            if (!_isNew)
            {
                textBox.Text = prop.GetValue(_item)?.ToString() ?? "";
            }

            return textBox;
        }

        private ComboBox CreateCountryComboBox(MovieBaseContext context)
        {
            var comboBox = new ComboBox
            {
                Style = (Style)FindResource("ModernComboBox"),
                ItemContainerStyle = (Style)FindResource("ModernComboBoxItem"),
                SelectedValuePath = "Contryid", // Путь к данным (ID)
                Tag = "Выберите страну"
            };

            // Создаем шаблон отображения программно
            var template = new DataTemplate();
            var textElement = new FrameworkElementFactory(typeof(TextBlock));
            textElement.SetBinding(TextBlock.TextProperty, new Binding("Name")); // Привязка к свойству Name
            template.VisualTree = textElement;

            comboBox.ItemTemplate = template; // Устанавливаем шаблон

            comboBox.ItemsSource = context.Countries.ToList();
            return comboBox;
        }

        private ComboBox CreateGenreComboBox(MovieBaseContext context)
        {
            var comboBox = new ComboBox
            {
                Style = (Style)FindResource("ModernComboBox"),
                ItemContainerStyle = (Style)FindResource("ModernComboBoxItem"),
                SelectedValuePath = "Genreid",
                Tag = "Выберите жанр"
            };

            // Создаем шаблон отображения программно
            var template = new DataTemplate();
            var textElement = new FrameworkElementFactory(typeof(TextBlock));
            textElement.SetBinding(TextBlock.TextProperty, new Binding("Name")); // Привязка к свойству Name
            template.VisualTree = textElement;

            comboBox.ItemTemplate = template; // Устанавливаем шаблон
            comboBox.ItemsSource = context.Genres.ToList();
            return comboBox;
        }

        private ComboBox CreateRoleComboBox(MovieBaseContext context)
        {
            var comboBox = new ComboBox
            {
                Style = (Style)FindResource("ModernComboBox"),
                ItemContainerStyle = (Style)FindResource("ModernComboBoxItem"),
                SelectedValuePath = "Roleid",
                Tag = "Выберите роль"
            };

            // Создаем шаблон отображения программно
            var template = new DataTemplate();
            var textElement = new FrameworkElementFactory(typeof(TextBlock));
            textElement.SetBinding(TextBlock.TextProperty, new Binding("Name"));
            template.VisualTree = textElement;

            comboBox.ItemTemplate = template; // Устанавливаем шаблон
            comboBox.ItemsSource = context.Roles.ToList();
            return comboBox;
        }

        private void SelectCoverFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите изображение для обложки",
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Все файлы|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Читаем файл в байтовый массив
                    _selectedImageData = System.IO.File.ReadAllBytes(openFileDialog.FileName);

                    // Проверяем размер файла (например, не больше 5 МБ)
                    if (_selectedImageData.Length > 5 * 1024 * 1024)
                    {
                        MessageBox.Show("Файл слишком большой. Максимальный размер - 5 МБ.",
                                      "Предупреждение",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Warning);
                        _selectedImageData = null;
                        _coverTextBox.Text = "Файл не выбран";
                        return;
                    }

                    _coverTextBox.Text = System.IO.Path.GetFileName(openFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке файла: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }

        private string GetDisplayName(string propertyName)
        {
            // Преобразуем название свойства в читаемый вид
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
                case "rolename": return "Название роли"; // Добавлено для Role
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

                // Записываем данные из контролов обратно в объект
                foreach (var entry in _controls)
                {
                    var prop = entry.Key;
                    var control = entry.Value;

                    try
                    {
                        object value = null;

                        // Обработка разных типов контролов
                        if (control is TextBox textBox)
                        {
                            if (!textBox.IsEnabled) continue; // Пропускаем заблокированные поля

                            // Для Cover используем _selectedImageData
                            if (prop.Name == "Cover")
                            {
                                value = _selectedImageData ?? prop.GetValue(_item);
                            }
                            else
                            {
                                // Преобразование типов (из строки в тип свойства)
                                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                                {
                                    if (DateTime.TryParse(textBox.Text, out DateTime dateValue))
                                        value = dateValue;
                                    else
                                        value = prop.GetValue(_item);
                                }
                                else if (prop.PropertyType == typeof(int) && string.IsNullOrEmpty(textBox.Text))
                                {
                                    // Для пустых int полей устанавливаем 0
                                    value = 0;
                                }
                                else
                                {
                                    value = Convert.ChangeType(textBox.Text, prop.PropertyType);
                                }
                            }

                            if (value != null)
                            {
                                prop.SetValue(_item, value);
                            }
                        }
                        else if (control is ComboBox comboBox)
                        {
                            if (comboBox.SelectedValue != null)
                            {
                                // Проверяем, является ли тип свойства Nullable (например, int?)
                                Type targetType = prop.PropertyType;
                                Type underlyingType = Nullable.GetUnderlyingType(targetType);

                                // Если это Nullable, конвертируем в базовый тип (например, int)
                                object convertedValue = Convert.ChangeType(comboBox.SelectedValue, underlyingType ?? targetType);
                                prop.SetValue(_item, convertedValue);
                            }
                            else
                            {
                                prop.SetValue(_item, null); // Или 0, если поле не поддерживает null
                            }
                        }
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