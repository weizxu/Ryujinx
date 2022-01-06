namespace Ryujinx.Graphics.Gpu.Shader
{
    /// <summary>
    /// State used by the <see cref="GpuAccessor"/>.
    /// </summary>
    struct GpuAccessorState
    {
        public readonly GpuChannelState ChannelState;

        public readonly ShaderSpecializationState SpecializationState;

        public GpuAccessorState(GpuChannelState channelState)
        {
            ChannelState = channelState;
            SpecializationState = new ShaderSpecializationState();
        }
    }
}