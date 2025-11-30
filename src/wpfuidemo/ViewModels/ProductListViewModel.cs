using Plant01.Core.Data;
using Plant01.Upper.Presentation.Core.Services;
using Plant01.Upper.Presentation.Core.ViewModels;
using Plant01.WpfUI.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace wpfuidemo.ViewModels;

public class ProductDto : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreateTime { get; set; }
    public bool IsSystem { get; set; }

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class ProductFilter
{
    public string? Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DemoDialogService : IDialogService
{
    public Task ShowMessageAsync(string message)
    {
        MessageBox.Show(message);
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task<TResult> ShowModalAsync<TViewModel, TResult>(TViewModel viewModel, string title = "")
    {
        MessageBox.Show($"Mock Modal for {title}");
        return Task.FromResult(default(TResult)!);
    }
}

public partial class ProductListViewModel : EntityListViewModelBase<ProductDto, ProductFilter>
{
    private List<ProductDto> _allProducts;
    private int _selectedCount;

    public int SelectedCount
    {
        get => _selectedCount;
        set => SetProperty(ref _selectedCount, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand BatchDeleteCommand { get; }

    public ProductListViewModel() : base(new DemoDialogService())
    {
        RefreshCommand = new SimpleCommand(_ => LoadDataCommand.Execute(null));
        BatchDeleteCommand = new SimpleCommand(OnBatchDelete);

        // Mock Data
        _allProducts = Enumerable.Range(1, 100).Select(i => new ProductDto
        {
            Id = i,
            Name = $"Product {i}",
            Category = i % 3 == 0 ? "Electronics" : (i % 3 == 1 ? "Clothing" : "Food"),
            Price = i * 10.5m,
            CreateTime = DateTime.Now.AddDays(-i),
            IsSystem = i % 10 == 0
        }).ToList();

        foreach (var p in _allProducts)
        {
            p.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ProductDto.IsSelected))
                {
                    UpdateSelectedCount();
                }
            };
        }

        // Initial Load
        LoadDataCommand.Execute(null);
    }

    private void UpdateSelectedCount()
    {
        SelectedCount = Items.Count(x => x.IsSelected);
    }

    private async void OnBatchDelete(object? obj)
    {
        var selected = Items.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0) return;

        if (await _dialogService.ConfirmAsync("确认删除", $"确定要删除选中的 {selected.Count} 项吗？"))
        {
            foreach (var item in selected)
            {
                _allProducts.Remove(item);
            }
            await LoadDataCommand.ExecuteAsync(null);
            SelectedCount = 0;
        }
    }

    protected override async Task<PagedResult<ProductDto>> GetPagedDataAsync(PageRequest request)
    {
        // Simulate Network Delay
        await Task.Delay(300);

        var query = _allProducts.AsQueryable();

        // Search
        if (!string.IsNullOrEmpty(request.SearchText))
        {
            query = query.Where(p => p.Name.Contains(request.SearchText, StringComparison.OrdinalIgnoreCase));
        }

        // Filter
        if (!string.IsNullOrEmpty(Filter.Category))
        {
            query = query.Where(p => p.Category == Filter.Category);
        }
        if (Filter.StartDate.HasValue)
        {
            query = query.Where(p => p.CreateTime >= Filter.StartDate.Value);
        }

        var total = query.Count();
        var items = query.Skip((request.PageIndex - 1) * request.PageSize).Take(request.PageSize).ToList();
        
        // Reset selection on page change if desired, or keep it. 
        // For simplicity, we might lose selection state if we don't track it globally.
        // Here we just return the objects which are already in memory and tracking state.

        return PagedResult<ProductDto>.Success(items, total);
    }

    protected override async Task<bool> OnDeleteAsync(ProductDto entity)
    {
        if (entity.IsSystem)
        {
            await _dialogService.ShowMessageAsync("System records cannot be deleted.");
            return false;
        }

        await Task.Delay(500); // Simulate API call
        var item = _allProducts.FirstOrDefault(p => p.Id == entity.Id);
        if (item != null)
        {
            _allProducts.Remove(item);
            return true;
        }
        return false;
    }

    protected override async Task OnCreateAsync()
    {
        await _dialogService.ShowMessageAsync("Open Create Dialog");
    }

    protected override async Task OnEditAsync(ProductDto entity)
    {
        await _dialogService.ShowMessageAsync($"Open Edit Dialog for {entity.Name}");
    }
}
