using Ryujinx.Common;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class ILibraryAppletSelfAccessor : IpcService
    {
        private Dictionary<ulong, AppletStandalone> _appletStandalone = new Dictionary<ulong, AppletStandalone>();

        public ILibraryAppletSelfAccessor(ServiceCtx context)
        {
            if (context.Device.Application.TitleId == 0x0100000000001008)
            {
                // Add SwKbd to standalone data list.
                _appletStandalone.Add(context.Device.Application.TitleId, new AppletStandalone()
                {
                    AppletId = AppletId.SoftwareKeyboard,
                    LibraryAppletMode = LibraryAppletMode.AllForeground
                });
            }

            else if (context.Device.Application.TitleId == 0x0100000000001009)
            {
                // Add MiiEdit to standalone data list.
                _appletStandalone.Add(context.Device.Application.TitleId, new AppletStandalone()
                {
                    AppletId = AppletId.MiiEdit,
                    LibraryAppletMode = LibraryAppletMode.AllForeground
                });
            }

            while (context.Device.System.AppletState.AppletData.TryDequeue(out var data))
            {
                _appletStandalone[context.Device.Application.TitleId].InputData.Enqueue(data);
            }
        }

        [CommandHipc(0)]
        // PopInData() -> object<nn::am::service::IStorage>
        public ResultCode PopInData(ServiceCtx context)
        {
            byte[] appletData = _appletStandalone[context.Device.Application.TitleId].InputData.Dequeue();

            if (appletData.Length == 0)
            {
                return ResultCode.NotAvailable;
            }

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }

        [CommandHipc(11)]
        // GetLibraryAppletInfo() -> nn::am::service::LibraryAppletInfo
        public ResultCode GetLibraryAppletInfo(ServiceCtx context)
        {
            LibraryAppletInfo libraryAppletInfo = new LibraryAppletInfo()
            {
                AppletId          = _appletStandalone[context.Device.Application.TitleId].AppletId,
                LibraryAppletMode = _appletStandalone[context.Device.Application.TitleId].LibraryAppletMode
            };

            context.ResponseData.WriteStruct(libraryAppletInfo);

            return ResultCode.Success;
        }

        [CommandHipc(14)]
        // GetCallerAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetCallerAppletIdentityInfo(ServiceCtx context)
        {
            AppletIdentifyInfo appletIdentifyInfo = new AppletIdentifyInfo()
            {
                AppletId = AppletId.QLaunch,
                TitleId  = 0x0100000000001000
            };

            context.ResponseData.WriteStruct(appletIdentifyInfo);

            return ResultCode.Success;
        }
    }
}