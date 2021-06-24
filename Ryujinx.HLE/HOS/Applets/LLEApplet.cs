using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;

namespace Ryujinx.HLE.HOS.Applets
{
    abstract class LLEApplet : IApplet
    {
        private readonly Horizon _system;

        public event EventHandler AppletStateChanged;

        public LLEApplet(Horizon system)
        {
            _system = system;
        }

        public ResultCode GetResult()
        {
            return ResultCode.Success;
        }

        public ResultCode Start(AppletSession normalSession, AppletSession interactiveSession)
        {
            while (normalSession.TryPop(out var data))
            {
                _system.AppletState.AppletData.Enqueue(data);
            }
            string contentPath = _system.ContentManager.GetInstalledContentPath(GetTitleId(), StorageId.NandSystem, LibHac.FsSystem.NcaUtils.NcaContentType.Program);
            contentPath = _system.Device.FileSystem.SwitchPathToSystemPath(contentPath);
            _system.Device.LoadNca(contentPath);
            return ResultCode.Success;
        }

        protected abstract ulong GetTitleId();
    }
}
