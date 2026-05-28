using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ARMDental.Models;
using Microsoft.EntityFrameworkCore;

namespace ARMDental.Views
{
    public partial class VisitsDialog : Page
    {
        private ObservableCollection<AppointmentDisplay> _visits = new ObservableCollection<AppointmentDisplay>();

        public VisitsDialog()
        {
            InitializeComponent();

            Loaded += (_, __) =>
            {
                _isLoaded = true;
                LoadVisits();
            };
        }

        private void LoadVisits()
        {
            try
            {
                using var db = new AppDbContext();
                var appointments = db.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(x => x.Service)
                    .Include(a => a.AppointmentDiagnoses)
                        .ThenInclude(x => x.Diagnosis)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList();

                _visits.Clear();
                foreach (var appt in appointments)
                {
                    _visits.Add(new AppointmentDisplay
                    {
                        Id = appt.Id,
                        PatientName = $"{appt.Patient?.LastName} {appt.Patient?.FirstName}",
                        DoctorName = $"{appt.Doctor?.LastName} {appt.Doctor?.FirstName}",
                        AppointmentDate = appt.AppointmentDate,
                        Status = appt.Status,
                        Notes = appt.Notes,
                        Services = string.Join(", ", appt.AppointmentServices .Select(s => s.Service.Name)),
                        Diagnoses = string.Join(", ", appt.AppointmentDiagnoses .Select(d => d.Diagnosis.Name))
                    });
                }

                dgVisits.ItemsSource = _visits;
                dgVisits.Columns.Clear();
                dgVisits.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("Id"), Width = 50 });
                dgVisits.Columns.Add(new DataGridTextColumn { Header = "Пациент", Binding = new System.Windows.Data.Binding("PatientName"), Width = 150 });
                dgVisits.Columns.Add(new DataGridTextColumn { Header = "Врач", Binding = new System.Windows.Data.Binding("DoctorName"), Width = 150 });
                
                var dateColumn = new DataGridTextColumn 
                { 
                    Header = "Дата и время", 
                    Binding = new System.Windows.Data.Binding("AppointmentDate") { StringFormat = "{0:dd.MM.yyyy HH:mm}" }, 
                    Width = 140 
                };
                dgVisits.Columns.Add(dateColumn);

                dgVisits.Columns.Add(new DataGridTextColumn
                {
                    Header = "Услуги",
                    Binding = new System.Windows.Data.Binding("Services"),
                    Width = 220
                });

                dgVisits.Columns.Add(new DataGridTextColumn
                {
                    Header = "Диагнозы",
                    Binding = new System.Windows.Data.Binding("Diagnoses"),
                    Width = 220
                });

