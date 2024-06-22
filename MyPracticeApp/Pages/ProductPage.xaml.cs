using MyPracticeApp.Data;
using MyPracticeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyPracticeApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductPage.xaml
    /// </summary>
    public partial class ProductPage : Page
    {
        private PrintingHouseEntities _context;
        private bool isEditing = false;
        private int editingProductId;

        public ProductPage()
        {
            InitializeComponent();
            _context = new PrintingHouseEntities();
            LoadProducts();
        }

        private void LoadProducts()
        {
            var products = _context.Products
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Price,
                    p.Quantity
                }).OrderBy(p => p.ProductName).ToList();

            ProductList.ItemsSource = products;
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            ProductForm.Visibility = Visibility.Visible;
            ProductList.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Collapsed;
            TextProduct.Visibility = Visibility.Collapsed;
            ResetSearchButton.Visibility = Visibility.Collapsed;
            AddProduct.Visibility = Visibility.Collapsed;
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingProductId = (int)button.CommandParameter;
            var product = _context.Products.FirstOrDefault(p => p.ProductId == editingProductId);

            if (product != null)
            {
                isEditing = true;
                ProductNameTextBox.Text = product.ProductName;
                ProductPriceTextBox.Text = product.Price.ToString();
                ProductQuantityTextBox.Text = product.Quantity.ToString();
                ProductForm.Visibility = Visibility.Visible;
                ProductList.Visibility = Visibility.Collapsed;
                SearchTextBox.Visibility = Visibility.Collapsed;
                TextProduct.Visibility = Visibility.Collapsed;
                ResetSearchButton.Visibility = Visibility.Collapsed;
                AddProduct.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var productId = (int)button.CommandParameter;
            var product = _context.Products.FirstOrDefault(p => p.ProductId == productId);

            if (product != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот продукт?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    _context.Products.Remove(product);
                    _context.SaveChanges();
                    LoadProducts();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var product = _context.Products.FirstOrDefault(p => p.ProductId == editingProductId);
                if (product != null)
                {
                    product.ProductName = ProductNameTextBox.Text;
                    product.Price = decimal.Parse(ProductPriceTextBox.Text);
                    product.Quantity = int.Parse(ProductQuantityTextBox.Text);
                }
            }
            else
            {
                var newProduct = new Products
                {
                    ProductName = ProductNameTextBox.Text,
                    Price = decimal.Parse(ProductPriceTextBox.Text),
                    Quantity = int.Parse(ProductQuantityTextBox.Text)
                };
                _context.Products.Add(newProduct);
            }

            _context.SaveChanges();
            ProductForm.Visibility = Visibility.Collapsed;
            ProductList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            AddProduct.Visibility = Visibility.Visible;
            LoadProducts();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ProductForm.Visibility = Visibility.Collapsed;
            ProductList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            AddProduct.Visibility = Visibility.Visible;
        }

        private void ClearForm()
        {
            ProductNameTextBox.Text = string.Empty;
            ProductPriceTextBox.Text = string.Empty;
            ProductQuantityTextBox.Text = string.Empty;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterProducts();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadProducts();
        }

        private void FilterProducts()
        {
            var searchText = SearchTextBox.Text.ToLower();
            var filteredProducts = _context.Products
                .Where(p => p.ProductName.ToLower().Contains(searchText))
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Price,
                    p.Quantity
                }).ToList();

            ProductList.ItemsSource = filteredProducts;
        }
    }
}
