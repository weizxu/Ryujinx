using Ryujinx.Memory.Range;
using System;
using System.Diagnostics;

namespace Ryujinx.Memory.WindowsShared
{
    class PlaceholderManager
    {
        private enum MappingState : byte
        {
            Free,
            Mapped
        }

        private class Mapping : INonOverlappingRange
        {
            public ulong Address { get; private set; }
            public ulong Size { get; private set; }
            public ulong EndAddress => Address + Size;

            public Mapping(ulong address, ulong size)
            {
                Address = address;
                Size = size;
            }

            public bool OverlapsWith(ulong address, ulong size)
            {
                return Address < address + size && address < EndAddress;
            }

            public INonOverlappingRange Split(ulong splitAddress)
            {
                Mapping newRegion = new Mapping(splitAddress, EndAddress - splitAddress);

                Size = splitAddress - Address;

                return newRegion;
            }
        }

        private readonly RangeList<Mapping> _mappings = new RangeList<Mapping>();

        public void ReserveRange(ulong address, ulong size)
        {
            lock (_mappings)
            {
                _mappings.Add(new Mapping(address, size));
            }
        }

        public void DoForEachSubRange(ulong address, ulong size, Action<ulong, ulong> callback)
        {
            ulong endAddress = address + size;

            Mapping[] overlaps = Array.Empty<Mapping>();
            int count = 0;

            lock (_mappings)
            {
                count = _mappings.FindOverlapsNonOverlapping(address, size, ref overlaps);
            }

            for (int index = 0; index < count; index++)
            {
                Mapping mapping = overlaps[index];

                ulong mappedAddress = mapping.Address;
                ulong mappedSize = mapping.Size;

                if (mappedAddress < address)
                {
                    ulong delta = address - mappedAddress;
                    mappedAddress = address;
                    mappedSize -= delta;
                }

                ulong mappedEndAddress = mappedAddress + mappedSize;

                if (mappedEndAddress > endAddress)
                {
                    ulong delta = mappedEndAddress - endAddress;
                    mappedSize -= delta;
                }

                callback(mappedAddress, mappedSize);
            }
        }

        public void UnmapRange(ulong address, ulong size)
        {
            lock (_mappings)
            {
                UnmapRangeInternal(address, size);
            }
        }

        private void UnmapRangeInternal(ulong address, ulong size)
        {
            bool needsCoalesce = false;
            ulong endAddress = address + size;

            Mapping[] overlaps = Array.Empty<Mapping>();

            int count = _mappings.FindOverlapsNonOverlapping(address, size, ref overlaps);

            for (int index = 0; index < count; index++)
            {
                Mapping overlap = overlaps[index];

                if (overlap.Address > address || overlap.EndAddress < endAddress)
                {
                    needsCoalesce = true;
                }

                // System.Console.WriteLine($"found overlap {overlap.Address:X} {overlap.Size:X}");

                _mappings.Remove(overlap);

                bool overlapStartsBefore = overlap.Address < address;
                bool overlapEndsAfter = overlap.EndAddress > endAddress;

                if (overlapStartsBefore && overlapEndsAfter)
                {
                    ulong rightSize = overlap.EndAddress - endAddress;

                    // System.Console.WriteLine($"virtual free 1 {address:X} {size:X}");

                    WindowsApi.VirtualFree(
                        (IntPtr)address,
                        (IntPtr)size,
                        AllocationType.Release | AllocationType.PreservePlaceholder);

                    _mappings.Add(new Mapping(overlap.Address, address - overlap.Address));
                    _mappings.Add(new Mapping(endAddress, rightSize));
                }
                else if (overlapStartsBefore)
                {
                    ulong overlappedSize = overlap.EndAddress - address;

                    // System.Console.WriteLine($"virtual free 2 {address:X} {overlappedSize:X}");

                    bool success = WindowsApi.VirtualFree(
                        (IntPtr)address,
                        (IntPtr)overlappedSize,
                        AllocationType.Release | AllocationType.PreservePlaceholder);

                    if (!success)
                    {
                        throw new Exception($"virtual free failed {WindowsApi.GetLastError():X}");
                    }

                    _mappings.Add(new Mapping(overlap.Address, address - overlap.Address));
                }
                else if (overlapEndsAfter)
                {
                    ulong overlappedSize = endAddress - overlap.Address;

                    // System.Console.WriteLine($"virtual free 3 {overlap.Address:X} {size:X}");

                    WindowsApi.VirtualFree(
                        (IntPtr)overlap.Address,
                        (IntPtr)overlappedSize,
                        AllocationType.Release | AllocationType.PreservePlaceholder);

                    _mappings.Add(new Mapping(endAddress, overlap.EndAddress - endAddress));
                }
            }

            if (needsCoalesce)
            {
                // System.Console.WriteLine($"virtual free coalesce {address:X} {size:X}");

                bool success = WindowsApi.VirtualFree(
                    (IntPtr)address,
                    (IntPtr)size,
                    AllocationType.Release | AllocationType.CoalescePlaceholders);

                if (!success)
                {
                    System.Console.WriteLine($"cant coalesce {WindowsApi.GetLastError():X}");
                }
            }

            _mappings.Add(new Mapping(address, size));
        }
    }
}