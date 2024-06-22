using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyPracticeApp.Data;
using MyPracticeApp.Models;

namespace MyPracticeApp.Pages
{
    public partial class OrderPage : Page
    {
        private PrintingHouseEntities _context;
        private bool isEditing = false;
        private List<OrderItem> currentOrderItems;
        private int editingOrderId;
        private int currentOrderId;
        
        public OrderPage()
        {
            InitializeComponent();
            _context = new PrintingHouseEntities();
            LoadClients();
            LoadEmployees();
            LoadOrders();
            LoadNewItemProduct();
        }
        private void LoadNewItemProduct()
        {
            var product = _context.Products.ToList();
            NewItemProductComboBox.ItemsSource = product;
        }
        private void LoadClients()
        {
            var clients = _context.Clients.ToList();
            ClientComboBox.ItemsSource = clients;
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees
               .Select(e => new
               {
                   e.EmployeeId,
                   FullName = e.FirstName + " " + e.MiddleName + " " + e.LastName
               }).ToList();
            EmployeeComboBox.ItemsSource = employees;
        }

        private void LoadOrders()
        {
            var orders = _context.Orders
                .Select(o => new
                {
                    o.OrderId,
                    ClientName = o.Clients.ClientName,
                    EmployeeName = o.Employees.FirstName + " " + o.Employees.MiddleName + " " + o.Employees.LastName,
                    o.OrderDate,
                    TotalCost = o.OrderDetails.Sum(od => od.Quantity * od.Products.Price)
                }).OrderByDescending(o => o.OrderDate).ToList();

            OrderList.ItemsSource = orders;
        }

        private void addButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            OrderItemsPanel.Visibility = Visibility.Collapsed;
            OrderForm.Visibility = Visibility.Visible;
            OrderList.Visibility = Visibility.Collapsed;
            TextOrder.Visibility = Visibility.Collapsed;
            Search.Visibility = Visibility.Collapsed;
            AddOrder.Visibility = Visibility.Collapsed;
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingOrderId = (int)button.CommandParameter;
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == editingOrderId);

