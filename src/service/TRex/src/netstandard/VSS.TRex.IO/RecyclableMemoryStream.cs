// This class is based on the class of the same name from Microsoft.IO.RecylcableMemorystream
// and is copied here for the purposes of TRex specific optimisations. The original 
// Microsoft copyright notice is included below.
//
// The MIT License (MIT)
// 
// Copyright (c) 2015-2016 Microsoft
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace VSS.TRex.IO
{
  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
  using System.IO;
  using System.Threading;

  /// <summary>
  /// MemoryStream implementation that deals with pooling and managing memory streams which use potentially large
  /// buffers.
  /// </summary>
  /// <remarks>
  /// This class works in tandem with the RecyclableMemoryStreamManager to supply MemoryStream
  /// objects to callers, while avoiding these specific problems:
  /// 1. LOH allocations - since all large buffers are pooled, they will never incur a Gen2 GC
  /// 2. Memory waste - A standard memory stream doubles its size when it runs out of room. This
  /// leads to continual memory growth as each stream approaches the maximum allowed size.
  /// 3. Memory copying - Each time a MemoryStream grows, all the bytes are copied into new buffers.
  /// This implementation only copies the bytes when GetBuffer is called.
  /// 4. Memory fragmentation - By using homogeneous buffer sizes, it ensures that blocks of memory
  /// can be easily reused.
  /// 
  /// The stream is implemented on top of a series of uniformly-sized blocks. As the stream's length grows,
  /// additional blocks are retrieved from the memory manager. It is these blocks that are pooled, not the stream
  /// object itself.
  /// 
  /// The biggest wrinkle in this implementation is when GetBuffer() is called. This requires a single 
  /// contiguous buffer. If only a single block is in use, then that block is returned. If multiple blocks 
  /// are in use, we retrieve a larger buffer from the memory manager. These large buffers are also pooled, 
  /// split by size--they are multiples/exponentials of a chunk size (1 MB by default).
  /// 
  /// Once a large buffer is assigned to the stream the blocks are NEVER again used for this stream. All operations take place on the 
  /// large buffer. The large buffer can be replaced by a larger buffer from the pool as needed. All blocks and large buffers 
  /// are maintained in the stream until the stream is disposed (unless AggressiveBufferReturn is enabled in the stream manager).
  /// 
  /// </remarks>
  public sealed class RecyclableMemoryStream : MemoryStream
  {
    private const long MaxStreamLength = Int32.MaxValue;
    private const int InitialBlocksArraySizeAndIncrement = 4;

    private static readonly byte[] emptyArray = new byte[0];

    /// <summary>
    /// All of these blocks must be the same size
    /// </summary>
    private byte[][] blocks = new byte[InitialBlocksArraySizeAndIncrement][];

    /// <summary>
    /// The number of used blocks in the blocks array
    /// </summary>
    private int blocksCount = 0;

    private readonly Guid id;

    private readonly RecyclableMemoryStreamManager memoryManager;

    private readonly string tag;

    /// <summary>
    /// This list is used to store buffers once they're replaced by something larger.
    /// This is for the cases where you have users of this class that may hold onto the buffers longer
    /// than they should and you want to prevent race conditions which could corrupt the data.
    /// </summary>
    private List<byte[]> dirtyBuffers;

    // long to allow Interlocked.Read (for .NET Standard 1.4 compat)
    private long disposedState;

    /// <summary>
    /// This is only set by GetBuffer() if the necessary buffer is larger than a single block size, or on
    /// construction if the caller immediately requests a single large buffer.
    /// </summary>
    /// <remarks>If this field is non-null, it contains the concatenation of the bytes found in the individual
    /// blocks. Once it is created, this (or a larger) largeBuffer will be used for the life of the stream.
    /// </remarks>
    private byte[] largeBuffer;

    /// <summary>
    /// The block size copied from the recyclable memory stream manager at the time this stream was constructed.
    /// Block size is an immutable property of the manager so is safe to maintain a locally referencable copy in the stream
    /// </summary>
    private readonly int blockSize;

    /// <summary>
    /// The maximum stream capacity from the recyclable memory stream manager at the time this stream was constructed
    /// Maximum stream capacity is an immutable property of the manager so is safe to maintain a locally referencable copy in the stream
    /// </summary>
    private readonly long maximumStreamCapacity;

    /// <summary>
    /// Unique identifier for this stream across it's entire lifetime
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    internal Guid Id
    {
      get
      {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
        this.CheckDisposed();
#endif
        return this.id;
      }
    }

    /// <summary>
    /// A temporary identifier for the current usage of this stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    internal string Tag
    {
      get
      {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
        this.CheckDisposed();
#endif
        return this.tag;
      }
    }

    /// <summary>
    /// Gets the memory manager being used by this stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    internal RecyclableMemoryStreamManager MemoryManager
    {
      get
      {
        this.CheckDisposed();
        return this.memoryManager;
      }
    }

    /// <summary>
    /// Callstack of the constructor. It is only set if MemoryManager.GenerateCallStacks is true,
    /// which should only be in debugging situations.
    /// </summary>
    internal string AllocationStack { get; }

    /// <summary>
    /// Callstack of the Dispose call. It is only set if MemoryManager.GenerateCallStacks is true,
    /// which should only be in debugging situations.
    /// </summary>
    internal string DisposeStack { get; private set; }

    #region Constructors
    /// <summary>
    /// Allocate a new RecyclableMemoryStream object.
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager)
        : this(memoryManager, null, 0, null) { }

    /// <summary>
    /// Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag)
        : this(memoryManager, tag, 0, null) { }

    /// <summary>
    /// Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
    public RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize)
        : this(memoryManager, tag, requestedSize, null) { }

    /// <summary>
    /// Allocate a new RecyclableMemoryStream object
    /// </summary>
    /// <param name="memoryManager">The memory manager</param>
    /// <param name="tag">A string identifying this stream for logging and debugging purposes</param>
    /// <param name="requestedSize">The initial requested size to prevent future allocations</param>
    /// <param name="initialLargeBuffer">An initial buffer to use. This buffer will be owned by the stream and returned to the memory manager upon Dispose.</param>
    internal RecyclableMemoryStream(RecyclableMemoryStreamManager memoryManager, string tag, int requestedSize,
                                    byte[] initialLargeBuffer)
        : base(emptyArray)
    {
      this.memoryManager = memoryManager;
      this.blockSize = memoryManager.BlockSize;
      this.maximumStreamCapacity = memoryManager.MaximumStreamCapacity == 0 ? long.MaxValue : memoryManager.MaximumStreamCapacity;

      this.id = Guid.NewGuid();
      this.tag = tag;

      if (requestedSize < this.blockSize)
      {
        requestedSize = this.blockSize;
      }

      if (initialLargeBuffer == null)
      {
        this.EnsureCapacity(requestedSize);
      }
      else
      {
        this.largeBuffer = initialLargeBuffer;
        this.capacity = initialLargeBuffer.Length;
      }

      if (this.memoryManager.GenerateCallStacks)
      {
        this.AllocationStack = Environment.StackTrace;
      }

      RecyclableMemoryStreamManager.Events.Writer.MemoryStreamCreated(this.id, this.tag, requestedSize);
      this.memoryManager.ReportStreamCreated();
    }
    #endregion

    #region Dispose and Finalize
    ~RecyclableMemoryStream()
    {
      this.Dispose(false);
    }

    /// <summary>
    /// Returns the memory used by this stream back to the pool.
    /// </summary>
    /// <param name="disposing">Whether we're disposing (true), or being called by the finalizer (false)</param>
    [SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly",
        Justification = "We have different disposal semantics, so SuppressFinalize is in a different spot.")]
    protected override void Dispose(bool disposing)
    {
      if (Interlocked.CompareExchange(ref this.disposedState, 1, 0) != 0)
      {
        string doubleDisposeStack = null;
        if (this.memoryManager.GenerateCallStacks)
        {
          doubleDisposeStack = Environment.StackTrace;
        }

        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDoubleDispose(this.id, this.tag,
                                                                             this.AllocationStack,
                                                                             this.DisposeStack,
                                                                             doubleDisposeStack);
        return;
      }

      RecyclableMemoryStreamManager.Events.Writer.MemoryStreamDisposed(this.id, this.tag);

      if (this.memoryManager.GenerateCallStacks)
      {
        this.DisposeStack = Environment.StackTrace;
      }

      if (disposing)
      {
        this.memoryManager.ReportStreamDisposed();

        GC.SuppressFinalize(this);
      }
      else
      {
        // We're being finalized.

        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamFinalized(this.id, this.tag, this.AllocationStack);

#if !NETSTANDARD1_4
        if (AppDomain.CurrentDomain.IsFinalizingForUnload())
        {
          // If we're being finalized because of a shutdown, don't go any further.
          // We have no idea what's already been cleaned up. Triggering events may cause
          // a crash.
          base.Dispose(disposing);
          return;
        }
#endif

        this.memoryManager.ReportStreamFinalized();
      }

      this.memoryManager.ReportStreamLength(this.length);

      if (this.largeBuffer != null)
      {
        this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
      }

      if (this.dirtyBuffers != null)
      {
        for (int i = 0, limit = this.dirtyBuffers.Count; i < limit; i++)
        {
          this.memoryManager.ReturnLargeBuffer(this.dirtyBuffers[i], this.tag);
        }
      }

      this.memoryManager.ReturnBlocks(this.blocks, this.blocksCount, this.tag);

      for (int i = 0; i < blocksCount; i++)
      {
        this.blocks[i] = null;
      }

      base.Dispose(disposing);
    }

    /// <summary>
    /// Equivalent to Dispose
    /// </summary>
