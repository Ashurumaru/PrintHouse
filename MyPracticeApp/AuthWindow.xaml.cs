using MyPracticeApp.Data;
using MyPracticeApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
using System.Windows.Shapes;

namespace MyPracticeApp
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private PrintingHouseEntities _context = new PrintingHouseEntities();

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void LogIn_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginUsernameTextBox.Text;
            string password = LoginPasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                LoginErrorMessage.Text = "Введите логин и пароль.";
                LoginErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                var user = _context.Employees.SingleOrDefault(u => u.Login == login && u.Password == password);
                if (user != null)
                {
                    CurrentUser.EmployeeId = user.EmployeeId;
                    CurrentUser.FirstName = user.FirstName;
                    CurrentUser.Patronymic = user.MiddleName;
                    CurrentUser.LastName = user.LastName;
                    CurrentUser.PositionId = (int)user.PositionId;
                    CurrentUser.Phone = user.Phone;
                    CurrentUser.Login = user.Login;
                    CurrentUser.Password = user.Password;

                    MainWindow dashboard = new MainWindow();
                    MessageBox.Show($"Добро пожаловать {CurrentUser.FirstName} {CurrentUser.LastName}", "Окно приветствия");
                    dashboard.Show();
                    this.Close();
                }
                else
                {
                    LoginErrorMessage.Text = "Неверный логин или пароль.";
                    LoginErrorMessage.Visibility = Visibility.Visible;
                }
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " Ошибки валидации: ", fullErrorMessage);

                MessageBox.Show("Ошибка: " + exceptionMessage);
                LoginErrorMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
                LoginErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsernameTextBox.Text;
            string password = RegisterPasswordBox.Password;
            string firstName = RegisterFirstNameTextBox.Text;
            string lastName = RegisterLastNameTextBox.Text;
            string email = RegisterEmailTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email))
            {
                RegisterErrorMessage.Text = "Пожалуйста, заполните все поля.";
                RegisterErrorMessage.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                var existingUser = _context.Employees.SingleOrDefault(u => u.Login == username);
                if (existingUser != null)
                {
                    RegisterErrorMessage.Text = "Пользователь с таким логином уже существует.";
                    RegisterErrorMessage.Visibility = Visibility.Visible;
                    return;
                }

                var newUser = new Employees
                {
                    Login = username,
                    Password = password,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    PositionId = 1, 
                };

                _context.Employees.Add(newUser);
                _context.SaveChanges();

                MessageBox.Show("Регистрация успешна!", "Успех");
                ClearRegistrationForm();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);

                var fullErrorMessage = string.Join("; ", errorMessages);
                var exceptionMessage = string.Concat(ex.Message, " Ошибки валидации: ", fullErrorMessage);

                MessageBox.Show("Ошибка: " + exceptionMessage);
                RegisterErrorMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
                RegisterErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void ClearRegistrationForm()
        {
            RegisterUsernameTextBox.Text = string.Empty;
            RegisterPasswordBox.Password = string.Empty;
            RegisterFirstNameTextBox.Text = string.Empty;
            RegisterLastNameTextBox.Text = string.Empty;
            RegisterEmailTextBox.Text = string.Empty;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}