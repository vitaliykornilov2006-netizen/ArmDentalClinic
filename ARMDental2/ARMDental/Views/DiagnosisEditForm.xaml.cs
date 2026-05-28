using System;
using System.Windows;
using ARMDental.Models;

namespace ARMDental.Views
{
    public partial class DiagnosisEditForm : Window
    {
        private Diagnosis? _diagnosis;

        public DiagnosisEditForm(Diagnosis? diagnosis = null)
        {
            InitializeComponent();
            _diagnosis = diagnosis;

            if (_diagnosis != null)
            {
                txtTitle.Text = "Редактирование диагноза";
                txtCode.Text = _diagnosis.Code;
                txtName.Text = _diagnosis.Name;
                txtDescription.Text = _diagnosis.Description;
            }
            else
            {
                txtTitle.Text = "Новый диагноз";
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(txtCode.Text))
                {
                    MessageBox.Show("Введите код МКБ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtCode.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("Введите название диагноза", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtName.Focus();
                    return;
                }

                using var db = new AppDbContext();

                if (_diagnosis != null)
                {
                    // Обновление существующего
                    var existingDiagnosis = db.Diagnoses.Find(_diagnosis.Id);
                    if (existingDiagnosis != null)
                    {
                        existingDiagnosis.Code = txtCode.Text.Trim();
                        existingDiagnosis.Name = txtName.Text.Trim();
                        existingDiagnosis.Description = txtDescription.Text.Trim();
                        db.SaveChanges();
                    }
                }
                else
                {
                    // Создание нового
                    var newDiagnosis = new Diagnosis
                    {
                        Code = txtCode.Text.Trim(),
                        Name = txtName.Text.Trim(),
                        Description = txtDescription.Text.Trim()
                    };
                    db.Diagnoses.Add(newDiagnosis);
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
