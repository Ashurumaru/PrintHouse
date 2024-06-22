using MyPracticeApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPracticeApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для ReportPage.xaml
    /// </summary>
    public partial class ReportPage : Page
    {
        public ReportPage()
        {
            InitializeComponent();
        }
        private void GenerateRevenueReport_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            try
            {
                ReportGenerator.GenerateRevenueReport(startDate, endDate);
                MessageBox.Show("Отчет по выручке успешно создан.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateEmployeeReport_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = StartDatePicker.SelectedDate ?? DateTime.MinValue;
            DateTime endDate = EndDatePicker.SelectedDate ?? DateTime.MaxValue;

            try
            {
                ReportGenerator.GenerateEmployeePerformanceReport(startDate, endDate);
                MessageBox.Show("Отчет по производительности сотрудников успешно создан.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
