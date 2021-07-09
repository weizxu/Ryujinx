using System;

namespace Ryujinx.Graphics.GAL
{
    [Flags]
    public enum BufferCreateFlags
    {
        None = 0,
        Reserve = 1 << 0
    }
}
