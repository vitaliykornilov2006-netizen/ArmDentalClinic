using System;
using System.Windows;
using ARMDental.Models;
using System.Linq;

namespace ARMDental.Views
{
    public partial class DoctorEditForm : Window
    {
        private Doctor? _doctor;

        public DoctorEditForm(Doctor? doctor = null)
        {
            InitializeComponent();
            using var db = new AppDbContext();
            cbUsers.ItemsSource = db.Users.ToList();

            _doctor = doctor;


            if (_doctor != null)
            {
                txtTitle.Text = "Редактирование врача";
                txtLastName.Text = _doctor.LastName;
                txtFirstName.Text = _doctor.FirstName;
                txtMiddleName.Text = _doctor.MiddleName;
                txtSpecialization.Text = _doctor.Specialization;
                txtPhone.Text = _doctor.Phone;
                dpEmploymentDate.SelectedDate = _doctor.EmploymentDate;
                cbUsers.SelectedValue = _doctor.UserId;
            }
            else
            {
                txtTitle.Text = "Новый врач";
                dpEmploymentDate.SelectedDate = DateTime.Today;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(txtLastName.Text))
                {
                    MessageBox.Show("Введите фамилию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLastName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFirstName.Text))
                {
                    MessageBox.Show("Введите имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtFirstName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtSpecialization.Text))
                {
                    MessageBox.Show("Введите специализацию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtSpecialization.Focus();
                    return;
                }

                if (cbUsers.SelectedValue == null)
                {
                    MessageBox.Show("Выберите пользователя");
                    return;
                }

                using var db = new AppDbContext();

                if (_doctor != null)
                {
                    // Обновление существующего
                    var existingDoctor = db.Doctors.Find(_doctor.Id);
                    if (existingDoctor != null)
                    {
                        existingDoctor.UserId = (int)cbUsers.SelectedValue;
                        existingDoctor.LastName = txtLastName.Text.Trim();
                        existingDoctor.FirstName = txtFirstName.Text.Trim();
                        existingDoctor.MiddleName = txtMiddleName.Text.Trim();
                        existingDoctor.Specialization = txtSpecialization.Text.Trim();
                        existingDoctor.Phone = txtPhone.Text.Trim();
                        existingDoctor.EmploymentDate = dpEmploymentDate.SelectedDate.Value.Date;
                        db.SaveChanges();
                    }
                }
                else
                {
                    // Создание нового

                    var newDoctor = new Doctor
                    {
                        UserId = (int)cbUsers.SelectedValue,
                        LastName = txtLastName.Text.Trim(),
                        FirstName = txtFirstName.Text.Trim(),
                        MiddleName = txtMiddleName.Text.Trim(),
                        Specialization = txtSpecialization.Text.Trim(),
                        Phone = txtPhone.Text.Trim(),
                        EmploymentDate = dpEmploymentDate.SelectedDate.Value.Date,
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Doctors.Add(newDoctor);
                    db.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка:\n{ex.Message}\n\nINNER:\n{ex.InnerException?.Message}");
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
