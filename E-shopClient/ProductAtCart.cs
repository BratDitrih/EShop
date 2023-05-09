using System.ComponentModel;

namespace E_shopClient
{
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
