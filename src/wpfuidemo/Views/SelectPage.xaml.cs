using System.Collections.Generic;
using System.Windows.Controls;

namespace wpfuidemo.Views
{
    public partial class SelectPage : UserControl
    {
        public List<string> Options { get; } = new List<string> { "Option 1", "Option 2", "Option 3" };

        public SelectPage()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
