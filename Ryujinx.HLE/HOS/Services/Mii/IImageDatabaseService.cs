using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Mii
{
    [Service("miiimg")] // 5.0.0+
    class IImageDatabaseService : IpcService
    {
        public IImageDatabaseService(ServiceCtx context) { }

        [CommandHipc(0)]
        // Initialize???
        public ResultCode Initialize(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceMii);

            return ResultCode.Success;
        }

        [CommandHipc(11)]
        // GetCount???
        public ResultCode GetCount(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceMii);

            return ResultCode.Success;
        }
    }
}