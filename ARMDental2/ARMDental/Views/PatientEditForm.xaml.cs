using System;
using System.Windows;
using ARMDental.Models;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace ARMDental.Views
{
    public partial class PatientEditForm : Window
    {
        private Patient? _patient;

        public PatientEditForm(Patient? patient = null)
        {
            InitializeComponent();
            _patient = patient;

            if (_patient != null)
            {
                txtTitle.Text = "Редактирование пациента";
                txtLastName.Text = _patient.LastName;
                txtFirstName.Text = _patient.FirstName;
                txtMiddleName.Text = _patient.MiddleName;
                txtPhone.Text = _patient.Phone;
                dpBirthDate.SelectedDate = _patient.BirthDate;
            }
            else
            {
                txtTitle.Text = "Новый пациент";
                dpBirthDate.SelectedDate = new DateTime(1990, 1, 1);
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

                if (!dpBirthDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату рождения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var db = new AppDbContext();

                if (_patient != null)
                {
                    // Обновление существующего
                    var existingPatient = db.Patients.Find(_patient.Id);
                    if (existingPatient != null)
                    {
                        existingPatient.LastName = txtLastName.Text.Trim();
                        existingPatient.FirstName = txtFirstName.Text.Trim();
                        existingPatient.MiddleName = txtMiddleName.Text.Trim();
                        existingPatient.BirthDate = dpBirthDate.SelectedDate.Value.Date;
                        existingPatient.Phone = txtPhone.Text.Trim();
                        db.SaveChanges();
                    }
                }
                else
                {
                    // Создание нового
                    var newPatient = new Patient
                    {
                        LastName = txtLastName.Text.Trim(),
                        FirstName = txtFirstName.Text.Trim(),
                        MiddleName = txtMiddleName.Text.Trim(),
                        BirthDate = dpBirthDate.SelectedDate.Value.Date,
                        Phone = txtPhone.Text.Trim(),
                        CreatedAt = DateTime.UtcNow
                    };
                    db.Patients.Add(newPatient);
                    db.SaveChanges();
                }

                DialogResult = true;
                Close();
        }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка:\n{ex.Message}\n\nINNER:\n{ex.InnerException?.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
