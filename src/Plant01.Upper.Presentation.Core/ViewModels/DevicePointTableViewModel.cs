using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;

using Plant01.Upper.Application.Interfaces;
using Plant01.Upper.Application.Interfaces.DeviceCommunication;
using Plant01.Upper.Application.Messages;
using Plant01.Upper.Presentation.Core.Models;
using Plant01.Upper.Presentation.Core.Services;
using System.Collections.ObjectModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

public partial class DevicePointTableViewModel : ObservableObject, IRecipient<TagValueChangedMessage>
{
    private readonly IEquipmentConfigService _equipmentConfigService;
    private readonly IDeviceCommunicationService _deviceCommunicationService;
    private readonly IMessenger _messenger;
    private readonly IDialogService _dialogService;
    private readonly IDispatcherService _dispatcherService;

    [ObservableProperty]
    private ObservableCollection<DeviceTagDisplayModel> _tags = new();

    public DevicePointTableViewModel(
        IEquipmentConfigService equipmentConfigService,
        IDeviceCommunicationService deviceCommunicationService,
        IMessenger messenger,
        IDialogService dialogService,
        IDispatcherService dispatcherService)
    {
        _equipmentConfigService = equipmentConfigService;
        _deviceCommunicationService = deviceCommunicationService;
        _messenger = messenger;
        _dialogService = dialogService;
        _dispatcherService = dispatcherService;

        _messenger.RegisterAll(this);
        InitializeTags();
    }

    private void InitializeTags()
    {
        var mappings = _equipmentConfigService.GetAllMappings();
        foreach (var equipmentMapping in mappings)
        {
            foreach (var tagMapping in equipmentMapping.TagMappings)
            {
                var tagValue = _deviceCommunicationService.GetTagValue(tagMapping.TagCode);
                var model = new DeviceTagDisplayModel
                {
                    EquipmentCode = equipmentMapping.EquipmentCode,
                    TagCode = tagMapping.TagCode,
                    TagName = tagMapping.TagName,
                    //Address = tagMapping.TagCode,
                    //Purpose = tagMapping.Purpose,
                    Value = tagValue.Value,
                    UpdateTime = tagValue.Timestamp == default ? DateTime.Now : tagValue.Timestamp
                };
                Tags.Add(model);
            }
        }
    }

    public void Receive(TagValueChangedMessage message)
    {
        var tag = Tags.FirstOrDefault(t => t.TagCode == message.TagCode && t.EquipmentCode == message.EquipmentCode);
        if (tag != null)
        {
            _dispatcherService.Invoke(() =>
            {
                tag.Value = message.NewValue;
                tag.UpdateTime = message.Timestamp;
            });
        }
    }

    [RelayCommand]
    private async Task WriteTag(DeviceTagDisplayModel model)
    {
        if (string.IsNullOrWhiteSpace(model.WriteValue))
        {
            await _dialogService.ShowMessageAsync("请输入要写入的值");
            return;
        }

        try
        {
            await _deviceCommunicationService.WriteTagAsync(model.TagName, model.WriteValue);
            await _dialogService.ShowMessageAsync($"写入成功！\n标签: {model.TagName}\n值: {model.WriteValue}");
            model.WriteValue = string.Empty; // 写入成功后清空输入框
        }
        catch (Exception ex)
        {
            await _dialogService.ShowMessageAsync($"写入失败: {ex.Message}");
        }
    }
}
