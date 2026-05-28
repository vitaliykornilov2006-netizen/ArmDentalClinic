using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ARMDental.Models;
using Microsoft.EntityFrameworkCore;

namespace ARMDental.Views
{
    public partial class DoctorsDialog : Page
    {
        private ObservableCollection<Doctor> _doctors = new ObservableCollection<Doctor>();

        private User _currentUser;

        public DoctorsDialog(User user)
        {
            InitializeComponent();

            _currentUser = user;

            if (_currentUser.Role != "Администратор")
            {
                btnAdd.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
            }

            LoadDoctors();
            LoadSpecializations();
        }

        private void LoadDoctors()
        {
            try
            {
                using var db = new AppDbContext();
                var doctors = db.Doctors.OrderBy(d => d.LastName).ToList();
                _doctors.Clear();
                foreach (var doctor in doctors)
                    _doctors.Add(doctor);

                dgDoctors.ItemsSource = _doctors;
                dgDoctors.Columns.Clear();
                dgDoctors.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("Id"), Width = 50 });
                dgDoctors.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new System.Windows.Data.Binding("LastName"), Width = 120 });
                dgDoctors.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new System.Windows.Data.Binding("FirstName"), Width = 100 });
                dgDoctors.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new System.Windows.Data.Binding("MiddleName"), Width = 120 });
                dgDoctors.Columns.Add(new DataGridTextColumn { Header = "Специализация", Binding = new System.Windows.Data.Binding("Specialization"), Width = 150 });
                dgDoctors.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone"), Width = 120 });

                // Колонка с датой трудоустройства
                var dateColumn = new DataGridTextColumn 
                { 
                    Header = "Дата трудоустройства", 
                    Binding = new System.Windows.Data.Binding("EmploymentDate") { StringFormat = "{0:dd.MM.yyyy}" }, 
                    Width = 100 
                };
                dgDoctors.Columns.Add(dateColumn);

                txtCount.Text = $"Всего врачей: {_doctors.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new DoctorEditForm();
            //form.Owner = this;
            if (form.ShowDialog() == true)
            {
                LoadDoctors();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgDoctors.SelectedItem is Doctor doctor)
            {
                var form = new DoctorEditForm(doctor);
                //form.Owner = this;
                if (form.ShowDialog() == true)
                {
                    LoadDoctors();
                }
            }
            else
            {
                MessageBox.Show("Выберите врача для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDoctors.SelectedItem is Doctor doctor)
            {
                var result = MessageBox.Show($"Удалить врача {doctor.LastName} {doctor.FirstName}?", 
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var db = new AppDbContext();
                        var dbDoctor = db.Doctors.Find(doctor.Id);
                        if (dbDoctor != null)
                        {
                            db.Doctors.Remove(dbDoctor);
                            db.SaveChanges();
                        }
                        LoadDoctors();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите врача для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDoctors();
        }
        private void FilterDoctors(object sender, EventArgs e)
        {
            if (txtSearch == null || cmbSpecialization == null || dpEmploymentDate == null)
                return;
            try
            {
                using var db = new AppDbContext();

                string search = txtSearch.Text.Trim().ToLower();

                string specialization =
                    (cmbSpecialization.SelectedItem as ComboBoxItem)?
                    .Content?.ToString() ?? "Все специализации";

                DateTime? employmentDate = dpEmploymentDate?.SelectedDate;

                var query = db.Doctors.AsQueryable();

                // Поиск по ФИО
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(d =>
                        d.LastName.ToLower().Contains(search) ||
                        d.FirstName.ToLower().Contains(search) ||
                        (d.MiddleName != null &&
                         d.MiddleName.ToLower().Contains(search)));
                }

                // Фильтр по специализации
                if (specialization != "Все специализации")
                {
                    query = query.Where(d =>
                        d.Specialization == specialization);
                }

                // Фильтр по дате трудоустройства
                if (employmentDate.HasValue)
                {
                    query = query.Where(d =>
                        d.EmploymentDate.Date ==
                        employmentDate.Value.Date);
                }

                var doctors = query
                    .OrderBy(d => d.LastName)
                    .ToList();

                _doctors.Clear();

                foreach (var doctor in doctors)
                    _doctors.Add(doctor);

                txtCount.Text = $"Найдено врачей: {_doctors.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }
        private void LoadSpecializations()
        {
            try
            {
                using var db = new AppDbContext();

                var specializations = db.Doctors
                    .Select(d => d.Specialization)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();

                foreach (var specialization in specializations)
                {
                    cmbSpecialization.Items.Add(
                        new ComboBoxItem { Content = specialization });
                }

                cmbSpecialization.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки специализаций: {ex.Message}");
            }
        }
        private void btnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();

            cmbSpecialization.SelectedIndex = 0;

            dpEmploymentDate.SelectedDate = null;

            LoadDoctors();
        }
    }
}
