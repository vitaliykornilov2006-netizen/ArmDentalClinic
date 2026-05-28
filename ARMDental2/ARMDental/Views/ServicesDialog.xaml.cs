using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ARMDental.Models;

namespace ARMDental.Views
{
    public partial class ServicesDialog : Page
    {
        private ObservableCollection<Service> _services = new ObservableCollection<Service>();

        public ServicesDialog()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices()
        {
            try
            {
                using var db = new AppDbContext();
                var services = db.Services.OrderBy(s => s.Name).ToList();
                _services.Clear();
                foreach (var service in services)
                    _services.Add(service);

                dgServices.ItemsSource = _services;
                dgServices.Columns.Clear();
                dgServices.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("Id"), Width = 50 });
                dgServices.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new System.Windows.Data.Binding("Name"), Width = 300 });
                dgServices.Columns.Add(new DataGridTextColumn { Header = "Описание", Binding = new System.Windows.Data.Binding("Description"), Width = 350 });
                
                var priceColumn = new DataGridTextColumn 
                { 
                    Header = "Цена (₽)", 
                    Binding = new System.Windows.Data.Binding("Price") { StringFormat = "{0:N2}" }, 
                    Width = 120 
                };
                dgServices.Columns.Add(priceColumn);

                txtCount.Text = $"Всего услуг: {_services.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new ServiceEditForm();
            //form.Owner = this;
            if (form.ShowDialog() == true)
            {
                LoadServices();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgServices.SelectedItem is Service service)
            {
                var form = new ServiceEditForm(service);
                //form.Owner = this;
                if (form.ShowDialog() == true)
                {
                    LoadServices();
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgServices.SelectedItem is Service service)
            {
                var result = MessageBox.Show($"Удалить услугу «{service.Name}»?", 
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var db = new AppDbContext();
                        var dbService = db.Services.Find(service.Id);
                        if (dbService != null)
                        {
                            db.Services.Remove(dbService);
                            db.SaveChanges();
                        }
                        LoadServices();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите услугу для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadServices();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtSearch == null)
                    return;

                using var db = new AppDbContext();

                string search = txtSearch.Text.Trim().ToLower();

                var services = db.Services
                    .Where(s =>
                        s.Name.ToLower().Contains(search))
                    .OrderBy(s => s.Name)
                    .ToList();

                _services.Clear();

                foreach (var service in services)
                    _services.Add(service);

                txtCount.Text = $"Найдено услуг: {_services.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }
    }
}
