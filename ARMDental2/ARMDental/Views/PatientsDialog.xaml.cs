using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ARMDental.Models;
using Microsoft.EntityFrameworkCore;

namespace ARMDental.Views
{
    public partial class PatientsDialog : Page
    {
        private ObservableCollection<Patient> _patients = new ObservableCollection<Patient>();

        private User _currentUser;

        public PatientsDialog(User user)
        {
            InitializeComponent();

            _currentUser = user;

            if (_currentUser.Role != "Администратор")
            {
                btnAddPatient.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
            }

            LoadPatients();
        }

        private void LoadPatients()
        {
            try
            {
                using var db = new AppDbContext();
                var patients = db.Patients.OrderBy(p => p.LastName).ToList();
                _patients.Clear();
                foreach (var patient in patients)
                    _patients.Add(patient);

                dgPatients.ItemsSource = _patients;
                dgPatients.Columns.Clear();
                dgPatients.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("Id"), Width = 50 });
                dgPatients.Columns.Add(new DataGridTextColumn { Header = "Фамилия", Binding = new System.Windows.Data.Binding("LastName"), Width = 130 });
                dgPatients.Columns.Add(new DataGridTextColumn { Header = "Имя", Binding = new System.Windows.Data.Binding("FirstName"), Width = 110 });
                dgPatients.Columns.Add(new DataGridTextColumn { Header = "Отчество", Binding = new System.Windows.Data.Binding("MiddleName"), Width = 130 });
                
                var birthDateColumn = new DataGridTextColumn 
                { 
                    Header = "Дата рождения", 
                    Binding = new System.Windows.Data.Binding("BirthDate") { StringFormat = "{0:dd.MM.yyyy}" }, 
                    Width = 110 
                };
                dgPatients.Columns.Add(birthDateColumn);
                
                dgPatients.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new System.Windows.Data.Binding("Phone"), Width = 130 });

                // Возраст


                txtCount.Text = $"Всего пациентов: {_patients.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            var form = new PatientEditForm();
            //form.Owner = this;
            if (form.ShowDialog() == true)
            {
                LoadPatients();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgPatients.SelectedItem is Patient patient)
            {
                var form = new PatientEditForm(patient);
                //form.Owner = this;
                if (form.ShowDialog() == true)
                {
                    LoadPatients();
                }
            }
            else
            {
                MessageBox.Show("Выберите пациента для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgPatients.SelectedItem is Patient patient)
            {
                var result = MessageBox.Show($"Удалить пациента {patient.LastName} {patient.FirstName}?", 
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var db = new AppDbContext();
                        var dbPatient = db.Patients.Find(patient.Id);
                        if (dbPatient != null)
                        {
                            db.Patients.Remove(dbPatient);
                            db.SaveChanges();
                        }
                        LoadPatients();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите пациента для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPatients();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                using var db = new AppDbContext();

                string search = txtSearch.Text.Trim().ToLower();

                var patients = db.Patients
                    .Where(p =>
                        p.LastName.ToLower().Contains(search) ||
                        p.FirstName.ToLower().Contains(search) ||
                        (p.MiddleName != null && p.MiddleName.ToLower().Contains(search)))
                    .OrderBy(p => p.LastName)
                    .ToList();

                _patients.Clear();

                foreach (var patient in patients)
                    _patients.Add(patient);

                txtCount.Text = $"Найдено пациентов: {_patients.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }

        private void dgPatients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