#if NETSTANDARD1_4
        public void Close()
#else
    public override void Close()
#endif
    {
      this.Dispose(true);
    }
    #endregion

    #region MemoryStream overrides

    /// <summary>
    /// Backing variable for capacity value
    /// </summary>
    private int capacity;

    /// <summary>
    /// Gets or sets the capacity
    /// </summary>
    /// <remarks>Capacity is always in multiples of the memory manager's block size, unless
    /// the large buffer is in use.  Capacity never decreases during a stream's lifetime. 
    /// Explicitly setting the capacity to a lower value than the current value will have no effect. 
    /// This is because the buffers are all pooled by chunks and there's little reason to 
    /// allow stream truncation.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int Capacity
    {
      get
      {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
        this.CheckDisposed();
#endif
        return capacity;
      }
      set
      {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
        this.CheckDisposed();
#endif
        this.EnsureCapacity(value);
      }
    }

    private int length;

    /// <summary>
    /// Gets the number of bytes written to this stream.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override long Length
    {
      get
      {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
        this.CheckDisposed();
#endif
        return this.length;
      }
    }

    private int position;

    /// <summary>
    /// Gets the current position in the stream
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override long Position
    {
      get
      {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
        this.CheckDisposed();
#endif
        return this.position;
      }
      set
      {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
        this.CheckDisposed();
#endif
        if (value < 0)
        {
          throw new ArgumentOutOfRangeException("value", "value must be non-negative");
        }

        if (value > MaxStreamLength)
        {
          throw new ArgumentOutOfRangeException("value", "value cannot be more than " + MaxStreamLength);
        }

        this.position = (int)value;
      }
    }

    /// <summary>
    /// Whether the stream can currently read
    /// </summary>
    public override bool CanRead => !this.Disposed;

    /// <summary>
    /// Whether the stream can currently seek
    /// </summary>
    public override bool CanSeek => !this.Disposed;

    /// <summary>
    /// Always false
    /// </summary>
    public override bool CanTimeout => false;

    /// <summary>
    /// Whether the stream can currently write
    /// </summary>
    public override bool CanWrite => !this.Disposed;

    /// <summary>
    /// Returns a single buffer containing the contents of the stream.
    /// The buffer may be longer than the stream length.
    /// </summary>
    /// <returns>A byte[] buffer</returns>
    /// <remarks>IMPORTANT: Doing a Write() after calling GetBuffer() invalidates the buffer. The old buffer is held onto
    /// until Dispose is called, but the next time GetBuffer() is called, a new buffer from the pool will be required.</remarks>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
