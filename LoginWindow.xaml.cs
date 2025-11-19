using System;
using System.Data.SQLite;
using System.Windows;

namespace MovieDatabase
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            UsernameTextBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите логин и пароль");
                return;
            }

            try
            {
                using (var connection = new SQLiteConnection("Data Source=MovieDatabase.db;Version=3;"))
                {
                    connection.Open();
                    
                    string query = "SELECT UserId, FullName FROM Users WHERE Username = @username AND Password = @password";
                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = reader.GetInt32(0);
                                string fullName = reader.GetString(1);

                                // Успешный вход - открываем главное окно
                                MainWindow mainWindow = new MainWindow(fullName);
                                mainWindow.Show();
                                this.Close();
                            }
                            else
                            {
                                ShowError("Неверный логин или пароль");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка подключения к базе данных:\n{ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessageText.Text = message;
            ErrorMessageText.Visibility = Visibility.Visible;
        }

        protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                LoginButton_Click(this, new RoutedEventArgs());
            }
        }
    }
}