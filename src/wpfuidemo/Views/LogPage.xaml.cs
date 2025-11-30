using System.Windows.Controls;
using wpfuidemo.ViewModels;
using Plant01.Infrastructure.Shared.Logging;
using Microsoft.Extensions.Logging;

namespace wpfuidemo.Views
{
    public partial class LogPage : UserControl
    {
        public LogPage()
        {
            InitializeComponent();
            
            // Manual DI for Demo
            var logStore = new LogStore();
            var loggerProvider = new ObservableLoggerProvider(logStore);
            var loggerFactory = LoggerFactory.Create(builder => 
            {
                builder.AddProvider(loggerProvider);
                builder.SetMinimumLevel(LogLevel.Debug);
            });
            var logger = loggerFactory.CreateLogger<LogViewModel>();
            
            DataContext = new LogViewModel(logger, logStore);
        }
    }
}