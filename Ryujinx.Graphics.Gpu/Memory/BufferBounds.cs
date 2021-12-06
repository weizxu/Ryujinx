using Ryujinx.Graphics.Shader;
using Ryujinx.Memory.Range;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Memory range used for buffers.
    /// </summary>
    struct BufferBounds
    {
        /// <summary>
        /// Region where the data is located.
        /// </summary>
        public MultiRange Range { get; }

        /// <summary>
        /// Buffer usage flags.
        /// </summary>
        public BufferUsageFlags Flags { get; }

        /// <summary>
        /// Creates a new buffer region.
        /// </summary>
        /// <param name="range">Region where the data is located</param>
        /// <param name="flags">Buffer usage flags</param>
        public BufferBounds(MultiRange range, BufferUsageFlags flags = BufferUsageFlags.None)
        {
            Range = range;
            Flags = flags;
        }
    }
}