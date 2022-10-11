using Reversal.ViewModels;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Reversal.Models
{
    public class MainModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        public ObservableCollection<SymbolViewModel> Symbols { get; set; } = new();
        private decimal _balance { get; set; }
        public decimal Balance
        {
            get { return _balance; }
            set
            {
                _balance = value;
                OnPropertyChanged("Balance");
            }
        }
        private bool _isLogin { get; set; }
        public bool IsLogin
        {
            get { return _isLogin; }
            set
            {
                _isLogin = value;
                OnPropertyChanged("IsLogin");
            }
        }
        private bool _selectAll { get; set; }
        public bool SelectAll
        {
            get { return _selectAll; }
            set
            {
                _selectAll = value;
                OnPropertyChanged("SelectAll");
            }
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
        private bool _listenKeyExpired { get; set; }
        public bool ListenKeyExpired
        {
            get { return _listenKeyExpired; }
            set
            {
                _listenKeyExpired = value;
                OnPropertyChanged("ListenKeyExpired");
            }
        }
        private bool _isReal { get; set; } = true;
        public bool IsReal
        {
            get { return _isReal; }
            set
            {
                _isReal = value;
                OnPropertyChanged("IsReal");
            }
        }
        private bool _isTestnet { get; set; }
        public bool IsTestnet
        {
            get { return _isTestnet; }
            set
            {
                _isTestnet = value;
                OnPropertyChanged("IsTestnet");
            }
        }
        private string _apiKey { get; set; }
        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                _apiKey = value;
                OnPropertyChanged("ApiKey");
            }
        }
        private string _secretKey { get; set; }
        public string SecretKey
        {
            get { return _secretKey; }
            set
            {
                _secretKey = value;
                OnPropertyChanged("SecretKey");
            }
        }
    }
}