#if NETSTANDARD1_4
        public byte[] GetBuffer()
#else
    public override byte[] GetBuffer()
#endif
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      if (this.largeBuffer != null)
      {
        return this.largeBuffer;
      }

      if (this.blocksCount == 1)
      {
        return this.blocks[0];
      }

      // Buffer needs to reflect the capacity, not the length, because
      // it's possible that people will manipulate the buffer directly
      // and set the length afterward. Capacity sets the expectation
      // for the size of the buffer.
      var newBuffer = this.memoryManager.GetLargeBuffer(this.capacity, this.tag);

      // InternalRead will check for existence of largeBuffer, so make sure we
      // don't set it until after we've copied the data.
      this.InternalRead(newBuffer, 0, this.length, 0);
      this.largeBuffer = newBuffer;

      if (this.blocksCount > 0 && this.memoryManager.AggressiveBufferReturn)
      {
        this.memoryManager.ReturnBlocks(this.blocks, this.blocksCount, this.tag);

        for (int i = 0; i < blocksCount; i++)
        {
          this.blocks[i] = null;
        }
      }

      return this.largeBuffer;
    }

    /// <summary>
    /// Returns an ArraySegment that wraps a single buffer containing the contents of the stream.
    /// </summary>
    /// <param name="buffer">An ArraySegment containing a reference to the underlying bytes.</param>
    /// <returns>Always returns true.</returns>
    /// <remarks>GetBuffer has no failure modes (it always returns something, even if it's an empty buffer), therefore this method
    /// always returns a valid ArraySegment to the same buffer returned by GetBuffer.</remarks>
