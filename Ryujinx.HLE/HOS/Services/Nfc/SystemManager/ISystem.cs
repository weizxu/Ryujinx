using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Nfc.Nfp.UserManager;

namespace Ryujinx.HLE.HOS.Services.Nfc.SystemManager
{
    class ISystem : IpcService
    {
        public ISystem() { }

        [CommandHipc(400)] // 4.0.0+
        // InitializeSystem()
        public ResultCode InitializeSystem(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNfc);

            return ResultCode.Success;
        }

        [CommandHipc(403)] // 4.0.0+
        // GetState() -> u32
        public ResultCode GetState(ServiceCtx context)
        {
            context.ResponseData.Write((uint)State.Initialized);

            Logger.Stub?.PrintStub(LogClass.ServiceNfc);

            return ResultCode.Success;
        }
    }
}