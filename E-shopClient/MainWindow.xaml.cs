using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public MainWindow()
        {
            InitializeComponent();
            //_token = token;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                HttpResponseMessage response = await client.GetAsync("http://localhost:5000/products");
                var json = await response.Content.ReadAsStringAsync();
                Products = JsonConvert.DeserializeObject<ObservableCollection<Product>>(json);
                ProductsListView.ItemsSource = Products;
            }
            catch (Exception ex)
            {
                StatusLabel.Content = "Ошибка: " + ex.Message;
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
                ProductsAtCart.Add(new ProductAtCart { 
                    ProductId = productToAdd.ProductId,
                    Name = productToAdd.Name,
                    Price = productToAdd.Price,
                    Quantity = 1
                });
            }
        }
    }

    public record Product(int ProductId, string Name, decimal Price, uint QuantityInStock);

    public class ProductAtCart : INotifyPropertyChanged
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        private uint _quantity;
        public uint Quantity {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                }
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
