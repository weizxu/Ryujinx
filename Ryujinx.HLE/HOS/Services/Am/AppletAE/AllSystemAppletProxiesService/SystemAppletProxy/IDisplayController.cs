using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Common;
using Ryujinx.HLE.HOS.Kernel.Memory;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IDisplayController : IpcService
    {
        private KTransferMemory _transferMem;

        public IDisplayController(ServiceCtx context)
        {
            _transferMem = context.Device.System.AppletCaptureBufferTransfer;
        }

        [CommandHipc(8)] // 2.0.0+
        // TakeScreenShotOfOwnLayer(b8, s32)
        public ResultCode TakeScreenShotOfOwnLayer(ServiceCtx context)
        {
            bool unknown1 = context.RequestData.ReadBoolean();
            int  unknown2 = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { unknown1, unknown2 });

            return ResultCode.Success;
        }

        [CommandHipc(11)]
        // ReleaseLastApplicationCaptureBuffer()
        public ResultCode ReleaseLastApplicationCaptureBuffer(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandHipc(16)]
        // AcquireLastApplicationCaptureBufferEx() -> (b8, handle<copy>)
        public ResultCode AcquireLastApplicationCaptureBufferEx(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(_transferMem, out int handle) != KernelResult.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(handle);

            context.ResponseData.Write(true);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

    }
}