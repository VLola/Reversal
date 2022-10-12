using Binance.Net.Clients;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures.Socket;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.CommonObjects;
using Newtonsoft.Json;
using Reversal.Command;
using Reversal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace Reversal.ViewModels
{
    public class MainViewModel
    {
        private const string _link = "https://drive.google.com/u/0/uc?id=13RLR9SIMLL2ibwDh8ouByOElk6Yw784J&export=download";
        private string _pathLog = $"{Directory.GetCurrentDirectory()}/log/";
        public MainModel MainModel { get; set; } = new();
        private BinanceClient? _client { get; set; }
        private BinanceSocketClient? _socketClient { get; set; }
        public delegate void AccountOnOrderUpdate(BinanceFuturesStreamOrderUpdate OrderUpdate);
        public event AccountOnOrderUpdate? OnOrderUpdate;
        private RelayCommand? _loginCommand;
        public RelayCommand LoginCommand
        {
            get { return _loginCommand ?? (_loginCommand = new RelayCommand(obj => { Login(); })); }
        }
        public MainViewModel()
        {
            if (!Directory.Exists(_pathLog)) Directory.CreateDirectory(_pathLog);
        }
        private void Login()
        {
            using (var client = new WebClient())
            {
                string json = client.DownloadString(_link);
                List<ClientModel>? clientModels = JsonConvert.DeserializeObject<List<ClientModel>>(json);
                if (clientModels != null)
                {
                    bool check = false;
                    foreach (var item in clientModels)
                    {
                        if (item.ClientName == MainModel.Name) check = item.Access;
                    }
                    if (check)
                    {
                        Load();
                    }
                    else
                    {
                        MessageBox.Show("Login name failed!");
                    }
                }
            }
        }
        private void Load()
        {
            MainModel.PropertyChanged += MainModel_PropertyChanged;
            if (MainModel.IsTestnet)
            {
                // ------------- Test Api ----------------
                BinanceClientOptions clientOption = new();
                clientOption.UsdFuturesApiOptions.BaseAddress = "https://testnet.binancefuture.com";
                _client = new(clientOption);

                BinanceSocketClientOptions socketClientOption = new BinanceSocketClientOptions
                {
                    AutoReconnect = true,
                    ReconnectInterval = TimeSpan.FromMinutes(1)
                };
                socketClientOption.UsdFuturesStreamsOptions.BaseAddress = "wss://stream.binancefuture.com";
                _socketClient = new BinanceSocketClient(socketClientOption);
                // ------------- Test Api ----------------
            }
            else if (MainModel.IsReal)
            {
                // ------------- Real Api ----------------
                _client = new();
                _socketClient = new();
                // ------------- Real Api ----------------
            }

            try
            {
                _client.SetApiCredentials(new ApiCredentials(MainModel.ApiKey, MainModel.SecretKey));
                _socketClient.SetApiCredentials(new ApiCredentials(MainModel.ApiKey, MainModel.SecretKey));

                MainModel.ApiKey = "";
                MainModel.SecretKey = "";
                if (CheckLogin())
                {
                    MainModel.IsLogin = true;
                    GetSumbolName();
                    BalanceFutureAsync();
                    SubscribeToAccountAsync();
                }
                else MessageBox.Show("Login failed!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MainModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == "SelectAll")
            {
                foreach (var item in MainModel.Symbols)
                {
                    item.Symbol.Select = MainModel.SelectAll;
                }
            }
            else if (e.PropertyName == "TakeProfit")
            {
                foreach (var item in MainModel.Symbols)
                {
                    item.Symbol.TakeProfit = MainModel.TakeProfit;
                }
            }
        }
        private bool CheckLogin()
        {
            try
            {
                var result = _client.UsdFuturesApi.Account.GetAccountInfoAsync().Result;
                if (!result.Success)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #region - List Sumbols -
        private void GetSumbolName()
        {
            List<string> list = new();
            foreach (var it in ListSymbols())
            {
                list.Add(it.Symbol);
            }
            list.Sort();
            foreach (var it in list)
            {
                SymbolViewModel symbolViewModel = new(_client, _socketClient, it);
                OnOrderUpdate += symbolViewModel.OrderUpdate;
                MainModel.Symbols.Add(symbolViewModel);
            }
        }
        private List<BinancePrice> ListSymbols()
        {
            try
            {
                var result = _client.UsdFuturesApi.ExchangeData.GetPricesAsync().Result;
                if (!result.Success) WriteLog($"Failed Success ListSymbols: {result.Error?.Message}");
                return result.Data.ToList();
            }
            catch (Exception ex)
            {
                WriteLog($"Failed ListSymbols: {ex.Message}");
                return null;
            }
        }
        #endregion

        #region - Balance (Async) -
        private async void BalanceFutureAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var result = _client.UsdFuturesApi.Account.GetAccountInfoAsync().Result;
                    if (!result.Success)
                    {
                        WriteLog($"Failed Success BalanceFutureAsync: {result.Error?.Message}");
                    }
                    else
                    {
                        MainModel.Balance = result.Data.TotalMarginBalance;
                    }
                });
            }
            catch (Exception ex)
            {
                WriteLog($"Failed BalanceFutureAsync: {ex.Message}");
            }
        }
        #endregion

        private async void SubscribeToAccountAsync()
        {
            var listenKey = await _client.UsdFuturesApi.Account.StartUserStreamAsync();
            if (!listenKey.Success)
            {
                WriteLog($"Failed to start user stream: listenKey");
            }
            else
            {
                WriteLog($"Listen Key Created");
                var result = await _socketClient.UsdFuturesStreams.SubscribeToUserDataUpdatesAsync(listenKey: listenKey.Data,
                    onLeverageUpdate => { },
                    onMarginUpdate => { },
                    onAccountUpdate =>
                    {
                        MainModel.Balance = onAccountUpdate.Data.UpdateData.Balances.ToList()[0].CrossWalletBalance;
                    },
                    onOrderUpdate =>
                    {
                        OnOrderUpdate?.Invoke(onOrderUpdate.Data);
                    },
                    onListenKeyExpired => {
                        if (!MainModel.ListenKeyExpired)
                        {
                            MainModel.ListenKeyExpired = true;
                            WriteLog($"Listen Key Expired");
                            UpdateSubscribeToAccountAsync();
                        }
                    });
                if (!result.Success)
                {
                    WriteLog($"Failed UserDataUpdates: {result.Error?.Message}");
                }
            }
        }
        private async void UpdateSubscribeToAccountAsync()
        {
            await Task.Run(async () =>
            {
                await Task.Delay(1000);
                MainModel.ListenKeyExpired = false;
                SubscribeToAccountAsync();
            });
        }
        private void WriteLog(string text)
        {
            try
            {
                File.AppendAllText(_pathLog + "_MAIN_LOG", $"{DateTime.Now} {text}\n");
            }
            catch { }
        }
    }
}
