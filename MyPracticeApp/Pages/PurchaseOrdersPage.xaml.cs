using MyPracticeApp.Data;
using MyPracticeApp.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MyPracticeApp.Pages
{
    /// <summary>
    /// Логика взаимодействия для PurchaseOrdersPage.xaml
    /// </summary>
    public partial class PurchaseOrdersPage : Page
    {
        private PrintingHouseEntities _context;
        private bool isEditing = false;
        private List<OrderItem> currentPurchaseOrderItems;
        private int editingPurchaseOrderId;
        private int currentPurchaseOrderId;

        public PurchaseOrdersPage()
        {
            InitializeComponent();
            _context = new PrintingHouseEntities();
            LoadSuppliers();
            LoadPurchaseOrders();
            LoadNewItemProduct();
        }

        private void LoadSuppliers()
        {
            var suppliers = _context.Suppliers.ToList();
            SupplierComboBox.ItemsSource = suppliers;
        }

        private void LoadNewItemProduct()
        {
            var products = _context.Products.ToList();
            NewItemProductComboBox.ItemsSource = products;
        }

        private void LoadPurchaseOrders()
        {
            var purchaseOrders = _context.PurchaseOrders
                .Select(po => new
                {
                    po.PurchaseOrderId,
                    SupplierName = po.Suppliers.SupplierName,
                    po.OrderDate,
                    TotalCost = po.PurchaseOrderDetails.Sum(pod => pod.Quantity * pod.Price)
                }).OrderByDescending(po => po.OrderDate).ToList();

            PurchaseOrderList.ItemsSource = purchaseOrders;
        }

        private void AddButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            PurchaseOrderItemsPanel.Visibility = Visibility.Collapsed;
            PurchaseOrderForm.Visibility = Visibility.Visible;
            PurchaseOrderList.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Collapsed;
            TextOrder.Visibility = Visibility.Collapsed;
            ResetSearchButton.Visibility = Visibility.Collapsed;
            AddOrder.Visibility = Visibility.Collapsed;
        }

        private void EditPurchaseOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingPurchaseOrderId = (int)button.CommandParameter;
            var purchaseOrder = _context.PurchaseOrders.FirstOrDefault(po => po.PurchaseOrderId == editingPurchaseOrderId);

            if (purchaseOrder != null)
            {
                isEditing = true;
                SupplierComboBox.SelectedValue = purchaseOrder.SupplierId;
                PurchaseOrderDatePicker.SelectedDate = purchaseOrder.OrderDate;
                TotalCostTextBox.Text = purchaseOrder.PurchaseOrderDetails.Sum(pod => pod.Quantity * pod.Price).ToString();
                PurchaseOrderItemsPanel.Visibility = Visibility.Collapsed;
                PurchaseOrderForm.Visibility = Visibility.Visible;
                PurchaseOrderList.Visibility = Visibility.Collapsed;
                SearchTextBox.Visibility = Visibility.Collapsed;
                TextOrder.Visibility = Visibility.Collapsed;
                ResetSearchButton.Visibility = Visibility.Collapsed;
                AddOrder.Visibility = Visibility.Collapsed;
            }
        }

        private void DeletePurchaseOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var purchaseOrderId = (int)button.CommandParameter;
            var purchaseOrder = _context.PurchaseOrders.FirstOrDefault(po => po.PurchaseOrderId == purchaseOrderId);

            if (purchaseOrder != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ на закупку и все связанные с ним записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var purchaseOrderDetails = _context.PurchaseOrderDetails.Where(pod => pod.PurchaseOrderId == purchaseOrderId).ToList();
                    _context.PurchaseOrderDetails.RemoveRange(purchaseOrderDetails);
                    _context.PurchaseOrders.Remove(purchaseOrder);
                    _context.SaveChanges();
                    LoadPurchaseOrders();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var purchaseOrder = _context.PurchaseOrders.FirstOrDefault(po => po.PurchaseOrderId == editingPurchaseOrderId);
                if (purchaseOrder != null)
                {
                    purchaseOrder.SupplierId = (int)SupplierComboBox.SelectedValue;
                    purchaseOrder.OrderDate = PurchaseOrderDatePicker.SelectedDate.Value;
                }
            }
            else
            {
                var newPurchaseOrder = new PurchaseOrders
                {
                    SupplierId = (int)SupplierComboBox.SelectedValue,
                    OrderDate = PurchaseOrderDatePicker.SelectedDate.Value,
                };
                _context.PurchaseOrders.Add(newPurchaseOrder);
                _context.SaveChanges();
                currentPurchaseOrderId = newPurchaseOrder.PurchaseOrderId;
            }

            SavePurchaseOrderItems();

            PurchaseOrderItemsPanel.Visibility = Visibility.Collapsed;
            PurchaseOrderForm.Visibility = Visibility.Collapsed;
            PurchaseOrderList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            AddOrder.Visibility = Visibility.Visible;
            LoadPurchaseOrders();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            PurchaseOrderItemsPanel.Visibility = Visibility.Collapsed;
            PurchaseOrderForm.Visibility = Visibility.Collapsed;
            PurchaseOrderList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            AddOrder.Visibility = Visibility.Visible;
        }

        private void ClearForm()
        {
            SupplierComboBox.SelectedIndex = -1;
            PurchaseOrderDatePicker.SelectedDate = null;
            TotalCostTextBox.Text = string.Empty;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterPurchaseOrders();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadPurchaseOrders();
        }

        private void FilterPurchaseOrders()
        {
            var searchText = SearchTextBox.Text.ToLower();
            var filteredPurchaseOrders = _context.PurchaseOrders
                .Where(po => po.Suppliers.SupplierName.ToLower().Contains(searchText) ||
                             po.OrderDate.ToString().Contains(searchText))
                .Select(po => new
                {
                    po.PurchaseOrderId,
                    SupplierName = po.Suppliers.SupplierName,
                    po.OrderDate,
                    TotalCost = po.PurchaseOrderDetails.Sum(pod => pod.Quantity * pod.Price)
                }).ToList();

            PurchaseOrderList.ItemsSource = filteredPurchaseOrders;
        }

        private void ShowPurchaseOrderItems_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            currentPurchaseOrderId = (int)button.CommandParameter;
            ShowPurchaseOrderItems(currentPurchaseOrderId);
        }

        private void ShowPurchaseOrderItems(int purchaseOrderId)
        {
            currentPurchaseOrderItems = _context.PurchaseOrderDetails
                .Where(pod => pod.PurchaseOrderId == purchaseOrderId)
                .Select(pod => new OrderItem
                {
                    ProductName = pod.Products.ProductName,
                    Quantity = (int)pod.Quantity,
                    Price = (decimal)pod.Price
                }).ToList();

            numberPurchaseOrder.Text = purchaseOrderId.ToString();
            PurchaseOrderItemsControl.ItemsSource = currentPurchaseOrderItems;
            PurchaseOrderItemsPanel.Visibility = Visibility.Visible;
            PurchaseOrderForm.Visibility = Visibility.Collapsed;
            PurchaseOrderList.Visibility = Visibility.Collapsed;
            TextOrder.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Collapsed;
            ResetSearchButton.Visibility = Visibility.Collapsed;
            AddOrder.Visibility = Visibility.Collapsed;
            UpdateTotalCost();
        }

        private void CancelPurchaseOrderItems_Click(object sender, RoutedEventArgs e)
        {
            PurchaseOrderItemsPanel.Visibility = Visibility.Collapsed;
            PurchaseOrderList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            TextOrder.Visibility = Visibility.Visible;
        }

        private void AddPurchaseOrderItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedProduct = NewItemProductComboBox.SelectedItem as Products;
            if (selectedProduct == null)
            {
                MessageBox.Show("Пожалуйста, выберите продукт для добавления.");
                return;
            }

            if (!int.TryParse(NewItemQuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Пожалуйста, введите правильное количество.");
                return;
            }

            var existingOrderItem = currentPurchaseOrderItems.FirstOrDefault(oi => oi.ProductName == selectedProduct.ProductName);

            if (existingOrderItem != null)
            {
                existingOrderItem.Quantity += quantity;
            }
            else
            {
                var newItem = new OrderItem
                {
                    ProductName = selectedProduct.ProductName,
                    Quantity = quantity,
                    Price = selectedProduct.Price
                };

                currentPurchaseOrderItems.Add(newItem);
            }

            PurchaseOrderItemsControl.ItemsSource = null;
            PurchaseOrderItemsControl.ItemsSource = currentPurchaseOrderItems;
            UpdateTotalCost();
            NewItemProductComboBox.SelectedIndex = -1;
            NewItemQuantityTextBox.Text = null;
        }

        private void SavePurchaseOrderItems_Click(object sender, RoutedEventArgs e)
        {
            SavePurchaseOrderItems();
        }

        private void SavePurchaseOrderItems()
        {
            var purchaseOrder = _context.PurchaseOrders.FirstOrDefault(po => po.PurchaseOrderId == currentPurchaseOrderId);

            if (purchaseOrder != null)
            {
                foreach (var item in currentPurchaseOrderItems)
                {
                    var purchaseOrderDetail = _context.PurchaseOrderDetails.FirstOrDefault(pod => pod.PurchaseOrderId == currentPurchaseOrderId && pod.Products.ProductName == item.ProductName);
                    if (purchaseOrderDetail != null)
                    {
                        purchaseOrderDetail.Quantity = item.Quantity;
                    }
                    else
                    {
                        var product = _context.Products.FirstOrDefault(p => p.ProductName == item.ProductName);
                        if (product != null)
                        {
                            purchaseOrderDetail = new PurchaseOrderDetails
                            {
                                PurchaseOrderId = currentPurchaseOrderId,
                                ProductId = product.ProductId,
                                Quantity = item.Quantity,
                                Price = item.Price
                            };
                            _context.PurchaseOrderDetails.Add(purchaseOrderDetail);
                        }
                    }
                }
            }

            _context.SaveChanges();
            MessageBox.Show("Позиции заказа на закупку успешно сохранены!");
            PurchaseOrderItemsPanel.Visibility = Visibility.Collapsed;
            PurchaseOrderList.Visibility = Visibility.Visible;
            SearchTextBox.Visibility = Visibility.Visible;
            TextOrder.Visibility = Visibility.Visible;
            ResetSearchButton.Visibility = Visibility.Visible;
            AddOrder.Visibility = Visibility.Visible;
            LoadPurchaseOrders();
        }

        private void DeletePurchaseOrderItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderItem = button.CommandParameter as OrderItem;

            var purchaseOrderDetail = _context.PurchaseOrderDetails.FirstOrDefault(pod => pod.PurchaseOrderId == currentPurchaseOrderId && pod.Products.ProductName == orderItem.ProductName);
            if (purchaseOrderDetail != null)
            {
                _context.PurchaseOrderDetails.Remove(purchaseOrderDetail);
                currentPurchaseOrderItems.Remove(orderItem);
                _context.SaveChanges();
                PurchaseOrderItemsControl.ItemsSource = null;
                PurchaseOrderItemsControl.ItemsSource = currentPurchaseOrderItems;
                UpdateTotalCost();
            }
        }

        private void QuantityTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.DataContext is OrderItem item)
            {
                if (int.TryParse(textBox.Text, out int quantity))
                {
                    item.Quantity = quantity;
                    UpdateTotalCost();
                }
            }
        }

        private void UpdateTotalCost()
        {
            var totalCost = currentPurchaseOrderItems.Sum(i => i.TotalCost);
            TotalCostTextBlock.Text = $"Итоговая стоимость: {totalCost:F2} ₽";
        }
    }
}