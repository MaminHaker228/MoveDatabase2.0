using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MovieDatabase
{
    public partial class MainWindow : Window
    {
        private DataTable moviesTable;
        private List<Category> categories;

        public MainWindow(string userName)
        {
            InitializeComponent();
            UserNameText.Text = $"👤 {userName}";
            LoadCategories();
            LoadMovies();
        }

        private void LoadCategories()
        {
            categories = new List<Category>();
            categories.Add(new Category { CategoryId = 0, CategoryName = "Все категории" });

            try
            {
                using (var connection = new SQLiteConnection("Data Source=MovieDatabase.db;Version=3;"))
                {
                    connection.Open();
                    string query = "SELECT CategoryId, CategoryName FROM Categories ORDER BY CategoryName";
                    
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category
                            {
                                CategoryId = reader.GetInt32(0),
                                CategoryName = reader.GetString(1)
                            });
                        }
                    }
                }

                CategoryComboBox.ItemsSource = categories;
                CategoryComboBox.DisplayMemberPath = "CategoryName";
                CategoryComboBox.SelectedValuePath = "CategoryId";
                CategoryComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadMovies(string searchText = "", int categoryId = 0)
        {
            try
            {
                using (var connection = new SQLiteConnection("Data Source=MovieDatabase.db;Version=3;"))
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT 
                            m.MovieId,
                            m.Title,
                            m.Year,
                            m.Director,
                            m.Description,
                            m.Rating,
                            m.Duration,
                            c.CategoryName
                        FROM Movies m
                        LEFT JOIN Categories c ON m.CategoryId = c.CategoryId
                        WHERE 1=1";

                    if (!string.IsNullOrEmpty(searchText))
                    {
                        query += " AND (m.Title LIKE @search OR m.Director LIKE @search OR m.Description LIKE @search)";
                    }

                    if (categoryId > 0)
                    {
                        query += " AND m.CategoryId = @categoryId";
                    }

                    query += " ORDER BY m.Title";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        if (!string.IsNullOrEmpty(searchText))
                        {
                            command.Parameters.AddWithValue("@search", $"%{searchText}%");
                        }
                        if (categoryId > 0)
                        {
                            command.Parameters.AddWithValue("@categoryId", categoryId);
                        }

                        using (var adapter = new SQLiteDataAdapter(command))
                        {
                            moviesTable = new DataTable();
                            adapter.Fill(moviesTable);
                            MoviesDataGrid.ItemsSource = moviesTable.DefaultView;
                            
                            UpdateStatus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фильмов: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatus()
        {
            int totalCount = moviesTable.Rows.Count;
            StatusText.Text = $"Найдено фильмов: {totalCount}";
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (CategoryComboBox.SelectedValue != null)
            {
                string searchText = SearchTextBox.Text.Trim();
                int categoryId = (int)CategoryComboBox.SelectedValue;
                LoadMovies(searchText, categoryId);
            }
        }

        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            CategoryComboBox.SelectedIndex = 0;
        }

        private void MoviesDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (MoviesDataGrid.SelectedItem != null)
            {
                DataRowView row = (DataRowView)MoviesDataGrid.SelectedItem;
                int movieId = Convert.ToInt32(row["MovieId"]);
                
                // Открываем окно с подробной информацией
                MovieDetailsWindow detailsWindow = new MovieDetailsWindow(movieId);
                detailsWindow.ShowDialog();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти?", "Подтверждение", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }
}