#if NET40 || NET45
        public bool TryGetBuffer(out ArraySegment<byte> buffer)  
#else
    public override bool TryGetBuffer(out ArraySegment<byte> buffer)
#endif
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif
      buffer = new ArraySegment<byte>(this.GetBuffer(), 0, (int)this.length);
      // GetBuffer has no failure modes, so this should always succeed
      return true;
    }

    /// <summary>
    /// Returns a new array with a copy of the buffer's contents. You should almost certainly be using GetBuffer combined with the Length to 
    /// access the bytes in this stream. Calling ToArray will destroy the benefits of pooled buffers, but it is included
    /// for the sake of completeness.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
#pragma warning disable CS0809
    [Obsolete("This method has degraded performance vs. GetBuffer and should be avoided.")]
    public override byte[] ToArray()
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif
      var newBuffer = new byte[this.length];

      this.InternalRead(newBuffer, 0, this.length, 0);
      string stack = this.memoryManager.GenerateCallStacks ? Environment.StackTrace : null;
      RecyclableMemoryStreamManager.Events.Writer.MemoryStreamToArray(this.id, this.tag, stack, 0);
      this.memoryManager.ReportStreamToArray();

      return newBuffer;
    }
#pragma warning restore CS0809

    /// <summary>
    /// Reads from the current position into the provided buffer
    /// </summary>
    /// <param name="buffer">Destination buffer</param>
    /// <param name="offset">Offset into buffer at which to start placing the read bytes.</param>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>The number of bytes read</returns>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">offset or count is less than 0</exception>
    /// <exception cref="ArgumentException">offset subtracted from the buffer length is less than count</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int Read(byte[] buffer, int offset, int count)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      if (buffer == null)
      {
        throw new ArgumentNullException(nameof(buffer));
      }

      if (offset < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be negative");
      }

      if (count < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(count), "count cannot be negative");
      }

      if (offset + count > buffer.Length)
      {
        throw new ArgumentException("buffer length must be at least offset + count");
      }

      int amountRead = this.InternalRead(buffer, offset, count, this.position);
      this.position += amountRead;
      return amountRead;
    }

#if NETCOREAPP2_1 || NETSTANDARD2_1
    /// <summary>
    /// Reads from the current position into the provided buffer
    /// </summary>
    /// <param name="buffer">Destination buffer</param>
    /// <returns>The number of bytes read</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int Read(Span<byte> buffer)
    {
      return this.SafeRead(buffer, ref this.position);
    }

    /// <summary>
    /// Reads from the specified position into the provided buffer
    /// </summary>
    /// <param name="buffer">Destination buffer</param>
    /// <param name="streamPosition">Position in the stream to start reading from</param>
    /// <returns>The number of bytes read</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public int SafeRead(Span<byte> buffer, ref int streamPosition)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      int amountRead = this.InternalRead(buffer, streamPosition);
      streamPosition += amountRead;
      return amountRead;
    }
