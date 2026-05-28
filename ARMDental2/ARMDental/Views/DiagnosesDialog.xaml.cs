using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ARMDental.Models;

namespace ARMDental.Views
{
    public partial class DiagnosesDialog : Page
    {
        private ObservableCollection<Diagnosis> _diagnoses = new ObservableCollection<Diagnosis>();

        public DiagnosesDialog()
        {
            InitializeComponent();
            LoadDiagnoses();
        }

        private void LoadDiagnoses()
        {
            try
            {
                using var db = new AppDbContext();
                var diagnoses = db.Diagnoses.OrderBy(d => d.Code).ToList();
                _diagnoses.Clear();
                foreach (var diagnosis in diagnoses)
                    _diagnoses.Add(diagnosis);

                dgDiagnoses.ItemsSource = _diagnoses;
                dgDiagnoses.Columns.Clear();
                dgDiagnoses.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("Id"), Width = 50 });
                dgDiagnoses.Columns.Add(new DataGridTextColumn { Header = "Код МКБ", Binding = new System.Windows.Data.Binding("Code"), Width = 100 });
                dgDiagnoses.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new System.Windows.Data.Binding("Name"), Width = 400 });
                dgDiagnoses.Columns.Add(new DataGridTextColumn { Header = "Описание", Binding = new System.Windows.Data.Binding("Description"), Width = 300 });

                txtCount.Text = $"Всего диагнозов: {_diagnoses.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var form = new DiagnosisEditForm();
            //form.Owner = this;
            if (form.ShowDialog() == true)
            {
                LoadDiagnoses();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiagnoses.SelectedItem is Diagnosis diagnosis)
            {
                var form = new DiagnosisEditForm(diagnosis);
                //form.Owner = this;
                if (form.ShowDialog() == true)
                {
                    LoadDiagnoses();
                }
            }
            else
            {
                MessageBox.Show("Выберите диагноз для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgDiagnoses.SelectedItem is Diagnosis diagnosis)
            {
                var result = MessageBox.Show($"Удалить диагноз «{diagnosis.Code} - {diagnosis.Name}»?", 
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var db = new AppDbContext();
                        var dbDiagnosis = db.Diagnoses.Find(diagnosis.Id);
                        if (dbDiagnosis != null)
                        {
                            db.Diagnoses.Remove(dbDiagnosis);
                            db.SaveChanges();
                        }
                        LoadDiagnoses();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите диагноз для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadDiagnoses();
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (txtSearch == null)
                    return;

                using var db = new AppDbContext();

                string search = txtSearch.Text.Trim().ToLower();

                var diagnoses = db.Diagnoses
                    .Where(d =>
                        d.Name.ToLower().Contains(search) ||
                        d.Code.ToLower().Contains(search))
                    .OrderBy(d => d.Code)
                    .ToList();

                _diagnoses.Clear();

                foreach (var diagnosis in diagnoses)
                    _diagnoses.Add(diagnosis);

                txtCount.Text = $"Найдено диагнозов: {_diagnoses.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }
    }
}