            if (order != null)
            {
                isEditing = true;
                ClientComboBox.SelectedValue = order.ClientId;
                EmployeeComboBox.SelectedValue = order.EmployeeId;
                OrderDatePicker.SelectedDate = order.OrderDate;
                TotalCostTextBox.Text = order.OrderDetails.Sum(od => od.Quantity * od.Products.Price).ToString();
                OrderItemsPanel.Visibility = Visibility.Collapsed;
                OrderForm.Visibility = Visibility.Visible;
                OrderList.Visibility = Visibility.Collapsed;
                TextOrder.Visibility = Visibility.Collapsed;
                Search.Visibility = Visibility.Collapsed;
                AddOrder.Visibility = Visibility.Collapsed;
            }
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderId = (int)button.CommandParameter;
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот заказ и все связанные с ним записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    var orderDetails = _context.OrderDetails.Where(od => od.OrderId == orderId).ToList();
                    _context.OrderDetails.RemoveRange(orderDetails);
                    _context.Orders.Remove(order);
                    _context.SaveChanges();
                    LoadOrders();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var order = _context.Orders.FirstOrDefault(o => o.OrderId == editingOrderId);
                if (order != null)
                {
                    order.ClientId = (int)ClientComboBox.SelectedValue;
                    order.EmployeeId = (int)EmployeeComboBox.SelectedValue;
                    order.OrderDate = OrderDatePicker.SelectedDate.Value;
                }
            }
            else
            {
                var newOrder = new Orders
                {
                    ClientId = (int)ClientComboBox.SelectedValue,
                    EmployeeId = (int)EmployeeComboBox.SelectedValue,
                    OrderDate = OrderDatePicker.SelectedDate.Value,
                };
                _context.Orders.Add(newOrder);
            }

            _context.SaveChanges();
            OrderItemsPanel.Visibility = Visibility.Collapsed;
            OrderForm.Visibility = Visibility.Collapsed;
            OrderList.Visibility = Visibility.Visible;
            TextOrder.Visibility = Visibility.Visible;
            Search.Visibility = Visibility.Visible;
            AddOrder.Visibility = Visibility.Visible; LoadOrders();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OrderItemsPanel.Visibility = Visibility.Collapsed;
            OrderForm.Visibility = Visibility.Collapsed;
            OrderList.Visibility = Visibility.Visible;
            TextOrder.Visibility = Visibility.Visible;
            Search.Visibility = Visibility.Visible;
            AddOrder.Visibility = Visibility.Visible;
        }

        private void ClearForm()
        {
            ClientComboBox.SelectedIndex = -1;
            EmployeeComboBox.SelectedIndex = -1;
            OrderDatePicker.SelectedDate = null;
            TotalCostTextBox.Text = string.Empty;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterOrders();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadOrders();
        }

        private void FilterOrders()
        {
            var searchText = SearchTextBox.Text.ToLower();
            var filteredOrders = _context.Orders
                .Where(o => o.Clients.ClientName.ToLower().Contains(searchText) ||
                            o.Employees.FirstName.ToLower().Contains(searchText) ||
                            o.Employees.MiddleName.ToLower().Contains(searchText) ||
                            o.Employees.LastName.ToLower().Contains(searchText) ||
                            o.OrderDate.ToString().Contains(searchText))
                .Select(o => new
                {
                    o.OrderId,
                    ClientName = o.Clients.ClientName,
                    EmployeeName = o.Employees.FirstName + " " + o.Employees.MiddleName + " " + o.Employees.LastName,
                    o.OrderDate,
                    TotalCost = o.OrderDetails.Sum(od => od.Quantity * od.Products.Price)
                }).ToList();

            OrderList.ItemsSource = filteredOrders;
        }

        private void ShowOrderItems_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            currentOrderId = (int)button.CommandParameter;
            ShowOrderItems(currentOrderId);
        }

        private void ShowOrderItems(int orderId)
        {
            currentOrderItems = _context.OrderDetails
                .Where(od => od.OrderId == orderId)
                .Select(od => new OrderItem
                {
                    ProductName = od.Products.ProductName,
                    Quantity = (int)od.Quantity,
                    Price = od.Products.Price
                }).ToList();

            numberOrder.Text = orderId.ToString();
            OrderItemsControl.ItemsSource = currentOrderItems;
            OrderItemsPanel.Visibility = Visibility.Visible;
            OrderForm.Visibility = Visibility.Collapsed;
            OrderList.Visibility = Visibility.Collapsed;
            TextOrder.Visibility = Visibility.Collapsed;
            Search.Visibility = Visibility.Collapsed;
            AddOrder.Visibility = Visibility.Collapsed;
            UpdateTotalCost();
        }

        private void CancelOrderItems_Click(object sender, RoutedEventArgs e)
        {
            OrderItemsPanel.Visibility = Visibility.Collapsed;
            OrderList.Visibility = Visibility.Visible;
            TextOrder.Visibility = Visibility.Visible;
            Search.Visibility = Visibility.Visible;
            AddOrder.Visibility = Visibility.Visible;
        }

        private void AddOrderItem_Click(object sender, RoutedEventArgs e)
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

            var existingOrderItem = currentOrderItems.FirstOrDefault(oi => oi.ProductName == selectedProduct.ProductName);

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

                currentOrderItems.Add(newItem);
            }

            OrderItemsControl.ItemsSource = null;
            OrderItemsControl.ItemsSource = currentOrderItems;
            UpdateTotalCost();
            NewItemProductComboBox.SelectedIndex = -1;
            NewItemQuantityTextBox.Text = null;

        }

        private void SaveOrderItems_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in currentOrderItems)
            {
                var orderDetail = _context.OrderDetails.FirstOrDefault(od => od.OrderId == currentOrderId && od.Products.ProductName == item.ProductName);
                if (orderDetail != null)
                {
                    orderDetail.Quantity = item.Quantity;
                }
                else
                {
                    var product = _context.Products.FirstOrDefault(p => p.ProductName == item.ProductName);
                    if (product != null)
                    {
                        orderDetail = new OrderDetails
                        {
                            OrderId = currentOrderId,
                            ProductId = product.ProductId,
                            Quantity = item.Quantity
                        };
                        _context.OrderDetails.Add(orderDetail);
                    }
                }
            }

            _context.SaveChanges();
            MessageBox.Show("Позиции заказа успешно сохранены!");
            OrderItemsPanel.Visibility = Visibility.Collapsed;
            OrderList.Visibility = Visibility.Visible;
            TextOrder.Visibility = Visibility.Visible;
            Search.Visibility = Visibility.Visible;
            AddOrder.Visibility = Visibility.Visible;
            LoadOrders();
        }



        private void DeleteOrderItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var orderItem = button.CommandParameter as OrderItem;

            var orderDetail = _context.OrderDetails.FirstOrDefault(od => od.OrderId == currentOrderId && od.Products.ProductName == orderItem.ProductName);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
                currentOrderItems.Remove(orderItem);
                _context.SaveChanges();
                OrderItemsControl.ItemsSource = null;
                OrderItemsControl.ItemsSource = currentOrderItems;
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
            var totalCost = currentOrderItems.Sum(i => i.TotalCost);
            TotalCostTextBlock.Text = $"Итоговая стоимость: {totalCost:F2} ₽";
        }

    }
}