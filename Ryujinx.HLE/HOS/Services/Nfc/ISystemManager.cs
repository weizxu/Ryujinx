using Ryujinx.HLE.HOS.Services.Nfc.Nfp;

namespace Ryujinx.HLE.HOS.Services.Nfc
{
    [Service("nfc:sys")]
    class ISystemManager : IpcService
    {
        public ISystemManager(ServiceCtx context) { }

        [CommandHipc(0)]
        // CreateSystemInterface() -> object<nn::nfp::detail::ISystem>
        public ResultCode CreateSystemInterface(ServiceCtx context)
        {
            // FIXME: This should return an system interface, not a user one.
            MakeObject(context, new IUser());

            return ResultCode.Success;
        }
    }
}