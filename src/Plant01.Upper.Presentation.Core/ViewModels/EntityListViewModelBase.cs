using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Plant01.Core.Data;
using Plant01.Upper.Presentation.Core.Services;

using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public abstract partial class EntityListViewModelBase<TEntity, TFilter> : ObservableObject
    where TFilter : new()
{
    protected readonly IDialogService _dialogService;
    private CancellationTokenSource? _searchCts;

    public EntityListViewModelBase(IDialogService dialogService)
    {
        _dialogService = dialogService;
        Filter = new TFilter();
    }

    [ObservableProperty]
    private ObservableCollection<TEntity> _items = new();

    [ObservableProperty]
    private TFilter _filter;

    [ObservableProperty]
    private int _pageIndex = 1;

    [ObservableProperty]
    private int _pageSize = 10;

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isReadOnly;

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                OnSearchTextChanged(value);
            }
        }
    }

    private void OnSearchTextChanged(string text)
    {
        _searchCts?.Cancel();
        _searchCts = new CancellationTokenSource();
        var token = _searchCts.Token;

        Task.Delay(500, token).ContinueWith(async t =>
        {
            if (t.IsCanceled) return;

            // Reset to page 1 when searching
            // We need to invoke this on the UI thread if we were updating UI properties directly, 
            // but here we are just setting a property and calling an async method.
            // However, LoadDataAsync updates ObservableCollection which must be on UI thread.
            // TaskScheduler.FromCurrentSynchronizationContext() handles this.

            PageIndex = 1;
            await LoadDataAsync();
        }, token, TaskContinuationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var request = new PageRequest
            {
                PageIndex = PageIndex,
                PageSize = PageSize,
                SearchText = SearchText,
                // SortField can be added later
            };

            var result = await GetPagedDataAsync(request);
            if (result.IsSuccess)
            {
                Items = new ObservableCollection<TEntity>(result.Content);
                TotalCount = result.TotalCount;
            }
            else
            {
                await _dialogService.ShowMessageAsync(result.Message);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public virtual async Task CreateAsync()
    {
        await OnCreateAsync();
    }

    [RelayCommand]
    public virtual async Task EditAsync(TEntity entity)
    {
        if (entity == null) return;
        await OnEditAsync(entity);
    }

    [RelayCommand]
    public virtual async Task DeleteAsync(TEntity entity)
    {
        if (entity == null) return;

        var confirmed = await _dialogService.ConfirmAsync("删除确认", "确定要删除这条记录吗？");
        if (confirmed)
        {
            var success = await OnDeleteAsync(entity);
            if (success)
            {
                await LoadDataAsync();
            }
        }
    }

    [RelayCommand]
    public virtual void ResetFilter()
    {
        SearchText = string.Empty;
        Filter = new TFilter();
        PageIndex = 1;
        LoadDataCommand.Execute(null);
    }

    partial void OnPageIndexChanged(int value) => LoadDataCommand.Execute(null);
    partial void OnPageSizeChanged(int value) => LoadDataCommand.Execute(null);

    protected abstract Task<PagedResult<TEntity>> GetPagedDataAsync(PageRequest request);

    protected virtual Task OnCreateAsync() => Task.CompletedTask;
    protected virtual Task OnEditAsync(TEntity entity) => Task.CompletedTask;
    protected virtual Task<bool> OnDeleteAsync(TEntity entity) => Task.FromResult(true);
}
