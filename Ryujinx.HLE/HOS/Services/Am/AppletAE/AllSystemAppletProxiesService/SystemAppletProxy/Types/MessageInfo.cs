namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    enum MessageInfo
    {
        AcquireForeground      = 0x1,
        ReleaseForeground      = 0x3,
        FocusStateChanged      = 0xf,
        Resume                 = 0x10,
        OperationModeChanged   = 0x1e,
        PerformanceModeChanged = 0x1f
    }
}