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
        private string _token;
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<ProductAtCart> ProductsAtCart { get; set; } = new();
        public MainWindow(string token)
        {
            InitializeComponent();
            _token = token;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                HttpResponseMessage response = await client.GetAsync("http://localhost:5283/products");

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
                var tokenHadnler = new JwtSecurityTokenHandler();
                var token = tokenHadnler.ReadJwtToken(_token);
                string customerId = token.Claims.First(c => c.Type == "nameid").Value;

                var client = new HttpClient();

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var products = ProductsAtCart.Select(p => new { p.ProductId , p.Quantity}).ToList();
                var content = new StringContent(JsonConvert.SerializeObject(new { CustomerId = customerId, Products = products }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("http://localhost:5283/makeOrder", content);

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                // responseString

                ProductsAtCart.Clear();
            }
            catch (Exception ex)
            {
                ShowErrorWindow(ex);
            }

        }

        private void ShowErrorWindow(Exception ex)
        {
            if (MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error) == MessageBoxResult.OK)
            {
                Close();
            }
        }
    }

    public record Product(int ProductId, string Name, decimal Price, uint QuantityInStock);
}
