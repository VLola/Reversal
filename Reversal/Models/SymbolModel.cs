using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Reversal.Models
{
    public class SymbolModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public List<long> IdOrders = new();
        private string _name { get; set; }
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private bool _select { get; set; }
        public bool Select
        {
            get { return _select; }
            set
            {
                _select = value;
                OnPropertyChanged("Select");
            }
        }
        private decimal _price { get; set; }
        public decimal Price
        {
            get { return _price; }
            set
            {
                _price = value;
                OnPropertyChanged("Price");
            }
        }
        private decimal _reversalPrice { get; set; }
        public decimal ReversalPrice
        {
            get { return _reversalPrice; }
            set
            {
                _reversalPrice = value;
                OnPropertyChanged("ReversalPrice");
            }
        }
        private decimal _quantity { get; set; }
        public decimal Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }
        private bool _run { get; set; }
        public bool Run
        {
            get { return _run; }
            set
            {
                _run = value;
                OnPropertyChanged("Run");
            }
        }
        private string _positionSide { get; set; }
        public string PositionSide
        {
            get { return _positionSide; }
            set
            {
                _positionSide = value;
                OnPropertyChanged("PositionSide");
            }
        }
        private string _side { get; set; }
        public string Side
        {
            get { return _side; }
            set
            {
                _side = value;
                OnPropertyChanged("Side");
            }
        }
        private bool _isOpenOrder { get; set; }
        public bool IsOpenOrder
        {
            get { return _isOpenOrder; }
            set
            {
                _isOpenOrder = value;
                OnPropertyChanged("IsOpenOrder");
            }
        }
        private bool _isCloseOrder { get; set; }
        public bool IsCloseOrder
        {
            get { return _isCloseOrder; }
            set
            {
                _isCloseOrder = value;
                OnPropertyChanged("IsCloseOrder");
            }
        }
        private decimal _takeProfit { get; set; } = 5m;
        public decimal TakeProfit
        {
            get { return _takeProfit; }
            set
            {
                _takeProfit = value;
                OnPropertyChanged("TakeProfit");
            }
        }
        private decimal _priceTakeProfit { get; set; }
        public decimal PriceTakeProfit
        {
            get { return _priceTakeProfit; }
            set
            {
                _priceTakeProfit = value;
                OnPropertyChanged("PriceTakeProfit");
            }
        }
    }
}
