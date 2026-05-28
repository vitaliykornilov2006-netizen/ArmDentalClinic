using System;
using System.Windows;
using ARMDental.Models;
using System.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

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

        private void txtPhone_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]");
        }

        private void txtPhone_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox)
                return;

            string digits = new string(textBox.Text.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("8"))
                digits = "7" + digits.Substring(1);

            if (!digits.StartsWith("7"))
                digits = "7" + digits;

            if (digits.Length > 11)
                digits = digits.Substring(0, 11);

            string formatted = "+7";

            if (digits.Length > 1)
                formatted += " (" + digits.Substring(1, Math.Min(3, digits.Length - 1));

            if (digits.Length >= 4)
                formatted += ") " + digits.Substring(4, Math.Min(3, digits.Length - 4));

            if (digits.Length >= 7)
                formatted += "-" + digits.Substring(7, Math.Min(2, digits.Length - 7));

            if (digits.Length >= 9)
                formatted += "-" + digits.Substring(9, Math.Min(2, digits.Length - 9));

            textBox.TextChanged -= txtPhone_TextChanged;
            textBox.Text = formatted;
            textBox.SelectionStart = textBox.Text.Length;
            textBox.TextChanged += txtPhone_TextChanged;
        }
    }
}
