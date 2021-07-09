namespace Ryujinx.Graphics.GAL
{
    public struct BufferRange
    {
        private static readonly BufferRange _empty = new BufferRange(BufferHandle.Null, 0, 0);

        public static BufferRange Empty => _empty;

        public BufferHandle Handle { get; }

        public ulong Offset { get; }
        public ulong Size   { get; }

        public BufferRange(BufferHandle handle, ulong offset, ulong size)
        {
            Handle = handle;
            Offset = offset;
            Size   = size;
        }
    }
}