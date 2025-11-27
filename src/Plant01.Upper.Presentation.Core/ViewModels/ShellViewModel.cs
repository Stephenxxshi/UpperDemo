using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using Plant01.Upper.Presentation.Core.Models;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _title = "Plant01.Upper Application";

    // 对应 ItemsSource
    public ObservableCollection<MenuItem> MenuItems { get; } = new()
    {
        new MenuItem{
            Name = "DashBoard",
            Description = "Overview of the system",
            IconChar = "&#xe62b;", 
            IconPlacement = "Top",
            Title = "仪表盘",
            ViewModelType = typeof(DashboardViewModel)
        },
        new MenuItem
        {
            Name = "Settings",
            Description = "Application settings",
            IconChar = "&#xe7c6;",
            IconPlacement = "Top",
            Title = "设置",
            ViewModelType = typeof(SettingsViewModel)
        }
    };

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    public object _currentView = new DashboardViewModel();

    public ShellViewModel()
    {

    }
    public ShellViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    public void NavigateTo(MenuItem item)
    {
        if (item == null) return;

        try
        {
            // 从 DI 容器中获取对应的 ViewModel 实例
            var viewModel = _serviceProvider.GetRequiredService(item.ViewModelType);
            CurrentView = viewModel;
            StatusMessage = $"Navigated to {item.Title}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: Could not load {item.Title}. {ex.Message}";
        }
    }

}
