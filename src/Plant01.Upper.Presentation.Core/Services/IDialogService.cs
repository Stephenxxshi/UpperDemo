namespace Plant01.Upper.Presentation.Core.Services
{
    public interface IDialogService
    {
        Task ShowMessageAsync(string message);
        Task<bool> ConfirmAsync(string title, string message);

        /// <summary>
        /// Shows a modal dialog with parameters and a callback for the result.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <param name="viewModel">The view model instance.</param>
        /// <param name="parameter">Data to pass to the dialog.</param>
        /// <param name="callback">Callback to execute when dialog closes.</param>
        /// <param name="title">Title of the dialog.</param>
        void ShowDialog<TViewModel>(TViewModel viewModel, object parameter, Action<object> callback, string title = "");

        /// <summary>
        /// Shows a modal dialog asynchronously and returns a result.
        /// </summary>
        /// <typeparam name="TViewModel">The type of the view model.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="viewModel">The view model instance.</param>
        /// <param name="parameter">Data to pass to the dialog.</param>
        /// <param name="title">Title of the dialog.</param>
        /// <returns>The result from the dialog.</returns>
        Task<TResult> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel, object parameter, string title = "");
    }
}
