using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyPracticeApp.Data;

namespace MyPracticeApp.Pages
{
    public partial class ClientPage : Page
    {
        private PrintingHouseEntities _context;
        private bool isEditing = false;
        private int editingClientId;

        public ClientPage()
        {
            InitializeComponent();
            _context = new PrintingHouseEntities();
            LoadClients();
        }

        private void LoadClients()
        {
            var clients = _context.Clients
                .Select(c => new
                {
                    c.ClientId,
                    c.ClientName,
                    c.ContactName,
                    c.Phone,
                    c.Email,
                    c.LegalAddress,
                    c.City,
                    c.Country
                }).ToList();

            ClientList.ItemsSource = clients;
        }

        private void addButtonClick(object sender, RoutedEventArgs e)
        {
            ClearForm();
            isEditing = false;
            SetFormVisibility(true);
        }

        private void EditClient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            editingClientId = (int)button.CommandParameter;
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == editingClientId);

            if (client != null)
            {
                isEditing = true;
                ClientNameTextBox.Text = client.ClientName;
                ContactNameTextBox.Text = client.ContactName;
                PhoneTextBox.Text = client.Phone;
                EmailTextBox.Text = client.Email;
                AddressTextBox.Text = client.LegalAddress;
                CityTextBox.Text = client.City;
                CountryTextBox.Text = client.Country;
                SetFormVisibility(true);
            }
        }

        private void DeleteClient_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var clientId = (int)button.CommandParameter;
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == clientId);

            if (client != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого клиента и все связанные с ним записи?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    // Удаление всех связанных заказов клиента
                    var orders = _context.Orders.Where(o => o.ClientId == clientId).ToList();
                    foreach (var order in orders)
                    {
                        var orderDetails = _context.OrderDetails.Where(od => od.OrderId == order.OrderId).ToList();
                        _context.OrderDetails.RemoveRange(orderDetails);
                    }
                    _context.Orders.RemoveRange(orders);

                    // Удаление клиента
                    _context.Clients.Remove(client);
                    _context.SaveChanges();
                    LoadClients();
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (isEditing)
            {
                var client = _context.Clients.FirstOrDefault(c => c.ClientId == editingClientId);
                if (client != null)
                {
                    client.ClientName = ClientNameTextBox.Text;
                    client.ContactName = ContactNameTextBox.Text;
                    client.Phone = PhoneTextBox.Text;
                    client.Email = EmailTextBox.Text;
                    client.LegalAddress = AddressTextBox.Text;
                    client.City = CityTextBox.Text;
                    client.Country = CountryTextBox.Text;
                }
            }
            else
            {
                var newClient = new Clients
                {
                    ClientName = ClientNameTextBox.Text,
                    ContactName = ContactNameTextBox.Text,
                    Phone = PhoneTextBox.Text,
                    Email = EmailTextBox.Text,
                    LegalAddress = AddressTextBox.Text,
                    City = CityTextBox.Text,
                    Country = CountryTextBox.Text
                };
                _context.Clients.Add(newClient);
            }

            _context.SaveChanges();
            SetFormVisibility(false);
            LoadClients();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            SetFormVisibility(false);
        }

        private void SetFormVisibility(bool isVisible)
        {
            if (isVisible)
            {
                ClientForm.Visibility = Visibility.Visible;
                foreach (UIElement element in MainGrid.Children)
                {
                    if (element != ClientForm)
                    {
                        element.Visibility = Visibility.Collapsed;
                    }
                }
            }
            else
            {
                ClientForm.Visibility = Visibility.Collapsed;
                foreach (UIElement element in MainGrid.Children)
                {
                    if (element != ClientForm)
                    {
                        element.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void ClearForm()
        {
            ClientNameTextBox.Text = string.Empty;
            ContactNameTextBox.Text = string.Empty;
            PhoneTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            AddressTextBox.Text = string.Empty;
            CityTextBox.Text = string.Empty;
            CountryTextBox.Text = string.Empty;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilterClients();
        }

        private void ResetSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadClients();
        }

        private void FilterClients()
        {
            var searchText = SearchTextBox.Text.ToLower();
            var filteredClients = _context.Clients
                .Where(c => c.ClientName.ToLower().Contains(searchText) ||
                            c.ContactName.ToLower().Contains(searchText) ||
                            c.Phone.Contains(searchText) ||
                            c.Email.ToLower().Contains(searchText) ||
                            c.LegalAddress.ToLower().Contains(searchText) ||
                            c.City.ToLower().Contains(searchText) ||
                            c.Country.ToLower().Contains(searchText))
                .Select(c => new
                {
                    c.ClientId,
                    c.ClientName,
                    c.ContactName,
                    c.Phone,
                    c.Email,
                    c.LegalAddress,
                    c.City,
                    c.Country
                }).ToList();

            ClientList.ItemsSource = filteredClients;
        }
    }
}
