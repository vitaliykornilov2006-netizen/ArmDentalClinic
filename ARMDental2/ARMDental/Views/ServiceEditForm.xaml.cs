using System;
using System.Windows;
using ARMDental.Models;

namespace ARMDental.Views
{
    public partial class ServiceEditForm : Window
    {
        private Service? _service;

        public ServiceEditForm(Service? service = null)
        {
            InitializeComponent();
            _service = service;

            if (_service != null)
            {
                txtTitle.Text = "Редактирование услуги";
                txtName.Text = _service.Name;
                txtDescription.Text = _service.Description;
                txtPrice.Text = _service.Price.ToString("F2");
            }
            else
            {
                txtTitle.Text = "Новая услуга";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название услуги", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtName.Focus();
                    return;
                }

                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
                {
                    MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPrice.Focus();
                    return;
                }

                using var db = new AppDbContext();

                if (_service != null)
                {
                    // Обновление существующего
                    var existingService = db.Services.Find(_service.Id);
                    if (existingService != null)
                    {
                        existingService.Name = txtName.Text.Trim();
                        existingService.Description = txtDescription.Text.Trim();
                        existingService.Price = price;
                        db.SaveChanges();
                    }
                }
                else
                {
                    // Создание нового
                    var newService = new Service
                    {
                        Name = txtName.Text.Trim(),
                        Description = txtDescription.Text.Trim(),
                        Price = price
                    };
                    db.Services.Add(newService);
                    db.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
