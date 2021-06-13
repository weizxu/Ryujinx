using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid:sys")]
    class IHidSystemServer : IpcService
    {
        public IHidSystemServer(ServiceCtx context) { }

        [CommandHipc(303)]
        // ApplyNpadSystemCommonPolicy(u64)
        public ResultCode ApplyNpadSystemCommonPolicy(ServiceCtx context)
        {
            ulong commonPolicy = context.RequestData.ReadUInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { commonPolicy });

            return ResultCode.Success;
        }

        [CommandHipc(306)]
        // GetLastActiveNpad(u32) -> u64, u64
        public ResultCode GetLastActiveNpad(ServiceCtx context)
        {
            NpadIdType npadId = (NpadIdType)context.RequestData.ReadUInt32();

            // TODO

            context.ResponseData.Write((byte)4);
            context.ResponseData.Write((byte)4);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadId });

            return ResultCode.Success;
        }

        [CommandHipc(314)] // 9.0.0+
        // GetAppletFooterUiType(u32) -> u8
        public ResultCode GetAppletFooterUiType(ServiceCtx context)
        {
            // TODO

            NpadIdType npadId = (NpadIdType)context.RequestData.ReadUInt32();

            // 0/1 - Nothing
            // 2 - JoyCon Left Handheld 
            // 3 - JoyCon Right Handheld 
            // 4 - Handheld
            // 5 - JoyCon Paired
            // 6 - JoyCon Left Vertical
            // 7 - JoyCon Right Vertical
            // 8 - JoyCon Left Horizontal
            // 9 - JoyCon Right Horizontal
            // 10 - JoyCon Left Horizontal ?
            // 11 - JoyCon Right Vertical ?
            // 12 - ProController
            // 13 - External Controller

            context.ResponseData.Write((byte)4);

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadId });

            return ResultCode.Success;
        }
    }
}