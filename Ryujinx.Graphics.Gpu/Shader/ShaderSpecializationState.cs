using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu.Engine.Threed;
using Ryujinx.Graphics.Gpu.Image;
using System.Collections.Generic;

namespace Ryujinx.Graphics.Gpu.Shader
{
    class ShaderSpecializationState
    {
        private enum QueriedStateFlags : byte
        {
            EarlyZForce = 1 << 0,
            PrimitiveTopology = 1 << 1,
            TessellationMode = 1 << 2
        }

        private QueriedStateFlags _queriedState;

        private bool _earlyZForce;
        private PrimitiveTopology _topology;
        private TessMode _tessellationMode;

        private enum QueriedTextureStateFlags : byte
        {
            CoordNormalized = 1 << 0
        }

        private class TextureSpecializationState
        {
            public QueriedTextureStateFlags QueriedFlags;
            public bool CoordNormalized;
        }

        private readonly Dictionary<int, TextureSpecializationState> _textureSpecialization;

        public ShaderSpecializationState()
        {
            _textureSpecialization = new Dictionary<int, TextureSpecializationState>();
        }

        public void RecordEarlyZForce(bool earlyZForce)
        {
            _earlyZForce = earlyZForce;
            _queriedState |= QueriedStateFlags.EarlyZForce;
        }

        public void RecordPrimitiveTopology(PrimitiveTopology topology)
        {
            _topology = topology;
            _queriedState |= QueriedStateFlags.PrimitiveTopology;
        }

        public void RecordTessellationMode(TessMode tessellationMode)
        {
            _tessellationMode = tessellationMode;
            _queriedState |= QueriedStateFlags.TessellationMode;
        }

        public void RecordTextureCoordNormalized(int stageIndex, int handle, int cbufSlot, bool coordNormalized)
        {
            int key = PackTextureKey(stageIndex, handle, cbufSlot);

            if (!_textureSpecialization.TryGetValue(key, out TextureSpecializationState state))
            {
                _textureSpecialization.Add(key, state = new TextureSpecializationState());
            }

            state.CoordNormalized = coordNormalized;
            state.QueriedFlags |= QueriedTextureStateFlags.CoordNormalized;
        }

        private static int PackTextureKey(int stageIndex, int handle, int cbufSlot)
        {
            return handle | ((byte)cbufSlot << 16) | (stageIndex << 24);
        }

        private static (int, int, int) UnpackTextureKey(int key)
        {
            int cbufSlot = (byte)(key >> 16);
            if (cbufSlot == byte.MaxValue)
            {
                cbufSlot = -1;
            }

            return ((byte)(key >> 24), (ushort)key, cbufSlot);
        }

        public bool MatchesGraphics(GpuChannel channel, GpuChannelState channelState)
        {
            return Matches(channel, channelState, isCompute: false);
        }

        public bool MatchesCompute(GpuChannel channel, GpuChannelState channelState)
        {
            return Matches(channel, channelState, isCompute: true);
        }

        private bool Matches(GpuChannel channel, GpuChannelState channelState, bool isCompute)
        {
            foreach (var kv in _textureSpecialization)
            {
                (int stageIndex, int handle, int cbufSlot) = UnpackTextureKey(kv.Key);
                TextureSpecializationState specializationState = kv.Value;
                TextureDescriptor descriptor;

                if (isCompute)
                {
                    descriptor = channel.TextureManager.GetComputeTextureDescriptor(
                        channelState.TexturePoolGpuVa,
                        channelState.TextureBufferIndex,
                        channelState.TexturePoolMaximumId,
                        handle,
                        cbufSlot);
                }
                else
                {
                    descriptor = channel.TextureManager.GetGraphicsTextureDescriptor(
                        channelState.TexturePoolGpuVa,
                        channelState.TextureBufferIndex,
                        channelState.TexturePoolMaximumId,
                        stageIndex,
                        handle,
                        cbufSlot);
                }

                if (specializationState.QueriedFlags.HasFlag(QueriedTextureStateFlags.CoordNormalized) &&
                    specializationState.CoordNormalized != descriptor.UnpackTextureCoordNormalized())
                {
                    return false;
                }
            }

            return true;
        }
    }
}