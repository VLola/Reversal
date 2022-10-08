using Reversal.ViewModels;
using System.Windows.Controls;

namespace Reversal.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel();
        }
    }
}
