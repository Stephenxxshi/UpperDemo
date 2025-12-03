using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Extensions.DependencyInjection;

using Plant01.Upper.Presentation.Core.Models;

using System.Collections.ObjectModel;
using System.Linq;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IServiceProvider? _serviceProvider;

    [ObservableProperty]
    private string _title = "Plant01.Upper Application";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private NavigateItem? _selectedMenuItem;

    // 对应 ItemsSource
    public ObservableCollection<NavigateItem> NavigateItems { get; } = new()
    {
        new NavigateItem{
            Title = "仪表盘",
            Description = "Overview of the system",
            IconChar = "&#xe62b;", 
            IconPlacement = "Top",
            ViewModelType = typeof(DashboardViewModel)
        },
        new NavigateItem
        {
            Title = "产品记录",
            Description = "View product records",
            IconChar = "&#xe663;",
            IconPlacement = "Top",
            ViewModelType = typeof(ProduceRecordViewModel)
        },
        new NavigateItem
        {
            Description = "Application settings",
            IconChar = "&#xe7c6;",
            IconPlacement = "Top",
            Title = "设置",
            ViewModelType = typeof(SettingsViewModel)
        }
    };

    [ObservableProperty]
    private object? _currentView;

    public ShellViewModel()
    {
        CurrentView = new DashboardViewModel();
    }

    public ShellViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        InitializeDefaultView();
    }

    private void InitializeDefaultView()
    {
        if (_serviceProvider == null) return;

        // Resolve the default dashboard from DI so it receives ILogger/ILogStore
        CurrentView = _serviceProvider.GetRequiredService<DashboardViewModel>();

        // Highlight the first menu entry to keep UI state in sync
        SelectedMenuItem = NavigateItems.FirstOrDefault();
    }

    partial void OnSelectedMenuItemChanged(NavigateItem? value)
    {
        if (value is not null)
        {
            NavigateTo(value);
        }
    }

    [RelayCommand]
    public void NavigateTo(NavigateItem item)
    {
        if (item == null) return;

        if (_serviceProvider == null) return;

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