#endif

    /// <summary>
    /// Writes the buffer to the stream
    /// </summary>
    /// <param name="buffer">Source buffer</param>
    /// <param name="offset">Start position</param>
    /// <param name="count">Number of bytes to write</param>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">offset or count is negative</exception>
    /// <exception cref="ArgumentException">buffer.Length - offset is not less than count</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void Write(byte[] buffer, int offset, int count)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      if (buffer == null)
      {
        throw new ArgumentNullException(nameof(buffer));
      }

      if (offset < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(offset), offset, "Offset must be in the range of 0 - buffer.Length-1");
      }

      if (count < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(count), count, "count must be non-negative");
      }

      if (count + offset > buffer.Length)
      {
        throw new ArgumentException("count must be greater than buffer.Length - offset");
      }

      long end = (long)this.position + count;

      // Check for overflow
      if (end > MaxStreamLength)
      {
        throw new IOException("Maximum capacity exceeded");
      }

      if (end >= this.capacity)
      {
        this.EnsureCapacity((int) end);
      }

      if (this.largeBuffer == null)
      {
        int blockIndex = this.position / this.blockSize;
        int blockOffset = (int)(this.position % this.blockSize);

        if (this.blockSize - blockOffset >= count)
        {
          // Happy path, only one block to consider
          Buffer.BlockCopy(buffer, offset, this.blocks[blockIndex], blockOffset, count);
        }
        else
        {
          // Sad path, need to iterate across block boundaries
          int bytesRemaining = count;
          int bytesWritten = 0;

          while (bytesRemaining > 0)
          {
            byte[] currentBlock = this.blocks[blockIndex];
            int remainingInBlock = this.blockSize - blockOffset;
            int amountToWriteInBlock = remainingInBlock < bytesRemaining ? remainingInBlock : bytesRemaining;

            Buffer.BlockCopy(buffer, offset + bytesWritten, currentBlock, blockOffset, amountToWriteInBlock);

            bytesRemaining -= amountToWriteInBlock;

            if (bytesRemaining > 0)
            {
              bytesWritten += amountToWriteInBlock;

              ++blockIndex;
              blockOffset = 0;
            }
          }
        }
      }
      else
      {
        Buffer.BlockCopy(buffer, offset, this.largeBuffer, this.position, count);
      }

      this.position = (int)end;

      if (this.position > this.length)
      {
        this.length = this.position;
      }
    }

#if NETCOREAPP2_1 || NETSTANDARD2_1
    /// <summary>
    /// Writes the buffer to the stream
    /// </summary>
    /// <param name="source">Source buffer</param>
    /// <exception cref="ArgumentNullException">buffer is null</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void Write(ReadOnlySpan<byte> source)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      long end = (long)this.position + source.Length;
      // Check for overflow
      if (end > MaxStreamLength)
      {
        throw new IOException("Maximum capacity exceeded");
      }

      if (end >= this.capacity)
      {
        this.EnsureCapacity((int) end);
      }

      if (this.largeBuffer == null)
      {
        var blockAndOffset = this.GetBlockAndRelativeOffset(this.position);

        while (source.Length > 0)
        {
          byte[] currentBlock = this.blocks[blockAndOffset.Block];
          int remainingInBlock = this.blockSize - blockAndOffset.Offset;
          int amountToWriteInBlock = Math.Min(remainingInBlock, source.Length);

          source.Slice(0, amountToWriteInBlock)
              .CopyTo(currentBlock.AsSpan(blockAndOffset.Offset));

          source = source.Slice(amountToWriteInBlock);

          ++blockAndOffset.Block;
          blockAndOffset.Offset = 0;
        }
      }
      else
      {
        source.CopyTo(this.largeBuffer.AsSpan(this.position));
      }
      this.position = (int)end;
      this.length = Math.Max(this.position, this.length);
    }
