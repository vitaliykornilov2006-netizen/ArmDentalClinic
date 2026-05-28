using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ARMDental.Models;
using Microsoft.EntityFrameworkCore;

namespace ARMDental.Views
{
    public partial class PaymentDialog : Page
    {
        private ObservableCollection<PaymentDisplay> _payments = new ObservableCollection<PaymentDisplay>();

        public PaymentDialog()
        {
            InitializeComponent();
            LoadPayments();
        }

        private void LoadPayments()
        {
            try
            {
                using var db = new AppDbContext();
                var payments = db.Payments
                    .Include(p => p.Appointment)
                    .ThenInclude(a => a!.Patient)
                    .OrderByDescending(p => p.PayDate)
                    .ToList();

                _payments.Clear();
                decimal totalAmount = 0;
                
                foreach (var payment in payments)
                {
                    _payments.Add(new PaymentDisplay
                    {
                        Id = payment.Id,
                        PatientName = payment.Appointment?.Patient != null 
                            ? $"{payment.Appointment.Patient.LastName} {payment.Appointment.Patient.FirstName}" 
                            : "—",
                        AppointmentId = payment.AppointmentId,
                        Amount = payment.Amount,
                        PayMethod = payment.PayMethod ?? "—",
                        PayDate = payment.PayDate,
                        Status = payment.Status
                    });
                    totalAmount += payment.Amount;
                }

                dgPayments.ItemsSource = _payments;
                dgPayments.Columns.Clear();
                dgPayments.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new System.Windows.Data.Binding("Id"), Width = 50 });
                dgPayments.Columns.Add(new DataGridTextColumn { Header = "Пациент", Binding = new System.Windows.Data.Binding("PatientName"), Width = 150 });
                
                var amountColumn = new DataGridTextColumn 
                { 
                    Header = "Сумма (₽)", 
                    Binding = new System.Windows.Data.Binding("Amount") { StringFormat = "{0:N2}" }, 
                    Width = 120 
                };
                dgPayments.Columns.Add(amountColumn);
                
                dgPayments.Columns.Add(new DataGridTextColumn { Header = "Метод оплаты", Binding = new System.Windows.Data.Binding("PayMethod"), Width = 130 });
                
                var dateColumn = new DataGridTextColumn 
                { 
                    Header = "Дата оплаты", 
                    Binding = new System.Windows.Data.Binding("PayDate") { StringFormat = "{0:dd.MM.yyyy}" }, 
                    Width = 110 
                };
                dgPayments.Columns.Add(dateColumn);
                
                dgPayments.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new System.Windows.Data.Binding("Status"), Width = 110 });

                txtCount.Text = $"Всего проведено оплат: {_payments.Count}  •  Сумма: {totalAmount:N0} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnNewPayment_Click(object sender, RoutedEventArgs e)
        {
            var form = new PaymentEditForm();
            //form.Owner = this;
            if (form.ShowDialog() == true)
            {
                LoadPayments();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgPayments.SelectedItem is PaymentDisplay payment)
            {
                using var db = new AppDbContext();
                var originalPayment = db.Payments.Find(payment.Id);
                if (originalPayment != null)
                {
                    var form = new PaymentEditForm(originalPayment);
                    //form.Owner = this;
                    if (form.ShowDialog() == true)
                    {
                        LoadPayments();
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите оплату для редактирования", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgPayments.SelectedItem is PaymentDisplay payment)
            {
                var result = MessageBox.Show($"Удалить оплату №{payment.Id} на сумму {payment.Amount:N2} ₽?", 
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using var db = new AppDbContext();
                        var dbPayment = db.Payments.Find(payment.Id);
                        if (dbPayment != null)
                        {
                            db.Payments.Remove(dbPayment);
                            db.SaveChanges();
                        }
                        LoadPayments();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите оплату для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPayments();
        }
    }

    // Вспомогательный класс
    public class PaymentDisplay
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = "";
        public int AppointmentId { get; set; }
        public decimal Amount { get; set; }
        public string PayMethod { get; set; } = "";
        public DateTime PayDate { get; set; }
        public string Status { get; set; } = "";
    }
}
