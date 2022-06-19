using Ryujinx.Memory.Range;
using System;
using System.Linq;

namespace Ryujinx.Graphics.Gpu.Memory
{
    /// <summary>
    /// Buffer cache.
    /// </summary>
    class BufferCache : IDisposable
    {
        private const int OverlapsBufferInitialCapacity = 10;
        private const int OverlapsBufferMaxCapacity     = 10000;

        private readonly GpuContext _context;
        private readonly PhysicalMemory _physicalMemory;

        private readonly MultiRangeList<Buffer> _discontinuousBuffers;

        private Buffer[] _bufferOverlaps;

        public event Action NotifyBuffersModified;

        private ulong _totalBufferSize;

        /// <summary>
        /// Creates a new instance of the buffer manager.
        /// </summary>
        /// <param name="context">The GPU context that the buffer manager belongs to</param>
        /// <param name="physicalMemory">Physical memory where the cached buffers are mapped</param>
        public BufferCache(GpuContext context, PhysicalMemory physicalMemory)
        {
            _context = context;
            _physicalMemory = physicalMemory;

            _discontinuousBuffers = new MultiRangeList<Buffer>();

            _bufferOverlaps = new Buffer[OverlapsBufferInitialCapacity];
        }

        public Buffer FindOrCreateMultiRangeBuffer(MultiRange range, out int offset)
        {
            Buffer[] overlaps = _bufferOverlaps;

            int overlapsCount = _discontinuousBuffers.FindOverlaps(range, ref overlaps);
            int fullyContainedOverlaps = 0;

            for (int index = 0; index < overlapsCount; index++)
            {
                Buffer overlap = overlaps[index];

                int offsetWithinOverlap = overlap.Range.FindOffset(range);
                if (offsetWithinOverlap != -1)
                {
                    // If the range is fully contained within one of the overlaps, we're done.
                    ShrinkOverlapsBufferIfNeeded();
                    offset = offsetWithinOverlap;
                    return overlap;
                }
                else if (range.Contains(overlap.Range))
                {
                    // System.Console.WriteLine("contained overlap " + overlap.Range);
                    overlaps[fullyContainedOverlaps++] = overlap;
                }
                else
                {
                    // System.Console.WriteLine($"rg {range} has non-containable overlap {overlap.Range}");
                }
            }

            if (overlapsCount != fullyContainedOverlaps)
            {
                // throw new Exception("non-containable overlaps found");
            }

            // System.Console.WriteLine("create buffer " + range);

            // None of the existing buffers fully contains the discontinuous range, create a new one.
            Buffer buffer = new Buffer(_context, _physicalMemory, range, overlaps.Take(fullyContainedOverlaps));

            _totalBufferSize += buffer.Size;

            _discontinuousBuffers.Add(buffer);

            for (int index = 0; index < fullyContainedOverlaps; index++)
            {
                Buffer overlap = overlaps[index];

                int offsetWithinOverlap = range.FindOffset(overlap.Range);
                if (offsetWithinOverlap != -1)
                {
                    overlap.CopyTo(buffer, offsetWithinOverlap);
                    _discontinuousBuffers.Remove(overlap);
                    buffer.InheritModifiedRanges(overlap);

                    overlap.DisposeData();
                    overlap.UpdateViews(buffer, offsetWithinOverlap);

                    _totalBufferSize -= overlap.Size;
                }
                else
                {
                    throw new Exception("bad offset");
                }
            }

            // System.Console.WriteLine("total buffer usage: " + (_totalBufferSize / (1024 * 1024)) + " MB");

            if (fullyContainedOverlaps != 0)
            {
                buffer.SynchronizeMemory(range);

                // Existing buffers were modified, we need to rebind everything.
                NotifyBuffersModified?.Invoke();
            }

            ShrinkOverlapsBufferIfNeeded();
            offset = 0;
            return buffer;
        }

