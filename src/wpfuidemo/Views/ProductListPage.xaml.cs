using System.Windows.Controls;
using wpfuidemo.ViewModels;

namespace wpfuidemo.Views
{
    public partial class ProductListPage : UserControl
    {
        public ProductListPage()
        {
            InitializeComponent();
            DataContext = new ProductListViewModel();
        }
    }
}
