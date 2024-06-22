using MyPracticeApp.Models;
using MyPracticeApp.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyPracticeApp
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool IsMaximized = false;

        public MainWindow()
        {
            InitializeComponent();
            if (CurrentUser.PositionId == 2)
            {
                btnEmployee.Visibility = Visibility.Visible;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (IsMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    IsMaximized = false;
                }
                else
                {
                    this.WindowState = WindowState.Maximized;

                    IsMaximized = true;
                }
            }
        }

        private void NavigateToClients(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ClientPage());
        }

        private void NavigateToEmployee(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new EmployeePage());
        }

        private void NavigateToProducts(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ProductPage());
        }

        private void NavigateToOrders(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrderPage());
        }

        private void NavigateToPurchaseOrders(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PurchaseOrdersPage());
        }

        private void NavigateToSuppliers(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new SuppliersPage());
        }

        private void NavigateToReports(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportPage());
        }
        private void LogOut(object sender, RoutedEventArgs e)
        {
            AuthWindow authWindow = new AuthWindow();
            authWindow.Show();
            this.Close();
        }
    }
}
