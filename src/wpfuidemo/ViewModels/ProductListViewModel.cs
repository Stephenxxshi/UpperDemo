using Plant01.Core.Data;
using Plant01.Upper.Presentation.Core.Services;
using Plant01.Upper.Presentation.Core.ViewModels;

using System.Windows;

namespace wpfuidemo.ViewModels;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreateTime { get; set; }
    public bool IsSystem { get; set; } // Simulate system record that cannot be deleted
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

    public ProductListViewModel() : base(new DemoDialogService())
    {
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

        // Initial Load
        LoadDataCommand.Execute(null);
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
