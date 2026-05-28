using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ARMDental.Views
{
    public partial class ReportSelectWindow : Window
    {
        public ReportSelectWindow()
        {
            InitializeComponent();

            cmbMonth.SelectedIndex = DateTime.Today.Month - 1;

            for (int year = 2023; year <= DateTime.Today.Year + 1; year++)
            {
                cmbYear.Items.Add(year);
            }

            cmbYear.SelectedItem = DateTime.Today.Year;
            cmbReportType.SelectedIndex = 0;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbMonth.SelectedIndex < 0 || cmbYear.SelectedItem == null)
                {
                    MessageBox.Show("Выберите месяц и год");
                    return;
                }

                int month = cmbMonth.SelectedIndex + 1;
                int year = (int)cmbYear.SelectedItem;

                DateTime selectedMonth = new DateTime(year, month, 1);

                string reportType = ((System.Windows.Controls.ComboBoxItem)cmbReportType.SelectedItem)
                    .Content
                    .ToString();

                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveDialog.FileName = "Отчёт.xlsx";

                if (saveDialog.ShowDialog() != true)
                    return;

                using var db = new AppDbContext();
                using var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add("Отчёт");

                if (reportType == "Количество пациентов за месяц")
                {
                    var patientsCount = db.Appointments
                        .Count(a => a.AppointmentDate.Month == month
                                 && a.AppointmentDate.Year == year);

                    worksheet.Cell(1, 1).Value = "Отчёт по пациентам";
                    worksheet.Cell(2, 1).Value = "Месяц";
                    worksheet.Cell(2, 2).Value = selectedMonth.ToString("MMMM yyyy");

                    worksheet.Cell(3, 1).Value = "Количество пациентов";
                    worksheet.Cell(3, 2).Value = patientsCount;
                }
                else if (reportType == "Прибыль за месяц")
                {
                    var totalProfit = db.Payments
                        .Where(p => p.PayDate.Month == month
                                 && p.PayDate.Year == year)
                        .Sum(p => p.Amount);

                    worksheet.Cell(1, 1).Value = "Отчёт по прибыли";

                    worksheet.Cell(2, 1).Value = "Месяц";
                    worksheet.Cell(2, 2).Value = selectedMonth.ToString("MMMM yyyy");

                    worksheet.Cell(3, 1).Value = "Общая прибыль";
                    worksheet.Cell(3, 2).Value = totalProfit;
                }
                worksheet.Columns().AdjustToContents();

                workbook.SaveAs(saveDialog.FileName);

                MessageBox.Show("Отчёт успешно сохранён");

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}


