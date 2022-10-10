using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures.Socket;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json;
using Reversal.Command;
using Reversal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Reversal.ViewModels.MainViewModel;

namespace Reversal.ViewModels
{
    public class SymbolViewModel
    {
        private UpdateSubscription? _updateSubscription { get; set; }
        public SymbolModel Symbol { get; set; }
        private BinanceSocketClient _socketClient { get; set; }
        private RelayCommand? _confirmReversalPriceCommand;
        public RelayCommand ConfirmReversalPriceCommand
        {
            get { return _confirmReversalPriceCommand ?? (_confirmReversalPriceCommand = new RelayCommand(obj => { ConfirmReversalPrice(); })); }
        }
        public SymbolViewModel(BinanceSocketClient socketClient, string symbolName)
        {
            Symbol = new();
            _socketClient = socketClient;
            Symbol.Name = symbolName;
            Symbol.PropertyChanged += Symbol_PropertyChanged;
        }

        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "Run")
            {
                if (Symbol.Run) SubscribeToAggregatedTradeUpdates();
                else Unsubscribe();
            }
            else if(e.PropertyName == "ReversalPrice")
            {
                Symbol.ConfirmedReversalPrice = false;
            }
        }

        private void SubscribeToAggregatedTradeUpdates()
        {
            try
            {
                _updateSubscription = _socketClient.UsdFuturesStreams.SubscribeToAggregatedTradeUpdatesAsync(Symbol.Name, Message =>
                {
                    Symbol.Price = Message.Data.Price;
                }).Result.Data;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SubscribeToAggregatedTradeUpdates {ex.Message}");
            }
        }
        private void Unsubscribe()
        {
            try
            {
                if(_updateSubscription != null) _socketClient.UnsubscribeAsync(_updateSubscription);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unsubscribe {ex.Message}");
            }
        }
        private void ConfirmReversalPrice()
        {
            Symbol.ConfirmedReversalPrice = true;
        }
        string Path = $"{Directory.GetCurrentDirectory()}/log/";
        int count = 0;
        public void OrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate)
        {
            if(OrderUpdate.UpdateData.Symbol == Symbol.Name)
            {
                if(OrderUpdate.UpdateData.Status == OrderStatus.Filled)
                {
                    if (!Symbol.Run)
                    {
                        Symbol.Run = true;
                        Symbol.ReversalPrice = OrderUpdate.UpdateData.AveragePrice;
                        Symbol.PositionSide = OrderUpdate.UpdateData.PositionSide;
                        Symbol.Quantity = OrderUpdate.UpdateData.Quantity;
                    }
                }
                count++;
                string json = JsonConvert.SerializeObject(OrderUpdate.UpdateData);
                File.WriteAllText(Path + Symbol.Name + count, json);
                //if (onOrderUpdate.Data.UpdateData.Symbol == symbol && onOrderUpdate.Data.UpdateData.Status == OrderStatus.Filled || onOrderUpdate.Data.UpdateData.Symbol == symbol && onOrderUpdate.Data.UpdateData.Status == OrderStatus.PartiallyFilled)
                //{
                //    Dispatcher.BeginInvoke(new Action(() =>
                //    {
                //        if (onOrderUpdate.Data.UpdateData.OrderId == short_order_id_0 && !CheckOrderIdToListOrders() || onOrderUpdate.Data.UpdateData.OrderId == long_order_id_0 && !CheckOrderIdToListOrders()) ClearListOrders();
                //        list_orders.Add(onOrderUpdate.Data.UpdateData);
                //        history_list_orders.Add(new OrderHistory(onOrderUpdate.Data.UpdateData));
                //        HISTORY_LIST.Items.Refresh();
                //        PriceOrder(onOrderUpdate.Data.UpdateData);
                //        LoadingChartOrders();
                //        TradeHistoryAsync();
                //    }));
                //}
            }
        }
    }
}
