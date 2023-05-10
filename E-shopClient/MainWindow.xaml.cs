using E_shopClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace E_shopClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string URL = "https://localhost:5283/";

        private string _token;
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<ProductAtCart> ProductsAtCart { get; set; } = new();
        public MainWindow(string token)
        {
            InitializeComponent();
            _token = token;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            RefreshProductList();
        }

        private async void RefreshProductList()
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage response = await client.GetAsync(URL + "products");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                Products = JsonConvert.DeserializeObject<ObservableCollection<Product>>(json);
                ProductsListView.ItemsSource = Products;
            }
            catch (Exception ex)
            {
                ShowErrorWindow(ex);
            }
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListView.SelectedItem == null) return;

            var productToAdd = (ProductsListView.SelectedItem as Product);

            var foundedProdcut = ProductsAtCart.FirstOrDefault(p => p.ProductId == productToAdd.ProductId);

            if (foundedProdcut != null)
            {
                foundedProdcut.Quantity += 1;
            }
            else
            {
                ProductsAtCart.Add(new ProductAtCart
                {
                    ProductId = productToAdd.ProductId,
                    Name = productToAdd.Name,
                    Price = productToAdd.Price,
                    Quantity = 1
                });
            }
        }

        private void DeleteFromCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsAtCartListView.SelectedItem == null) return;

            var selectedItem = (ProductsAtCartListView.SelectedItem as ProductAtCart);

            if (selectedItem.Quantity == 1)
            {
                ProductsAtCart.Remove(selectedItem);
            }
            else
            {
                selectedItem.Quantity -= 1;
            }
        }

        private async void MakeOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var products = ProductsAtCart.Select(p => new { p.ProductId, p.Quantity }).ToList();
                var content = new StringContent(JsonConvert.SerializeObject(new { CustomerId = CurrentId, Products = products }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(URL + "makeOrder", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                ProductsAtCart.Clear();
                RefreshProductList();

                ShowConfirmWindow("Заказ успешно офрлмен", "Готово", MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowErrorWindow(ex);
            }

        }

        public string CurrentId
        {
            get
            {
                var tokenHadnler = new JwtSecurityTokenHandler();
                var token = tokenHadnler.ReadJwtToken(_token);
                return token.Claims.First(c => c.Type == "nameid").Value;
            }
        }

        private void ShowConfirmWindow(string message, string title, MessageBoxImage messageBoxImage, bool needToClose = false)
        {
            if (MessageBox.Show(message, title, MessageBoxButton.OK, messageBoxImage) == MessageBoxResult.OK)
            {
                if (needToClose)
                {
                    Close();
                }   
            }
        }

        private void ShowErrorWindow(Exception ex) => ShowConfirmWindow(ex.Message, "Ошибка", MessageBoxImage.Error, true);

        private async void ShowOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage response = await client.GetAsync(URL + $"customers/{CurrentId}/orders");
                response.EnsureSuccessStatusCode();

                var responseString = await response.Content.ReadAsStringAsync();

                if (responseString == "У данного пользователя нет заказов")
                {
                    ShowConfirmWindow(responseString, "Ошибка", MessageBoxImage.Information);
                }

                var orders = JsonConvert.DeserializeObject<List<Order>>(responseString);

                OrdersWindow ordersWindow = new OrdersWindow(orders, this);
                ordersWindow.Show();
                Hide();
            }
            catch (Exception ex)
            {
                ShowErrorWindow(ex);
            }
        }
    }

    public record Product(int ProductId, string Name, decimal Price, uint QuantityInStock);
}
