using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Plant01.Upper.Presentation.Core.Models;

public partial class DeviceTagDisplayModel : ObservableObject
{
    [ObservableProperty]
    private string _equipmentCode = string.Empty;

    [ObservableProperty]
    private string _tagCode = string.Empty;

    [ObservableProperty]
    private string _tagName = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private object? _value;

    [ObservableProperty]
    private string _purpose = string.Empty;

    [ObservableProperty]
    private DateTime _updateTime;

    [ObservableProperty]
    private string _writeValue = string.Empty;
}
