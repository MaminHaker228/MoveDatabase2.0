using System.Windows;

namespace MovieDatabase
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Проверяем наличие базы данных
            if (!System.IO.File.Exists("MovieDatabase.db"))
            {
                MessageBox.Show("Ошибка: Файл базы данных MovieDatabase.db не найден!\n\nУбедитесь, что файл MovieDatabase.db находится в той же папке, что и программа.",
                    "Ошибка базы данных", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}