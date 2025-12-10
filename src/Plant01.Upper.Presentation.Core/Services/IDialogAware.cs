using System;

namespace Plant01.Upper.Presentation.Core.Services
{
    public interface IDialogAware
    {
        /// <summary>
        /// Dialog title
        /// </summary>
        string Title { get; }

        /// <summary>
        /// Event to request closing the dialog with a result
        /// </summary>
        event Action<object> RequestClose;

        /// <summary>
        /// Called when the dialog is opened
        /// </summary>
        /// <param name="parameters">Parameters passed to the dialog</param>
        void OnDialogOpened(object parameters);
    }
}
