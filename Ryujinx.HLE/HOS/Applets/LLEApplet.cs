using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ryujinx.HLE.HOS.Applets
{
    class LLEApplet : IApplet
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
            _system.AppletState.AppletData = normalSession.Pop();
            string contentPath = _system.ContentManager.GetInstalledContentPath(0x0100000000001009, StorageId.NandSystem, LibHac.FsSystem.NcaUtils.NcaContentType.Program);
            contentPath = _system.Device.FileSystem.SwitchPathToSystemPath(contentPath);
            _system.Device.LoadNca(contentPath);
            return ResultCode.Success;
        }
    }
}
