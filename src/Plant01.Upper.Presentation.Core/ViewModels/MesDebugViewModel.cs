using CommunityToolkit.Mvvm.ComponentModel;

namespace Plant01.Upper.Presentation.Core.ViewModels;

/// <summary>
/// MES 调试 ViewModel (Container)
/// </summary>
public partial class MesDebugViewModel : ObservableObject
{
    public PlcDebugViewModel PlcDebug { get; }
    public MesInterfaceDebugViewModel MesInterfaceDebug { get; }

    public MesDebugViewModel(
        PlcDebugViewModel plcDebugViewModel,
        MesInterfaceDebugViewModel mesInterfaceDebugViewModel)
    {
        PlcDebug = plcDebugViewModel;
        MesInterfaceDebug = mesInterfaceDebugViewModel;
    }
}
