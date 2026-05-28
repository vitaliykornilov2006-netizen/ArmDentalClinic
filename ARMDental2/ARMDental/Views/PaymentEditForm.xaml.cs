using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ARMDental.Models;
using Microsoft.EntityFrameworkCore;

namespace ARMDental.Views
{
    public partial class PaymentEditForm : Window
    {
        private Payment? _payment;
        private List<Appointment> _appointments = new List<Appointment>();

        public PaymentEditForm(Payment? payment = null)
        {
            InitializeComponent();
            _payment = payment;
            
            LoadComboBoxes();

            if (_payment != null)
            {
                txtTitle.Text = "Редактирование оплаты";
                txtAmount.Text = _payment.Amount.ToString("F2");
                dpPayDate.SelectedDate = _payment.PayDate;
                
                var selectedAppointment = _appointments.FirstOrDefault(a => a.Id == _payment.AppointmentId);
                if (selectedAppointment != null)
                    cmbAppointment.SelectedItem = _appointments
                        .Select(a => new AppointmentDisplayItem 
                        { 
                            Id = a.Id, 
                            DisplayText = $"№{a.Id} - {a.AppointmentDate:dd.MM.yyyy HH:mm}" 
                        })
                        .FirstOrDefault(x => x.Id == _payment.AppointmentId);
                        
                var methodIndex = cmbPayMethod.Items.Cast<string>()
                    .ToList()
                    .FindIndex(m => m == _payment.PayMethod);
                if (methodIndex >= 0) cmbPayMethod.SelectedIndex = methodIndex;
                
                var statusIndex = cmbStatus.Items.Cast<string>()
                    .ToList()
                    .FindIndex(s => s == _payment.Status);
                if (statusIndex >= 0) cmbStatus.SelectedIndex = statusIndex;
            }
            else
            {
                txtTitle.Text = "Новая оплата";
                dpPayDate.SelectedDate = DateTime.Today;
                cmbPayMethod.SelectedIndex = 0;
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void LoadComboBoxes()
        {
            try
            {
                using var db = new AppDbContext();
                
                // Загрузка приёмов (только завершённые и без полной оплаты)
                _appointments = db.Appointments
                    .Include(a => a.Patient)
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList();
                    
                var appointmentList = _appointments.Select(a => new AppointmentDisplayItem 
                { 
                    Id = a.Id, 
                    DisplayText = $"№{a.Id} - {a.Patient?.LastName} {a.Patient?.FirstName} - {a.AppointmentDate:dd.MM.yyyy HH:mm}" 
                }).ToList();
                cmbAppointment.ItemsSource = appointmentList;
                if (appointmentList.Any())
                    cmbAppointment.SelectedIndex = 0;
                
                // Загрузка методов оплаты
                cmbPayMethod.ItemsSource = new List<string>
                {
                    "Наличные",
                    "Банковская карта",
                    "Перевод на счёт",
                    "Страховка"
                };
                
                // Загрузка статусов
                cmbStatus.ItemsSource = new List<string>
                {
                    "Оплачено",
                    "В ожидании",
                    "Возврат",
                    "Частичная оплата"
                };
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
                if (cmbAppointment.SelectedItem == null)
                {
                    MessageBox.Show("Выберите приём", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbAppointment.Focus();
                    return;
                }

                if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount < 0)
                {
                    MessageBox.Show("Введите корректную сумму", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtAmount.Focus();
                    return;
                }

                if (!dpPayDate.SelectedDate.HasValue)
                {
                    MessageBox.Show("Выберите дату оплаты", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var appointmentId = ((AppointmentDisplayItem)cmbAppointment.SelectedItem).Id;
                var payMethod = cmbPayMethod.SelectedItem?.ToString() ?? "Наличные";
                var status = cmbStatus.SelectedItem?.ToString() ?? "Оплачено";

                using var db = new AppDbContext();

                if (_payment != null)
                {
                    // Обновление существующего
                    var existingPayment = db.Payments.Find(_payment.Id);
                    if (existingPayment != null)
                    {
                        existingPayment.AppointmentId = appointmentId;
                        existingPayment.Amount = amount;
                        existingPayment.PayMethod = payMethod;
                        existingPayment.PayDate = DateTime.SpecifyKind(dpPayDate.SelectedDate.Value.Date, DateTimeKind.Utc);
                        existingPayment.Status = status;
                        db.SaveChanges();
                    }
                }
                else
                {
                    // Создание нового
                    var newPayment = new Payment
                    {
                        AppointmentId = appointmentId,
                        Amount = amount,
                        PayMethod = payMethod,
                        PayDate = DateTime.SpecifyKind(dpPayDate.SelectedDate.Value.Date, DateTimeKind.Utc),
                        Status = status
                    };
                    db.Payments.Add(newPayment);
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

    // Вспомогательный класс
    public class AppointmentDisplayItem
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } = "";
    }
}
