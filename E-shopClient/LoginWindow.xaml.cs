﻿using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly string URL = "https://localhost:5283/";
        public LoginWindow()
        {
            InitializeComponent();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new HttpClient();

                var user = new User(EmailTextBox.Text, PasswordBox.Password);

                var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(URL + "login", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                string token = JObject.Parse(json)["token"].ToString();

                MainWindow mainWindow = new MainWindow(token);
                mainWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                ErrorMessageTextBlock.Text = "Ошибка: " + ex.Message;
            }
        }

        private void CreateAccountButton_Click(object sender, RoutedEventArgs e)
        {
            var createAccountWindow = new CreateAccountWindow(this);
            createAccountWindow.Show();
            Hide();
        }
    }

    public record User(string Email, string Password);
}
