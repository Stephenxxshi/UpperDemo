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

    public void ShowDialog<TViewModel>(TViewModel viewModel, object parameter, Action<object> callback, string title = "")
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var modal = new AntModal
            {
                Title = title,
                Content = viewModel,
                Owner = System.Windows.Application.Current.MainWindow,
                SizeToContent = SizeToContent.WidthAndHeight
            };

            object dialogResultData = null;

            if (viewModel is IDialogAware dialogAware)
            {
                dialogAware.OnDialogOpened(parameter);

                Action<object> closeHandler = null;
                closeHandler = (result) =>
                {
                    dialogResultData = result;
                    dialogAware.RequestClose -= closeHandler;
                    // Setting DialogResult closes the window if it's a Window
                    try
                    {
                        modal.DialogResult = true;
                    }
                    catch (InvalidOperationException)
                    {
                        // Dialog might be already closing or not opened as dialog
                        modal.Close();
                    }
                };

                dialogAware.RequestClose += closeHandler;
                
                // Handle window closing to unsubscribe if not closed via RequestClose
                modal.Closed += (s, e) => 
                {
                    dialogAware.RequestClose -= closeHandler;
                };
            }

            modal.ShowDialog();

            callback?.Invoke(dialogResultData);
        });
    }

    public Task<TResult> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel, object parameter, string title = "")
    {
        var tcs = new TaskCompletionSource<TResult>();

        ShowDialog(viewModel, parameter, (result) =>
        {
            if (result is TResult tResult)
            {
                tcs.SetResult(tResult);
            }
            else
            {
                tcs.SetResult(default!);
            }
        }, title);

        return tcs.Task;
    }
}
