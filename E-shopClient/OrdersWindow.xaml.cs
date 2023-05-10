using E_shopClient.Models;
using System;
using System.Collections.Generic;
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

namespace E_shopClient
{
    /// <summary>
    /// Логика взаимодействия для OrdersWindow.xaml
    /// </summary>
    public partial class OrdersWindow : Window
    {
        public ICollection<Order> Orders { get; set; }
        private MainWindow _mainWindow;
        public OrdersWindow(ICollection<Order> orders, MainWindow mainWindow)
        {
            InitializeComponent();
            Orders = orders;
            OrdersListView.ItemsSource = Orders;
            _mainWindow = mainWindow;
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindow.Show();
            Close();
        }
    }
}