                dgVisits.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new System.Windows.Data.Binding("Status"), Width = 120 });
                dgVisits.Columns.Add(new DataGridTextColumn { Header = "Заметки", Binding = new System.Windows.Data.Binding("Notes"), Width = 250 });

                // Подсчёт приёмов на сегодня
                var today = DateTime.Today;
                var todayCount = appointments.Count(a => a.AppointmentDate.Date == today);
                SetCountText(_visits.Count);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNewVisit_Click(object sender, RoutedEventArgs e)
        {
            var form = new VisitEditForm();
            //form.Owner = this;
            if (form.ShowDialog() == true)
            {
                LoadVisits();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgVisits.SelectedItem is AppointmentDisplay appointment)
            {
                // Получаем оригинальный объект
                using var db = new AppDbContext();
                var originalAppointment = db.Appointments.Find(appointment.Id);
                if (originalAppointment != null)
                {
                    var form = new VisitEditForm(originalAppointment);
                    //form.Owner = this;
                    if (form.ShowDialog() == true)
                    {
                        LoadVisits();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите приём для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgVisits.SelectedItem is AppointmentDisplay appointment)
            {
                var result = MessageBox.Show($"Удалить приём №{appointment.Id}?", 
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var db = new AppDbContext();
                        var dbAppointment = db.Appointments.Find(appointment.Id);
                        if (dbAppointment != null)
                        {
                            db.Appointments.Remove(dbAppointment);
                            db.SaveChanges();
                        }
                        LoadVisits();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите приём для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadVisits();
        }

        private bool _isLoaded;
        private void Filters_Changed(object sender, EventArgs e)
        {
            if (!_isLoaded) return;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            try
            {
                using var db = new AppDbContext();

                string patientSearch = txtPatientSearch.Text.Trim().ToLower();
                string doctorSearch = txtDoctorSearch.Text.Trim().ToLower();
                string serviceSearch = txtServiceSearch.Text.Trim().ToLower();
                string diagnosisSearch = txtDiagnosisSearch.Text.Trim().ToLower();

                string selectedStatus =
                    (cmbStatusFilter.SelectedItem as ComboBoxItem)?.Content?.ToString()
                    ?? "Все статусы";

                DateTime? selectedDate = dpDateFilter.SelectedDate;


                var query = db.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.AppointmentServices)
                        .ThenInclude(x => x.Service)
                    .Include(a => a.AppointmentDiagnoses)
                        .ThenInclude(x => x.Diagnosis)
                    .AsQueryable();

                // Пациент
                if (!string.IsNullOrWhiteSpace(patientSearch))
                {
                    query = query.Where(a =>
                        (a.Patient.LastName + " " + a.Patient.FirstName)
                            .ToLower()
                            .Contains(patientSearch));
                }

                // Врач
                if (!string.IsNullOrWhiteSpace(doctorSearch))
                {
                    query = query.Where(a =>
                        (a.Doctor.LastName + " " + a.Doctor.FirstName)
                            .ToLower()
                            .Contains(doctorSearch));
                }

                // Услуга
                if (!string.IsNullOrWhiteSpace(serviceSearch))
                {
                    query = query.Where(a =>
                        a.AppointmentServices.Any(s =>
                            s.Service.Name.ToLower().Contains(serviceSearch)));
                }

                // Диагноз
                if (!string.IsNullOrWhiteSpace(diagnosisSearch))
                {
                    query = query.Where(a =>
                        a.AppointmentDiagnoses.Any(d =>
                            d.Diagnosis.Name.ToLower().Contains(diagnosisSearch)));
                }

                // Статус
                if (selectedStatus != "Все статусы")
                {
                    query = query.Where(a => a.Status == selectedStatus);
                }

                // Дата
                if (dpDateFilter.SelectedDate.HasValue)
                {
                    var start = DateTime.SpecifyKind(
                        dpDateFilter.SelectedDate.Value.Date,
                        DateTimeKind.Utc);

                    var end = start.AddDays(1);

                    query = query.Where(a =>
                        a.AppointmentDate >= start &&
                        a.AppointmentDate < end);
                }

                var appointments = query
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList();

                _visits.Clear();

                foreach (var appt in appointments)
                {
                    _visits.Add(new AppointmentDisplay
                    {
                        Id = appt.Id,
                        PatientName = $"{appt.Patient?.LastName} {appt.Patient?.FirstName}",
                        DoctorName = $"{appt.Doctor?.LastName} {appt.Doctor?.FirstName}",
                        AppointmentDate = appt.AppointmentDate,
                        Status = appt.Status,
                        Notes = appt.Notes,
                        Services = string.Join(", ",
                            appt.AppointmentServices.Select(s => s.Service.Name)),
                        Diagnoses = string.Join(", ",
                            appt.AppointmentDiagnoses.Select(d => d.Diagnosis.Name))
                    });
                }

                txtCount.Text = $"Найдено приёмов: {_visits.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            txtPatientSearch.Clear();
            txtDoctorSearch.Clear();
            txtServiceSearch.Clear();
            txtDiagnosisSearch.Clear();

            dpDateFilter.SelectedDate = null;
            cmbStatusFilter.SelectedIndex = 0;

            LoadVisits();
        }
        private void SetCountText(int count)
        {
            if (txtCount == null) return;

            txtCount.Text = $"Найдено приёмов: {count}";
        }
    }

    // Вспомогательный класс для отображения
    public class AppointmentDisplay
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public string Services { get; set; } = "";
        public string Diagnoses { get; set; } = "";

    }
}
