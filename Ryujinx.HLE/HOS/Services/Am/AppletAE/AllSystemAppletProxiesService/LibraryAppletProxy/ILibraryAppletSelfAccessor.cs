using Ryujinx.Common.Logging;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class ILibraryAppletSelfAccessor : IpcService
    {
        private const AppletId _testAppletID = AppletId.MiiEdit;
        private const ulong _testAppletTitle = 0x0100000000001009;
        private Queue<byte[]> _testData;

        public ILibraryAppletSelfAccessor()
        {
            _testData = new Queue<byte[]>();

            // mii edit
            _testData.Enqueue(new byte[] { 3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 156, 170, 222, 0, 248, 166, 222, 1, 184, 94, 218, 49, 0, 10, 32, 0, 0, 0, 0, 0, 156, 170, 222, 0, 248, 166, 222, 1, 72, 49, 32, 0, 18, 0, 0, 0, 0, 0, 0, 0, 208, 94, 218, 49, 188, 8, 32, 0, 0, 106, 248, 0, 156, 170, 222, 0, 0, 106, 248, 0, 0, 0, 0, 0, 248, 94, 218, 49, 88, 15, 32, 0, 0, 0, 0, 0, 0, 106, 248, 0, 204, 97, 0, 0, 0, 145, 0, 106, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 95, 218, 49, 20, 14, 32, 0, 0, 0, 0, 0, 18, 0, 0, 0, 0, 0, 0, 0, 168, 227, 118, 1, 24, 155, 247, 0, 48, 127, 255, 105, 0, 145, 0, 106, 0, 0, 0, 0, 192, 95, 218, 49, 228, 22, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
        }

        [CommandHipc(0)]
        // PopInData() -> object<nn::am::service::IStorage>
        public ResultCode PopInData(ServiceCtx context)
        {
            byte[] appletData = _testData.Dequeue();

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }

        [CommandHipc(11)]
        // GetLibraryAppletInfo() -> nn::am::service::LibraryAppletInfo
        public ResultCode GetLibraryAppletInfo(ServiceCtx context)
        {
            context.ResponseData.Write((int)_testAppletID); // AppletId
            context.ResponseData.Write(0x00); // LibraryAppletMode

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandHipc(14)]
        // GetCallerAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetCallerAppletIdentityInfo(ServiceCtx context)
        {
            context.ResponseData.Write(0x01); // AppletId
            context.ResponseData.Write(0x00); // Padding
            context.ResponseData.Write(0x0100000000001000); // QLaunch TitleId

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}