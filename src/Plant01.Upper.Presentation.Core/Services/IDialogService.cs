namespace Plant01.Upper.Presentation.Core.Services;

public interface IDialogService
{
    /// <summary>
    /// 显示消息对话框（异步）。
    /// </summary>
    /// <param name="message">要显示的消息内容。</param>
    Task ShowMessageAsync(string message);

    /// <summary>
    /// 显示确认对话框（异步）。
    /// </summary>
    /// <param name="title">对话框标题。</param>
    /// <param name="message">对话框内容。</param>
    /// <returns>用户是否确认。</returns>
    Task<bool> ConfirmAsync(string title, string message);

    /// <summary>
    /// 显示带参数和回调的模态对话框。
    /// </summary>
    /// <typeparam name="TViewModel">视图模型类型。</typeparam>
    /// <param name="viewModel">视图模型实例。</param>
    /// <param name="parameter">传递给对话框的数据。</param>
    /// <param name="callback">对话框关闭时的回调。</param>
    /// <param name="title">对话框标题。</param>
    void ShowDialog<TViewModel>(TViewModel viewModel, object parameter, Action<object> callback, string title = "");

    /// <summary>
    /// 异步显示模态对话框并返回结果。
    /// </summary>
    /// <typeparam name="TViewModel">视图模型类型。</typeparam>
    /// <typeparam name="TResult">结果类型。</typeparam>
    /// <param name="viewModel">视图模型实例。</param>
    /// <param name="parameter">传递给对话框的数据。</param>
    /// <param name="title">对话框标题。</param>
    /// <returns>对话框返回的结果。</returns>
    Task<TResult> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel, object parameter, string title = "");
}
