using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using AppLogItem = Plant01.Upper.Application.Models.Logging.LogItem;
using UiLogItem = Plant01.WpfUI.Models.LogItem;
using UiLogSeverity = Plant01.WpfUI.Models.LogSeverity;

namespace Plant01.Upper.Wpf.Converters
{
    public class LogCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<AppLogItem> sourceCollection)
            {
                return new MappedLogCollection(sourceCollection);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MappedLogCollection : ObservableCollection<UiLogItem>
    {
        private readonly ObservableCollection<AppLogItem> _source;
        private readonly Dispatcher _dispatcher;

        public MappedLogCollection(ObservableCollection<AppLogItem> source)
        {
            _source = source;
            _dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            
            foreach (var item in _source)
            {
                Add(Map(item));
            }
            _source.CollectionChanged += Source_CollectionChanged;
        }

        private void Source_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 确保在 UI 线程执行集合操作
            if (_dispatcher.CheckAccess())
            {
                ProcessCollectionChange(e);
            }
            else
            {
                _dispatcher.InvokeAsync(() => ProcessCollectionChange(e));
            }
        }

        private void ProcessCollectionChange(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        int startIndex = e.NewStartingIndex;
                        foreach (AppLogItem item in e.NewItems)
                        {
                            if (startIndex >= 0 && startIndex <= Count)
                            {
                                Insert(startIndex, Map(item));
                                startIndex++;
                            }
                            else
                            {
                                Add(Map(item));
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        if (e.OldStartingIndex >= 0)
                        {
                            for (int i = 0; i < e.OldItems.Count; i++)
                            {
                                if (e.OldStartingIndex < Count)
                                    RemoveAt(e.OldStartingIndex);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Clear();
                    foreach (var item in _source)
                    {
                        Add(Map(item));
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.NewItems != null && e.OldItems != null && e.NewStartingIndex >= 0)
                    {
                        for (int i = 0; i < e.NewItems.Count; i++)
                        {
                            if (e.NewStartingIndex + i < Count)
                            {
                                this[e.NewStartingIndex + i] = Map((AppLogItem)e.NewItems[i]!);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                     if (e.OldStartingIndex >= 0 && e.NewStartingIndex >= 0)
                     {
                         Move(e.OldStartingIndex, e.NewStartingIndex);
                     }
                     break;
            }
        }

        private static UiLogItem Map(AppLogItem item)
        {
            return new UiLogItem
            {
                Timestamp = item.Timestamp,
                Message = item.Message,
                Exception = item.Exception,
                Source = item.Source,
                Severity = MapSeverity(item.Severity)
            };
        }

        private static UiLogSeverity MapSeverity(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => UiLogSeverity.Info,
                LogLevel.Debug => UiLogSeverity.Debug,
                LogLevel.Information => UiLogSeverity.Info,
                LogLevel.Warning => UiLogSeverity.Warning,
                LogLevel.Error => UiLogSeverity.Error,
                LogLevel.Critical => UiLogSeverity.Error,
                _ => UiLogSeverity.Info
            };
        }
    }
}
