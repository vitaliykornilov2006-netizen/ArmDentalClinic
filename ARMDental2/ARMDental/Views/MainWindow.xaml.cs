using System;
using System.Windows;
using System.Windows.Threading;
using ARMDental.Models;

namespace ARMDental.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private bool _isAdmin = false;
        private User _currentUser;
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(User user) : this()
        {
            _currentUser = user;
            InitializeComponent();
            txtHeaderUserName.Text = user.Login;
            txtHeaderUserRole.Text = user.Role;
            txtUserName.Text = user.Login;
            txtUserRole.Text = user.Role;
            
            // Установить информацию о пользователе если уже вошли
            if (CurrentUser.User != null)
            {
                SetUserInfo(CurrentUser.User);
            }
            
            // Запустить таймер для обновления времени
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
            UpdateTime();

            if (user.Role == "Администратор")
            {
                _isAdmin = true;
            }

            adminMenuPanel.Visibility = _isAdmin ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateTime();
        }

        private void UpdateTime()
        {
            txtCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        public void SetUserInfo(User user)
        {
            string fullName = user.Doctor != null 
                ? $"{user.Doctor.LastName} {user.Doctor.FirstName}" 
                : user.Login;
            
            txtUserName.Text = fullName;
            txtUserRole.Text = user.Role;
            txtHeaderUserName.Text = fullName;
            txtHeaderUserRole.Text = user.Role;
        }

        private void btnPatients_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Navigate(new PatientsDialog(_currentUser));
            txtPageTitle.Text = "Пациенты";
            //var dialog = new PatientsDialog();
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }

        private void btnDoctors_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Navigate(new DoctorsDialog(_currentUser));
            txtPageTitle.Text = "Доктора";
            //var dialog = new DoctorsDialog();
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }

        private void btnServices_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Navigate(new ServicesDialog());
            txtPageTitle.Text = "Услуги";
            //var dialog = new ServicesDialog();
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }

        private void btnDiagnoses_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Navigate(new DiagnosesDialog());
            txtPageTitle.Text = "Диагнозы";
            //var dialog = new DiagnosesDialog();
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }

        private void btnVisit_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Navigate(new VisitsDialog());
            txtPageTitle.Text = "Приём";
            //var dialog = new VisitsDialog();
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }

        private void btnPayment_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Navigate(new PaymentDialog());
            txtPageTitle.Text = "Оплата";
            //var dialog = new PaymentDialog();
            //dialog.Owner = this;
            //dialog.ShowDialog();
        }

        private void btnUsers_Click(object sender, RoutedEventArgs e)
        {
            txtPageTitle.Text = "Управление пользователями";
            MessageBox.Show("Управление пользователями будет доступно в следующей версии", 
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SettingsWindow();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                timer.Stop();
                Application.Current.Shutdown();
            }
        }

        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("У вас нет новых уведомлений", "Уведомления", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            contentControl.Navigate(new ReportsDialog());
            txtPageTitle.Text = "Отчёты";
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            timer?.Stop();
        }
    }
}
