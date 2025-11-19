using System;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Net;

namespace MovieDatabase
{
    public partial class MovieDetailsWindow : Window
    {
        public MovieDetailsWindow(int movieId)
        {
            InitializeComponent();
            LoadMovieDetails(movieId);
        }

        private void LoadMovieDetails(int movieId)
        {
            try
            {
                using (var connection = new SQLiteConnection("Data Source=MovieDatabase.db;Version=3;"))
                {
                    connection.Open();
                    
                    string query = @"
                        SELECT 
                            m.Title,
                            m.Year,
                            m.Director,
                            m.Description,
                            m.Rating,
                            m.Duration,
                            c.CategoryName,
                            m.PosterUrl
                        FROM Movies m
                        LEFT JOIN Categories c ON m.CategoryId = c.CategoryId
                        WHERE m.MovieId = @movieId";

                    using (var command = new SQLiteCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@movieId", movieId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Заполняем данные
                                string title = reader["Title"].ToString();
                                string year = reader["Year"].ToString();
                                string director = reader["Director"].ToString();
                                string description = reader["Description"].ToString();
                                string rating = reader["Rating"].ToString();
                                string duration = reader["Duration"].ToString();
                                string category = reader["CategoryName"].ToString();
                                string posterUrl = reader["PosterUrl"]?.ToString();

                                // Устанавливаем текст
                                TitleText.Text = title;
                                YearText.Text = year;
                                DirectorText.Text = director;
                                DescriptionText.Text = description;
                                RatingText.Text = $"{rating}/10";
                                DurationText.Text = $"{duration} мин";
                                CategoryText.Text = category;

                                // Загружаем постер
                                LoadPoster(posterUrl, title);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPoster(string posterUrl, string movieTitle)
        {
            try
            {
                // Если URL постера не указан или пустой - показываем заглушку
                if (string.IsNullOrEmpty(posterUrl))
                {
                    ShowPlaceholder();
                    return;
                }

                // Пробуем загрузить изображение из URL
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(posterUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                PosterImage.Source = bitmap;
                PlaceholderGrid.Visibility = Visibility.Collapsed;
            }
            catch
            {
                // Если не удалось загрузить - показываем заглушку
                ShowPlaceholder();
            }
        }

        private void ShowPlaceholder()
        {
            PosterImage.Source = null;
            PlaceholderGrid.Visibility = Visibility.Visible;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}