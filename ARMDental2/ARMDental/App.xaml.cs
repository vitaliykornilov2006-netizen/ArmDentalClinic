using System;
using System.Windows;
using ARMDental.Views;

namespace ARMDental
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                var loginWindow = new LoginWindow();

                if (loginWindow.ShowDialog() == true)
                {
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                }
                else
                {
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}\n\n{ex.StackTrace}");
                Shutdown();
            }
        }
    }
}