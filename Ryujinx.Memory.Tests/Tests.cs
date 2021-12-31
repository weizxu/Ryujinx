using NUnit.Framework;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Memory.Tests
{
    public class Tests
    {
        private const ulong MemorySize = 0x8000;

        private MemoryBlock _memoryBlock;

        [SetUp]
        public void Setup()
        {
            _memoryBlock = new MemoryBlock(MemorySize);
        }

        [TearDown]
        public void Teardown()
        {
            _memoryBlock.Dispose();
        }

        [Test]
        public void Test_Read()
        {
            Marshal.WriteInt32(_memoryBlock.Pointer, 0x2020, 0x1234abcd);

            Assert.AreEqual(_memoryBlock.Read<int>(0x2020), 0x1234abcd);
        }

        [Test]
        public void Test_Write()
        {
            _memoryBlock.Write(0x2040, 0xbadc0de);

            Assert.AreEqual(Marshal.ReadInt32(_memoryBlock.Pointer, 0x2040), 0xbadc0de);
        }

        [Test]
        public void Test_Alias()
        {
            MemoryBlock backing = new MemoryBlock(0x10000, MemoryAllocationFlags.Mirrorable);
            MemoryBlock toAlias = new MemoryBlock(0x10000, MemoryAllocationFlags.Reserve | MemoryAllocationFlags.ViewCompatible);

            toAlias.MapView(backing, 0x1000, 0, 0x4000);
            toAlias.UnmapView(0x3000, 0x1000);

            toAlias.Write(0, 0xbadc0de);

            Assert.AreEqual(Marshal.ReadInt32(backing.Pointer, 0x1000), 0xbadc0de);
        }
    }
}