﻿using Binance.Net.Enums;
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
        private bool _confirmedReversalPrice { get; set; }
        public bool ConfirmedReversalPrice
        {
            get { return _confirmedReversalPrice; }
            set
            {
                _confirmedReversalPrice = value;
                OnPropertyChanged("ConfirmedReversalPrice");
            }
        }
        private PositionSide _positionSide { get; set; }
        public PositionSide PositionSide
        {
            get { return _positionSide; }
            set
            {
                _positionSide = value;
                OnPropertyChanged("PositionSide");
            }
        }
    }
}