        public void RemoveBufferIfUnused(Buffer buffer)
        {
            if (!buffer.HasViews)
            {
                _discontinuousBuffers.Remove(buffer);
                buffer.Dispose();
                _totalBufferSize -= buffer.Size;

                NotifyBuffersModified?.Invoke();
            }
        }

        public void SplitRange(Buffer buffer, ulong offset, MultiRange range)
        {
            // Split a buffer into up to 3 buffers, given a range that should be
            // split into a separate buffer.
            // We have 3 cases:
            // - Range covers the entire buffer, no split is performed.
            // - Range is at the start or end of the buffer, the buffer is split in 2.
            // - Range is at the middle of the buffer, the buffer is split in 3.

            // In the future, we might want to delete the middle buffer
            // if there is no other "buffer view" that might use it.

            ulong rangeSize = range.GetSize();
            if (rangeSize == buffer.Size)
            {
                // No split needed as the range covers the entire buffer.
                return;
            }

            ulong rightOffset = offset + rangeSize;
            ulong rightSize = buffer.Size - rightOffset;

            Buffer middleBuffer = new Buffer(_context, _physicalMemory, range);
            buffer.CopyTo(middleBuffer, (int)offset, 0, (int)rangeSize);

            lock (_discontinuousBuffers)
            {
                _discontinuousBuffers.Remove(buffer);
                _discontinuousBuffers.Add(middleBuffer);
            }

            if (offset != 0)
            {
                Buffer leftBuffer = new Buffer(_context, _physicalMemory, buffer.Range.GetSlice(0, offset));
                buffer.CopyTo(leftBuffer, 0, 0, (int)offset);

                lock (_discontinuousBuffers)
                {
                    _discontinuousBuffers.Add(leftBuffer);
                }
            }

            if (rightSize != 0)
            {
                Buffer rightBuffer = new Buffer(_context, _physicalMemory, buffer.Range.GetSlice(rightOffset, rightSize));
                buffer.CopyTo(rightBuffer, (int)rightOffset, 0, (int)rightSize);

                lock (_discontinuousBuffers)
                {
                    _discontinuousBuffers.Add(rightBuffer);
                }
            }
        }

