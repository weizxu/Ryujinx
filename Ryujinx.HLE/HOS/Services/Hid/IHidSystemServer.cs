using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid:sys")]
    class IHidSystemServer : IpcService
    {
        public IHidSystemServer(ServiceCtx context) { }

        [CommandHipc(303)]
        // ApplyNpadSystemCommonPolicy(?)
        public ResultCode ApplyNpadSystemCommonPolicy(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }
    }
}