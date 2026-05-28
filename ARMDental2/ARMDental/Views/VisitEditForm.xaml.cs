using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ARMDental.Models;
using Microsoft.EntityFrameworkCore;

namespace ARMDental.Views
{
    public partial class VisitEditForm : Window
    {
        //списки данных из базы для формы
        private Appointment? _appointment;
        private List<Patient> _patients = new List<Patient>();
        private List<Doctor> _doctors = new List<Doctor>();
        private List<Service> _services = new List<Service>();
        private List<Diagnosis> _diagnoses = new List<Diagnosis>();


        public VisitEditForm(Appointment? appointment = null)
        {
            InitializeComponent();
            _appointment = appointment;
            
            LoadComboBoxes();

            if (_appointment != null)
            {
                txtTitle.Text = "Редактирование приёма";
                // установка значений в комбобоксы
                var selectedPatient = _patients.FirstOrDefault(p => p.Id == _appointment.PatientId);
                if (selectedPatient != null)
                    cmbPatient.SelectedItem = selectedPatient;
                    
                var selectedDoctor = _doctors.FirstOrDefault(d => d.Id == _appointment.DoctorId);
                if (selectedDoctor != null)
                    cmbDoctor.SelectedItem = selectedDoctor;
                    
                dpDate.SelectedDate = _appointment.AppointmentDate.Date;
                txtTime.Text = _appointment.AppointmentDate.ToString("HH:mm");
                txtNotes.Text = _appointment.Notes;
            }
            else
            {
                txtTitle.Text = "Новый приём";
                dpDate.SelectedDate = DateTime.Today;
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                using var db = new AppDbContext();
                
                // Загрузка пациентов
                _patients = db.Patients.OrderBy(p => p.LastName).ToList();
                var patientList = _patients.Select(p => new PatientDisplayItem 
                { 
                    Id = p.Id, 
                    DisplayName = $"{p.LastName} {p.FirstName}" 
                }).ToList();
                cmbPatient.ItemsSource = patientList;
                
                // Загрузка врачей
                _doctors = db.Doctors.OrderBy(d => d.LastName).ToList();
                var doctorList = _doctors.Select(d => new DoctorDisplayItem 
                { 
                    Id = d.Id, 
                    DisplayName = $"{d.LastName} {d.FirstName} ({d.Specialization})" 
                }).ToList();
                cmbDoctor.ItemsSource = doctorList;

                _services = db.Services
                    .OrderBy(s => s.Name)
                    .ToList();

                _diagnoses = db.Diagnoses 
                    .OrderBy(d => d.Name)
                    .ToList();

                lstDiagnoses.ItemsSource = _diagnoses;

                lstServices.ItemsSource = _services;
                // Загрузка статусов
                cmbStatus.ItemsSource = new List<string>
                {
                    "Запланирована",
                    "В процессе",
                    "Завершена",
                    "Отменена"
                };
                cmbStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (cmbPatient.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пациента", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbPatient.Focus();
                    return;
                }

                if (cmbDoctor.SelectedItem == null)
                {
                    MessageBox.Show("Выберите врача", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbDoctor.Focus();
                    return;
                }

                if (!dpDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату приёма", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!TimeSpan.TryParse(txtTime.Text, out TimeSpan time))
                {
                    MessageBox.Show("Введите корректное время (ЧЧ:ММ)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtTime.Focus();
                    return;
                }

                var patientId = ((PatientDisplayItem)cmbPatient.SelectedItem).Id;
                var doctorId = ((DoctorDisplayItem)cmbDoctor.SelectedItem).Id;
                var appointmentDate = DateTime.SpecifyKind(dpDate.SelectedDate.Value.Date.Add(time), DateTimeKind.Utc);
                var status = cmbStatus.SelectedItem?.ToString() ?? "Запланирована";

                using var db = new AppDbContext();

                // Проверка занятости врача
                // Проверка занятости врача (минимум 1 час между приёмами)
                bool doctorBusy = db.Appointments.Any(a => a.DoctorId == doctorId && 
                    Math.Abs((a.AppointmentDate - appointmentDate).TotalMinutes) < 60 && 
                    (_appointment == null || a.Id != _appointment.Id));

                if (doctorBusy)
                {
                    MessageBox.Show(
                        "У врача уже есть запись на эту дату и время",
                        "Конфликт расписания",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                if (_appointment != null)
                {
                    // Обновление существующего
                    var existingAppointment = db.Appointments.Find(_appointment.Id);
                    if (existingAppointment != null)
                    {
                        existingAppointment.PatientId = patientId;
                        existingAppointment.DoctorId = doctorId;
                        existingAppointment.AppointmentDate = appointmentDate;
                        existingAppointment.Status = status;
                        existingAppointment.Notes = txtNotes.Text.Trim();
                        // удалить старые услуги
                        var oldServices = db.AppointmentServices
                            .Where(x => x.AppointmentId == existingAppointment.Id);

                        db.AppointmentServices.RemoveRange(oldServices);

                        // добавить новые услуги
                        foreach (Service service in lstServices.SelectedItems)
                        {
                            existingAppointment.AppointmentServices.Add(new AppointmentService
                            {
                                ServiceId = service.Id,
                                Quantity = 1,
                                Price = service.Price
                            });
                        }

                        // удалить старые диагнозы
                        var oldDiagnoses = db.AppointmentDiagnoses
                            .Where(x => x.AppointmentId == existingAppointment.Id);

                        db.AppointmentDiagnoses.RemoveRange(oldDiagnoses);

                        // добавить новые диагнозы
                        foreach (Diagnosis diagnosis in lstDiagnoses.SelectedItems)
                        {
                            existingAppointment.AppointmentDiagnoses.Add(new AppointmentDiagnosis
                            {
                                DiagnosisId = diagnosis.Id
                            });
                        }
                        db.SaveChanges();
                    }
                }
                else
                {
                    // Создание нового
                    var newAppointment = new Appointment
                    {
                        PatientId = patientId,
                        DoctorId = doctorId,
                        AppointmentDate = appointmentDate,
                        Status = status,
                        Notes = txtNotes.Text.Trim()
                    };
                    db.Appointments.Add(newAppointment);
                    foreach (Service service in lstServices.SelectedItems) //сохранение услуг
                    {
                        newAppointment.AppointmentServices.Add(new AppointmentService
                        {
                            ServiceId = service.Id,
                            Quantity = 1,
                            Price = service.Price
                        });
                    }

                    foreach (Diagnosis diagnosis in lstDiagnoses.SelectedItems) //сохранение диагнозов
                    {
                        newAppointment.AppointmentDiagnoses.Add(new AppointmentDiagnosis
                        {
                            DiagnosisId = diagnosis.Id
                        });
                    }
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
    }

    // Вспомогательные классы для отображения
    public class PatientDisplayItem
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = "";
    }

    public class DoctorDisplayItem
    {
        public int Id { get; set; }
        public string DisplayName { get; set; } = "";
    }
}