#endif

    /// <summary>
    /// Returns a useful string for debugging. This should not normally be called in actual production code.
    /// </summary>
    public override string ToString()
    {
      return $"Id = {this.id}, Tag = {this.tag}, Length = {this.length:N0} bytes";
    }

    /// <summary>
    /// Writes a single byte to the current position in the stream.
    /// </summary>
    /// <param name="value">byte value to write</param>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void WriteByte(byte value)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      long end = (long)this.position + 1;

      // Check for overflow
      if (end > MaxStreamLength)
      {
        throw new IOException("Maximum capacity exceeded");
      }

      if (end >= this.capacity)
      {
        this.EnsureCapacity((int) end);
      }

      if (this.largeBuffer == null)
      {
        this.blocks[this.position / this.blockSize][this.position % this.blockSize] = value;
      }
      else
      {
        this.largeBuffer[this.position] = value;
      }

      this.position = (int)end;
      if (this.position > this.length)
      {
        this.length = this.position;
      }
    }

    /// <summary>
    /// Reads a single byte from the current position in the stream.
    /// </summary>
    /// <returns>The byte at the current position, or -1 if the position is at the end of the stream.</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override int ReadByte()
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      if (this.position == this.length)
      {
        return -1;
      }

      byte value;
      if (this.largeBuffer == null)
      {
        value = this.blocks[this.position / this.blockSize][this.position % this.blockSize];
      }
      else
      {
        value = this.largeBuffer[this.position];
      }
      this.position++;
      return value;
    }

    /// <summary>
    /// Sets the length of the stream
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">value is negative or larger than MaxStreamLength</exception>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    public override void SetLength(long value)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      if (value < 0 || value > MaxStreamLength)
      {
        throw new ArgumentOutOfRangeException(nameof(value),
                                              "value must be non-negative and at most " + MaxStreamLength);
      }

      this.EnsureCapacity((int)value);

      this.length = (int)value;
      if (this.position > value)
      {
        this.position = (int)value;
      }
    }

    /// <summary>
    /// Sets the position to the offset from the seek location
    /// </summary>
    /// <param name="offset">How many bytes to move</param>
    /// <param name="loc">From where</param>
    /// <returns>The new position</returns>
    /// <exception cref="ObjectDisposedException">Object has been disposed</exception>
    /// <exception cref="ArgumentOutOfRangeException">offset is larger than MaxStreamLength</exception>
    /// <exception cref="ArgumentException">Invalid seek origin</exception>
    /// <exception cref="IOException">Attempt to set negative position</exception>
    public override long Seek(long offset, SeekOrigin loc)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      if (offset > MaxStreamLength)
      {
        throw new ArgumentOutOfRangeException(nameof(offset), "offset cannot be larger than " + MaxStreamLength);
      }

      int newPosition;
      switch (loc)
      {
        case SeekOrigin.Begin:
          newPosition = (int)offset;
          break;
        case SeekOrigin.Current:
          newPosition = (int)offset + this.position;
          break;
        case SeekOrigin.End:
          newPosition = (int)offset + this.length;
          break;
        default:
          throw new ArgumentException("Invalid seek origin", nameof(loc));
      }
      if (newPosition < 0)
      {
        throw new IOException("Seek before beginning");
      }
      this.position = newPosition;
      return this.position;
    }

    /// <summary>
    /// Synchronously writes this stream's bytes to the parameter stream.
    /// </summary>
    /// <param name="stream">Destination stream</param>
    /// <remarks>Important: This does a synchronous write, which may not be desired in some situations</remarks>
    public override void WriteTo(Stream stream)
    {
#if USE_CHECK_DISPOSE_FASTIDIOUSLY
      this.CheckDisposed();
#endif

      if (stream == null)
      {
        throw new ArgumentNullException(nameof(stream));
      }

      if (this.largeBuffer == null)
      {
        int currentBlock = 0;
        int bytesRemaining = this.length;

        while (bytesRemaining > 0)
        {
          int amountToCopy = Math.Min(this.blocks[currentBlock].Length, bytesRemaining);
          stream.Write(this.blocks[currentBlock], 0, amountToCopy);

          bytesRemaining -= amountToCopy;

          ++currentBlock;
        }
      }
      else
      {
        stream.Write(this.largeBuffer, 0, this.length);
      }
    }
    #endregion

    #region Helper Methods
    private bool Disposed => Interlocked.Read(ref this.disposedState) != 0;

    private void CheckDisposed()
    {
      if (this.Disposed)
      {
        throw new ObjectDisposedException($"The stream with Id {this.id} and Tag {this.tag} is disposed.");
      }
    }

    private int InternalRead(byte[] buffer, int offset, int count, int fromPosition)
    {
      if (this.length - fromPosition <= 0)
      {
        return 0;
      }

      int amountToCopy;

      if (this.largeBuffer == null)
      {
        int blockIndex = fromPosition / this.blockSize;
        int blockOffset = fromPosition % this.blockSize;

        var block = this.blocks[blockIndex];

        int bytesWritten = 0;
        int bytesRemaining = Math.Min(count, this.length - fromPosition);
        while (bytesRemaining > 0)
        {
          amountToCopy = Math.Min(block.Length - blockOffset, bytesRemaining);
          Buffer.BlockCopy(block, blockOffset, buffer, bytesWritten + offset, amountToCopy);

          bytesWritten += amountToCopy;
          bytesRemaining -= amountToCopy;

          if (bytesRemaining <= 0)
          {
            return bytesWritten;
          }

          block = blocks[++blockIndex];
          blockOffset = 0;
        }
      }

      amountToCopy = Math.Min(count, this.length - fromPosition);
      Buffer.BlockCopy(this.largeBuffer, fromPosition, buffer, offset, amountToCopy);
      return amountToCopy;
    }

