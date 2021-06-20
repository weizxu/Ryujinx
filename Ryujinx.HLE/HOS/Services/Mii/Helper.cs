﻿using Ryujinx.HLE.HOS.Services.Account.Acc;
using Ryujinx.HLE.Utilities;
using System;
using System.Buffers.Binary;

namespace Ryujinx.HLE.HOS.Services.Mii
{
    static class Helper
    {
        public static ushort CalculateCrc16(ReadOnlySpan<byte> data, int crc, bool reverseEndianess)
        {
            const ushort poly = 0x1021;

            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i] << 8;

                for (int j = 0; j < 8; j++)
                {
                    crc <<= 1;

                    if ((crc & 0x10000) != 0)
                    {
                        crc = (crc ^ poly) & 0xFFFF;
                    }
                }
            }

            if (reverseEndianess)
            {
                return (ushort)(BinaryPrimitives.ReverseEndianness(crc) >> 16);
            }

            return (ushort)crc;
        }

        public static UInt128 GetDeviceId()
        {
            // FIXME: call set:sys GetMiiAuthorId
            return new UInt128("5279754d69694e780000000000000000");
        }

        public static ReadOnlySpan<byte> Ver3FacelineColorTable => new byte[] { 0, 1, 2, 3, 4, 5 };
        public static ReadOnlySpan<byte> Ver3HairColorTable     => new byte[] { 8, 1, 2, 3, 4, 5, 6, 7 };
        public static ReadOnlySpan<byte> Ver3EyeColorTable      => new byte[] { 8, 9, 10, 11, 12, 13 };
        public static ReadOnlySpan<byte> Ver3MouthColorTable    => new byte[] { 19, 20, 21, 22, 23 };
        public static ReadOnlySpan<byte> Ver3GlassColorTable    => new byte[] { 8, 14, 15, 16, 17, 18, 0 };
    }
}
