namespace Plant01.Upper.Presentation.Core.Services
{
    public interface IDialogService
    {
        Task ShowMessageAsync(string message);
        Task<bool> ConfirmAsync(string title, string message);
        Task<TResult> ShowModalAsync<TViewModel, TResult>(TViewModel viewModel, string title = "");
    }
}
