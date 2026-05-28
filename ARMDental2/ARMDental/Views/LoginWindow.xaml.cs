using System;
using System.Linq;
using System.Windows;
using ARMDental.Models;
using Microsoft.EntityFrameworkCore;

namespace ARMDental.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            txtLogin.Focus();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    txtError.Text = "Введите логин и пароль";
                    return;
                }

                using (var db = new AppDbContext())
                {
                    var user = db.Users
                    .Include(u => u.Doctor)
                    .FirstOrDefault(u => u.Login == login && u.Password == password);
                    if (user != null)
                    {
                        //CurrentUser.User = user;
                        MainWindow mainWindow = new MainWindow(user);
                        mainWindow.Show();
                        this.Close();
                        //DialogResult = true;
                        //Close();

                    }
                    else
                    {
                        txtError.Text = "Неверный логин или пароль";
                    }
                }
            }
                
            catch (Exception ex)
            {
                txtError.Text = "Ошибка: " + ex.Message;
                System.Diagnostics.Debug.WriteLine($"Login error: {ex}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //DialogResult = false;
            Close();
        }
    }
}