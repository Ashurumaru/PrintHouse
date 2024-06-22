using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyPracticeApp.Data;

namespace MyPracticeApp.Pages
{
    public partial class EmployeePage : Page
    {
        private PrintingHouseEntities _context;
        private bool isEditing = false;
        private int editingEmployeeId;

        public EmployeePage()
        {
            InitializeComponent();
            _context = new PrintingHouseEntities();
            LoadPositions();
            LoadEmployees();
        }

        private void LoadPositions()
        {
            var positions = _context.Positions.ToList();
            PositionComboBox.ItemsSource = positions;
        }

        private void LoadEmployees()
        {
            var employees = _context.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                    e.Positions.PositionName,
                    e.Phone,
                    e.Login,
                    e.Password,
                    TotalRevenue = _context.Orders
                        .Where(o => o.EmployeeId == e.EmployeeId)
                        .Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Price)),
                    OrderCount = _context.Orders
                        .Count(o => o.EmployeeId == e.EmployeeId)
                }).ToList();

            EmployeeList.ItemsSource = employees;
        }

        private void addButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            SetFormVisibility(true);
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingEmployeeId = (int)button.CommandParameter;
            var employee = _context.Employees.FirstOrDefault(emp => emp.EmployeeId == editingEmployeeId);

            if (employee != null)
            {
                isEditing = true;
                FirstNameTextBox.Text = employee.FirstName;
                PatronymicTextBox.Text = employee.MiddleName;
                LastNameTextBox.Text = employee.LastName;
                PositionComboBox.SelectedValue = employee.PositionId;
                PhoneTextBox.Text = employee.Phone;
                LoginTextBox.Text = employee.Login;
                PasswordTextBox.Text = employee.Password;
                SetFormVisibility(true);
            }
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var employeeId = (int)button.CommandParameter;
            var employee = _context.Employees.FirstOrDefault(emp => emp.EmployeeId == employeeId);

            if (employee != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого сотрудника и все связанные с ним записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаление всех связанных заказов сотрудника
                    var orders = _context.Orders.Where(o => o.EmployeeId == employeeId).ToList();
                    foreach (var order in orders)
                    {
                        var orderDetails = _context.OrderDetails.Where(od => od.OrderId == order.OrderId).ToList();
                        _context.OrderDetails.RemoveRange(orderDetails);
                    }
                    _context.Orders.RemoveRange(orders);

                    // Удаление сотрудника
                    _context.Employees.Remove(employee);
                    _context.SaveChanges();
                    LoadEmployees();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var employee = _context.Employees.FirstOrDefault(emp => emp.EmployeeId == editingEmployeeId);
                if (employee != null)
                {
                    employee.FirstName = FirstNameTextBox.Text;
                    employee.MiddleName = PatronymicTextBox.Text;
                    employee.LastName = LastNameTextBox.Text;
                    employee.PositionId = (int)PositionComboBox.SelectedValue;
                    employee.Phone = PhoneTextBox.Text;
                    employee.Login = LoginTextBox.Text;
                    employee.Password = PasswordTextBox.Text;
                }
            }
            else
            {
                var newEmployee = new Employees
                {
                    FirstName = FirstNameTextBox.Text,
                    MiddleName = PatronymicTextBox.Text,
                    LastName = LastNameTextBox.Text,
                    PositionId = (int)PositionComboBox.SelectedValue,
                    Phone = PhoneTextBox.Text,
                    Login = LoginTextBox.Text,
                    Password = PasswordTextBox.Text
                };
                _context.Employees.Add(newEmployee);
            }

            _context.SaveChanges();
            SetFormVisibility(false);
            LoadEmployees();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetFormVisibility(false);
        }

        private void SetFormVisibility(bool isVisible)
        {
            if (isVisible)
            {
                EmployeeForm.Visibility = Visibility.Visible;
                foreach (UIElement element in MainGrid.Children)
                {
                    if (element != EmployeeForm)
                    {
                        element.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                EmployeeForm.Visibility = Visibility.Collapsed;
                foreach (UIElement element in MainGrid.Children)
                {
                    if (element != EmployeeForm)
                    {
                        element.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void ClearForm()
        {
            FirstNameTextBox.Text = string.Empty;
            PatronymicTextBox.Text = string.Empty;
            LastNameTextBox.Text = string.Empty;
            PositionComboBox.SelectedIndex = -1;
            PhoneTextBox.Text = string.Empty;
            LoginTextBox.Text = string.Empty;
            PasswordTextBox.Text = string.Empty;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterEmployees();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadEmployees();
        }

        private void FilterEmployees()
        {
            var searchText = SearchTextBox.Text.ToLower();
            var filteredEmployees = _context.Employees
                .Where(e => e.FirstName.ToLower().Contains(searchText) ||
                            e.MiddleName.ToLower().Contains(searchText) ||
                            e.LastName.ToLower().Contains(searchText) ||
                            e.Login.ToLower().Contains(searchText) ||
                            e.Phone.Contains(searchText))
                .Select(e => new
                {
                    e.EmployeeId,
                    FullName = e.FirstName + " " + e.MiddleName + " " + e.LastName,
                    e.Positions.PositionName,
                    e.Phone,
                    e.Login,
                    e.Password,
                    TotalRevenue = _context.Orders
                        .Where(o => o.EmployeeId == e.EmployeeId)
                        .Sum(o => o.OrderDetails.Sum(od => od.Quantity * od.Price)),
                    OrderCount = _context.Orders
                        .Count(o => o.EmployeeId == e.EmployeeId)
                }).ToList();

            EmployeeList.ItemsSource = filteredEmployees;
        }
    }
}
