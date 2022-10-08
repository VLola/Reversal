using Binance.Net.Clients;
using CryptoExchange.Net.CommonObjects;
using Reversal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reversal.ViewModels
{
    public class SymbolViewModel
    {
        public SymbolModel Symbol { get; set; }
        private static BinanceSocketClient _socketClient { get; set; }
        public SymbolViewModel(BinanceSocketClient socketClient, string symbolName)
        {
            Symbol = new();
            _socketClient = socketClient;
            Symbol.Name = symbolName;
            SubscribeToAggregatedTrade();
        }
        private async void SubscribeToAggregatedTrade()
        {
            try
            {
                await _socketClient.UsdFuturesStreams.SubscribeToAggregatedTradeUpdatesAsync(Symbol.Name, Message =>
                {
                    Symbol.Price = Message.Data.Price;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SubscribeToAggregatedTrade {ex.Message}");
            }
        }
    }
}
