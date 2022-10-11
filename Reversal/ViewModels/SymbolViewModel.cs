using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures.Socket;
using CryptoExchange.Net.Sockets;
using Newtonsoft.Json;
using Reversal.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Reversal.ViewModels
{
    public class SymbolViewModel
    {
        private UpdateSubscription? _updateSubscription { get; set; }
        public SymbolModel Symbol { get; set; }
        private BinanceSocketClient _socketClient { get; set; }
        private BinanceClient _client { get; set; }
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public SymbolViewModel(BinanceClient client, BinanceSocketClient socketClient, string symbolName)
        {
            Symbol = new();
            _client = client;
            _socketClient = socketClient;
            Symbol.Name = symbolName;
            Symbol.PropertyChanged += Symbol_PropertyChanged;
        }

        private void Symbol_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Run")
            {
                if (Symbol.Run) SubscribeToAggregatedTradeUpdatesAsync();
                else UnsubscribeAsync();
            }
        }
        private async void SubscribeToAggregatedTradeUpdatesAsync()
        {
            try
            {
                var result = await _socketClient.UsdFuturesStreams.SubscribeToAggregatedTradeUpdatesAsync(Symbol.Name, Message =>
                {
                    Symbol.Price = Message.Data.Price;
                    if (!Symbol.IsOpenOrder)
                    {
                        if (Symbol.PositionSide == "Both")
                        {
                            if (Symbol.Side == "Buy")
                            {
                                if (Symbol.Price < Symbol.ReversalPrice)
                                {
                                    Symbol.IsOpenOrder = true;
                                    OpenOrder();
                                }
                            }
                            else if (Symbol.Side == "Sell")
                            {
                                if (Symbol.Price > Symbol.ReversalPrice)
                                {
                                    Symbol.IsOpenOrder = true;
                                    OpenOrder();
                                }
                            }
                        }
                    }
                });
                if (!result.Success) WriteLog($"Failed Success SubscribeToAggregatedTradeUpdatesAsync: { result.Error?.Message}");
                else
                {
                    _updateSubscription = result.Data;
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Failed SubscribeToAggregatedTradeUpdatesAsync: {ex.Message}");
            }
        }
        private async void UnsubscribeAsync()
        {
            try
            {
                if (_updateSubscription != null) {
                    await _socketClient.UnsubscribeAsync(_updateSubscription);
                    Wait();
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Failed UnsubscribeAsync: {ex.Message}");
            }
        }
        private void Wait()
        {
            Symbol.ReversalPrice = 0m;
            Symbol.Quantity = 0m;
            Symbol.PositionSide = "";
            Symbol.Side = "";
            Symbol.Price = 0m;
        }
        public void OrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate)
        {
            if(Symbol.Select && OrderUpdate.UpdateData.Symbol == Symbol.Name)
            {
                if(OrderUpdate.UpdateData.Status == OrderStatus.Filled)
                {
                    if (!Symbol.Run)
                    {
                        Symbol.Run = true;
                        Symbol.ReversalPrice = OrderUpdate.UpdateData.AveragePrice;
                        Symbol.Quantity = OrderUpdate.UpdateData.Quantity;
                        if(OrderUpdate.UpdateData.PositionSide == PositionSide.Both) Symbol.PositionSide = "Both";
                        else if (OrderUpdate.UpdateData.PositionSide == PositionSide.Short) Symbol.PositionSide = "Short";
                        else if (OrderUpdate.UpdateData.PositionSide == PositionSide.Long) Symbol.PositionSide = "Long";
                        if (OrderUpdate.UpdateData.Side == OrderSide.Buy) Symbol.Side = "Buy";
                        else Symbol.Side = "Sell";
                    }
                    else
                    {
                        CheckOrderId(OrderUpdate.UpdateData.OrderId);
                    }
                }
                // Write log
                string json = JsonConvert.SerializeObject(OrderUpdate.UpdateData);
                WriteLog(json);
            }
        }
        private async void CheckOrderId(long id)
        {
            await Task.Run(async () => {
                try
                {
                    await Task.Delay(100);
                    if (Symbol.IdOrders.Contains(id))
                    {
                        Symbol.IsOpenOrder = false;
                        Symbol.IdOrders.Remove(id);
                    }
                    else
                    {
                        Symbol.Run = false;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Failed CheckOrderId: {ex.Message}");
                }
            });
        }
        private async void OpenOrder()
        {
            await Task.Run(async () => {
                try
                {
                    if (Symbol.PositionSide == "Both")
                    {
                        if (Symbol.Side == "Buy")
                        {
                            long orderId = await OrderAsync(OrderSide.Sell, Symbol.Quantity * 2, PositionSide.Both);
                            Symbol.IdOrders.Add(orderId);
                            Symbol.Side = "Sell";
                        }
                        else if (Symbol.Side == "Sell")
                        {
                            long orderId = await OrderAsync(OrderSide.Buy, Symbol.Quantity * 2, PositionSide.Both);
                            Symbol.IdOrders.Add(orderId);
                            Symbol.Side = "Buy";
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Failed OpenOrder: {ex.Message}");
                }
            });
        }
        private async Task<long> OrderAsync(OrderSide side, decimal quantity, PositionSide positionSide)
        {
            var result = await _client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol: Symbol.Name, side: side, type: FuturesOrderType.Market, quantity: quantity, positionSide: positionSide);
            if (!result.Success) WriteLog($"Failed OrderAsync: {result.Error?.Message}");
            return result.Data.Id;
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + Symbol.Name, $"{DateTime.Now} {text}\n");
            }
            catch{ }
        }
    }
}