        /* private void CreateDiscontinuousBuffer(MultiRange range)
        {
            if (range.Count == 1)
            {
                MemoryRange subRange = range.GetSubRange(0);

                if (((subRange.Address | subRange.Size) & BufferAlignmentMask) != 0)
                {
                    ulong endAddress = subRange.Address + subRange.Size;
                    ulong alignedAddress = subRange.Address & ~BufferAlignmentMask;
                    ulong alignedEndAddress = (endAddress + BufferAlignmentMask) & ~BufferAlignmentMask;

                    // The buffer must have the size of at least one page.
                    if (alignedEndAddress == alignedAddress)
                    {
                        alignedEndAddress += BufferAlignmentSize;
                    }

                    range = new MultiRange(alignedAddress, alignedEndAddress - alignedAddress);
                }
            }
            else
            {
                MemoryRange firstSubRange = range.GetSubRange(0);
                MemoryRange lastSubRange = range.GetSubRange(range.Count - 1);

                if ((firstSubRange.Address & BufferAlignmentMask) != 0 ||
                    (lastSubRange.EndAddress & BufferAlignmentMask) != 0)
                {
                    MemoryRange[] subRanges = new MemoryRange[range.Count];

                    ulong alignedAddress = firstSubRange.Address & ~BufferAlignmentMask;
                    ulong alignedEndAddress = (lastSubRange.EndAddress + BufferAlignmentMask) & ~BufferAlignmentMask;

                    subRanges[0] = new MemoryRange(alignedAddress, firstSubRange.EndAddress - alignedAddress);
                    subRanges[range.Count - 1] = new MemoryRange(lastSubRange.Address, alignedEndAddress - lastSubRange.Address);

                    for (int index = 1; index < range.Count - 1; index++)
                    {
                        subRanges[index] = range.GetSubRange(index);
                    }

                    range = new MultiRange(subRanges);
                }
            }


            CreateDiscontinuousBufferAligned(range);
        }

        private void CreateDiscontinuousBufferAligned(MultiRange range)
        {
            int overlapsCount = _discontinuousBuffers.FindOverlaps(range, ref _bufferOverlaps);

            MultiRange requestedRange = range;

            for (int index = 0; index < overlapsCount; index++)
            {
                Buffer overlap = _bufferOverlaps[index];

                if (overlap.Range.Contains(requestedRange))
                {
                    // If the range is fully contained within one of the overlaps, we're done.
                    ShrinkOverlapsBufferIfNeeded();
                    return;
                }
            }

            for (int index = 0; index < overlapsCount; index++)
            {
                Buffer overlap = _bufferOverlaps[index];
                MemoryRange firstSubRange = overlap.Range.GetSubRange(0);
                MemoryRange lastSubRange = overlap.Range.GetSubRange(overlap.Range.Count - 1);

                MemoryRange currentFirstSubRange = range.GetSubRange(0);
                MemoryRange currentLastSubRange = range.GetSubRange(range.Count - 1);

                if (lastSubRange.OverlapsWith(currentFirstSubRange))
                {
                    range = MergeRanges(overlap.Range, range, lastSubRange, currentFirstSubRange);
                }
                else if (firstSubRange.OverlapsWith(currentLastSubRange))
                {
                    range = MergeRanges(range, overlap.Range, currentLastSubRange, firstSubRange);
                }
            }

            // None of the existing buffers fully contains the discontinuous range, create a new one.
            Buffer buffer = new Buffer(_context, _physicalMemory, range);

            _discontinuousBuffers.Add(buffer);

            bool anyOverlapContained = false;

            for (int index = 0; index < overlapsCount; index++)
            {
                Buffer overlap = _bufferOverlaps[index];

                int offset = range.FindOffset(overlap.Range);
                if (offset != -1)
                {
                    overlap.CopyTo(buffer, offset);
                    _discontinuousBuffers.Remove(overlap);
                    overlap.Dispose();
                    anyOverlapContained = true;
                }
            }

            if (anyOverlapContained)
            {
                buffer.SynchronizeMemory(range);

                // Existing buffers were modified, we need to rebind everything.
                NotifyBuffersModified?.Invoke();
            }

            ShrinkOverlapsBufferIfNeeded();
        }

        private static MultiRange MergeRanges(MultiRange left, MultiRange right, MemoryRange lLastRange, MemoryRange rFirstRange)
        {
            MemoryRange[] subRanges = new MemoryRange[left.Count + right.Count - 1];

            int overlapIndex = left.Count - 1;

            for (int i = 0; i < overlapIndex; i++)
            {
                subRanges[i] = left.GetSubRange(i);
            }

            for (int i = 1; i < right.Count; i++)
            {
                subRanges[overlapIndex + i] = right.GetSubRange(i);
            }

            ulong minAddress = Math.Min(lLastRange.Address, rFirstRange.Address);
            ulong maxAddress = Math.Max(lLastRange.EndAddress, rFirstRange.EndAddress);

            subRanges[overlapIndex] = new MemoryRange(minAddress, maxAddress - minAddress);

            return new MultiRange(subRanges);
        } */

        /// <summary>
        /// Resizes the temporary buffer used for range list intersection results, if it has grown too much.
        /// </summary>
        private void ShrinkOverlapsBufferIfNeeded()
        {
            if (_bufferOverlaps.Length > OverlapsBufferMaxCapacity)
            {
                Array.Resize(ref _bufferOverlaps, OverlapsBufferMaxCapacity);
            }
        }

        /// <summary>
        /// Disposes all buffers in the cache.
        /// It's an error to use the buffer manager after disposal.
        /// </summary>
        public void Dispose()
        {
            foreach (Buffer buffer in _discontinuousBuffers)
            {
                buffer.Dispose();
            }
        }
    }
}