using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
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

namespace E_shopClient
{
    /// <summary>
    /// Логика взаимодействия для CreateAccount.xaml
    /// </summary>
    public partial class CreateAccountWindow : Window
    {
        private readonly string URL = "https://localhost:5283/";

        private LoginWindow _loginWindow;
        public CreateAccountWindow(LoginWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _loginWindow.Show();
            Close();
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new HttpClient();

                var user = new NewUser(EmailTextBox.Text, PasswordBox.Password, NameTextBox.Text, PhoneNumberTextBox.Text, AddressTextBox.Text);

                var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(URL + "register", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                string token = JObject.Parse(json)["token"].ToString();

                MainWindow mainWindow = new MainWindow(token);
                mainWindow.Show();
                _loginWindow.Close();
                Close();
            }
            catch (Exception ex)
            {
                ErrorMessageTextBlock.Text = "Ошибка: " + ex.Message;
            }
        }
    }

    public record NewUser(string Email, string Password, string Name, string PhoneNumber, string Address);
}
