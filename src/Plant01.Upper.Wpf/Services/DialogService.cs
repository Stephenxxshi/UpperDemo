using Plant01.Upper.Presentation.Core.Services;
using Plant01.WpfUI.Controls;

using System.Windows;

namespace Plant01.Upper.Wpf.Services;

public class DialogService : IDialogService
{
    public Task ShowMessageAsync(string message)
    {
        MessageBox.Show(message, "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        return Task.CompletedTask;
    }

    public Task<bool> ConfirmAsync(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }

    public Task<TResult> ShowModalAsync<TViewModel, TResult>(TViewModel viewModel, string title = "")
    {
        var tcs = new TaskCompletionSource<TResult>();

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var modal = new AntModal
            {
                Title = title,
                Content = viewModel,
                Owner = System.Windows.Application.Current.MainWindow,
                // Auto-size or default size
                SizeToContent = SizeToContent.WidthAndHeight
            };

            bool? result = modal.ShowDialog();

            if (typeof(TResult) == typeof(bool))
            {
                tcs.SetResult((TResult)(object)(result ?? false));
            }
            else
            {
                // For other types, we might need a more specific mechanism.
                // For now, return default.
                tcs.SetResult(default!);
            }
        });

        return tcs.Task;
    }
}
