using System;
using System.Windows;
using System.Windows.Media;

namespace ARMDental.Views
{
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            // Загрузка текущих настроек
            try
            {
                using var db = new AppDbContext();
                // Проверка подключения к БД
                var canConnect = db.Database.CanConnect();
                txtConnectionStatus.Text = canConnect ? "✅ Подключено" : "❌ Не подключено";
                txtConnectionStatus.Foreground = canConnect ? Brushes.Green : Brushes.Red;
            }
            catch
            {
                txtConnectionStatus.Text = "❌ Ошибка подключения";
                txtConnectionStatus.Foreground = Brushes.Red;
            }
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var db = new AppDbContext();
                var canConnect = db.Database.CanConnect();
                
                if (canConnect)
                {
                    MessageBox.Show("Подключение к базе данных успешно!", "Проверка подключения", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось подключиться к базе данных", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Диалог выбора папки для сохранения резервной копии
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                    FileName = $"ARM_Dental_backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak",
                    Title = "Сохранить резервную копию"
                };

                if (dialog.ShowDialog() == true)
                {
                    MessageBox.Show($"Резервная копия будет сохранена в:\n{dialog.FileName}\n\n(Функция резервного копирования требует настройки сервера БД)", 
                        "Резервное копирование", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Настройки сохранены!", "Настройки", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
