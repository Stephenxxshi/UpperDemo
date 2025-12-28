using System;

namespace Plant01.Upper.Presentation.Core.Services;

public interface IDialogAware
{
    /// <summary>
    /// 对话框标题
    /// </summary>
    string Title { get; }

    /// <summary>
    /// 请求关闭对话框并返回结果的事件
    /// </summary>
    event Action<object> RequestClose;

    /// <summary>
    /// 对话框打开时调用
    /// </summary>
    /// <param name="parameters">传递给对话框的参数</param>
    void OnDialogOpened(object parameters);
}