#if NETCOREAPP2_1 || NETSTANDARD2_1
    private int InternalRead(Span<byte> buffer, int fromPosition)
    {
      if (this.length - fromPosition <= 0)
      {
        return 0;
      }

      int amountToCopy;

      if (this.largeBuffer == null)
      {
        var blockAndOffset = this.GetBlockAndRelativeOffset(fromPosition);
        int bytesWritten = 0;
        int bytesRemaining = Math.Min(buffer.Length, this.length - fromPosition);

        while (bytesRemaining > 0)
        {
          amountToCopy = Math.Min(this.blocks[blockAndOffset.Block].Length - blockAndOffset.Offset,
                                  bytesRemaining);
          this.blocks[blockAndOffset.Block].AsSpan(blockAndOffset.Offset, amountToCopy)
              .CopyTo(buffer.Slice(bytesWritten));

          bytesWritten += amountToCopy;
          bytesRemaining -= amountToCopy;

          ++blockAndOffset.Block;
          blockAndOffset.Offset = 0;
        }
        return bytesWritten;
      }
      amountToCopy = Math.Min(buffer.Length, this.length - fromPosition);
      this.largeBuffer.AsSpan(fromPosition, amountToCopy).CopyTo(buffer);
      return amountToCopy;
    }
#endif

    private struct BlockAndOffset
    {
      public int Block;
      public int Offset;

      public BlockAndOffset(int block, int offset)
      {
        this.Block = block;
        this.Offset = offset;
      }
    }

    private BlockAndOffset GetBlockAndRelativeOffset(int offset)
    {
      return new BlockAndOffset(offset / this.blockSize, offset % this.blockSize);
    }

    private void EnsureCapacity(int newCapacity)
    {
      if (newCapacity > this.maximumStreamCapacity)
      {
        RecyclableMemoryStreamManager.Events.Writer.MemoryStreamOverCapacity(newCapacity,
                                                                            this.maximumStreamCapacity, this.tag,
                                                                            this.AllocationStack);
        throw new InvalidOperationException("Requested capacity is too large: " + newCapacity + ". Limit is " +
                                            this.maximumStreamCapacity);
      }

      if (this.largeBuffer != null)
      {
        if (newCapacity > this.capacity)
        {
          var newBuffer = this.memoryManager.GetLargeBuffer(newCapacity, this.tag);
          this.InternalRead(newBuffer, 0, this.length, 0);
          this.ReleaseLargeBuffer();
          this.largeBuffer = newBuffer;
          this.capacity = newBuffer.Length;
        }
      }
      else
      {
        while (this.capacity < newCapacity)
        { 
          if (this.blocksCount == this.blocks.Length)
            Array.Resize(ref this.blocks, this.blocks.Length + InitialBlocksArraySizeAndIncrement);

          this.blocks[this.blocksCount++] = this.memoryManager.GetBlock();

          long size = (long)this.blocksCount * this.blockSize;
          this.capacity = (int)Math.Min(int.MaxValue, size);
        }
      }
    }

    /// <summary>
    /// Release the large buffer (either stores it for eventual release or returns it immediately).
    /// </summary>
    private void ReleaseLargeBuffer()
    {
      if (this.memoryManager.AggressiveBufferReturn)
      {
        this.memoryManager.ReturnLargeBuffer(this.largeBuffer, this.tag);
      }
      else
      {
        if (this.dirtyBuffers == null)
        {
          // We most likely will only ever need space for one
          this.dirtyBuffers = new List<byte[]>(1);
        }
        this.dirtyBuffers.Add(this.largeBuffer);
      }

      this.largeBuffer = null;
    }
    #endregion
  }
}
