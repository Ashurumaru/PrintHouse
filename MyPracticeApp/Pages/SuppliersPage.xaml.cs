using MyPracticeApp.Data;
using MyPracticeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyPracticeApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для SuppliersPage.xaml
    /// </summary>
    public partial class SuppliersPage : Page
    {
        private PrintingHouseEntities _context;
        private bool isEditing = false;
        private int editingSupplierId;

        public SuppliersPage()
        {
            InitializeComponent();
            _context = new PrintingHouseEntities();
            LoadSuppliers();
        }

        private void LoadSuppliers()
        {
            var suppliers = _context.Suppliers
                .Select(s => new
                {
                    s.SupplierId,
                    s.SupplierName,
                    s.ContactName,
                    s.ContactEmail
                }).OrderBy(s => s.SupplierId).ToList();

            SupplierList.ItemsSource = suppliers;
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            SupplierForm.Visibility = Visibility.Visible;
            SupplierList.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Collapsed;
            TextSupplier.Visibility = Visibility.Collapsed;
            ResetSearchButton.Visibility = Visibility.Collapsed;
            AddSupplier.Visibility = Visibility.Collapsed;
        }

        private void EditSupplier_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingSupplierId = (int)button.CommandParameter;
            var supplier = _context.Suppliers.FirstOrDefault(s => s.SupplierId == editingSupplierId);

            if (supplier != null)
            {
                isEditing = true;
                SupplierNameTextBox.Text = supplier.SupplierName;
                ContactNameTextBox.Text = supplier.ContactName;
                ContactEmailTextBox.Text = supplier.ContactEmail;
                SupplierForm.Visibility = Visibility.Visible;
                SupplierList.Visibility = Visibility.Collapsed;
                SearchTextBox.Visibility = Visibility.Collapsed;
                TextSupplier.Visibility = Visibility.Collapsed;
                ResetSearchButton.Visibility = Visibility.Collapsed;
                AddSupplier.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var supplierId = (int)button.CommandParameter;
            var supplier = _context.Suppliers.FirstOrDefault(s => s.SupplierId == supplierId);

            if (supplier != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого поставщика?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Suppliers.Remove(supplier);
                    _context.SaveChanges();
                    LoadSuppliers();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var supplier = _context.Suppliers.FirstOrDefault(s => s.SupplierId == editingSupplierId);
                if (supplier != null)
                {
                    supplier.SupplierName = SupplierNameTextBox.Text;
                    supplier.ContactName = ContactNameTextBox.Text;
                    supplier.ContactEmail = ContactEmailTextBox.Text;
                }
            }
            else
            {
                var newSupplier = new Suppliers
                {
                    SupplierName = SupplierNameTextBox.Text,
                    ContactName = ContactNameTextBox.Text,
                    ContactEmail = ContactEmailTextBox.Text
                };
                _context.Suppliers.Add(newSupplier);
            }

            _context.SaveChanges();
            SupplierForm.Visibility = Visibility.Collapsed;
            SupplierList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            AddSupplier.Visibility = Visibility.Visible;
            LoadSuppliers();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SupplierForm.Visibility = Visibility.Collapsed;
            SupplierList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            AddSupplier.Visibility = Visibility.Visible;
        }

        private void ClearForm()
        {
            SupplierNameTextBox.Text = string.Empty;
            ContactNameTextBox.Text = string.Empty;
            ContactEmailTextBox.Text = string.Empty;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterSuppliers();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadSuppliers();
        }

        private void FilterSuppliers()
        {
            var searchText = SearchTextBox.Text.ToLower();
            var filteredSuppliers = _context.Suppliers
                .Where(s => s.SupplierName.ToLower().Contains(searchText) ||
                            s.ContactName.ToLower().Contains(searchText) ||
                            s.ContactEmail.ToLower().Contains(searchText))
                .Select(s => new
                {
                    s.SupplierId,
                    s.SupplierName,
                    s.ContactName,
                    s.ContactEmail
                }).ToList();

            SupplierList.ItemsSource = filteredSuppliers;
        }
    }
}
